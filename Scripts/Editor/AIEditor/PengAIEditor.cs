using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class PengAIEditor : EditorWindow
{
    public float currentScale = 1f;
    public PengAIEditorNodeConnection selectingPoint = null;
    public Rect nodeMapRect;
    public Rect sidePanelRect;
    public Vector2 initPos = new Vector2(270, 40);
    public Vector2 gridOffset = Vector2.zero;
    public int currentActorID = 0;
    public GameObject currentSelectingGO = null;
    public List<PengAIEditorNode.PengAIEditorNode> nodes = new List<PengAIEditorNode.PengAIEditorNode>();

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
        nodes.Add(new PengAIEditorNode.EventDecide(initPos, this, 0, "0:-1", ""));
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
            ReadActorAIData(Selection.activeGameObject.GetComponent<PengActor>().actorID);
        }

        sidePanelRect = new Rect(0, 0, 250, position.height);
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
        //nodes.Clear();
        currentActorID = actorID;
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
    }

    public void DrawSidePanel()
    {
        GUIStyle style = new GUIStyle("ObjectPickerPreviewBackground");
        GUI.Box(sidePanelRect, "", style);
        PengEditorMain.DrawPengFrameworkIcon("AI编辑器");
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
                menu.AddItem(new GUIContent("添加节点/按类型分类/" + PengNode.GetCatalogByFunction(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(mousePos, scriptType); });
                menu.AddItem(new GUIContent("添加节点/按首字母分类/" + PengNode.GetCatalogByName(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(mousePos, scriptType); });
                menu.AddItem(new GUIContent("添加节点/按封装程度分类/" + PengNode.GetCatalogByPackage(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(mousePos, scriptType); });
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
}
