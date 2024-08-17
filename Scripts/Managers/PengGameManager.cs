using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PengGameManager : MonoBehaviour
{
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
    private void Awake()
    {
        ReadGlobalFrameRate();
        Physics.gravity = Vector3.zero;
        this.tag = "PengGameManager";
        bb = new PengBlackBoard<PengGameManager>(this);
        buff = this.AddComponent<PengBuffManager>();
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
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentFrame = Mathf.FloorToInt( Time.time / globalFrameRate );
    }

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
}
