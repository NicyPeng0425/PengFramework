using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using System.IO;
using PengLevelRuntimeFunction;
using static PengActorState;
using static UnityEditor.VersionControl.Asset;

public partial class PengLevelEditor : EditorWindow
{
    /// <summary>
    /// 关卡编辑器设计思路：
    /// 由功能与触发器组成。当触发器的条件满足后，发出一个执行流，执行后续的功能操作。
    /// </summary>
    /// 

    Vector2 gridOffset = Vector2.zero;
    public float currentScale = 1f;
    public Rect header;
    public Rect nodeMapRect;
    public Rect moreInfo;
    public Vector2 initPos = new Vector2(20, 80);
    public int levelID;
    public PengLevelNodeConnection selectingPoint = null;
    public List<PengLevelEditorNode> nodes = new List<PengLevelEditorNode>();


    [MenuItem("PengFramework/关卡编辑器", false, 21)]
    static void Init()
    {
        PengLevelEditor window = (PengLevelEditor)GetWindow(typeof(PengLevelEditor));
        window.position = new Rect(100, 100, 1200, 700);
        window.titleContent = new GUIContent("彭框架关卡编辑器");
        window.minSize = new Vector2(600, 400);
    }

    public void OnEnable()
    {
        ReadLevelData(200000);
    }

    private void OnGUI()
    {
        UpdateSegment();
        DrawGridBG();
        DrawNodes();
        ProcessEvents(Event.current);
        DrawPendingConnectionLine(Event.current);

        DrawMoreInfo();
    }

    void UpdateSegment()
    {
        header = new Rect(0, 0, position.width, 60);
        moreInfo = new Rect(0, position.height - 140, position.width, 140);
        nodeMapRect = new Rect(0, header.height, position.width, position.height - header.height - moreInfo.height);
    }

    void DrawMoreInfo()
    {
        GUIStyle style = new GUIStyle("ObjectPickerPreviewBackground");
        GUI.Box(header, "", style);
        GUI.Box(moreInfo, "", style);

        GUIStyle styleSave = new GUIStyle("Button");
        styleSave.alignment = TextAnchor.MiddleCenter;
        styleSave.fontStyle = FontStyle.Bold;
        styleSave.fontSize = 13;
        styleSave.normal.textColor = new Color(0.4f, 0.95f, 0.6f);
        Rect saveButton = new Rect(header.x + 180, header.y + 10, 40, 40);

        if (GUI.Button(saveButton, "保存\n数据", styleSave))
        {
            SaveLevelData(true);
        }

        PengEditorMain.DrawPengFrameworkIcon("关卡编辑器");
    }

