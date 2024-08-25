using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class PengGameManager : MonoBehaviour
{
    [ReadOnly]
    public float globalFrameRate = 60;

    [HideInInspector]
    public List<PengActor> actors;
    [HideInInspector]
    public PengBlackBoard<PengGameManager> bb;
    [HideInInspector]
    public PengBuffManager buff;
    [HideInInspector]
    public PengActorInput input;
    [HideInInspector]
    public Camera main;

    public Coroutine globalTimeScaleCoroutine = null;
    public PengEventManager eventManager;
    [HideInInspector]
    public int currentFrame;

    [HideInInspector]
    public List<PengEffectManager> psPool = new List<PengEffectManager>();
    [HideInInspector]
    public Transform vfxRoot;
    [HideInInspector]
    public AudioSource globalSource;
    [HideInInspector]
    float checkHasActorControlledByPlayerTimeCount = 0;

    [HideInInspector]
    public PengActor mainActor;
    [HideInInspector]
    public CinemachineFreeLook mainFL;
    [HideInInspector]
    public List<int> usedID = new List<int>();
    [HideInInspector]
    public PengLevelRuntimeManager level;

#if UNITY_EDITOR
    Rect timeRect = new Rect(0, 0, 160, 30);
#endif
    private void Awake()
    {
        ReadGlobalFrameRate();
        Physics.gravity = Vector3.zero;
        this.tag = "PengGameManager";
        bb = new PengBlackBoard<PengGameManager>(this);
        buff = this.gameObject.AddComponent<PengBuffManager>();
        main = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        eventManager = new PengEventManager();
        eventManager.game = this;
        buff.gameOwner = this;
        input = new PengActorInput();
        input.Enable();
        currentFrame = 0;
        vfxRoot = new GameObject().transform;
        vfxRoot.name = "PengVFXRoot";
        vfxRoot.SetParent(this.transform);
        globalSource = this.GetComponent<AudioSource>();
        level = this.AddComponent<PengLevelRuntimeManager>();
        level.game = this;
        GetAllExistingActor();

        GameObject mainFLPF = Resources.Load("Cameras/MainFreeLook") as GameObject;
        if (mainFLPF == null)
        {
            Debug.Log("实例化自由视角相机失败，请检查地址与命名是否为Resources/Cameras/MainFreeLook，若没有该预制体，则请在PengFramework/Prefab下寻找并复制粘贴到制指定为位置。");
        }
        else
        {
            GameObject mainFLGO = GameObject.Instantiate(mainFLPF);
            mainFL = mainFLGO.GetComponent<CinemachineFreeLook>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!HasControlledActor())
        {
            Debug.Log("暂无被玩家控制的Actor，请在运行时监控中设置。");
        }
        checkHasActorControlledByPlayerTimeCount = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        currentFrame = Mathf.FloorToInt( Time.time * globalFrameRate );
        if (Time.time - checkHasActorControlledByPlayerTimeCount >= 1.5f)
        {
            checkHasActorControlledByPlayerTimeCount = Time.time;
            if (!HasControlledActor())
            {
                Debug.Log("暂无被玩家控制的Actor，请在运行时监控中设置。");
            }
        }


    }

#if UNITY_EDITOR
    public void OnGUI()
    {
        GUIStyle styleBox = new GUIStyle("Box");
        styleBox.fontSize = 14;
        styleBox.alignment = TextAnchor.MiddleCenter;
        string time = ((float)currentFrame / globalFrameRate).ToString("f2");
        GUI.Box(timeRect, "运行时间："+ time +"s");
        
    }
#endif


    public void ReadGlobalFrameRate()
    {
        XmlDocument xml = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("GlobalConfiguration/GlobalSetting");
        if(textAsset != null)
        {
            xml.LoadXml(textAsset.text);
            XmlNode frameSettingNode = xml.SelectSingleNode("FrameSetting");
            if (frameSettingNode != null)
            {
                XmlElement ele = (XmlElement)frameSettingNode;
                globalFrameRate = float.Parse(ele.GetAttribute("ActionFrameRate"));
            }
            else
            {
                Debug.LogWarning("全局配置中没有全局动作帧率信息！");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
        }
        else
        {
            Debug.LogWarning("未读取到全局配置！");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public void GloablTimeScaleFunc(float timeScale, float duration)
    {
        if (globalTimeScaleCoroutine != null)
        {
            StopCoroutine(globalTimeScaleCoroutine);
            globalTimeScaleCoroutine = null;
            Time.timeScale = 1;
        }
        globalTimeScaleCoroutine = StartCoroutine(GlobalTimeScale(timeScale, duration));
    }

    IEnumerator GlobalTimeScale(float timeScale, float duration)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
        yield break;
    }

    public void DisableActorInput()
    {
        if (actors.Count == 0)
        {
            return;
        }

        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].GetComponent<PengActorControl>().acceptInput = false;
        }
    }

    public void EnableActorInput()
    {
        if (actors.Count == 0)
        {
            return;
        }

        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].GetComponent<PengActorControl>().acceptInput = true;
        }
    }

    public ParticleSystem GenerateVFX(string path, Vector3 pos, Vector3 rot, Vector3 scale, PengActor master, bool follow, float deleteTime)
    {
        ParticleSystem result = null;
        bool reused = false;
        if (psPool.Count > 0)
        {
            for (int i = 0; i < psPool.Count; i++)
            {
                if (psPool[i].path == path && !psPool[i].gameObject.activeSelf)
                {
                    reused = true;
                    psPool[i].gameObject.SetActive(true);
                    psPool[i].ps.Play();
                    psPool[i].time = 0;

                    psPool[i].posOffset = pos;
                    psPool[i].rotOffset = rot;
                    psPool[i].scaleOffset = scale;
                    psPool[i].deleteTime = deleteTime;

                    psPool[i].transform.SetParent(master.transform, false);
                    psPool[i].transform.localPosition = pos;
                    psPool[i].transform.localRotation = Quaternion.Euler(rot);
                    psPool[i].transform.localScale = scale;
                    psPool[i].transform.SetParent(vfxRoot, true);

                    result = psPool[i].ps;
                    if (follow)
                    {
                        psPool[i].followTarget = master.transform;
                    }
                    else
                    {
                        psPool[i].followTarget = null;
                    }
                    break;
                }
            }
            if (!reused)
            {
                result = SetVFX(path, pos, rot, scale, master, follow, deleteTime).ps;
            }
        }
        else
        {
            result = SetVFX(path, pos, rot, scale, master, follow, deleteTime).ps;
        }
        return result;
    }

    public PengEffectManager SetVFX(string path, Vector3 pos, Vector3 rot, Vector3 scale, PengActor master, bool follow, float deleteTime)
    {
        GameObject go = GameObject.Instantiate(Resources.Load("Effects/" + path)) as GameObject;
        PengEffectManager pvem = go.AddComponent<PengEffectManager>();
        psPool.Add(pvem);
        pvem.ps = go.GetComponent<ParticleSystem>();
        pvem.game = this;
        pvem.time = 0;
        pvem.path = path;

        pvem.posOffset = pos;
        pvem.rotOffset = rot;
        pvem.scaleOffset = scale;
        pvem.deleteTime = deleteTime;

        go.transform.SetParent(master.transform, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(rot);
        go.transform.localScale = scale;
        go.transform.SetParent(vfxRoot, true);

        return pvem;
    }

    public bool HasControlledActor()
    {
        bool result = false;
        if (actors.Count > 0)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                if (!actors[i].input.aiCtrl)
                {
                    result = true; break;
                }
            }
        }
        return result;
    }

    public void SetMainActor(PengActor actor, bool setMainFL)
    {
        foreach (PengActor p in actors)
        {
            p.input.aiCtrl = true;
        }
        actor.input.aiCtrl = false;
        mainActor = actor;

        if (setMainFL)
        {
            OnMainActorChangedSetMainFL();
        }
    }

    public void OnMainActorChangedSetMainFL()
    {
        mainFL.Follow = mainActor.transform;
        mainFL.LookAt = mainActor.lookAt;
    }

    public int GenerateRuntimeID()
    {
        int id = 1000;
        id += usedID.Count;
        usedID.Add(id);
        return id;
    }

    public void GetAllExistingActor()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("PengActor");
        foreach (GameObject go in gos)
        {
            PengActor actor = null;
            try
            {
                actor = go.GetComponent<PengActor>();
            }
            catch
            { }
            
            if (actor != null)
            {
                ConfigActor(actor);
            }
        }
    }

    public PengActor AddNewActor(int actorID, Vector3 position, Vector3 lookAtPosition)
    {
        GameObject actorPF = Resources.Load("Actors/" + actorID.ToString() + "/Actor" + actorID.ToString()) as GameObject;
        if (actorPF == null)
        {
            Debug.LogWarning("Actor" + actorID.ToString() + "不存在，无法召唤。");
            return null;
        }
        GameObject actorGO = GameObject.Instantiate(actorPF);
        actorGO.transform.position = position;
        Vector3 dir = lookAtPosition - position;
        dir -= dir.y * Vector3.up;
        dir = dir.normalized;
        actorGO.transform.LookAt(actorGO.transform.position + dir);
        PengActor actor = actorGO.GetComponent<PengActor>();

        ConfigActor(actor);
        return actor;
    }

    public void ConfigActor(PengActor actor)
    {
        actors.Add(actor);
        actor.runtimeID = GenerateRuntimeID();
        actor.game = this;
        actor.input.InputListener();
    }
}


public class LabelAttribute : PropertyAttribute
{
    private string name = "";
    public string Name { get { return name; } }
    public LabelAttribute(string name)
    {
        this.name = name;
    }
}