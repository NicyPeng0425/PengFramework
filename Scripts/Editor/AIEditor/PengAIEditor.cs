using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Runtime.Remoting.Messaging;
using static PengActorControl;

public partial class PengAIEditor : EditorWindow
{
    public float currentScale = 1f;
    public PengAIEditorNodeConnection selectingPoint = null;
    public Rect nodeMapRect;
    public Rect sidePanelRect;
    public Vector2 initPos = new Vector2(540, 40);
    public Vector2 gridOffset = Vector2.zero;
    public int currentActorID = 0;
    public GameObject currentSelectingGO = null;
    public List<PengAIEditorNode.PengAIEditorNode> nodes = new List<PengAIEditorNode.PengAIEditorNode>();
    public PengActorControl.AIAttribute attribute = new PengActorControl.AIAttribute();

    [MenuItem("PengFramework/AI编辑器", false, 33)]
    static void Init()
    {
        PengAIEditor window = (PengAIEditor)GetWindow(typeof(PengAIEditor));
        window.position = new Rect(140, 140, 1200, 700);
        window.titleContent = new GUIContent("彭框架AI编辑器");
        window.minSize = new Vector2(700, 400);
    }

    private void OnEnable()
    {
    }

    private void OnGUI()
    {
        currentSelectingGO = Selection.activeGameObject;

        if (Selection.activeGameObject == null)
        {
            EditorGUILayout.HelpBox("请选择对象", MessageType.Info);
            return;
        }

        if (Selection.activeGameObject.GetComponent<PengActor>() == null)
        {
            EditorGUILayout.HelpBox("所选对象不含PengActor组件", MessageType.Warning);
            return;
        }

        if (Selection.activeGameObject.GetComponent<PengActor>().actorID != currentActorID)
        {
            currentActorID = Selection.activeGameObject.GetComponent<PengActor>().actorID;
            ReadActorAIData(Selection.activeGameObject.GetComponent<PengActor>().actorID);
        }

        if (nodes.Count == 0)
        {
            if (Selection.activeGameObject != null || currentSelectingGO != null)
            {
                currentActorID = Selection.activeGameObject.GetComponent<PengActor>().actorID;
                ReadActorAIData(currentActorID);
            }
        }

        sidePanelRect = new Rect(0, 0, 300, position.height);
        nodeMapRect = new Rect(sidePanelRect.width, 0, position.width - sidePanelRect.width, position.height);
        GUIStyle style1 = new GUIStyle("flow background");
        GUI.Box(nodeMapRect, "", style1);
        DrawNodeGraph();
        ProcessEvents(Event.current);
        DrawPendingConnectionLine(Event.current);

        DrawSidePanel();
    }

