using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Timeline;
using static Cinemachine.CinemachineBlendDefinition;
using static UnityEditor.VersionControl.Asset;

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
    public Vector2 timelineScrollPos = Vector2.zero;
    private int m_currentFrameLength;
    public int currentFrameLength
    {
        get { return m_currentFrameLength; }
        set { m_currentFrameLength = value; timelineLength = 10f * value; }
    }
    public int currentTrackLength;
    public int currentSelectedFrame;
    int m_currentSelectedTrack;
    public int currentSelectedTrack
    { 
        get { return m_currentSelectedTrack; }
        set { m_currentSelectedTrack = value; OnCurrentSelectedTrackChanged(); }
    }
    public int currentDeleteTrack = 0;
    public float mouseXDelta = 0;
    public bool isDragging = false;
    public int dragObject = -1;
    public int dragTrackIndex = -1;
    public bool isHorizontalBarDragging = false;
    public bool isVerticalBarDragging = false;
    public float sideScrollOffset = 0;
    public bool isSideScrollBarDragging = false;
    public bool trackNameEditing = false;
    public bool currentStateLoop = false;
    public List<PengTrack> tracks = new List<PengTrack>();
    //状态组
    public Dictionary<string, List<string>> states = new Dictionary<string, List<string>>();
    //状态组是否折叠
    public Dictionary<string, bool> statesFold = new Dictionary<string, bool>();
    //所有状态及其对应的轨道组
    public Dictionary<string, List<PengTrack>> statesTrack = new Dictionary<string, List<PengTrack>>();
    //所有状态及其对应的长度
    public Dictionary<string, int> statesLength = new Dictionary<string, int>();
    //所有状态及其对应的是否循环
    public Dictionary<string, bool> statesLoop = new Dictionary<string, bool>();
    public string currentStateName;
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
        UpdateCurrentStateInfo();/*
        gridOffset = new Vector2(300f, 415f);
        DragAllNodes(new Vector2(300f, 415f));*/
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle("ObjectPickerPreviewBackground");
        GUIStyle style1 = new GUIStyle("flow background");
        timeLineRect = new Rect(0, 45f, position.width, timelineHeight);
        sideBarRect = new Rect(0, 0, sideBarWidth, position.height);
        nodeMapRect = new Rect(sideBarWidth, timelineHeight + 30, position.width - sideBarWidth, position.height - timelineHeight);

        if (currentStateName != "")
        {
            GUI.Box(nodeMapRect, "", style1);
            DrawNodeGraph();
            ProcessEvents(Event.current);
            DrawPendingConnectionLine(Event.current);
            GUI.Box(new Rect(0, 0, position.width, timelineHeight), "", style);
            DrawTimelineMap();
        }
        //绘制Timeline Title
        GUI.Box(new Rect(0, 0, sideBarWidth, position.height), "", style);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Height(position.height), GUILayout.Width(sideBarWidth));
        PengEditorMain.DrawPengFrameworkIcon("角色状态编辑器");
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUILayout.Height(timelineHeight), GUILayout.Width(position.width));
        if (currentStateName != "")
            DrawTimeLine();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        DrawSideBar(new Rect(3, 150, sideBarWidth - 6, position.height - 153));

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
        
        EditorGUILayout.EndVertical();
    }

    public void DrawTimelineMap()
    {
        GUIStyle style = new GUIStyle("LODBlackBox");
        GUIStyle style1 = new GUIStyle("IN EditColliderButton");
        GUIStyle style2 = new GUIStyle("Tooltip");
        GUIStyle style3 = new GUIStyle("LargeBoldLabel");
        style3.alignment = TextAnchor.UpperLeft;
        style3.normal.textColor = new Color(0.94f, 0.4f, 0.26f);
        style3.fontSize = 10;
        GUIStyle style4 = new GUIStyle("LargeBoldLabel");
        style4.alignment = TextAnchor.UpperLeft;
        style4.normal.textColor = Color.white;
        style4.fontSize = 12;
        GUIStyle style5 = new GUIStyle("LargeBoldLabel");
        style5.alignment = TextAnchor.UpperLeft;
        style5.normal.textColor = Color.gray;
        style5.fontSize = 6;
        GUIStyle style6 = new GUIStyle("LargeBoldLabel");
        style6.alignment = TextAnchor.UpperLeft;
        style6.normal.textColor = Color.white;
        style6.fontSize = 20;
        GUIStyle style7 = new GUIStyle("ObjectPickerPreviewBackground");
        GUIStyle style8 = new GUIStyle("SelectionRect");
        GUIStyle style9 = new GUIStyle("LargeBoldLabel");
        style9.alignment = TextAnchor.UpperLeft;
        style9.normal.textColor = new Color(0.94f, 0.4f, 0.26f);
        style9.fontSize = 12;

        if (tracks.Count > 0)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                Rect rectBG = new Rect(sideBarWidth + 100 - timelineScrollPos.x, 70 + 27 * i - timelineScrollPos.y, timelineLength, 14);
                Rect rectButton = new Rect(sideBarWidth + 5 - timelineScrollPos.x, 68 + 27 * i - timelineScrollPos.y, 90, 18);
                Rect rectTrack = new Rect(sideBarWidth + 100 + tracks[i].start * 10 - timelineScrollPos.x, 70 + 27 * i - timelineScrollPos.y, (tracks[i].end - tracks[i].start + 1) * 10f, 12);
                Rect rectLeft = new Rect(sideBarWidth + 97 + tracks[i].start * 10 - timelineScrollPos.x, 69 + 27 * i - timelineScrollPos.y, 6, 16);
                Rect rectRight = new Rect(sideBarWidth + 97 + tracks[i].start * 10 + (tracks[i].end - tracks[i].start + 1) * 10f - timelineScrollPos.x, 69 + 27 * i - timelineScrollPos.y, 6, 16);
                
                Rect rectTrackCursor = new Rect(rectTrack.x + 3, rectTrack.y, rectTrack.width - 6, rectTrack.height);
                Rect rectLeftCursor = new Rect(rectLeft.x - 3, rectLeft.y, rectLeft.width + 6, rectLeft.height);
                Rect rectRightCursor = new Rect(rectRight.x - 3, rectRight.y, rectRight.width + 6, rectRight.height);
                Rect rectLeftFrame = new Rect(rectLeft.x, rectLeft.y + rectLeft.height - 2, 80, 80);
                Rect rectRightFrame = new Rect(rectRight.x, rectRight.y + rectRight.height - 2, 80, 80);

                if (timelineHeight >= rectButton.y + rectButton.height)
                {
                    GUI.Box(rectBG, "", style);
                    if (GUI.Button(rectButton, tracks[i].name))
                    {
                        currentSelectedTrack = i;
                    }
                    GUI.Box(rectTrack, "", style1);
                    GUI.Box(rectLeft, "", style2);
                    GUI.Box(rectRight, "", style2);

                    EditorGUIUtility.AddCursorRect(rectTrackCursor, MouseCursor.Link);
                    EditorGUIUtility.AddCursorRect(rectLeftCursor, MouseCursor.SlideArrow);
                    EditorGUIUtility.AddCursorRect(rectRightCursor, MouseCursor.SlideArrow);

                    if(timelineHeight + 65 >= rectLeftFrame.y + rectLeftFrame.height)
                    { 
                        GUI.Box(rectLeftFrame, tracks[i].start.ToString(), style3);
                        GUI.Box(rectRightFrame, tracks[i].end.ToString(), style3);
                    }

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rectBG.Contains(Event.current.mousePosition))
                    {
                        currentDeleteTrack = i;
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("删除轨道"), false, () => { DeleteTrack(); });
                        menu.ShowAsContext();
                        Event.current.Use();
                    }

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                        (rectTrackCursor.Contains(Event.current.mousePosition) ||
                        rectLeftCursor.Contains(Event.current.mousePosition) ||
                        rectRightCursor.Contains(Event.current.mousePosition)))
                    {
                        if (rectTrackCursor.Contains(Event.current.mousePosition))
                        {
                            dragObject = 1;
                        }
                        else if (rectLeftCursor.Contains(Event.current.mousePosition))
                        {
                            dragObject = 0;
                        }
                        else if (rectRightCursor.Contains(Event.current.mousePosition))
                        {
                            dragObject = 2;
                        }
                        dragTrackIndex = i;
                        isDragging = true;
                        mouseXDelta = 0;
                        GUI.changed = true;
                        Event.current.Use();
                    }

                    if (Event.current.type == EventType.MouseUp)
                    {
                        isDragging = false;
                    }

                    if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                    {
                        if(tracks[dragTrackIndex].start > currentFrameLength)
                        {
                            tracks[dragTrackIndex].start = currentFrameLength - 1;
                        }
                        if (tracks[dragTrackIndex].end > currentFrameLength)
                        {
                            tracks[dragTrackIndex].end = currentFrameLength - 1;
                        }
                        switch (dragObject)
                        {
                            case 0:
                                if (!isDragging)
                                {
                                    break;
                                }
                                mouseXDelta += Event.current.delta.x;
                                while (Mathf.Abs(mouseXDelta) >= 10)
                                {
                                    if (mouseXDelta >= 10)
                                    {
                                        tracks[dragTrackIndex].start++;
                                        mouseXDelta -= 10;
                                        if (tracks[dragTrackIndex].start > tracks[dragTrackIndex].end)
                                        {
                                            tracks[dragTrackIndex].start--;
                                        }
                                    }
                                    else if (mouseXDelta <= -10)
                                    {
                                        tracks[dragTrackIndex].start--;
                                        mouseXDelta += 10;
                                        if (tracks[dragTrackIndex].start < 0)
                                        {
                                            tracks[dragTrackIndex].start++;
                                        }
                                    }
                                }
                                Event.current.Use();
                                break;
                            case 1:
                                if (!isDragging)
                                {
                                    break;
                                }
                                mouseXDelta += Event.current.delta.x;
                                while (Mathf.Abs(mouseXDelta) >= 10)
                                {
                                    if (mouseXDelta >= 10)
                                    {
                                        tracks[dragTrackIndex].start++;
                                        tracks[dragTrackIndex].end++;
                                        mouseXDelta -= 10;
                                        if (tracks[dragTrackIndex].end >= currentFrameLength)
                                        {
                                            tracks[dragTrackIndex].start--;
                                            tracks[dragTrackIndex].end--;
                                        }
                                    }
                                    else if (mouseXDelta <= -10)
                                    {
                                        tracks[dragTrackIndex].start--;
                                        tracks[dragTrackIndex].end--;
                                        mouseXDelta += 10;
                                        if (tracks[dragTrackIndex].start < 0)
                                        {
                                            tracks[dragTrackIndex].start++;
                                            tracks[dragTrackIndex].end++;
                                        }
                                    }
                                }
                                Event.current.Use();
                                break;
                            case 2:
                                if (!isDragging)
                                {
                                    break;
                                }
                                mouseXDelta += Event.current.delta.x;
                                while (Mathf.Abs(mouseXDelta) >= 10)
                                {
                                    if (mouseXDelta >= 10)
                                    {
                                        tracks[dragTrackIndex].end++;
                                        mouseXDelta -= 10;
                                        if (tracks[dragTrackIndex].end >= currentFrameLength)
                                        {
                                            tracks[dragTrackIndex].end--;
                                        }
                                    }
                                    else if (mouseXDelta <= -10)
                                    {
                                        tracks[dragTrackIndex].end--;
                                        mouseXDelta += 10;
                                        if (tracks[dragTrackIndex].end < tracks[dragTrackIndex].start)
                                        {
                                            tracks[dragTrackIndex].end++;
                                        }
                                    }
                                }
                                Event.current.Use();
                                break;
                            default:
                                isDragging = false;
                                break;
                        }
                        GUI.changed = true;
                    }
                }
            }
        }

        GUI.Box(new Rect(0, 0, position.width, 68), "", style7);


        Rect rect = new Rect(sideBarWidth + 100 - timelineScrollPos.x, 42, timelineLength, 20);
        GUI.Box(rect, "", style);

        if (currentFrameLength == 0)
        {
            return;
        }
        for (int i = 0; i < currentFrameLength; i++)
        {
            Rect pointer = new Rect(rect.x + i * 10f - 3, rect.y + rect.height - 6, 10, 6);
            Rect index = new Rect(rect.x + i * 10, rect.y, 10, rect.height);
            if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && index.Contains(Event.current.mousePosition))
            {
                currentSelectedFrame = i;
                Event.current.Use();
            }
            if (i == currentFrameLength - 1)
            {
                Rect rectFrameNum = new Rect(rect.x + i * 10f, rect.y + 3, 80, 80);
                GUI.Box(rectFrameNum, i.ToString(), style4);
                GUI.Box(pointer, "|", style6);
            }
            else if (i % 5 == 0)
            {
                Rect rectFrameNum = new Rect(rect.x + (i / 5) * 50f - 1, rect.y + 3, 80, 80);
                GUI.Box(rectFrameNum, i.ToString(), style4);
                GUI.Box(pointer, "|", style6);
            }
            else
            {
                GUI.Box(pointer, "|", style5);
            }
        }

        Rect currentFrame = new Rect(rect.x + currentSelectedFrame * 10, rect.y, 10, 285);
        Rect currentFrameIndex = new Rect(rect.x + currentSelectedFrame * 10 - 1, rect.y + 3, 40, 40);
        GUI.Box(currentFrame, "", style8);
        GUI.Box(currentFrameIndex, currentSelectedFrame.ToString(), style9);
    }

    public void DeleteTrack()
    {
        if(currentDeleteTrack == tracks.Count - 1)
        {
            if(tracks.Count != 1 && currentSelectedTrack == tracks.Count - 1)
            {
                currentSelectedTrack--;
            }
            tracks.RemoveAt(tracks.Count - 1);
        }
        else
        {
            tracks.RemoveAt(currentDeleteTrack);
        }
        GUI.changed = true;
    }

    public void DrawSideBar(Rect rectangle)
    {
        GUIStyle style = new GUIStyle("grey_border");
        GUIStyle style1 = new GUIStyle("dockarea");
        style1.alignment = TextAnchor.MiddleLeft;
        style1.fontStyle = FontStyle.Bold;
        GUIStyle style2 = new GUIStyle("AssetLabel");
        style2.alignment = TextAnchor.MiddleLeft;
        style2.fontStyle = FontStyle.Bold;
        GUIStyle style3 = new GUIStyle("AssetLabel Partial");
        style3.alignment = TextAnchor.MiddleLeft;
        style3.fontStyle = FontStyle.Normal;
        GUIStyle style4 = new GUIStyle("ShurikenMinus");
        style4.alignment = TextAnchor.UpperLeft;
        GUIStyle style5 = new GUIStyle("ShurikenPlus");
        GUIStyle style6 = new GUIStyle("AssetLabel Partial");
        style6.alignment = TextAnchor.MiddleLeft;
        style6.fontStyle = FontStyle.Bold;
        style6.normal.textColor = new Color(0.94f, 0.4f, 0.26f);
        style6.fontSize = 13;
        GUIStyle style7 = new GUIStyle("Button");
        style7.alignment = TextAnchor.MiddleCenter;
        style7.fontStyle = FontStyle.Bold;
        style7.fontSize = 10;

        Rect header = new Rect(rectangle.x + 1, rectangle.y + 1, rectangle.width - 2, 30);

        if (states.Count > 0)
        {
            bool deleteStateGroup = false;
            int deleteStateGroupAt = 0;
            bool deleteState = false;
            int deleteStateAt = 0;
            int row = 0;

            int showRow = 0;
            for (int i = 0; i < states.Count; i++)
            {
                showRow++;
                if(states.ElementAt(i).Value.Count > 0 && !statesFold[states.ElementAt(i).Key])
                {
                    showRow += states.ElementAt(i).Value.Count;
                }
            }

            bool hasScroll = false;
            if(showRow * 20 + 6 >= rectangle.height - 36)
            {
                GUIStyle styleBG = new GUIStyle("LODBlackBox");
                GUIStyle styleHandle = new GUIStyle("Button");

                hasScroll = true;
                float ratio = (rectangle.height - 30) / (showRow * 20 + 6);
                float offsetRatio = sideScrollOffset / (showRow * 20 + 6);
                Rect scrollBG = new Rect(rectangle.x + rectangle.width - 20, rectangle.y + 33, 20, rectangle.height - 36);
                Rect scrollHandle = new Rect(scrollBG.x + 2, scrollBG.y + 3 + offsetRatio * (rectangle.height - 36), scrollBG.width - 4, (scrollBG.height - 6) * ratio);

                GUI.Box(scrollBG, "", styleBG);
                GUI.Box(scrollHandle, "", styleHandle);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && scrollHandle.Contains(Event.current.mousePosition))
                {
                    isSideScrollBarDragging = true;
                    GUI.changed = true;
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseUp || !sideBarRect.Contains(Event.current.mousePosition))
                {
                    isSideScrollBarDragging = false;
                }

                if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                {
                    if (isSideScrollBarDragging)
                    {
                        sideScrollOffset += (Event.current.delta.y / (rectangle.height - 36)) * (showRow * 20 + 6);
                        Event.current.Use();
                        GUI.changed = true;
                    }
                }
                if (sideScrollOffset <= 0)
                {
                    sideScrollOffset = 0;
                }
                else if (sideScrollOffset >= (showRow * 20 + 6) - (rectangle.height - 36))
                {
                    sideScrollOffset = (showRow * 20 + 6) - (rectangle.height - 36);
                }
            }
            else
            {
                sideScrollOffset = 0;
            }


            for (int i = 0; i < states.Count; i++)
            {
                Rect entry = new Rect(header.x + 5, header.y + header.height + 3 + 20 * row - sideScrollOffset, header.width - 10 - (hasScroll ? 20 : 0), 20);
                Rect entryFold = new Rect(entry.x, entry.y, entry.width - 30, 20);
                Rect entryAdd = new Rect(entryFold.x + entryFold.width, entryFold.y + 5, 15, 15);
                Rect entryDelete = new Rect(entryFold.x + entryFold.width + 15, entryFold.y + 5, 15, 15);

                if(entry.y + entry.height > header.y + header.height)
                {
                    GUI.Box(entry, states.ElementAt(i).Key, style2);
                    GUI.Box(entryAdd, "", style5);
                    GUI.Box(entryDelete, "", style4);
                    if(Event.current.type == EventType.MouseDown &&  Event.current.button == 0 && entryFold.Contains(Event.current.mousePosition))
                    {
                        statesFold[states.ElementAt(i).Key] = !statesFold[states.ElementAt(i).Key];
                        GUI.changed = true;
                    }
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && entryAdd.Contains(Event.current.mousePosition))
                    {
                        //创建新状态
                        string stateName = "NewState";
                        bool sameName = true;
                        int index = 1;
                        while (sameName)
                        {
                            sameName = false;
                            if(statesTrack.Count > 0)
                            {
                                for(int k = 0; k < statesTrack.Count; k ++)
                                {
                                    if(stateName == statesTrack.ElementAt(k).Key)
                                    {
                                        sameName = true;
                                        stateName = "NewState" + index.ToString();
                                        index++;
                                    }
                                }
                            }
                        }
                        states.ElementAt(i).Value.Add(stateName);
                        statesFold[states.ElementAt(i).Key] = false;

                        List<PengTrack> tracks1 = new List<PengTrack>();
                        tracks1.Add(new PengTrack(PengTrack.ExecTime.Update, "Track", 3, 20, this));
                        statesTrack.Add(stateName, tracks1);

                        statesLength.Add(stateName, 55);
                        statesLoop.Add(stateName, false);
                        GUI.changed = true;
                    }
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && entryDelete.Contains(Event.current.mousePosition))
                    {
                        deleteStateGroupAt = i;
                        deleteStateGroup = EditorUtility.DisplayDialog("删除状态分组", "是否要删除状态分组" + states.ElementAt(deleteStateGroupAt).Key + "?", "删除", "取消");
                    }
                }

                if (states.ElementAt(i).Value.Count > 0 && ! statesFold[states.ElementAt(i).Key])
                {
                    for (int j = 0; j < states.ElementAt(i).Value.Count; j++)
                    {
                        row++;
                        Rect sonEntry = new Rect(header.x + 35, header.y + header.height + 3 + 20 * row - sideScrollOffset, header.width - 40 - (hasScroll ? 20 : 0), 20);
                        Rect sonEntrySelect = new Rect(sonEntry.x, sonEntry.y, sonEntry.width - 15, 20);
                        Rect sonEntryDelete = new Rect(sonEntry.x + sonEntry.width - 15, sonEntry.y + 5, 15, 15);
                        Rect sonEntryLength = new Rect(sonEntry.x + sonEntry.width - 50, sonEntry.y, 30, 15);
                        Rect sonEntryLoop = new Rect(sonEntry.x + sonEntry.width - 75, sonEntry.y, 25, 15);

                        if (sonEntry.y + sonEntry.height > header.y + header.height)
                        {
                            if (currentStateName != states.ElementAt(i).Value[j])
                                GUI.Box(sonEntry, states.ElementAt(i).Value[j], style3);
                            else
                                GUI.Box(sonEntry, states.ElementAt(i).Value[j], style6);
                            GUI.Box(sonEntryDelete, "", style4);

                            GUIStyle styleLoop = new GUIStyle("MiniLabel");
                            styleLoop.normal.textColor = new Color(0.36f, 0.95f, 0.72f, 0.7f);
                            styleLoop.fontSize = 9;
                            styleLoop.alignment = TextAnchor.MiddleRight;

                            if (statesLoop[states.ElementAt(i).Value[j]])
                            {
                                GUI.Box(sonEntryLoop, "Loop", styleLoop);
                            }
                            GUI.Box(sonEntryLength, statesLength[states.ElementAt(i).Value[j]].ToString() + "F", styleLoop);
                            //更改当前选择的状态
                            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && sonEntrySelect.Contains(Event.current.mousePosition))
                            {
                                if (currentStateName != states.ElementAt(i).Value[j])
                                {
                                    currentStateName = states.ElementAt(i).Value[j];
                                    tracks = statesTrack[currentStateName];
                                    currentSelectedTrack = 0;
                                    currentFrameLength = statesLength[currentStateName];
                                    currentStateLoop = statesLoop[currentStateName];
                                    GUI.changed = true;
                                }
                            }
                            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && sonEntryDelete.Contains(Event.current.mousePosition))
                            {
                                deleteStateGroupAt = i;
                                deleteStateAt = j;
                                deleteState = EditorUtility.DisplayDialog("删除状态", "是否要删除状态" + states.ElementAt(deleteStateGroupAt).Value[deleteStateAt] + "?", "删除", "取消");
                            }
                        }
                    }
                }
                row++;
            }
            if(deleteStateGroup)
            {
                string key = states.ElementAt(deleteStateGroupAt).Key;
                states.Remove(key);
                statesFold.Remove(key);
                GUI.changed = true;
            }
            if(deleteState)
            {
                string deleteStateName = states.ElementAt(deleteStateGroupAt).Value[deleteStateAt];
                if (deleteStateName == currentStateName)
                {
                    currentStateName = "";
                }
                states.ElementAt(deleteStateGroupAt).Value.Remove(deleteStateName);
                statesTrack.Remove(deleteStateName);
                statesLength.Remove(deleteStateName);
                statesLoop.Remove(deleteStateName);
                GUI.changed = true;
            }
        }

        Rect border = new Rect(rectangle.x - 1, rectangle.y - 1, rectangle.width + 2, rectangle.height + 2);
        Rect headerTitle = new Rect(header.x + 7, header.y + 6, 150, 20);
        Rect addState = new Rect(header.x + header.width - 25, header.y + 5, 20, 20);
        Rect unfoldAll = new Rect(header.x + header.width - 115, header.y + 5, 40, 20);
        Rect foldAll = new Rect(header.x + header.width - 70, header.y + 5, 40, 20);

        GUI.Box(border, "", style);
        GUI.Box(header, "", style1);
        GUI.Box(headerTitle, "角色状态列表", style1);
        if (GUI.Button(addState, "+"))
        {
            List<string> stateGroup = new List<string>();
            string key = "NewStateGroup";
            int index = 1;
            while (states.ContainsKey(key))
            {
                key = "NewStateGroup" + index.ToString();
                index++;
            }
            states.Add(key, stateGroup);
            statesFold.Add(key, true);

            GUI.changed = true;
        }

        if (GUI.Button(unfoldAll, "全展开", style7))
        {
            if (statesFold.Count > 0)
            {
                for (int i = 0; i < statesFold.Count; i++)
                {
                    statesFold[statesFold.ElementAt(i).Key] = false;
                }
            }
            GUI.changed = true;
        }

        if (GUI.Button(foldAll, "全折叠", style7))
        {
            if (statesFold.Count > 0)
            {
                for (int i = 0; i < statesFold.Count; i++)
                {
                    statesFold[statesFold.ElementAt(i).Key] = true;
                }
            }
            GUI.changed = true;
        }
    }

    public void DrawTimeLineTitle()
    {
        GUIStyle style = new GUIStyle("LODBlackBox");
        GUIStyle style7 = new GUIStyle("grey_border");
        GUIStyle style8 = new GUIStyle("Button");

        Rect border1 = new Rect(sideBarWidth + 4, 65, position.width - sideBarWidth - 8, 281);
        Rect border2 = new Rect(sideBarWidth + 1, 40, position.width - sideBarWidth - 1, 310);
        
        if(currentFrameLength * 10f + 150 >= position.width - sideBarWidth)
        {
            border1.height -= 16;
            Rect scrollHorizontal = new Rect(sideBarWidth+2, border1.y + border1.height+2, position.width - sideBarWidth - 4, 15);

            float ratio = (position.width - sideBarWidth) / (currentFrameLength * 10f + 150);
            Rect scrollHorizontalHandle = new Rect((timelineScrollPos.x / (currentFrameLength * 10f + 150)) * (scrollHorizontal.width - 2) + sideBarWidth + 4, scrollHorizontal.y + 2, (scrollHorizontal.width - 2) * ratio, 11);

            EditorGUIUtility.AddCursorRect(scrollHorizontalHandle, MouseCursor.Link);

            GUI.Box(scrollHorizontal, "", style);
            GUI.Box(scrollHorizontalHandle, "", style8);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && scrollHorizontalHandle.Contains(Event.current.mousePosition))
            {
                isHorizontalBarDragging = true;
                GUI.changed = true;
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseUp || nodeMapRect.Contains(Event.current.mousePosition))
            {
                isHorizontalBarDragging = false;
            }

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                if (isHorizontalBarDragging)
                {
                    timelineScrollPos.x += (Event.current.delta.x / (position.width - sideBarWidth)) * (currentFrameLength * 10f + 150);
                    Event.current.Use();
                    GUI.changed = true;
                }
            }
            if (timelineScrollPos.x <= 0)
            {
                timelineScrollPos.x = 0;
            }
            else if (timelineScrollPos.x >= (currentFrameLength * 10f + 150) - (position.width - sideBarWidth))
            {
                timelineScrollPos.x = (currentFrameLength * 10f + 150) - (position.width - sideBarWidth);
            }
        }
        else
        {
            timelineScrollPos.x = 0;
        }

        if(27 * tracks.Count + 20 >= timelineHeight - border1.y - 20)
        {
            //border1.height -= 16;
            Rect scrollVertical = new Rect(position.width - 19, border1.y + 3, 15, timelineHeight - border1.y - 20);

            float ratio = (timelineHeight - border1.y - 20) / (27 * tracks.Count + 20);
            Rect scrollVerticalHandle = new Rect(scrollVertical.x + 2, (timelineScrollPos.y / (27 * tracks.Count + 20)) * (scrollVertical.height - 2) + border1.y + 5,  11, (scrollVertical.height - 2) * ratio);

            EditorGUIUtility.AddCursorRect(scrollVerticalHandle, MouseCursor.Link);

            GUI.Box(scrollVertical, "", style);
            GUI.Box(scrollVerticalHandle, "", style8);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && scrollVerticalHandle.Contains(Event.current.mousePosition))
            {
                isVerticalBarDragging = true;
                GUI.changed = true;
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseUp || nodeMapRect.Contains(Event.current.mousePosition))
            {
                isVerticalBarDragging = false;
            }

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                if (isVerticalBarDragging)
                {
                    timelineScrollPos.y += (Event.current.delta.y / (timelineHeight - border1.y - 20)) * (27 * tracks.Count + 20);
                    Event.current.Use();
                    GUI.changed = true;
                }
            }
            if (timelineScrollPos.y <= 0)
            {
                timelineScrollPos.y = 0;
            }
            else if (timelineScrollPos.y >= (27 * tracks.Count + 20) - (timelineHeight - border1.y - 20))
            {
                timelineScrollPos.y = (27 * tracks.Count + 20) - (timelineHeight - border1.y - 20);
            }
        }
        else
        {
            timelineScrollPos.y = 0;
        }

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(40));

        EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(20));
        GUILayout.Label("PengActor ID：");
        GUILayout.Space(10);
        GUILayout.Label("100425");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(20));

        if(currentStateName!= "")
        {
            if (GUILayout.Button("创建轨道", GUILayout.Width(100)))
            {
                int index = (tracks.Count + 1);
                string trackName = "Track" + index.ToString();
                if (tracks.Count > 0)
                {
                    for (int i = 0; i < tracks.Count; i++)
                    {
                        if (trackName == tracks[i].name)
                        {
                            index++;
                            trackName = "Track" + index.ToString();
                        }
                    }
                }
                statesTrack[currentStateName].Add(new PengTrack(PengTrack.ExecTime.Update, "Track", 3, 20, this));
            }

            GUILayout.Space(10);

            GUILayout.Label("状态名称：", GUILayout.Width(65));

            GUILayout.Space(5);

            string stateName = currentStateName;
            stateName = EditorGUILayout.TextField(stateName, GUILayout.Width(100));
            if (stateName != currentStateName)
            {
                if(states.Count > 0)
                {
                    int index1 = 0;
                    int index2 = 0;
                    for (int i = 0;i < states.Count;i++)
                    {
                        if (states.ElementAt(i).Value.Count  > 0)
                        {
                            for(int j = 0; j < states.ElementAt(i).Value.Count;j++)
                            {
                                if (states.ElementAt(i).Value[j] == currentStateName)
                                {
                                    index1 = i;
                                    index2 = j;
                                    break;
                                }
                            }
                        }
                    }
                    states.ElementAt(index1).Value[index2] = stateName;
                }

                if (statesTrack.Count > 0)
                {
                    for (int i = 0; i < statesTrack.Count; i++)
                    {
                        if(statesTrack.ElementAt(i).Key == currentStateName)
                        {
                            statesTrack.Add(stateName, statesTrack.ElementAt(i).Value);
                            statesTrack.Remove(currentStateName);
                            break;
                        }
                    }
                }

                if (statesLength.Count > 0)
                {
                    for (int i = 0; i < statesLength.Count; i++)
                    {
                        if (statesLength.ElementAt(i).Key == currentStateName)
                        {
                            statesLength.Add(stateName, statesLength.ElementAt(i).Value);
                            statesLength.Remove(currentStateName);
                            break;
                        }
                    }
                }

                if (statesLoop.Count > 0)
                {
                    for (int i = 0; i < statesLoop.Count; i++)
                    {
                        if (statesLoop.ElementAt(i).Key == currentStateName)
                        {
                            statesLoop.Add(stateName, statesLoop.ElementAt(i).Value);
                            statesLoop.Remove(currentStateName);
                            break;
                        }
                    }
                }
                currentStateName = stateName;
            }
            GUILayout.Space(10);
            GUILayout.Label("是否循环：", GUILayout.Width(65));
            GUILayout.Space(5);
            statesLoop[currentStateName] = EditorGUILayout.Toggle(statesLoop[currentStateName], GUILayout.Width(25));

            GUILayout.Space(10);
            GUILayout.Label("状态长度：", GUILayout.Width(65));
            GUILayout.Space(5);

            statesLength[currentStateName] = EditorGUILayout.IntField(statesLength[currentStateName], GUILayout.Width(65));
            currentFrameLength = statesLength[currentStateName];

        }

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
        if (tracks.Count > 0 && currentSelectedTrack < tracks.Count)
        {
            if (tracks[currentSelectedTrack].nodes.Count > 0)
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
                }
            }
        }
    }

    private void RightMouseMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("添加节点/按功能类型/角色表现/播放动画"), false, () => { ProcessAddNode(mousePos, PengScript.PengScriptType.PlayAnimation); });
        menu.AddItem(new GUIContent("添加节点/按名称字母/B/播放动画"), false, () => { ProcessAddNode(mousePos, PengScript.PengScriptType.PlayAnimation); });
        menu.ShowAsContext();
    }

    private void ProcessAddNode(Vector2 mousePos, PengScript.PengScriptType type)
    {
        switch (type)
        {
            case PengScript.PengScriptType.PlayAnimation:
                tracks[currentSelectedTrack].nodes.Add(new PlayAnimation(mousePos, this, tracks[currentSelectedTrack], "Idle", true, 0, 0, 0));
                break;
        }
    }

    private void DrawNodes()
    {
        if (tracks.Count > 0 && currentSelectedTrack < tracks.Count)
        {
            if (tracks[currentSelectedTrack].nodes.Count > 0) 
            {
                for (int i = 0; i < tracks[currentSelectedTrack].nodes.Count; i++)
                {
                    tracks[currentSelectedTrack].nodes[i].Draw();
                } 
            }

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

            Rect box = new Rect(sideBarWidth, timelineHeight, position.width - sideBarWidth, 30);
            Rect label1 = new Rect(box.x + 8, box.y + 3, 80, 20);
            Rect text1 = new Rect(box.x + 88, box.y + 3, 120, 20);

            GUI.Box(box, "", style);
            GUI.Box(label1, "轨道名称：", style);

            if (trackNameEditing)
            {
                tracks[currentSelectedTrack].name = GUI.TextField(text1, tracks[currentSelectedTrack].name, style1);
            }
            else
            {
                GUI.Box(text1, tracks[currentSelectedTrack].name, style2);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if(text1.Contains(Event.current.mousePosition))
                {
                    trackNameEditing = true;
                    GUI.changed = true;
                }
                else
                {
                    trackNameEditing = false;
                    GUI.changed = true;
                }

            }
        }
    }

    private void DrawConnectionLines()
    {
        if (tracks.Count > 0 && currentSelectedTrack < tracks.Count)
        {
            if (tracks[currentSelectedTrack].lines.Count > 0)
            {
                for (int i = 0; i < tracks[currentSelectedTrack].lines.Count; i++)
                {
                    tracks[currentSelectedTrack].lines[i].Draw();
                }
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
        if (tracks.Count > 0 && currentSelectedTrack < tracks.Count)
        {
            if(tracks[currentSelectedTrack].nodes.Count > 0) 
            {
                for (int i = 0; i < tracks[currentSelectedTrack].nodes.Count; i++)
                {
                    tracks[currentSelectedTrack].nodes[i].ProcessDrag(change);
                }
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
        List<string> stateGroup1 = new List<string>();
        List<string> stateGroup2 = new List<string>();
        stateGroup1.Add("StopL");
        stateGroup1.Add("StopR");
        stateGroup1.Add("Idle");
        stateGroup1.Add("Intro");
        stateGroup2.Add("Move");
        stateGroup2.Add("Dash");
        stateGroup2.Add("Boost");
        states.Add("Idle", stateGroup1);
        states.Add("Move", stateGroup2);
        statesFold.Add("Idle", true);
        statesFold.Add("Move", true);
        if (states.Count > 0)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states.ElementAt(i).Value.Count > 0)
                {
                    for (int j = 0; j < states.ElementAt(i).Value.Count; j++)
                    {
                        List<PengTrack> tracks1 = new List<PengTrack>();
                        tracks1.Add(new PengTrack(PengTrack.ExecTime.Update, "Track", 3, 20, this));
                        statesTrack.Add(states.ElementAt(i).Value[j], tracks1);

                        statesLength.Add(states.ElementAt(i).Value[j], 55);
                        statesLoop.Add(states.ElementAt(i).Value[j], false);
                    }
                }
            }
        }

        currentFrameLength = 60;
        currentSelectedFrame = 0;
        currentSelectedTrack = 0;
        currentTrackLength = 0;
        timelineScrollPos = Vector2.zero;
        timelineLength = currentFrameLength * 10f;
        currentStateLoop = false;
        currentStateName = "";
    }

    public void OnCurrentSelectedTrackChanged()
    {
        
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