    public void SaveLevelData(bool showMsg)
    {
        XmlDocument doc = new XmlDocument();
        XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", "");
        XmlElement root = doc.CreateElement("Level");
        XmlElement info = doc.CreateElement("LevelInfo");
        XmlElement script = doc.CreateElement("LevelScript");

        XmlElement id = doc.CreateElement("ID");
        id.SetAttribute("LevelID", levelID.ToString());
        info.AppendChild(id);

        for (int i = 0; i < nodes.Count; i++)
        {
            XmlElement node = doc.CreateElement("Script" + nodes[i].nodeID.ToString());
            node.SetAttribute("Name", nodes[i].name);
            node.SetAttribute("ScriptType", nodes[i].type.ToString());
            node.SetAttribute("NodeType", nodes[i].nodeType.ToString());
            node.SetAttribute("NodePos", PengLevelEditorNode.ParseVector2ToString(nodes[i].pos));
            node.SetAttribute("ScriptID", nodes[i].nodeID.ToString());
            node.SetAttribute("Position", PengLevelEditorNode.ParseVector2ToString(nodes[i].pos));
            node.SetAttribute("ParaNum", nodes[i].paraNum.ToString());
            node.SetAttribute("OutID", PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(nodes[i].outID));
            node.SetAttribute("VarOutID", PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(nodes[i].varOutID));
            node.SetAttribute("VarInID", PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(nodes[i].varInID));
            node.SetAttribute("SpecialInfo", nodes[i].SpecialParaDescription());

            script.AppendChild(node);
        }

        root.AppendChild(info);
        root.AppendChild(script);
        doc.AppendChild(root);

        if (!Directory.Exists(Application.dataPath + "/Resources/Plot/" + levelID.ToString()))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources/Plot/" + levelID.ToString());
        }
        doc.Save(Application.dataPath + "/Resources/Plot/" + levelID.ToString() + "/" + levelID.ToString() + ".xml");
        if (showMsg)
        {
            Debug.Log("保存成功，保存于" + Application.dataPath + "/Resources/Plot/" + levelID.ToString() + "/" + levelID.ToString() + ".xml");
        }
        AssetDatabase.Refresh();
    }

    public void ReadLevelData(int id)
    {
        nodes.Clear();
        levelID = id;
        string path = Application.dataPath + "/Resources/Plot/" + levelID.ToString() + "/" + levelID.ToString() + ".xml";
        
        if (!File.Exists(path))
        {
            Debug.LogError("未读取到ID为" + levelID.ToString() + "的关卡数据！读取地址：" + path);
            return;
        }
        TextAsset textAsset = (TextAsset)Resources.Load("Plot/" + levelID.ToString() + "/" + levelID.ToString());
        if (textAsset == null)
        {
            Debug.LogError(levelID.ToString() + "的数据读取失败！怎么回事呢？");
            return;
        }
        XmlDocument doc = new XmlDocument();
        XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", "");
        doc.LoadXml(textAsset.text);

        XmlElement root = doc.DocumentElement;
        XmlNodeList childs = root.ChildNodes;

        XmlElement levelInfo = null;
        XmlElement levelScript = null;
        foreach (XmlElement ele in childs)
        {
            if (ele.Name == "LevelInfo")
            {
                levelInfo = ele;
                continue;
            }
            if (ele.Name == "LevelScript")
            {
                levelScript = ele;
                continue;
            }
        }

        if (levelInfo == null || levelInfo.ChildNodes.Count == 0)
        {
            Debug.LogError(levelID.ToString() + "的关卡数据里没有关卡信息！怎么回事呢？");
            return;
        }

        if (levelScript == null || levelScript.ChildNodes.Count == 0)
        {
            Debug.LogError(levelID.ToString() + "的关卡数据里没有关卡脚本！怎么回事呢？");
            return;
        }

        XmlNodeList infoChilds = levelInfo.ChildNodes;
        foreach (XmlElement ele in infoChilds)
        {
            if (ele.Name == "ID")
            {
                //读取ID
                levelID = int.Parse(ele.GetAttribute("LevelID"));
                continue;
            }
        }

        XmlNodeList scriptChild = levelScript.ChildNodes;
        foreach (XmlElement ele in scriptChild)
        {
            PengLevelEditorNode node = ReadPengLevelEditorNode(ele);
            node.editor = this;
            nodes.Add(node);
        }

        currentScale = 1;

        if (nodes.Count > 0)
        {
            Vector2 currentPos = nodes[0].pos;
            Vector2 targetPos = initPos - currentPos;
            DragAllNodes(targetPos);
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDiv = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDiv = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        {
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
            Vector3 go = new Vector3(gridOffset.x % gridSpacing, gridOffset.y % gridSpacing, 0);

            for (int i = 0; i < widthDiv; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + go, new Vector3(gridSpacing * i, position.height + gridSpacing, 0f) + go);
            }
            for (int i = 0; i < widthDiv; i++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * i, 0) + go, new Vector3(position.width + gridSpacing, gridSpacing * i, 0f) + go);
            }
            Handles.color = Color.white;
        }
        Handles.EndGUI();
    }

    private void DrawGridBG()
    {
        GUIStyle style1 = new GUIStyle("flow background");
        GUI.Box(nodeMapRect, "", style1);
        DrawGrid(20 * currentScale, 0.2f, Color.gray);
        DrawGrid(100 * currentScale, 0.4f, Color.gray);
    }

    private void DrawNodes()
    {
        GUIStyle style = new GUIStyle("AnimLeftPaneSeparator");
        style.alignment = TextAnchor.MiddleLeft;
        style.fontStyle = FontStyle.Bold;
        GUIStyle style1 = new GUIStyle("BoldTextField");
        style1.alignment = TextAnchor.MiddleLeft;
        style1.fontStyle = FontStyle.Bold;
        style1.fontSize = 12;
        GUIStyle style2 = new GUIStyle("dragtab");
        style2.alignment = TextAnchor.MiddleLeft;
        style2.fontStyle = FontStyle.Bold;
        style2.fontSize = 12;

        if (nodes.Count > 0)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }

        DrawConnectionLines();
    }

    public void ProcessRemoveNode(PengLevelEditorNode node)
    {
        if (nodes.Count > 1)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].outID.Count > 0)
                {
                    for (int j = 0; j < nodes[i].outID.Count; j++)
                    {
                        if (nodes[i].outID.ElementAt(j).Value.nodeID == node.nodeID)
                        {
                            PengLevelEditorNode.NodeIDConnectionID nici = new PengLevelEditorNode.NodeIDConnectionID();
                            nici.nodeID = -1;
                            nodes[i].outID[j] = nici;
                        }
                    }
                }

                if (nodes[i].varInID.Count > 0)
                {
                    for (int j = 0; j < nodes[i].varInID.Count; j++)
                    {
                        if (nodes[i].varInID.ElementAt(j).Value.nodeID == node.nodeID)
                        {
                            nodes[i].varInID[j] = PengLevelEditorNode.DefaultNodeIDConnectionID();
                        }
                    }
                }
            }
        }
        nodes.Remove(node);
    }

    private void DrawConnectionLines()
    {
        if (nodes.Count > 0)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].DrawLines();
            }
        }
    }

    private void ProcessEvents(Event e)
    {
        if (nodes.Count > 0)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool dragged = nodes[i].ProcessEvents(e);
                if (dragged)
                {
                    GUI.changed = true;
                }
            }

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1 && nodeMapRect.Contains(e.mousePosition))
                    {
                        RightMouseMenu(e.mousePosition);
                    }
                    if (e.button == 0)
                    {
                        selectingPoint = null;
                    }
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0 && nodeMapRect.Contains(e.mousePosition))
                    {
                        DragAllNodes(e.delta);
                        gridOffset += e.delta;
                        GUI.changed = true;
                    }
                    break;
                case EventType.ScrollWheel:/*
                    if (nodeMapRect.Contains(e.mousePosition))
                    {
                        currentScale -= e.delta.y * 0.02f;
                        if (currentScale <= 0.5f)
                        {
                            currentScale = 0.5f;
                        }
                        if (currentScale >= 1.5f)
                        {
                            currentScale = 1.5f;
                        }
                        GUI.changed = true;
                    }*/
                    break;

            }
        }
    }

    private void DragAllNodes(Vector2 change)
    {
        if (nodes.Count > 0)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].ProcessDrag(change);
            }
        }
    }

    private void RightMouseMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        for (int i = 0; i < Enum.GetValues(typeof(LevelFunctionType)).Length; i++)
        {
            RightMouseMenuDetail(ref menu, (LevelFunctionType)Enum.GetValues(typeof(LevelFunctionType)).GetValue(i), mousePos);
        }

        menu.ShowAsContext();
    }

    public void RightMouseMenuDetail(ref GenericMenu menu, LevelFunctionType scriptType, Vector2 mousePos)
    {
        if (PengLevelEditorNode.GetCodedDown(scriptType) /*&& scriptType != LevelFunctionType.Start*/)
        {
            menu.AddItem(new GUIContent("添加节点/" + PengLevelEditorNode.GetCatalog(scriptType) + "/" + PengLevelEditorNode.GetDescription(scriptType)), false, () => { ProcessAddNode(mousePos, scriptType); });
        }
    }

    private void DrawPendingConnectionLine(Event e)
    {
        if (selectingPoint != null)
        {
            Vector3 start = (selectingPoint.connectionType == PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn) ? selectingPoint.rect.center : e.mousePosition;
            Vector3 end = (selectingPoint.connectionType == PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn) ? e.mousePosition : selectingPoint.rect.center;

            if (selectingPoint.connectionType ==PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn || selectingPoint.connectionType == PengLevelNodeConnection.PengLevelNodeConnectionType.VarOut) { Handles.DrawBezier(start, end, start + Vector3.left * 40f, end - Vector3.left * 40f, Color.white, null, 3f); }
            else if (selectingPoint.connectionType == PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn || selectingPoint.connectionType == PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut) { Handles.DrawBezier(start, end, start + Vector3.left * 40f, end - Vector3.left * 40f, Color.white, null, 6f); }

            GUI.changed = true;

        }
    }
}
