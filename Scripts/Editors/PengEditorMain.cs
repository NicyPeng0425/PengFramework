using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml;

public class PengEditorMain : EditorWindow
{
    /// <summary>
    /// ������������Ҫ���������
    /// ���õ�UI���Ʒ���Ҳ�������
    /// </summary>
    XmlDocument globalConfiguration;
    [MenuItem("PengFramework/Starter")]
    static void Init()
    {
        PengEditorMain window = (PengEditorMain)EditorWindow.GetWindow(typeof(PengEditorMain));
        window.position = new Rect(100,100,300,300);
        window.titleContent = new GUIContent("����������");
    }

    private void OnEnable()
    {
        if (File.Exists(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml"))
        {
            TextAsset textAsset = null;
            textAsset = (TextAsset)Resources.Load("GlobalConfiguration/GlobalSetting");
            if (textAsset != null)
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(textAsset.text);
                globalConfiguration = xml;
            }
            else
            {
                Debug.LogWarning("���⣺Resources/GlobalConfiguration/GlobalSetting.xml���ڣ���ȴ������TextAsset��");
            }
        }/*
        else
        {
            EditorGUILayout.HelpBox("�ݲ�����ȫ�������ļ�����ʹ���Ϸ���ť�����ɣ�", MessageType.Warning);
        }*/
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        
        DrawPengFrameworkIcon("������");
        
        

        GUILayout.Space(20);
        DrawCreateResourcesDirectory();
        GUILayout.Space(10);
        DrawFrameSetting();

        EditorGUILayout.EndVertical();
    }

    public static void DrawPengFrameworkIcon(string title)
    {
        EditorGUILayout.BeginHorizontal();

        GUIStyle style = new GUIStyle("Box");
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 12;
        Texture2D logo = new Texture2D(52, 42);
        byte[] logoData = File.ReadAllBytes(Application.dataPath + "/PengFramework/Logo/PengFWLogoSmall.png");
        logo.LoadImage(logoData);
        EditorGUI.DrawPreviewTexture(new Rect(4, 4, 52, 42), logo);
        GUILayout.Space(70);
        EditorGUILayout.BeginVertical();
        GUILayout.Space(13);
        GUILayout.Box(new GUIContent("����" + title), style);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    public void DrawCreateResourcesDirectory()
    {
        GUIStyle style = new GUIStyle("Button");
        style.normal.textColor = Color.green;
        if (GUILayout.Button("��������ȫ���޸�Resources�ļ��нṹ", style))
        {
            CreateResourcesDirectory("");
            CreateResourcesDirectory("/Actors");
            CreateResourcesDirectory("/ActorData");
            CreateResourcesDirectory("/AIs");
            CreateResourcesDirectory("/Cameras");
            CreateResourcesDirectory("/Fonts");
            CreateResourcesDirectory("/Items");
            CreateResourcesDirectory("/Managers");
            CreateResourcesDirectory("/Materials");
            CreateResourcesDirectory("/Scenes");
            CreateResourcesDirectory("/Sounds");
            CreateResourcesDirectory("/Sprites");
            CreateResourcesDirectory("/UIs");
            CreateResourcesDirectory("/Animators");
            CreateResourcesDirectory("/Animations");
            CreateResourcesDirectory("/GlobalConfiguration");
            CreateResourcesDirectory("/ActorData");
            AssetDatabase.Refresh();
        }
    }

    static void CreateResourcesDirectory(string dirName)
    {
        if (!Directory.Exists(Application.dataPath + "/Resources" + dirName))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources" + dirName);
        }

        if (dirName == "/GlobalConfiguration")
        {
            if(!File.Exists(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml"))
            {
                XmlDocument global = new XmlDocument();
                XmlDeclaration dec = global.CreateXmlDeclaration("1.0", "UTF-8", "");

                XmlElement frameSetting = global.CreateElement("FrameSetting");
                frameSetting.SetAttribute("ActionFrameRate", "60");

                //������Ӹ���ȫ��������Ϣ

                global.AppendChild(frameSetting);
                global.Save(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml");
                Debug.Log("�Ѵ���ȫ������");
            }
            else
            {
                XmlDocument xml = new XmlDocument();
                TextAsset textAsset = null;
                textAsset = (TextAsset)Resources.Load("GlobalConfiguration/GlobalSetting");
                if (textAsset != null)
                {
                    xml.LoadXml(textAsset.text);
                    
                    XmlNode frameSettingNode = xml.SelectSingleNode("FrameSetting");
                    if (frameSettingNode == null)
                    {
                        XmlElement frameSetting = xml.CreateElement("FrameSetting");
                        frameSetting.SetAttribute("ActionFrameRate", "60");
                        xml.AppendChild(frameSetting);
                        Debug.LogWarning("ȫ��������ȱʧȫ�ֶ���֡֡�ʣ����Ѳ�ȫ��");
                    }

                    //������ȫ����ȫ��������Ϣ

                    xml.Save(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml");
                }
                else
                {
                    Debug.LogWarning("���⣺Resources/GlobalConfiguration/GlobalSetting.xml���ڣ���ȴ������TextAsset��");
                }
            }
        }
        else
        {
            if(!Directory.Exists(Application.dataPath + "/Resources" + dirName + "/Universal") && dirName != "")
            {
                Directory.CreateDirectory(Application.dataPath + "/Resources" + dirName + "/Universal");
            }
        }
    }

    void DrawFrameSetting()
    {
        EditorGUILayout.BeginHorizontal();
        if(globalConfiguration == null)
        {
            if (File.Exists(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml"))
            {
                TextAsset textAsset = null;
                textAsset = (TextAsset)Resources.Load("GlobalConfiguration/GlobalSetting");
                if (textAsset != null)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(textAsset.text);
                    globalConfiguration = xml;
                }
                else
                {
                    Debug.LogWarning("���⣺Resources/GlobalConfiguration/GlobalSetting.xml���ڣ���ȴ������TextAsset��");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("�ݲ�����ȫ�������ļ�����ʹ���Ϸ���ť�����ɣ�", MessageType.Warning);
            }
        }
        else
        {
            XmlElement frameSettingElement = (XmlElement)globalConfiguration.SelectSingleNode("FrameSetting");
            if (frameSettingElement != null)
            {
                float frameRate = float.Parse(frameSettingElement.GetAttribute("ActionFrameRate"));
                float frameRateNew = frameRate;

                GUILayout.Label("ȫ�ֶ���֡�ʣ�");
                GUILayout.Space(10);
                frameRateNew = EditorGUILayout.FloatField(frameRateNew, GUILayout.Width(100));
                if (frameRateNew != frameRate)
                {
                    frameSettingElement.SetAttribute("ActionFrameRate", frameRateNew.ToString());
                    globalConfiguration.Save(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("ȫ�������в����ڶ���֡֡����Ϣ����ʹ���Ϸ���ť���޸���", MessageType.Warning);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public static List<string> DrawStringListEditorGUI(List<string> list)
    {
        List<string> result = new List<string>();

        GUIStyle styleG = new GUIStyle("Button");
        styleG.normal.textColor = Color.green;
        GUIStyle styleR = new GUIStyle("Button");
        styleR.normal.textColor = Color.red;

        EditorGUILayout.BeginVertical();

        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string element = list[i];
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("����" + i.ToString() + "��", GUILayout.Width(60));
                GUILayout.Space(10);
                element = GUILayout.TextField(element, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
                result.Add(element);
            }
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+", styleG, GUILayout.Width(25), GUILayout.Height(25)))
        {
            string newString = result[result.Count - 1];
            result.Add(newString);
        }

        GUILayout.Space(150);

        if (GUILayout.Button("-", styleR, GUILayout.Width(25), GUILayout.Height(25)))
        {
            result.RemoveAt(result.Count - 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        return result;
    }
}
