using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class PengActorStateEditorWindow : EditorWindow
{
    [MenuItem("PengFramework/StateEditor")]
    static void Init()
    {
        PengActorStateEditorWindow window = (PengActorStateEditorWindow)EditorWindow.GetWindow(typeof(PengActorStateEditorWindow));
        window.position = new Rect(100, 100, 1000, 800);
        window.titleContent = new GUIContent("���ܽ�ɫ״̬�༭��");
    }

    private void OnEnable()
    {
        
    }

    private void OnGUI()
    {
        
    }

    public static void CreateStateXML(string id, string stateName, int length)
    {
        XmlDocument xml = new XmlDocument();
        XmlElement data = xml.CreateElement("Data");
        XmlElement info = xml.CreateElement("Info");
        XmlElement scripts = xml.CreateElement("Script");

        info.SetAttribute("Name", stateName);
        info.SetAttribute("Loop", stateName == "Idle" ? "1" : "0");
        info.SetAttribute("Length", length.ToString());

        //��һ�����Ŷ����Ľű�

        data.AppendChild(info);
        data.AppendChild(scripts);
        xml.AppendChild(data);
        xml.Save(Application.dataPath + "Resources/ActorData/" + id + "/" + id + "@" + stateName + ".xml");
    }

    public static void SaveStateXML(string id, string stateName)
    {

    }
}
