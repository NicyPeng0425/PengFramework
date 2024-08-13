using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PengGameManager : MonoBehaviour
{
    public float globalFrameRate = 60;
    
    public List<PengActor> actors;
    public PengBlackBoard<PengGameManager> bb;
    public PengBuffManager buff;


    public Coroutine globalTimeScaleCoroutine = null;
    private void Awake()
    {
        ReadGlobalFrameRate();
        Physics.gravity = Vector3.zero;
        this.tag = "PengGameManager";
        bb = new PengBlackBoard<PengGameManager>(this);
        buff = this.AddComponent<PengBuffManager>();
        buff.gameOwner = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

    
}
