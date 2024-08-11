using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.Experimental.GraphView;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using System.Threading.Tasks;
using UnityEditor.Animations;
using UnityEditorInternal.VersionControl;

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
    //private float nodeMapScale = 1f;
    public Rect timeLineRect;
    public Rect sideBarRect;
    public Rect nodeMapRect;

    float sideBarWidth = 250f;
    float timelineHeight = 350f;
    float timelineLength = 10f;

    public float globalFrameRate;


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
    public int currentActorID = 100425;
    public int currentActorCamp = 1;
    public string currentActorCampString = "1";
    public string currentActorName = "";
    public float currentScale = 1;
    public bool debug = false;
    public bool editorPlaying = false;
    private bool m_runTimeEdit = false;
    public bool runTimeEdit
    {
        get { return m_runTimeEdit; }
        set { m_runTimeEdit = value;
            if (!m_runTimeEdit) { PauseEditingActor(); }
        }
    }
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
    public PengActor runTimeSelectionPauseActor = null;

    public XmlElement copyInfo = null;
    XmlDocument copyInternalDoc = null;
    //


    [MenuItem("PengFramework/角色状态编辑器")]
    static void Init()
    {
        PengActorStateEditorWindow window = (PengActorStateEditorWindow)EditorWindow.GetWindow(typeof(PengActorStateEditorWindow));
        window.position = new Rect(100, 100, 1200, 700);
        window.titleContent = new GUIContent("彭框架角色状态编辑器");
    }

    private void OnEnable()
    {
        editorPlaying = EditorApplication.isPlaying;
    }

    private void OnGUI()
    {
        if(!LoadGlobalConfiguration())
        {
            return;
        }

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
            ReadActorData(Selection.activeGameObject.GetComponent<PengActor>().actorID);
        }

        if (EditorApplication.isPlaying)
        {
            if (!editorPlaying)
            {
                editorPlaying = true;
                Selection.activeGameObject = null;
                runTimeEdit = false;
            }

            if (runTimeEdit)
            {
                if (Selection.activeTransform != null && Selection.activeTransform.GetComponent<PengActor>() != null)
                {
                    if (runTimeSelectionPauseActor == null)
                    {
                        runTimeSelectionPauseActor = Selection.activeTransform.GetComponent<PengActor>();
                    }
                    else
                    {
                        if (runTimeSelectionPauseActor != Selection.activeTransform.GetComponent<PengActor>())
                        {
                            runTimeSelectionPauseActor = Selection.activeTransform.GetComponent<PengActor>();
                        }
                    }
                }

                if (runTimeSelectionPauseActor != null)
                {
                    Vector3 pos = runTimeSelectionPauseActor.transform.position;
                    runTimeSelectionPauseActor.pauseTime = 100f;
                    runTimeSelectionPauseActor.ctrl.enabled = false;

                    //非常傻逼的采样方法，有没有人能救救
                    runTimeSelectionPauseActor.anim.Play(currentStateName, 0, 0);
                    runTimeSelectionPauseActor.anim.Update(0);
                    runTimeSelectionPauseActor.anim.Update(((float)currentSelectedFrame / globalFrameRate));

                    runTimeSelectionPauseActor.transform.position = pos;
                }
            }
            else
            {
                if(runTimeSelectionPauseActor != null)
                {
                    runTimeSelectionPauseActor.TransState("Idle", true);
                }
                runTimeSelectionPauseActor = null;
            }
        }
        else
        {
            editorPlaying = false;
            runTimeSelectionPauseActor = null;
            if (Selection.activeTransform != null && Selection.activeTransform.GetComponent<PengActor>() != null)
            {
                Vector3 pos = Selection.activeTransform.position;
                AnimationClip[] clips = Selection.activeTransform.GetComponent<Animator>().runtimeAnimatorController.animationClips;
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i].name == Selection.activeTransform.GetComponent<PengActor>().actorID.ToString() + "@" + currentStateName)
                    {
                        clips[i].SampleAnimation(Selection.activeTransform.gameObject, (float)currentSelectedFrame / globalFrameRate);
                        break;
                    }
                }
                Selection.activeTransform.position = pos;
            }
        }

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
        DrawActorInfo(new Rect(3, 50, sideBarWidth - 6, 99));

        GUIStyle styleSave = new GUIStyle("Button");
        styleSave.alignment = TextAnchor.MiddleCenter;
        styleSave.fontStyle = FontStyle.Bold;
        styleSave.fontSize = 13;
        styleSave.normal.textColor = new Color(0.4f, 0.95f, 0.6f);
        Rect save = new Rect(sideBarWidth - 45, 5, 40, 40);
        if (GUI.Button(save, "保存\n数据", styleSave))
        {
            Save(true);
        }
        EditorGUIUtility.AddCursorRect(save, MouseCursor.Link);

        if (GUI.changed)
        {
            Repaint();
        }
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
        GUIStyle style10 = new GUIStyle("Button");
        style10.normal.textColor = new Color(0.94f, 0.4f, 0.26f);
        style10.fontStyle = FontStyle.Bold;

        Rect rect = new Rect(sideBarWidth + 100 - timelineScrollPos.x, 42, timelineLength, 20);
        Rect onEnter = new Rect();
        int enterIndex = -1;
        Rect onExit = new Rect();
        int exitIndex = -1;

        if (tracks.Count > 0)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].execTime == PengTrack.ExecTime.Update)
                {
                    Rect rectBG = new Rect(sideBarWidth + 100 - timelineScrollPos.x, 73 + 27 * (i - 2) - timelineScrollPos.y, timelineLength, 14);
                    Rect rectButton = new Rect(sideBarWidth + 5 - timelineScrollPos.x, 71 + 27 * (i - 2) - timelineScrollPos.y, 90, 18);
                    Rect rectTrack = new Rect(sideBarWidth + 100 + tracks[i].start * 10 - timelineScrollPos.x, 73 + 27 * (i - 2) - timelineScrollPos.y, (tracks[i].end - tracks[i].start + 1) * 10f, 12);
                    Rect rectLeft = new Rect(sideBarWidth + 97 + tracks[i].start * 10 - timelineScrollPos.x, 72 + 27 * (i - 2) - timelineScrollPos.y, 6, 16);
                    Rect rectRight = new Rect(sideBarWidth + 97 + tracks[i].start * 10 + (tracks[i].end - tracks[i].start + 1) * 10f - timelineScrollPos.x, 72 + 27 * (i - 2) - timelineScrollPos.y, 6, 16);

                    Rect rectTrackCursor = new Rect(rectTrack.x + 3, rectTrack.y, rectTrack.width - 6, rectTrack.height);
                    Rect rectLeftCursor = new Rect(rectLeft.x - 3, rectLeft.y, rectLeft.width + 6, rectLeft.height);
                    Rect rectRightCursor = new Rect(rectRight.x - 3, rectRight.y, rectRight.width + 6, rectRight.height);
                    Rect rectLeftFrame = new Rect(rectLeft.x, rectLeft.y + rectLeft.height - 2, 80, 80);
                    Rect rectRightFrame = new Rect(rectRight.x, rectRight.y + rectRight.height - 2, 80, 80);

                    if (i == currentSelectedTrack && currentSelectedTrack >= 2)
                    {
                        Rect currentSelected = new Rect(rectBG.x - 100, rectBG.y - 4, rectBG.width + 103, rectBG.height + 14);
                        GUI.Box(currentSelected, "", style8);
                    }

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

                        if (timelineHeight + 65 >= rectLeftFrame.y + rectLeftFrame.height)
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
                            if (tracks[dragTrackIndex].start > currentFrameLength)
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
                if (tracks[i].execTime == PengTrack.ExecTime.Enter || tracks[i].execTime == PengTrack.ExecTime.Exit)
                {
                    if (tracks[i].execTime == PengTrack.ExecTime.Exit)
                    {
                        onExit = new Rect(rect.x + rect.width + 5, rect.y + 3, 90, 18);
                        exitIndex = i;
                    }
                    else
                    {
                        onEnter = new Rect(rect.x - 95, rect.y + 3, 90, 18);
                        enterIndex = i;
                    }
                }
            }
        }

        GUI.Box(new Rect(0, 0, position.width, 68), "", style7);

        if (enterIndex >= 0 && exitIndex >= 0)
        {
            if (GUI.Button(onEnter, tracks[enterIndex].name, style10))
            {
                currentSelectedTrack = enterIndex;
            }
            if (GUI.Button(onExit, tracks[exitIndex].name, style10))
            {
                currentSelectedTrack = exitIndex;
            }
            GUI.changed = true;
        }

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
        if (currentDeleteTrack == tracks.Count - 1)
        {
            if (currentSelectedTrack == tracks.Count - 1)
            {
                currentSelectedTrack--;
            }
            if (dragTrackIndex == tracks.Count - 1)
            {
                dragTrackIndex--;
            }
            tracks.RemoveAt(tracks.Count - 1);
        }
        else
        {
            if (currentDeleteTrack < currentSelectedTrack)
            {
                currentSelectedTrack--;
            }
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
                if (states.ElementAt(i).Value.Count > 0 && !statesFold[states.ElementAt(i).Key])
                {
                    showRow += states.ElementAt(i).Value.Count;
                }
            }

            bool hasScroll = false;
            if (showRow * 20 + 6 >= rectangle.height - 36)
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

                EditorGUIUtility.AddCursorRect(entry, MouseCursor.Link);

                if (entry.y + entry.height > header.y + header.height)
                {
                    GUI.Box(entry, states.ElementAt(i).Key, style2);
                    GUI.Box(entryAdd, "", style5);
                    GUI.Box(entryDelete, "", style4);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && entryFold.Contains(Event.current.mousePosition))
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
                            if (statesTrack.Count > 0)
                            {
                                for (int k = 0; k < statesTrack.Count; k++)
                                {
                                    if (stateName == statesTrack.ElementAt(k).Key)
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
                        tracks1.Add(new PengTrack(PengTrack.ExecTime.Enter, "OnEnter", 0, 0, this, true));
                        tracks1.Add(new PengTrack(PengTrack.ExecTime.Exit, "OnExit", 0, 0, this, true));
                        tracks1.Add(new PengTrack(PengTrack.ExecTime.Update, "Track", 3, 20, this, true));
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

                if (states.ElementAt(i).Value.Count > 0 && !statesFold[states.ElementAt(i).Key])
                {
                    for (int j = 0; j < states.ElementAt(i).Value.Count; j++)
                    {
                        row++;
                        Rect sonEntry = new Rect(header.x + 35, header.y + header.height + 3 + 20 * row - sideScrollOffset, header.width - 40 - (hasScroll ? 20 : 0), 20);
                        Rect sonEntrySelect = new Rect(sonEntry.x, sonEntry.y, sonEntry.width - 15, 20);
                        Rect sonEntryDelete = new Rect(sonEntry.x + sonEntry.width - 15, sonEntry.y + 5, 15, 15);
                        Rect sonEntryLength = new Rect(sonEntry.x + sonEntry.width - 50, sonEntry.y, 30, 15);
                        Rect sonEntryLoop = new Rect(sonEntry.x + sonEntry.width - 75, sonEntry.y, 25, 15);

                        EditorGUIUtility.AddCursorRect(sonEntry, MouseCursor.Link);

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
                                    currentSelectedTrack = 2;
                                    currentFrameLength = statesLength[currentStateName];
                                    currentStateLoop = statesLoop[currentStateName];
                                    if (dragTrackIndex >= tracks.Count)
                                    {
                                        dragTrackIndex = tracks.Count - 1;
                                    }
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
            if (deleteStateGroup)
            {
                string key = states.ElementAt(deleteStateGroupAt).Key;
                states.Remove(key);
                statesFold.Remove(key);
                GUI.changed = true;
            }
            if (deleteState)
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

        EditorGUIUtility.AddCursorRect(addState, MouseCursor.Link);
        EditorGUIUtility.AddCursorRect(unfoldAll, MouseCursor.Link);
        EditorGUIUtility.AddCursorRect(foldAll, MouseCursor.Link);
    }

    public void DrawActorInfo(Rect rectangle)
    {
        GUIStyle style = new GUIStyle("grey_border");
        GUIStyle style1 = new GUIStyle("dockarea");
        style1.alignment = TextAnchor.MiddleLeft;
        style1.fontStyle = FontStyle.Bold;
        GUIStyle style2 = new GUIStyle("AM HeaderStyle");
        style2.alignment = TextAnchor.MiddleLeft;
        style2.fontStyle = FontStyle.Bold;

        Rect border = new Rect(rectangle.x - 1, rectangle.y - 1, rectangle.width + 2, rectangle.height);
        Rect header = new Rect(rectangle.x + 1, rectangle.y + 1, rectangle.width - 2, 30);
        Rect headerTitle = new Rect(header.x + 7, header.y + 6, 150, 20);

        Rect actorNameLabel = new Rect(headerTitle.x, header.y + header.height + 2, sideBarWidth * 0.25f, 20);
        Rect actorName = new Rect(headerTitle.x + sideBarWidth * 0.25f, header.y + header.height + 2, sideBarWidth * 0.3f, 20);

        Rect actorCampLabel = new Rect(headerTitle.x + sideBarWidth * 0.5f, header.y + header.height + 2, sideBarWidth * 0.25f, 20);
        Rect actorCamp = new Rect(headerTitle.x + sideBarWidth * 0.75f, header.y + header.height + 2, sideBarWidth * 0.3f, 20);

        GUI.Box(border, "", style);
        GUI.Box(header, "", style1);
        GUI.Box(headerTitle, "角色基本信息", style1);

        if(EditorApplication.isPlaying)
        {
            Rect runTimeEditRect = new Rect(headerTitle.x + 100, headerTitle.y, 100, headerTitle.height);
            runTimeEdit = GUI.Toggle(runTimeEditRect, runTimeEdit, "运行时编辑");
        }

        GUI.Box(actorNameLabel, "角色ID：", style2);
        GUI.Box(actorName, currentActorID.ToString(), style2);

        GUI.Box(actorCampLabel, "角色阵营：", style2);
        currentActorCampString = GUI.TextField(actorCamp, currentActorCamp.ToString(), style2);

        if (int.TryParse(currentActorCampString, out currentActorCamp))
        {

        }
        else
        {
            currentActorCampString = currentActorCamp.ToString();
            EditorUtility.DisplayDialog("警告", "阵营必须以整型数字来标识！", "ok");
        }

    }

    public void PauseEditingActor()
    {
        foreach (PengActor actor in GameObject.FindWithTag("PengGameManager").GetComponent<PengGameManager>().actors)
        {
            actor.pauseTime = 0f;
            actor.anim.speed = 1f;
            actor.ctrl.enabled = true;
        }
    }

    public void DrawTimeLineTitle()
    {
        GUIStyle style = new GUIStyle("LODBlackBox");
        GUIStyle style7 = new GUIStyle("grey_border");
        GUIStyle style8 = new GUIStyle("Button");

        Rect border1 = new Rect(sideBarWidth + 4, 65, position.width - sideBarWidth - 8, 281);
        Rect border2 = new Rect(sideBarWidth + 1, 40, position.width - sideBarWidth - 1, 310);

        if (currentFrameLength * 10f + 200 >= position.width - sideBarWidth)
        {
            border1.height -= 16;
            Rect scrollHorizontal = new Rect(sideBarWidth + 2, border1.y + border1.height + 2, position.width - sideBarWidth - 4, 15);

            float ratio = (position.width - sideBarWidth) / (currentFrameLength * 10f + 200);
            Rect scrollHorizontalHandle = new Rect((timelineScrollPos.x / (currentFrameLength * 10f + 200)) * (scrollHorizontal.width - 2) + sideBarWidth + 4, scrollHorizontal.y + 2, (scrollHorizontal.width - 2) * ratio, 11);

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
                    timelineScrollPos.x += (Event.current.delta.x / (position.width - sideBarWidth)) * (currentFrameLength * 10f + 200);
                    Event.current.Use();
                    GUI.changed = true;
                }
            }
            if (timelineScrollPos.x <= 0)
            {
                timelineScrollPos.x = 0;
            }
            else if (timelineScrollPos.x >= (currentFrameLength * 10f + 200) - (position.width - sideBarWidth))
            {
                timelineScrollPos.x = (currentFrameLength * 10f + 200) - (position.width - sideBarWidth);
            }
        }
        else
        {
            timelineScrollPos.x = 0;
        }

        if (27 * (tracks.Count - 2) + 20 >= timelineHeight - border1.y - 20)
        {
            //border1.height -= 16;
            Rect scrollVertical = new Rect(position.width - 19, border1.y + 3, 15, timelineHeight - border1.y - 20);

            float ratio = (timelineHeight - border1.y - 20) / (27 * (tracks.Count - 2) + 20);
            Rect scrollVerticalHandle = new Rect(scrollVertical.x + 2, (timelineScrollPos.y / (27 * (tracks.Count - 2) + 20)) * (scrollVertical.height - 2) + border1.y + 5, 11, (scrollVertical.height - 2) * ratio);

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
                    timelineScrollPos.y += (Event.current.delta.y / (timelineHeight - border1.y - 20)) * (27 * (tracks.Count - 2) + 20);
                    Event.current.Use();
                    GUI.changed = true;
                }
            }
            if (timelineScrollPos.y <= 0)
            {
                timelineScrollPos.y = 0;
            }
            else if (timelineScrollPos.y >= (27 * (tracks.Count - 2) + 20) - (timelineHeight - border1.y - 20))
            {
                timelineScrollPos.y = (27 * (tracks.Count - 2) + 20) - (timelineHeight - border1.y - 20);
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
        GUILayout.Label(currentActorID.ToString());
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width - sideBarWidth), GUILayout.Height(20));

        if (currentStateName != "" && currentStateName != null)
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
                statesTrack[currentStateName].Add(new PengTrack(PengTrack.ExecTime.Update, "Track", 3, 20, this, true));
            }

            GUILayout.Space(10);

            if (copyInfo != null)
            {
                if (GUILayout.Button("粘贴轨道", GUILayout.Width(100)))
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
                    PengTrack track = GenerateTrack(copyInfo);
                    track.master = this;
                    statesTrack[currentStateName].Add(track);
                    Save(false);
                }
                GUILayout.Space(10);
            }

            GUILayout.Label("状态名称：", GUILayout.Width(65));

            GUILayout.Space(5);

            if (EditorApplication.isPlaying)
            {
                GUILayout.Label(currentStateName, GUILayout.Width(100));
            }
            else
            {
                string stateName = currentStateName;
                stateName = EditorGUILayout.TextField(stateName, GUILayout.Width(100));
                if (stateName != currentStateName)
                {
                    if (states.Count > 0)
                    {
                        int index1 = 0;
                        int index2 = 0;
                        for (int i = 0; i < states.Count; i++)
                        {
                            if (states.ElementAt(i).Value.Count > 0)
                            {
                                for (int j = 0; j < states.ElementAt(i).Value.Count; j++)
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
                            if (statesTrack.ElementAt(i).Key == currentStateName)
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
            }
            GUILayout.Space(10);
            GUILayout.Label("是否循环：", GUILayout.Width(65));
            GUILayout.Space(5);
            try
            {
                statesLoop[currentStateName] = EditorGUILayout.Toggle(statesLoop[currentStateName], GUILayout.Width(25));
            }
            catch
            {
                if(Selection.activeGameObject != null)
                {
                    if (Selection.activeGameObject.GetComponent<PengActor>() != null)
                    {
                        ReadActorData(Selection.activeGameObject.GetComponent<PengActor>().actorID);
                    }
                }
            }
            GUILayout.Space(10);
            GUILayout.Label("状态长度：", GUILayout.Width(65));
            GUILayout.Space(5);

            try
            {
                statesLength[currentStateName] = EditorGUILayout.IntField(statesLength[currentStateName], GUILayout.Width(65));
                currentFrameLength = statesLength[currentStateName];
            }
            catch
            {

            }

        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

    }

    public void DrawNodeGraph()
    {
        DrawGrid(20 * currentScale, 0.2f, Color.gray);
        DrawGrid(100 * currentScale, 0.4f, Color.gray);
        DrawNodes();
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
    }

    private void RightMouseMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        for (int i = 0; i < Enum.GetValues(typeof(PengScript.PengScriptType)).Length; i++)
        {
            RightMouseMenuDetail(ref menu, (PengScript.PengScriptType)Enum.GetValues(typeof(PengScript.PengScriptType)).GetValue(i), mousePos);
        }

        menu.ShowAsContext();
    }

    public void RightMouseMenuDetail(ref GenericMenu menu, PengScript.PengScriptType scriptType, Vector2 mousePos)
    {
        if(PengNode.GetCodedDown(scriptType) && scriptType != PengScript.PengScriptType.OnTrackExecute)
        {
            menu.AddItem(new GUIContent("添加节点/按类型分类/" + PengNode.GetCatalogByFunction(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(mousePos, scriptType); });
            menu.AddItem(new GUIContent("添加节点/按首字母分类/" + PengNode.GetCatalogByName(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(mousePos, scriptType); });
            menu.AddItem(new GUIContent("添加节点/按封装程度分类/" + PengNode.GetCatalogByPackage(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(mousePos, scriptType); });
        }
    }

    private void ProcessAddNode(Vector2 mousePos, PengScript.PengScriptType type)
    {
        int id = 1;
        bool idSame = true;
        PengTrack track = tracks[currentSelectedTrack];
        while (idSame)
        {
            idSame = false;
            for (int i = 0; i < tracks[currentSelectedTrack].nodes.Count; i++)
            {
                if (id == tracks[currentSelectedTrack].nodes[i].nodeID)
                {
                    idSame = true;
                    id++;
                }
            }
        }
        switch (type)
        {
            case PengScript.PengScriptType.PlayAnimation:
                track.nodes.Add(new PlayAnimation(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(5)), ""));
                break;
            case PengScript.PengScriptType.IfElse:
                track.nodes.Add(new IfElse(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.ValuePengInt:
                track.nodes.Add(new ValuePengInt(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.ValuePengFloat:
                track.nodes.Add(new ValuePengFloat(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.ValuePengString:
                track.nodes.Add(new ValuePengString(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.ValuePengBool:
                track.nodes.Add(new ValuePengBool(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.GetTargetsByRange:
                track.nodes.Add(new GetTargetsByRange(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(4)), ""));
                break;
            case PengScript.PengScriptType.ForIterator:
                track.nodes.Add(new ForIterator(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(2)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(2)), ""));
                break;
            case PengScript.PengScriptType.ValuePengVector3:
                track.nodes.Add(new ValuePengVector3(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(3)), ""));
                break;
            case PengScript.PengScriptType.DebugLog:
                track.nodes.Add(new DebugLog(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.ValueFloatToString:
                track.nodes.Add(new ValueFloatToString(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
        }
        tracks[currentSelectedTrack] = track;
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

            DrawConnectionLines();

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

            if (tracks[currentSelectedTrack].execTime != PengTrack.ExecTime.Enter && tracks[currentSelectedTrack].execTime != PengTrack.ExecTime.Exit)
            {
                Rect label2 = new Rect(box.x + 218, box.y + 3, 50, 20);
                Rect start = new Rect(label2.x + 60, box.y + 3, 50, 20);
                Rect label3 = new Rect(box.x + 318, box.y + 3, 50, 20);
                Rect end = new Rect(label3.x + 60, box.y + 3, 50, 20);
                Rect label4 = new Rect(box.x + 418, box.y + 3, 50, 20);
                Rect period = new Rect(label4.x + 60, box.y + 3, 50, 20);

                GUI.Box(label2, "开始帧：", style);
                GUI.Box(start, tracks[currentSelectedTrack].start.ToString(), style);
                GUI.Box(label3, "结束帧：", style);
                GUI.Box(end, tracks[currentSelectedTrack].end.ToString(), style);
                GUI.Box(label4, "持续帧：", style);
                GUI.Box(period, (tracks[currentSelectedTrack].end - tracks[currentSelectedTrack].start + 1).ToString(), style);
            }

            if (trackNameEditing && tracks[currentSelectedTrack].execTime != PengTrack.ExecTime.Enter && tracks[currentSelectedTrack].execTime != PengTrack.ExecTime.Exit)
            {
                tracks[currentSelectedTrack].name = GUI.TextField(text1, tracks[currentSelectedTrack].name, style1);
            }
            else
            {
                GUI.Box(text1, tracks[currentSelectedTrack].name, style2);
            }

            if (tracks[currentSelectedTrack].execTime != PengTrack.ExecTime.Enter && tracks[currentSelectedTrack].execTime != PengTrack.ExecTime.Exit)
            {
                Rect copy = new Rect(box.x + 520, box.y + 3, 80, 20);
                if (GUI.Button(copy, "复制轨道"))
                {
                    copyInternalDoc = new XmlDocument();
                    XmlDeclaration decl = copyInternalDoc.CreateXmlDeclaration("1.0", "UTF-8", "");
                    copyInfo = GenerateTrackInfo(ref copyInternalDoc, tracks[currentSelectedTrack]);
                }
            }

            Rect recovery = new Rect(box.x + 620, box.y + 3, 80, 20);
            if (GUI.Button(recovery, "回正"))
            {
                if (tracks[currentSelectedTrack].nodes.Count > 0)
                {
                    Vector2 currentPos = tracks[currentSelectedTrack].nodes[0].pos;
                    Vector2 targetPos = new Vector2(300f, 415f) - currentPos;
                    DragAllNodes(targetPos);
                }
            }

            Rect debugRect = new Rect(box.x + 720, box.y + 3, 80, 20);
            debug = GUI.Toggle(debugRect, debug, "Debug");

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (text1.Contains(Event.current.mousePosition))
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
            if (tracks[currentSelectedTrack].nodes.Count > 0)
            {
                for (int i = 0; i < tracks[currentSelectedTrack].nodes.Count; i++)
                {
                    tracks[currentSelectedTrack].nodes[i].DrawLines();
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
        if (tracks[currentSelectedTrack].nodes.Count > 1)
        {
            for (int i = 0; i < tracks[currentSelectedTrack].nodes.Count; i++)
            {
                if (tracks[currentSelectedTrack].nodes[i].outID.Count > 0)
                {
                    for (int j = 0; j < tracks[currentSelectedTrack].nodes[i].outID.Count; j++)
                    {
                        if (tracks[currentSelectedTrack].nodes[i].outID.ElementAt(j).Value == node.nodeID)
                        {
                            tracks[currentSelectedTrack].nodes[i].outID[j] = -1;
                        }
                    }
                }

                if (tracks[currentSelectedTrack].nodes[i].varInID.Count > 0)
                {
                    for (int j = 0; j < tracks[currentSelectedTrack].nodes[i].varInID.Count; j++)
                    {
                        if (tracks[currentSelectedTrack].nodes[i].varInID.ElementAt(j).Value.nodeID == node.nodeID)
                        {
                            tracks[currentSelectedTrack].nodes[i].varInID[j] = PengNode.DefaultNodeIDConnectionID();
                        }
                    }
                }
            }
        }
        tracks[currentSelectedTrack].nodes.Remove(node);
    }

    private void DragAllNodes(Vector2 change)
    {
        if (tracks.Count > 0 && currentSelectedTrack < tracks.Count)
        {
            if (tracks[currentSelectedTrack].nodes.Count > 0)
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

    public void UpdateCurrentStateInfo()
    {
        ReadActorData(100425);
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

    public void Save(bool showMsg)
    {
        SaveActorData(currentActorID, currentActorCamp, currentActorName, states, statesTrack, statesLength, statesLoop, showMsg);
    }

    public static void SaveActorData(int actorID, int actorCamp, string actorName, Dictionary<string, List<string>> stateGroup, Dictionary<string, List<PengTrack>> stateTrack, Dictionary<string, int> statesLength, Dictionary<string, bool> statesLoop, bool showMsg)
    {
        //Actor数据结构：
        //<Actor>
        //  <ActorInfo>
        //      <ActorID ActorID = "..." />
        //      <Camp Camp = "..." />
        //  </ActorInfo>
        //  <ActorState>
        //      <StateGroup Name = "...">
        //          <State Name = "..." IsLoop = "..." Length = "...">
        //              <Track Name = "..." Start = "..." End = "..." ExecTime = "...">
        //                  <Script Name = "..." ScriptType = "..." NodeType = "..." ... />
        //                  <Script Name = "..." ScriptType = "..." NodeType = "..." ... />
        //                  ...
        //              </Track>
        //              ...
        //          </State>
        //          <State Name = "..." IsLoop = "..." Length = "...">
        //              <Track Name = "..." Start = "..." End = "..." ExecTime = "...">
        //                  <Script Name = "..." ScriptType = "..." NodeType = "..." ... />
        //                  <Script Name = "..." ScriptType = "..." NodeType = "..." ... />
        //                  ...
        //              </Track>
        //              ...
        //          </State>
        //          ...
        //      </StateGroup>
        //      ...
        //  </ActorState>
        //</Actor>
        XmlDocument doc = new XmlDocument();
        XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", "");
        //一二级结构
        XmlElement root = doc.CreateElement("Actor");
        XmlElement info = doc.CreateElement("ActorInfo");
        XmlElement states = doc.CreateElement("ActorState");

        //info下的三级结构
        XmlElement ID = doc.CreateElement("ActorID");
        ID.SetAttribute("ActorID", actorID.ToString());
        XmlElement camp = doc.CreateElement("Camp");
        camp.SetAttribute("Camp", actorCamp.ToString());
        XmlElement name = doc.CreateElement("ActorName");
        name.SetAttribute("ActorName", actorName.ToString());

        info.AppendChild(ID);
        info.AppendChild(camp);
        info.AppendChild(name);

        //states下的三级结构
        //死亡迭代
        if (stateGroup.Count > 0)
        {
            for (int i = 0; i < stateGroup.Count; i++)
            {
                XmlElement group = doc.CreateElement("StateGroup");
                group.SetAttribute("Name", stateGroup.ElementAt(i).Key);
                if (stateGroup.ElementAt(i).Value.Count > 0)
                {
                    for (int j = 0; j < stateGroup.ElementAt(i).Value.Count; j++)
                    {
                        XmlElement state = doc.CreateElement("State");
                        string stateName = stateGroup.ElementAt(i).Value[j];
                        state.SetAttribute("Name", stateName);
                        state.SetAttribute("IsLoop", statesLoop[stateName] ? "1" : "0");
                        state.SetAttribute("Length", statesLength[stateName].ToString());
                        if (stateTrack[stateName].Count > 0)
                        {
                            for (int k = 0; k < stateTrack[stateName].Count; k++)
                            {
                                PengTrack track = stateTrack[stateName][k];
                                state.AppendChild(GenerateTrackInfo(ref doc, track));
                            }
                        }
                        group.AppendChild(state);
                    }
                }
                states.AppendChild(group);
            }
        }

        root.AppendChild(info);
        root.AppendChild(states);
        doc.AppendChild(root);

        if (!Directory.Exists(Application.dataPath + "/Resources/ActorData/" + actorID.ToString()))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources/ActorData/" + actorID.ToString());
        }
        doc.Save(Application.dataPath + "/Resources/ActorData/" + actorID.ToString() + "/" + actorID.ToString() + ".xml");
        if (showMsg)
        {
            Debug.Log("保存成功，保存于" + Application.dataPath + "/Resources/ActorData/" + actorID.ToString() + "/" + actorID.ToString() + ".xml");
        }
        AssetDatabase.Refresh();

        if (EditorApplication.isPlaying)
        {
            foreach (PengActor actor in GameObject.FindWithTag("PengGameManager").GetComponent<PengGameManager>().actors)
            {
                if (actor.actorID == actorID)
                {
                    actor.actorStates = new Dictionary<string, IPengActorState>();
                    actor.LoadActorState();
                }
            }
        }
    }

    

    public void ReadActorData(int actorID)
    {
        string path = Application.dataPath + "/Resources/ActorData/" + actorID.ToString() + "/" + actorID.ToString() + ".xml";
        if (!File.Exists(path))
        {
            Debug.LogError("未读取到ID为" + actorID.ToString() + "的角色数据！读取地址：" + Application.dataPath + "/Resources/ActorData/" + actorID.ToString() + "/" + actorID.ToString() + ".xml");
            return;
        }
        TextAsset textAsset = (TextAsset)Resources.Load("ActorData/" + actorID.ToString() + "/" + actorID.ToString());
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

        XmlElement actorInfo = null;
        XmlElement actorState = null;
        foreach(XmlElement ele in childs)
        {
            if(ele.Name == "ActorInfo")
            {
                actorInfo = ele;
                continue;
            }
            if(ele.Name == "ActorState")
            {
                actorState = ele;
                continue;
            }
        }

        if (actorInfo == null || actorInfo.ChildNodes.Count == 0)
        {
            Debug.LogError(actorID.ToString() + "的角色数据里没有角色信息！怎么回事呢？");
            return;
        }

        if (actorState == null || actorState.ChildNodes.Count == 0)
        {
            Debug.LogError(actorID.ToString() + "的角色数据里没有角色状态！怎么回事呢？");
            return;
        }

        XmlNodeList infoChilds = actorInfo.ChildNodes;
        foreach (XmlElement ele in infoChilds)
        {
            if(ele.Name == "ActorID")
            {
                //读取ID
                currentActorID = int.Parse(ele.GetAttribute("ActorID"));
                continue;
            }
            if(ele.Name == "Camp")
            {
                currentActorCamp = int.Parse(ele.GetAttribute("Camp"));
            }
            if(ele.Name == "ActorName")
            {
                currentActorName = ele.GetAttribute("ActorName");
            }
        }

        XmlNodeList stateGroupChild = actorState.ChildNodes;
        states = new Dictionary<string, List<string>>();
        statesFold = new Dictionary<string, bool>();
        statesLength = new Dictionary<string, int>();
        statesLoop = new Dictionary<string, bool>();
        statesTrack = new Dictionary<string, List<PengTrack>>();
        foreach (XmlElement ele in stateGroupChild)
        {
            List<string> stateNames = new List<string>();
            foreach(XmlElement ele2 in ele.ChildNodes)
            {
                stateNames.Add(ele2.GetAttribute("Name"));
                statesLength.Add(ele2.GetAttribute("Name"), int.Parse(ele2.GetAttribute("Length")));
                statesLoop.Add(ele2.GetAttribute("Name"), int.Parse(ele2.GetAttribute("IsLoop")) > 0);
                List<PengTrack> pengTracks = new List<PengTrack>();
                foreach(XmlElement trackEle in ele2.ChildNodes)
                {
                    PengTrack track = GenerateTrack(trackEle);
                    track.master = this;
                    pengTracks.Add(track);
                }
                statesTrack.Add(ele2.GetAttribute("Name"), pengTracks);
            }
            states.Add(ele.GetAttribute("Name"), stateNames);
            statesFold.Add(ele.GetAttribute("Name"), false);
        }

        currentStateName = "Idle";
        currentFrameLength = statesLength[currentStateName];
        currentStateLoop = statesLoop[currentStateName];
        tracks = statesTrack[currentStateName];
        currentSelectedTrack = 2;
        currentSelectedFrame = 0;
        dragTrackIndex = 0;
        currentScale = 1;
        timelineScrollPos = Vector2.zero;

        if (tracks[currentSelectedTrack].nodes.Count > 0)
        {
            Vector2 currentPos = tracks[currentSelectedTrack].nodes[0].pos;
            Vector2 targetPos = new Vector2(300f, 415f) - currentPos;
            DragAllNodes(targetPos);
        }
    }



    public static PengNode ReadPengNode(XmlElement ele, ref PengTrack track)
    {
        PengScript.PengScriptType type;
        if (ele.GetAttribute("ScriptType") == "OnExecute")
        {
            type = PengScript.PengScriptType.OnTrackExecute;
        }
        else
        {
            type = (PengScript.PengScriptType)Enum.Parse(typeof(PengScript.PengScriptType), ele.GetAttribute("ScriptType"));
        }
        int ID = int.Parse(ele.GetAttribute("ScriptID"));
        string outID = ele.GetAttribute("OutID");
        string varOutID = ele.GetAttribute("VarOutID");
        string varInID = ele.GetAttribute("VarInID");
        string specialInfo = ele.GetAttribute("SpecialInfo");
        switch (type)
        {
            default:
                return null;
            case PengScript.PengScriptType.OnTrackExecute:
                return new OnTrackExecute(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.PlayAnimation:
                return new PlayAnimation(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.IfElse:
                return new IfElse(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengInt:
                return new ValuePengInt(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengFloat:
                return new ValuePengFloat(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengString:
                return new ValuePengString(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengBool:
                return new ValuePengBool(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.GetTargetsByRange:
                return new GetTargetsByRange(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ForIterator:
                return new ForIterator(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengVector3:
                return new ValuePengVector3(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValueFloatToString:
                return new ValueFloatToString(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.DebugLog:
                return new DebugLog(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
        }
    }

    public static PengNode GetPengNodeByID(int id, List<PengNode> nodes)
    {
        PengNode node = null;
        if (nodes.Count > 0)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].nodeID == id)
                {
                    node = nodes[i];
                    break;
                }
            }
        }
        return node;
    }
    
    public static PengVariables.PengVar GetPengVarByVarID(PengNode node, int id, ConnectionPointType type)
    {
        PengVariables.PengVar result = null;
        switch (type)
        {
            case ConnectionPointType.In:
                result = node.inVars[id];
                break;
            case ConnectionPointType.Out:
                result = node.outVars[id];
                break;
        }
        return result;
    }

    public static PengTrack GenerateTrack(XmlElement trackEle)
    {
        PengTrack track = new PengTrack((PengTrack.ExecTime)Enum.Parse(typeof(PengTrack.ExecTime), trackEle.GetAttribute("ExecTime")),
                        trackEle.GetAttribute("Name"), int.Parse(trackEle.GetAttribute("Start")), int.Parse(trackEle.GetAttribute("End")), null, false);
        foreach (XmlElement nodeEle in trackEle.ChildNodes)
        {
            PengNode node = ReadPengNode(nodeEle, ref track);
            track.nodes.Add(node);
        }
        return track;
    }

    public static XmlElement GenerateTrackInfo(ref XmlDocument doc, PengTrack pengTrack)
    {
        XmlElement track = doc.CreateElement("Track");
        track.SetAttribute("Name", pengTrack.name);
        track.SetAttribute("Start", pengTrack.start.ToString());
        track.SetAttribute("End", pengTrack.end.ToString());
        track.SetAttribute("ExecTime", pengTrack.execTime.ToString());
        if (pengTrack.nodes.Count > 0)
        {
            for (int l = 0; l < pengTrack.nodes.Count; l++)
            {
                XmlElement node = doc.CreateElement("Script");
                PengNode pengNode = pengTrack.nodes[l];
                node.SetAttribute("Name", pengNode.nodeName);
                node.SetAttribute("ScriptType", pengNode.scriptType.ToString());
                node.SetAttribute("NodeType", pengNode.type.ToString());
                node.SetAttribute("NodePos", PengNode.ParseVector2ToString(pengNode.pos));
                node.SetAttribute("ScriptID", pengNode.nodeID.ToString());
                node.SetAttribute("Position", PengNode.ParseVector2ToString(pengNode.pos));
                node.SetAttribute("ParaNum", pengNode.paraNum.ToString());
                node.SetAttribute("OutID", PengNode.ParseDictionaryIntIntToString(pengNode.outID));
                node.SetAttribute("VarOutID", PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(pengNode.varOutID));
                node.SetAttribute("VarInID", PengNode.ParseDictionaryIntNodeIDConnectionIDToString(pengNode.varInID));
                node.SetAttribute("SpecialInfo", pengNode.SpecialParaDescription());

                track.AppendChild(node);
            }
        }
        return track;
    }

    public void ClearAllInfo()
    {
        timelineScrollPos = Vector2.zero;
        currentTrackLength = 0;
        currentSelectedFrame = 0;
        currentSelectedTrack = 0;
        currentDeleteTrack = 0;
        mouseXDelta = 0;
        isDragging = false;
        dragObject = -1;
        dragTrackIndex = -1;
        isHorizontalBarDragging = false;
        isVerticalBarDragging = false;
        sideScrollOffset = 0;
        isSideScrollBarDragging = false;
        trackNameEditing = false;
        currentStateLoop = false;
        tracks = new List<PengTrack>();
        currentActorID = 100425;
        currentActorCamp = 1;
        currentActorCampString = "1";
        currentActorName = "";
        currentScale = 1;
        debug = false;
        editorPlaying = false;
        states = new Dictionary<string, List<string>>();
        statesFold = new Dictionary<string, bool>();
        statesTrack = new Dictionary<string, List<PengTrack>>();
        statesLength = new Dictionary<string, int>();
        statesLoop = new Dictionary<string, bool>();
        currentStateName = "";
        copyInfo = null;
        copyInternalDoc = null;
    }

    public bool LoadGlobalConfiguration()
    {
        if (File.Exists(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml"))
        {
            TextAsset textAsset = null;
            textAsset = (TextAsset)Resources.Load("GlobalConfiguration/GlobalSetting");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(textAsset.text);
            XmlElement frameSettingElement = (XmlElement)xml.SelectSingleNode("FrameSetting");
            globalFrameRate = float.Parse(frameSettingElement.GetAttribute("ActionFrameRate"));
            return true;
        }
        else
        {
            EditorGUILayout.HelpBox("没有全局配置，请使用启动器修复！", MessageType.Warning);
            return false;
        }
    }
}
