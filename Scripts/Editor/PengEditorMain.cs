using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Linq;
using Cinemachine;

public class PengEditorMain : EditorWindow
{
    /// <summary>
    /// 启动器。必须要先用这个。
    /// 常用的UI绘制方法也放在这儿
    /// </summary>
    XmlDocument globalConfiguration;
    [MenuItem("PengFramework/启动器", false, -12)]
    static void Init()
    {
        PengEditorMain window = (PengEditorMain)GetWindow(typeof(PengEditorMain));
        window.position = new Rect(60,60,300,300);
        window.titleContent = new GUIContent("彭框架启动器");
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
                Debug.LogWarning("意外：Resources/GlobalConfiguration/GlobalSetting.xml存在，但却读不出TextAsset？");
            }
        }/*
        else
        {
            EditorGUILayout.HelpBox("暂不存在全局配置文件，请使用上方按钮来生成！", MessageType.Warning);
        }*/
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        
        DrawPengFrameworkIcon("启动器");
        
        

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
        GUILayout.Box(new GUIContent("彭框架" + title), style);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    public void DrawCreateResourcesDirectory()
    {
        GUIStyle style = new GUIStyle("Button");
        style.normal.textColor = Color.green;
        if (GUILayout.Button("创建、补全或修复Resources文件夹结构", style))
        {
            CreateResourcesDirectory("");
            CreateResourcesDirectory("/Actors");
            CreateResourcesDirectory("/ActorData");
            CreateResourcesDirectory("/AIs");
            CreateResourcesDirectory("/Cameras");
            CreateResourcesDirectory("/Effects");
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
            CreateResourcesDirectory("/BuffData");
            CreateResourcesDirectory("/InputData");
            CreateResourcesDirectory("/Plot");
            CreateResourcesDirectory("/Interactive");

            AddTag("PengGameManager");
            AddTag("PengActor");
            AddTag("Temporary");
            AddTag("PengLevel");
            AddTag("AirWall");
            AddLayer("PengActor", 10);
            AddLayer("Ground", 11);
            AddLayer("Wall", 12);
            AddLayer("AirWall", 13);
            AddLayer("PostProcess", 14);

            GameObject gameManager = new GameObject();
            gameManager.name = "Game";
            gameManager.tag = "PengGameManager";
            gameManager.AddComponent<PengGameManager>();
            gameManager.AddComponent<AudioSource>();
            if (!Directory.Exists(Application.dataPath + "/Resources/Managers/GameManager"))
            {
                Directory.CreateDirectory(Application.dataPath + "/Resources/Managers/GameManager");
            }
            bool success = false;
            PrefabUtility.SaveAsPrefabAsset(gameManager, Application.dataPath + "/Resources/Managers/GameManager/Game.prefab", out success);
            DestroyImmediate(gameManager);
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

                //后续添加更多全局配置信息

                global.AppendChild(frameSetting);
                global.Save(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml");
                Debug.Log("已创建全局配置");
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
                        Debug.LogWarning("全局配置中缺失全局动作帧帧率，现已补全。");
                    }

                    //后续补全更多全局配置信息

                    xml.Save(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml");
                }
                else
                {
                    Debug.LogWarning("意外：Resources/GlobalConfiguration/GlobalSetting.xml存在，但却读不出TextAsset？");
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
                    Debug.LogWarning("意外：Resources/GlobalConfiguration/GlobalSetting.xml存在，但却读不出TextAsset？");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("暂不存在全局配置文件，请使用上方按钮来生成！", MessageType.Warning);
            }
        }
        else
        {
            XmlElement frameSettingElement = (XmlElement)globalConfiguration.SelectSingleNode("FrameSetting");
            if (frameSettingElement != null)
            {
                float frameRate = float.Parse(frameSettingElement.GetAttribute("ActionFrameRate"));
                float frameRateNew = frameRate;

                GUILayout.Label("全局动作帧率：");
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
                EditorGUILayout.HelpBox("全局配置中不存在动作帧帧率信息，请使用上方按钮来修复！", MessageType.Warning);
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
                GUILayout.Label("索引" + i.ToString() + "：", GUILayout.Width(60));
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

    public static void AddTag(string tagname)
    {
        UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if ((asset != null) && (asset.Length > 0))
        {
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; ++i)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tagname)
                {
                    return;
                }
            }
            tags.InsertArrayElementAtIndex(0);
            tags.GetArrayElementAtIndex(0).stringValue = tagname;
            so.ApplyModifiedProperties();
            so.Update();
        }
    }

    void AddLayer(string layerName, int index)
    {
        int[] builtInIndex = { 0, 1, 2, 4, 5 };
        if (builtInIndex.Contains(index))
        {
            Debug.Log("can't use built-in layer index:" + index);
            return;
        }
        UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if ((asset != null) && (asset.Length > 0))
        {
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("layers");

            for (int i = 0; i < tags.arraySize; ++i)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(index);
            tags.GetArrayElementAtIndex(index).stringValue = layerName;
            so.ApplyModifiedProperties();
            so.Update();
        }
    }
}

[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LabelAttribute attr = attribute as LabelAttribute;
        if (attr.Name.Length > 0)
        {
            label.text = attr.Name;
        }
        EditorGUI.PropertyField(position, property, label);
    }
}