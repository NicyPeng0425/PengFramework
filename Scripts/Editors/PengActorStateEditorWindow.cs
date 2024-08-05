using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

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
    public Rect nodeMapRect;

    float sideBarWidth = 250f;
    float timelineHeight = 350f;
    float timelineLength = 10f;


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
        tracks.Add(new PengTrack(PengTrack.ExecTime.Update, "Track", 3, 20, this));

        UpdateCurrentStateInfo();
        gridOffset = new Vector2(300f, 415f);
        DragAllNodes(new Vector2(300f, 415f));
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle("ObjectPickerPreviewBackground");
        GUIStyle style1 = new GUIStyle("flow background");
        timeLineRect = new Rect(0, 45f, position.width, timelineHeight);
        sideBarRect = new Rect(0, 0, sideBarWidth, position.height);
        nodeMapRect = new Rect(sideBarWidth, timelineHeight, position.width - sideBarWidth, position.height - timelineHeight);
        GUI.Box(nodeMapRect, "", style1);
        
        DrawNodeGraph();
        ProcessEvents(Event.current);
        DrawPendingConnectionLine(Event.current);

        GUI.Box(new Rect(0, 0, position.width, timelineHeight), "", style);
        DrawTimelineMap();

        //绘制Timeline Title
        GUI.Box(new Rect(0, 0, position.width, 45), "", style);
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
        /*
        timelineScrollPos = EditorGUILayout.BeginScrollView(timelineScrollPos, GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(timelineHeight - 50));

        GUIStyle style = new GUIStyle("GroupBox");
        GUIStyle pointer = new GUIStyle("MeBlendPosition");
        

        EditorGUILayout.BeginHorizontal(GUILayout.Width(currentFrameLength * 8f + 100), GUILayout.Height(20));
        GUILayout.Space(100);
        GUILayout.Box("", style, GUILayout.Width(currentFrameLength * 8f), GUILayout.Height(20));
        GUILayout.Space( - currentFrameLength * 8f);
        if(GUILayout.Button("", pointer, GUILayout.Height(20)))
        {

        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical();
        if (tracks.Count > 0)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(currentFrameLength * 8f + 100), GUILayout.Height(20));
                if (GUILayout.Button(tracks[i].name, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    currentSelectedTrack = i;
                }
                GUILayout.Box("", style, GUILayout.Width(currentFrameLength * 8f), GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();


        EditorGUILayout.EndScrollView();*/
        EditorGUILayout.EndVertical();
    }

    public void DrawTimelineMap()
    {
        GUIStyle style = new GUIStyle("LODBlackBox");
        GUIStyle style1 = new GUIStyle("IN EditColliderButton");
        Rect rect = new Rect(sideBarWidth + 100, 45, timelineLength, 20);
        GUI.Box(rect, "", style);

        if (tracks.Count > 0)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                Rect rectBG = new Rect(sideBarWidth + 100, 70 + 20 * i, timelineLength, 20);
                GUI.Box(rectBG, "", style);
                Rect rectButton = new Rect(sideBarWidth + 5, 70 + 20 * i, 90, 20 );
                if(GUI.Button(rectButton, tracks[i].name))
                {
                    currentSelectedTrack = i;
                }
                Rect rectTrack = new Rect(sideBarWidth + 100 + tracks[i].start * 10, 70 + 20 * i, (tracks[i].end - tracks[i].start + 1) * 10f, 18);
                GUI.Box(rectTrack, "", style1);
            }
        }
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
            tracks.Add(new PengTrack(PengTrack.ExecTime.Update, "Track", 3, 20, this));
        }

        //enumpop

        //stateName

        //stateLoop

        //stateLength

        //save
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

    }    

    public void DrawNodeGraph()
    {
        DrawGrid(20 * nodeMapScale, 0.2f, Color.gray);
        DrawGrid(100 * nodeMapScale, 0.4f, Color.gray);
        DrawNodes();
        DrawConnectionLines();
    }

    private void ProcessEvents(Event e)
    {
        if (tracks[currentSelectedTrack].nodes.Count > 0 && tracks.Count > 0 && currentSelectedTrack < tracks.Count)
        {
            for (int i = tracks[currentSelectedTrack].nodes.Count - 1; i >= 0; i--)
            {
                bool dragged = tracks[currentSelectedTrack].nodes[i].ProcessEvents(e);
                if (dragged)
                {
                    GUI.changed = true;
                }
            }

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1)
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
            }
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
        tracks[currentSelectedTrack].nodes.Add(new PlayAnimation(mousePos, this, tracks[currentSelectedTrack], "Idle", true, 0, 0, 0));
    }

    private void DrawNodes()
    {
        if (tracks[currentSelectedTrack].nodes.Count > 0 && tracks.Count > 0 && currentSelectedTrack < tracks.Count)
        {
            for (int i = 0; i < tracks[currentSelectedTrack].nodes.Count; i++)
            {
                tracks[currentSelectedTrack].nodes[i].Draw();
            }
        }
    }

    private void DrawConnectionLines()
    {
        if (tracks[currentSelectedTrack].lines.Count > 0 && tracks.Count > 0 && currentSelectedTrack < tracks.Count)
        {
            for (int i = 0; i < tracks[currentSelectedTrack].lines.Count; i++)
            {
                tracks[currentSelectedTrack].lines[i].Draw();
            }
        }
    }

    private void DrawPendingConnectionLine(Event e)
    {
        if (selectingPoint != null)
        {
            Vector3 start = (selectingPoint.type == ConnectionPointType.In) ? selectingPoint.rect.center : e.mousePosition;
            Vector3 end = (selectingPoint.type == ConnectionPointType.In) ? e.mousePosition : selectingPoint.rect.center;
            
            if (selectingPoint.type == ConnectionPointType.In || selectingPoint.type == ConnectionPointType.Out) { Handles.DrawBezier(start, end, start + Vector3.left * 40f, end - Vector3.left * 40f, Color.white, null, 3f); }
            else if (selectingPoint.type == ConnectionPointType.FlowIn || selectingPoint.type == ConnectionPointType.FlowOut) { Handles.DrawBezier(start, end, start + Vector3.left * 40f, end - Vector3.left * 40f, Color.white, null, 6f); }

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
        tracks[currentSelectedTrack].nodes.Remove(node);
    }

    private void DragAllNodes(Vector2 change)
    {
        if (tracks[currentSelectedTrack].nodes.Count > 0 && tracks.Count > 0 && currentSelectedTrack < tracks.Count)
        {
            for (int i = 0; i < tracks[currentSelectedTrack].nodes.Count; i++)
            {
                tracks[currentSelectedTrack].nodes[i].ProcessDrag(change);
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
        currentFrameLength = 60;
        currentSelectedFrame = 40;
        currentSelectedTrack = 0;
        currentTrackLength = 0;
        timelineScrollPos = Vector2.zero;
        timelineLength = currentFrameLength * 10f;
    }

    public static XmlElement ConstructNewTrackXML(ref XmlDocument doc, PengTrack.ExecTime execTime, int start, int end)
    {
        XmlElement xml = doc.CreateElement("Track");
        xml.SetAttribute("Name", "Track");
        xml.SetAttribute("ExecTime", execTime.ToString());
        xml.SetAttribute("Start", start.ToString());
        xml.SetAttribute("End", end.ToString());

        return xml;
    }

    public static XmlElement ConstructNewScriptXML(ref XmlDocument doc, PengNode node)
    {
        XmlElement ele = doc.CreateElement(node.nodeName);


        return ele;
    }
}