    public void ProcessRemoveNode(PengAIEditorNode.PengAIEditorNode node)
    {
        if (nodes.Count > 1)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].outID.Count > 0)
                {
                    for (int j = 0; j < nodes[i].outID.Count; j++)
                    {
                        if (nodes[i].outID.ElementAt(j).Value == node.nodeID)
                        {
                            nodes[i].outID[j] = -1;
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

    public void ReadActorAIData(int actorID)
    {
        nodes.Clear();
        attribute = new PengActorControl.AIAttribute();
        currentActorID = actorID;
        string path = Application.dataPath + "/Resources/AIs/" + actorID.ToString() + "/" + actorID.ToString() + ".xml";

        if (!File.Exists(path))
        {
            Debug.LogError("未读取到ID为" + actorID.ToString() + "的关卡数据！读取地址：" + path);
            return;
        }
        TextAsset textAsset = (TextAsset)Resources.Load("AIs/" + actorID.ToString() + "/" + actorID.ToString());
        if (textAsset == null)
        {
            Debug.LogError(actorID.ToString() + "的数据读取失败！怎么回事呢？");
            return;
        }
        XmlDocument doc = new XmlDocument();
        XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", "");
        doc.LoadXml(textAsset.text);

        XmlElement root = doc.DocumentElement;
        XmlNodeList childs = root.ChildNodes;

        XmlElement aiInfo = null;
        XmlElement aiScript = null;
        foreach (XmlElement ele in childs)
        {
            if (ele.Name == "ActorAIInfo")
            {
                aiInfo = ele;
                continue;
            }
            if (ele.Name == "ActorAIScript")
            {
                aiScript = ele;
                continue;
            }
        }

        if (aiInfo == null || aiInfo.ChildNodes.Count == 0)
        {
            Debug.LogError(actorID.ToString() + "的关卡数据里没有关卡信息！怎么回事呢？");
            return;
        }

        if (aiScript == null || aiScript.ChildNodes.Count == 0)
        {
            Debug.LogError(actorID.ToString() + "的关卡数据里没有关卡脚本！怎么回事呢？");
            return;
        }

        XmlNodeList infoChilds = aiInfo.ChildNodes;
        bool hasAttr = false;
        AIAttribute attr = new AIAttribute();
        foreach (XmlElement ele in infoChilds)
        {
            if (ele.Name == "ID")
            {
                //读取ID
                actorID = int.Parse(ele.GetAttribute("ActorID"));
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

        attribute = attr;

        XmlNodeList scriptChild = aiScript.ChildNodes;
        foreach (XmlElement ele in scriptChild)
        {
            PengAIEditorNode.PengAIEditorNode node = ReadPengAIEditorNode(ele, this);
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

    public void DrawNodeGraph()
    {
        DrawGrid(20 * currentScale, 0.2f, Color.gray);
        DrawGrid(100 * currentScale, 0.4f, Color.gray);
        DrawNodes();
    }

    public void DrawNodes()
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

    public void DrawSidePanel()
    {
        GUIStyle style = new GUIStyle("ObjectPickerPreviewBackground");
        GUI.Box(sidePanelRect, "", style);
        style.alignment = TextAnchor.MiddleLeft;
        PengEditorMain.DrawPengFrameworkIcon("AI编辑器");

        GUIStyle styleSave = new GUIStyle("Button");
        styleSave.alignment = TextAnchor.MiddleCenter;
        styleSave.fontStyle = FontStyle.Bold;
        styleSave.fontSize = 13;
        styleSave.normal.textColor = new Color(0.4f, 0.95f, 0.6f);
        Rect saveButton = new Rect(sidePanelRect.x + 180, sidePanelRect.y + 5, 40, 40);
        EditorGUIUtility.AddCursorRect(saveButton, MouseCursor.Link);
        if (GUI.Button(saveButton, "保存\n数据", styleSave))
        {
            SaveActorAIData(true, currentActorID, nodes, attribute);
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].isSelected)
            {
                nodes[i].DrawSideBar(new Rect(sidePanelRect.x, sidePanelRect.y + 150, sidePanelRect.width, sidePanelRect.height - 150));
            }
        }

        Rect basicInfo = new Rect(sidePanelRect.x + 5, sidePanelRect.y + 50, sidePanelRect.width - 10, 80);
        Rect id = new Rect(basicInfo.x, basicInfo.y, basicInfo.width, 20);
        Rect id2 = new Rect(id.x + 70, id.y, id.width - 70, 20);
        Rect chaseDisLbl = new Rect(id.x, id.y + 25, basicInfo.width * 0.16f, 20);
        Rect chaseDis = new Rect(chaseDisLbl.x + chaseDisLbl.width, chaseDisLbl.y, chaseDisLbl.width, 20);
        Rect chaseStopDisLbl = new Rect(chaseDis.x + chaseDis.width + 5, chaseDis.y, chaseDis.width, 20);
        Rect chaseStopDis = new Rect(chaseStopDisLbl.x + chaseStopDisLbl.width, chaseStopDisLbl.y, chaseStopDisLbl.width, 20);
        Rect dcdCDLbl = new Rect(chaseStopDis.x + chaseStopDis.width + 5, chaseStopDis.y, chaseStopDis.width, 20);
        Rect dcdCD = new Rect(dcdCDLbl.x + dcdCDLbl.width, dcdCDLbl.y, dcdCDLbl.width, 20);

        Rect vsbDisLbl = new Rect(id.x, id.y + 50, basicInfo.width * 0.16f, 20);
        Rect vsbDis = new Rect(vsbDisLbl.x + vsbDisLbl.width, vsbDisLbl.y, vsbDisLbl.width, 20);
        Rect vsbHgtLbl = new Rect(vsbDis.x + vsbDis.width + 5, vsbDis.y, vsbDis.width, 20);
        Rect vsbHgt = new Rect(vsbHgtLbl.x + vsbHgtLbl.width, vsbHgtLbl.y, vsbHgtLbl.width, 20);
        Rect vsbAgLbl = new Rect(vsbHgt.x + vsbHgt.width + 5, vsbHgt.y, vsbHgt.width, 20);
        Rect vsbAg = new Rect(vsbAgLbl.x + vsbAgLbl.width, vsbAgLbl.y, vsbAgLbl.width, 20);

        GUI.Box(id, "角色ID：", style);
        GUI.Box(id2, currentActorID.ToString(), style);
        GUI.Box(chaseDisLbl, "追逐距离", style);
        attribute.chaseDistance = EditorGUI.FloatField(chaseDis, attribute.chaseDistance);
        GUI.Box(chaseStopDisLbl, "停止距离", style);
        attribute.chaseStopDistance = EditorGUI.FloatField(chaseStopDis, attribute.chaseStopDistance);
        GUI.Box(dcdCDLbl, "决策间隔", style);
        attribute.decideCD = EditorGUI.FloatField(dcdCD, attribute.decideCD);
        GUI.Box(vsbDisLbl, "可视距离", style);
        attribute.visibleDistance = EditorGUI.FloatField(vsbDis, attribute.visibleDistance);
        GUI.Box(vsbHgtLbl, "可视高度", style);
        attribute.visibleHeight = EditorGUI.FloatField(vsbHgt, attribute.visibleHeight);
        GUI.Box(vsbAgLbl, "可视角度", style);
        attribute.visibleAngle = EditorGUI.FloatField(vsbAg, attribute.visibleAngle);

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

    private void RightMouseMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        for (int i = 0; i < Enum.GetValues(typeof(PengAIScript.AIScriptType)).Length; i++)
        {
            RightMouseMenuDetail(ref menu, (PengAIScript.AIScriptType)Enum.GetValues(typeof(PengAIScript.AIScriptType)).GetValue(i), mousePos);
        }

        menu.ShowAsContext();
    }

    public void RightMouseMenuDetail(ref GenericMenu menu, PengAIScript.AIScriptType scriptType, Vector2 mousePos)
    {
        if (PengNode.GetCodedDown(scriptType) && scriptType != PengAIScript.AIScriptType.EventDecide)
        {
             menu.AddItem(new GUIContent("添加节点/" + PengNode.GetCatalogByFunction(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(mousePos, scriptType); });
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

    private void DrawPendingConnectionLine(Event e)
    {
        if (selectingPoint != null)
        {
            Vector3 start = (selectingPoint.type == PengAIEditorNodeConnection.AINodeConnectionType.In) ? selectingPoint.rect.center : e.mousePosition;
            Vector3 end = (selectingPoint.type == PengAIEditorNodeConnection.AINodeConnectionType.In) ? e.mousePosition : selectingPoint.rect.center;
            if (selectingPoint.type == PengAIEditorNodeConnection.AINodeConnectionType.In || selectingPoint.type == PengAIEditorNodeConnection.AINodeConnectionType.Out) { Handles.DrawBezier(start, end, start - Vector3.up * 40f, end + Vector3.up * 40f, Color.white, null, 6f); }

            GUI.changed = true;

        }
    }

    public static void SaveActorAIData(bool showMsg, int actorID, List<PengAIEditorNode.PengAIEditorNode> nodes, PengActorControl.AIAttribute attribute)
    {
        XmlDocument doc = new XmlDocument();
        XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", "");
        XmlElement root = doc.CreateElement("ActorAI");
        XmlElement info = doc.CreateElement("ActorAIInfo");
        XmlElement script = doc.CreateElement("ActorAIScript");

        XmlElement id = doc.CreateElement("ID");
        id.SetAttribute("ActorID", actorID.ToString());
        info.AppendChild(id);

        XmlElement attr = doc.CreateElement("Attribute");
        attr.SetAttribute("ChaseDistance", attribute.chaseDistance.ToString());
        attr.SetAttribute("ChaseStopDistance", attribute.chaseStopDistance.ToString());
        attr.SetAttribute("DecideCD", attribute.decideCD.ToString());
        attr.SetAttribute("VisibleDistance", attribute.visibleDistance.ToString());
        attr.SetAttribute("VisibleHeight", attribute.visibleHeight.ToString());
        attr.SetAttribute("VisibleAngle", attribute.visibleAngle.ToString());

        info.AppendChild(attr);

        bool conditioNodeHasOutNoConnected = false;
        bool ratioOutOfRange = false;
        for (int i = 0; i < nodes.Count; i++)
        {
            XmlElement node = doc.CreateElement("Script" + nodes[i].nodeID.ToString());
            node.SetAttribute("Name", nodes[i].name);
            node.SetAttribute("ScriptType", nodes[i].type.ToString());
            node.SetAttribute("NodeType", nodes[i].nodeType.ToString());
            node.SetAttribute("NodePos", PengGameManager.ParseVector2ToString(nodes[i].pos));
            node.SetAttribute("ScriptID", nodes[i].nodeID.ToString());
            node.SetAttribute("Position", PengGameManager.ParseVector2ToString(nodes[i].pos));
            node.SetAttribute("OutID", PengGameManager.ParseDictionaryIntIntToString(nodes[i].outID));
            node.SetAttribute("SpecialInfo", nodes[i].SpecialParaDescription());

            if (nodes[i].type == PengAIScript.AIScriptType.Condition ||
                nodes[i].type == PengAIScript.AIScriptType.Random ||
                nodes[i].type == PengAIScript.AIScriptType.Sequence)
            {
                switch (nodes[i].type)
                {
                    case PengAIScript.AIScriptType.Condition:
                        PengAIEditorNode.Condition cond = nodes[i] as PengAIEditorNode.Condition;
                        if (cond.outID.Count > 0)
                        {
                            for (int j = 0; j < cond.outID.Count; j++)
                            {
                                if (cond.outID[j] < 0)
                                {
                                    conditioNodeHasOutNoConnected = true;
                                }
                            }
                        }
                        break;
                    case PengAIScript.AIScriptType.Random:
                        PengAIEditorNode.Random rand = nodes[i] as PengAIEditorNode.Random;
                        if (rand.outID.Count > 0)
                        {
                            for (int j = 0; j < rand.outID.Count; j++)
                            {
                                if (rand.outID[j] < 0)
                                {
                                    conditioNodeHasOutNoConnected = true;
                                }
                            }
                        }
                        if(rand.SpecialParaDescription() == "TotalRatioOutOfRange")
                        {
                            ratioOutOfRange = true;
                        }
                        break;
                    case PengAIScript.AIScriptType.Sequence:
                        PengAIEditorNode.Sequence seq = nodes[i] as PengAIEditorNode.Sequence;
                        if (seq.outID.Count > 0)
                        {
                            for (int j = 0; j < seq.outID.Count; j++)
                            {
                                if (seq.outID[j] < 0)
                                {
                                    conditioNodeHasOutNoConnected = true;
                                }
                            }
                        }
                        break;
                }

            }


            script.AppendChild(node);
        }

        if (conditioNodeHasOutNoConnected)
        {
            EditorUtility.DisplayDialog("风险", "存在分支节点的分支没有连接后续节点，将不会保存！", "确认");
            return;
        }
        else if (ratioOutOfRange)
        {
            EditorUtility.DisplayDialog("风险", "存在随机分支节点的概率总和不等于1，将不会保存！", "确认");
            return;
        }

        root.AppendChild(info);
        root.AppendChild(script);
        doc.AppendChild(root);

        if (!Directory.Exists(Application.dataPath + "/Resources/AIs/" + actorID.ToString()))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources/AIs/" + actorID.ToString());
        }
        doc.Save(Application.dataPath + "/Resources/AIs/" + actorID.ToString() + "/" + actorID.ToString() + ".xml");
        if (showMsg)
        {
            Debug.Log("保存成功，保存于" + Application.dataPath + "/Resources/AIs/" + actorID.ToString() + "/" + actorID.ToString() + ".xml");
        }
        AssetDatabase.Refresh();
    }
}
