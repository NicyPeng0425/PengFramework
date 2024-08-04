using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class PengActorStateEditorWindow : EditorWindow
{
    /// <summary>
    /// 节点图部分主要参考：
    /// https://blog.csdn.net/qq_31967569/article/details/81025419
    /// https://blog.csdn.net/u013412391/article/details/120873714
    /// 感谢两位大佬！
    /// </summary>
    public List<PengNode> nodes = new List<PengNode>();
    public List<PengNodeConnectionLine> lines = new List<PengNodeConnectionLine>();
    public PengNodeConnection selectingPoint;
    private Vector2 gridOffset;
    private float nodeMapScale = 1f;
    public Rect timeLineRect;
    public Rect sideBarRect;

    float sideBarWidth = 250f;
    float timelineHeight = 350f;


    //当前编辑状态的信息缓存
    public Vector2 timelineScrollPos;
    public int currentFrameLength;
    public int currentTrackLength;
    public int currentSelectedFrame;
    public int currentSelectedTrack;
    public List<PengTrack> tracks = new List<PengTrack>();
    //


    [MenuItem("PengFramework/StateEditor")]
    static void Init()
    {
        PengActorStateEditorWindow window = (PengActorStateEditorWindow)EditorWindow.GetWindow(typeof(PengActorStateEditorWindow));
        window.position = new Rect(100, 100, 1200, 700);
        window.titleContent = new GUIContent("彭框架角色状态编辑器");
    }

    private void OnEnable()
    {
        nodes.Add(new PengNode(new Vector2(0, 0), "默认节点", this, PengNode.NodeType.Event));

        UpdateCurrentStateInfo();
        gridOffset = new Vector2(300f, 415f);
        DragAllNodes(new Vector2(300f, 415f));
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle("ObjectPickerPreviewBackground");
        timeLineRect = new Rect(0, 0, position.width, timelineHeight);
        sideBarRect = new Rect(0, 0, sideBarWidth, position.height);
        
        DrawNodeGraph(nodes);
        ProcessEvents(Event.current);
        DrawPendingConnectionLine(Event.current);

        GUI.Box(new Rect(0, 0, position.width, timelineHeight), "", style);
        GUI.Box(new Rect(0, 0, sideBarWidth, position.height), "", style);

        EditorGUILayout.BeginHorizontal();


        EditorGUILayout.BeginVertical(GUILayout.Height(position.height), GUILayout.Width(sideBarWidth));

        PengEditorMain.DrawPengFrameworkIcon("角色状态编辑器");

        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical(GUILayout.Height(timelineHeight), GUILayout.Width(position.width));

        DrawTimeLine();
        EditorGUILayout.EndVertical();


        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            Repaint();
        }
        
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

        //存一个播放动画的脚本

        data.AppendChild(info);
        data.AppendChild(scripts);
        xml.AppendChild(data);
        xml.Save(Application.dataPath + "Resources/ActorData/" + id + "/" + id + "@" + stateName + ".xml");
    }

    public static void SaveStateXML(string id, string stateName)
    {

    }

    public void DrawTimeLine()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(timelineHeight));

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(50));

        DrawTimeLineTitle();

        EditorGUILayout.EndVertical();

        timelineScrollPos = EditorGUILayout.BeginScrollView(timelineScrollPos, GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(timelineHeight - 50));

        GUIStyle style = new GUIStyle("GroupBox");
        GUIStyle pointer = new GUIStyle("MeBlendPosition");
        

        EditorGUILayout.BeginHorizontal(GUILayout.Width(currentFrameLength * 8f + 50), GUILayout.Height(20));
        GUILayout.Space(50);
        GUILayout.Box("", style, GUILayout.Width(currentFrameLength * 8f), GUILayout.Height(20));
        GUILayout.Space( - currentFrameLength * 8f);
        if(GUILayout.Button("", pointer, GUILayout.Height(20)))
        {

        }

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    public void DrawSideBar()
    {

    }

    public void DrawTimeLineTitle()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(40));

        EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(20));
        GUILayout.Label("PengActor ID：");
        GUILayout.Space(10);
        GUILayout.Label("100425");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(20));
        if(GUILayout.Button("创建轨道", GUILayout.Width(100)))
        {
            
        }

        //enumpop

        //stateName

        //stateLoop

        //stateLength

        //save
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

    }    

    public void DrawNodeGraph(List<PengNode> nodes)
    {
        DrawGrid(20 * nodeMapScale, 0.2f, Color.gray);
        DrawGrid(100 * nodeMapScale, 0.4f, Color.gray);
        DrawNodes();
        DrawConnectionLines();
    }

    private void ProcessEvents(Event e)
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
                if(e.button == 1)
                {
                    RightMouseMenu(e.mousePosition);
                }
                if (e.button == 0)
                {
                    selectingPoint = null;
                }
                break;
            case EventType.MouseDrag:
                if (e.button == 0 && !timeLineRect.Contains(e.mousePosition) && !sideBarRect.Contains(e.mousePosition))
                {
                    DragAllNodes(e.delta);
                    gridOffset += e.delta;
                    GUI.changed = true;
                }
                break;/*
            case EventType.ScrollWheel:
                if (e.button == 2 && !timeLineRect.Contains(e.mousePosition) && !sideBarRect.Contains(e.mousePosition))
                {
                    nodeMapScale += e.delta.y;
                    GUI.changed = true;
                }
                break;*/
        }
    }

    private void RightMouseMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("添加节点"), false, () => { ProcessAddNode(mousePos); });
        menu.ShowAsContext();
    }

    private void ProcessAddNode(Vector2 mousePos)
    {
        nodes.Add(new PengNode(mousePos, "默认节点", this, PengNode.NodeType.Action));
    }

    private void DrawNodes()
    {
        if (nodes.Count > 0)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
    }

    private void DrawConnectionLines()
    {
        if (lines.Count > 0)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].Draw();
            }
        }
    }

    private void DrawPendingConnectionLine(Event e)
    {
        if (selectingPoint != null)
        {
            Vector3 start = (selectingPoint.type == ConnectionPointType.In) ? selectingPoint.rect.center : e.mousePosition;
            Vector3 end = (selectingPoint.type == ConnectionPointType.In) ? e.mousePosition : selectingPoint.rect.center;
            Handles.DrawBezier(start, end, start + Vector3.left * 40f, end - Vector3.left * 40f, Color.white, null, 3f);

            GUI.changed = true;

        }
    }

    public void ProcessRemoveNode(PengNode node)
    {
        List<PengNodeConnectionLine> toRemove = new List<PengNodeConnectionLine>();
        if (lines.Count > 0)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].inPoint == node.inPoint || lines[i].outPoint == node.outPoint)
                {  toRemove.Add(lines[i]); }
            }

            for(int i = 0; i < toRemove.Count;i++)
            {
                lines.Remove(toRemove[i]);
            }
        }
        toRemove = null;
        nodes.Remove(node);
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
                Handles.DrawLine(new Vector3(gridSpacing * i, - gridSpacing, 0) + go, new Vector3(gridSpacing * i, position.height + gridSpacing, 0f) + go);
            }
            for (int i = 0; i < widthDiv; i++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * i, 0) + go, new Vector3(position.width + gridSpacing, gridSpacing * i, 0f) + go);
            }
            Handles.color = Color.white;
        }
        Handles.EndGUI();
    }

    public void UpdateCurrentStateInfo() 
    {
        currentFrameLength = 300;
        currentSelectedFrame = 50;
        currentSelectedTrack = 5;
        currentTrackLength = 8;
        timelineScrollPos = Vector2.zero;
    }
}
