using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml;
using UnityEditor.Animations;
using static PengActorControl;

public class PengAIGenerator : EditorWindow
{
    public enum AITemplate
    {
        新角色,
        复制角色,
    }

    public int actorID;
    public int copyID;
    public int pasteID;
    public AITemplate aiTplt = AITemplate.新角色;

    [MenuItem("PengFramework/AI生成器", false, 34)]
    static void Init()
    {
        PengAIGenerator window = (PengAIGenerator)GetWindow(typeof(PengAIGenerator));
        window.position = new Rect(150, 150, 300, 300);
        window.minSize = new Vector2(300, 300);
        window.maxSize = new Vector2(500, 500);
        window.titleContent = new GUIContent("彭框架AI生成器");
    }

    private void OnEnable()
    {
        actorID = 100001;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        PengEditorMain.DrawPengFrameworkIcon("AI生成器");
        GUILayout.Space(20);

        GUIStyle style = new GUIStyle("Box");
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        GUILayout.Box("填写注意：\n" +
            "好像暂时没啥需要注意的。\n", style);
        GUILayout.Space(10);

        aiTplt = (AITemplate)EditorGUILayout.EnumPopup(aiTplt);

        switch (aiTplt)
        {
            case AITemplate.新角色:
                GenerateNew();
                break;
            case AITemplate.复制角色:
                CopyActor();
                break;
        }
        EditorGUILayout.EndVertical();
    }

    private void GenerateNew()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("角色ID：");
        actorID = EditorGUILayout.IntField(actorID, GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("一键生成AI"))
        {
            if (File.Exists(Application.dataPath + "/Resources/AIs/" + actorID.ToString() + "/" + actorID.ToString() + ".xml"))
            {
                EditorUtility.DisplayDialog("警告", "已存在AI数据！", "ok");
                EditorGUILayout.EndVertical();
                return;
            }
            List<PengAIEditorNode.PengAIEditorNode> nodes = new List<PengAIEditorNode.PengAIEditorNode>();

            nodes.Add(new PengAIEditorNode.EventDecide(new Vector2(270, 40), null, 0, "0:-1", ""));

            PengActorControl.AIAttribute attr = new PengActorControl.AIAttribute();
            attr.chaseDistance = 10f;
            attr.chaseStopDistance = 3f;
            attr.decideCD = 2f;
            attr.visibleDistance = 15f;
            attr.visibleHeight = 3f;
            attr.visibleAngle = 180f;
            PengAIEditor.SaveActorAIData(true, actorID, nodes, attr);
            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndVertical();
    }

    private void CopyActor()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("原有对象ID：");
        copyID = EditorGUILayout.IntField(copyID);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("目标对象ID：");
        pasteID = EditorGUILayout.IntField(pasteID);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("一键复制"))
        {
            if (File.Exists(Application.dataPath + "/Resources/AIs/" + copyID.ToString() + "/" + copyID.ToString() + ".xml"))
            {
                TextAsset textAsset = (TextAsset)Resources.Load("AIs/" + copyID.ToString() + "/" + copyID.ToString());
                if (textAsset == null)
                {
                    Debug.LogError(copyID.ToString() + "的数据读取失败！怎么回事呢？");
                    return;
                }
                XmlDocument doc = new XmlDocument();
                XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", "");
                doc.LoadXml(textAsset.text);
                XmlElement root = doc.DocumentElement;
                XmlElement info = null;
                XmlElement script = null;
                foreach (XmlElement node in root.ChildNodes)
                {
                    if (node.Name == "ActorAIInfo")
                    {
                        info = node;
                    }
                    if (node.Name == "ActorAIScript")
                    {
                        script = node;
                    }
                }
                bool hasAttr = false;
                AIAttribute attr = new AIAttribute();
                foreach (XmlElement ele in info.ChildNodes)
                {
                    if (ele.Name == "ActorID")
                    {
                        ele.SetAttribute("ActorID", pasteID.ToString());
                        continue;
                    }
                    if (ele.Name == "Attribute")
                    {
                        hasAttr = true;
                        attr.chaseDistance = float.Parse(ele.GetAttribute("ChaseDistance"));
                        attr.chaseStopDistance = float.Parse(ele.GetAttribute("ChaseStopDistance"));
                        attr.decideCD = float.Parse(ele.GetAttribute("DecideCD"));
                        attr.visibleDistance = float.Parse(ele.GetAttribute("VisibleDistance"));
                        attr.visibleHeight = float.Parse(ele.GetAttribute("VisibleHeight"));
                        attr.visibleAngle = float.Parse(ele.GetAttribute("VisibleAngle"));
                    }
                }
                if (!hasAttr)
                {
                    attr.chaseDistance = 10f;
                    attr.chaseStopDistance = 3f;
                    attr.decideCD = 2f;
                    attr.visibleDistance = 15f;
                    attr.visibleHeight = 3f;
                    attr.visibleAngle = 180f;
                }

                if (!Directory.Exists(Application.dataPath + "/Resources/AIs/" + pasteID.ToString()))
                {
                    Directory.CreateDirectory(Application.dataPath + "/Resources/AIs/" + pasteID.ToString());
                }

                List<PengAIEditorNode.PengAIEditorNode> nodes = new List<PengAIEditorNode.PengAIEditorNode>();
                foreach (XmlElement ele in script.ChildNodes)
                {
                    PengAIEditorNode.PengAIEditorNode node = PengAIEditor.ReadPengAIEditorNode(ele, null);
                    nodes.Add(node);
                }

                PengAIEditor.SaveActorAIData(true, pasteID, nodes, attr);
                AssetDatabase.Refresh();
            }
        }

        EditorGUILayout.EndVertical();
    }
}
