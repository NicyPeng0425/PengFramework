using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using System.IO;
using static PengScript.GetTargetsByRange;
using PengScript;

public partial class PengActorStateEditorWindow : EditorWindow
{
    /// <summary>
    /// 节点图部分主要参考：
    /// https://blog.csdn.net/qq_31967569/article/details/81025419
    /// https://blog.csdn.net/u013412391/article/details/120873714
    /// 感谢两位大佬！
    /// </summary>
    public List<PengNode> nodes = new List<PengNode>();
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
    public bool m_editGlobal = false;
    public bool editGlobal
    {
        get { return m_editGlobal; }
        set { m_editGlobal = value; OnCurrentSelectedTrackChanged(); }
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
    public PengActorState.StateType currentStateType = PengActorState.StateType.其他;
    public List<PengEditorTrack> tracks = new List<PengEditorTrack>();
    public int currentActorID = 100425;
    public int currentActorCamp = 1;
    public string currentActorName = "";
    public float currentScale = 1;
    public bool debug = false;
    public bool editorPlaying = false;
    private bool m_runTimeEdit = false;
    GameObject m_currentSelection = null;
    public GameObject currentSelectionGameObject
    {
        get { return m_currentSelection; }
        set { 
            if (m_currentSelection == null && value != null)
            {
                OnCurrentStateChanged();
                m_currentSelection = value;
            }
            if (value != m_currentSelection)
            {
                OnCurrentStateChanged();
                m_currentSelection = value;
            }
            if (value == null)
            {
                OnCurrentStateChanged();
                m_currentSelection = null;
            }
}
    }

    public float currentActorMaxHP = 1000f;
    public float currentActorAttackPower = 200f;
    public float currentActorDefendPower = 50f;
    public float currentActorCriticalRate = 0.2f;
    public float currentActorCriticalDamageRatio = 1.5f;
    public float currentActorResist = 200f;
    public PengActorAttributesEditor attrEditor = null;

    public bool editorStatePlaying = false;
    public float editorStatePreviewTimeCount = 0;
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
    public Dictionary<string, List<PengEditorTrack>> statesTrack = new Dictionary<string, List<PengEditorTrack>>();
    //所有状态及其对应的长度
    public Dictionary<string, int> statesLength = new Dictionary<string, int>();
    //所有状态及其对应的是否循环
    public Dictionary<string, bool> statesLoop = new Dictionary<string, bool>();
    //所有状态及其对应的状态类型
    public Dictionary<string, PengActorState.StateType> statesTypes = new Dictionary<string, PengActorState.StateType>();
    public PengEditorTrack globalTrack;
    public Dictionary<int, PlayAudio> previewAudios = new Dictionary<int, PlayAudio>();
    string m_currentStateName;    
    public string currentStateName
    {
        get{ return m_currentStateName; }
        set { OnCurrentStateChanged(); m_currentStateName = value; }
    }
    public PengActor runTimeSelectionPauseActor = null;

    public XmlElement copyInfo = null;
    XmlDocument copyInternalDoc = null;
    public XmlElement copyNodeInfo = null;
    XmlDocument copyNodeInternalDoc = null;
    public List<ParticleSystem> psList = new List<ParticleSystem>();
    //


    [MenuItem("PengFramework/角色状态编辑器", false, 1)]
    static void Init()
    {
        PengActorStateEditorWindow window = (PengActorStateEditorWindow)EditorWindow.GetWindow(typeof(PengActorStateEditorWindow));
        window.position = new Rect(100, 100, 1200, 700);
        window.titleContent = new GUIContent("彭框架角色状态编辑器");
    }

    private void OnEnable()
    {
        editorPlaying = EditorApplication.isPlaying;
        SceneView.duringSceneGui += this.OnSceneGUI;
        EditorApplication.playModeStateChanged += EnterPlayModeClearVFX;
        EditorApplication.update += UpdateState;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
        EditorApplication.playModeStateChanged -= EnterPlayModeClearVFX;
        EditorApplication.update -= UpdateState;
        if (psList.Count > 0)
        {
            for (int i = psList.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(psList[i].gameObject);
            }
            psList.Clear();
        }
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Temporary");
        if (temp.Length > 0)
        {
            for (int i = temp.Length - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(temp[i]);
            }
        }
    }

    private void OnGUI()
    {
        if(!LoadGlobalConfiguration())
        {
            return;
        }

        currentSelectionGameObject = Selection.activeGameObject;

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
            OnCurrentStateChanged();
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
        style.normal.textColor = new Color(0.8f, 0.9f, 0.7f);
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
                    Rect rectButton = new Rect(sideBarWidth + 5 - timelineScrollPos.x, 71 + 27 * (i - 2) - timelineScrollPos.y, 70, 18);
                    Rect rectButtonOtherInfo = new Rect(rectButton.x + rectButton.width + 2, rectButton.y, 18, 18);
                    Rect rectTrack = new Rect(sideBarWidth + 100 + tracks[i].start * 10 - timelineScrollPos.x, 73 + 27 * (i - 2) - timelineScrollPos.y, (tracks[i].end - tracks[i].start + 1) * 10f, 12);
                    Rect rectLeft = new Rect(sideBarWidth + 97 + tracks[i].start * 10 - timelineScrollPos.x, 72 + 27 * (i - 2) - timelineScrollPos.y, 6, 16);
                    Rect rectRight = new Rect(sideBarWidth + 97 + tracks[i].start * 10 + (tracks[i].end - tracks[i].start + 1) * 10f - timelineScrollPos.x, 72 + 27 * (i - 2) - timelineScrollPos.y, 6, 16);

                    Rect rectTrackCursor = new Rect(rectTrack.x + 3, rectTrack.y, rectTrack.width - 6, rectTrack.height);
                    Rect rectLeftCursor = new Rect(rectLeft.x - 3, rectLeft.y, rectLeft.width + 6, rectLeft.height);
                    Rect rectRightCursor = new Rect(rectRight.x - 3, rectRight.y, rectRight.width + 6, rectRight.height);
                    Rect rectLeftFrame = new Rect(rectLeft.x, rectLeft.y + rectLeft.height - 2, 80, 80);
                    Rect rectRightFrame = new Rect(rectRight.x, rectRight.y + rectRight.height - 2, 80, 80);

                    if (i == currentSelectedTrack && currentSelectedTrack >= 2 && !editGlobal)
                    {
                        Rect currentSelected = new Rect(rectBG.x - 100, rectBG.y - 4, rectBG.width + 103, rectBG.height + 14);
                        GUI.Box(currentSelected, "", style8);
                    }

                    if (timelineHeight >= rectButton.y + rectButton.height)
                    {
                        GUI.Box(rectBG, tracks[i].otherInfo, style);
                        if (GUI.Button(rectButton, tracks[i].trackName))
                        {
                            currentSelectedTrack = i;
                            editGlobal = false;
                        }
                        if (GUI.Button(rectButtonOtherInfo, "//"))
                        {
                            PengOtherInfoEditor editor = PengOtherInfoEditor.Init(this, i);
                            editor.position = new Rect(Event.current.mousePosition.x + 40, Event.current.mousePosition.y + 40, editor.position.width, editor.position.height);
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
                            if (dragTrackIndex > tracks.Count - 1)
                            {
                                dragTrackIndex = tracks.Count - 1;
                            }
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
            if (GUI.Button(onEnter, tracks[enterIndex].trackName, style10))
            {
                currentSelectedTrack = enterIndex;
                editGlobal = false;
            }
            if (GUI.Button(onExit, tracks[exitIndex].trackName, style10))
            {
                currentSelectedTrack = exitIndex;
                editGlobal = false;
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

        Rect global = new Rect(position.width - 120, timelineHeight - 60, 100, 40);
        if (globalTrack != null)
        {
            if (GUI.Button(global, "全局节点图", style10))
            {
                editGlobal = true;
            }
        }
        else
        {
            if (GUI.Button(global, "创建全局节点图", style10))
            {
                globalTrack = new PengEditorTrack(PengTrack.ExecTime.Global, "全局", 0, 0, this, true);
                editGlobal = true;
            }
        }
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

                        List<PengEditorTrack> tracks1 = new List<PengEditorTrack>();
                        tracks1.Add(new PengEditorTrack(PengTrack.ExecTime.Enter, "OnEnter", 0, 0, this, true));
                        tracks1.Add(new PengEditorTrack(PengTrack.ExecTime.Exit, "OnExit", 0, 0, this, true));
                        tracks1.Add(new PengEditorTrack(PengTrack.ExecTime.Update, "Track", 3, 20, this, true));
                        statesTrack.Add(stateName, tracks1);

                        statesLength.Add(stateName, 55);
                        statesLoop.Add(stateName, false);
                        statesTypes.Add(stateName, PengActorState.StateType.其他);
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
                                    editGlobal = false;
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
                statesTypes.Remove(deleteStateName); 
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
        currentActorCamp = EditorGUI.IntField(actorCamp, currentActorCamp, style2);

        Rect editBtn = new Rect(actorNameLabel.x, actorNameLabel.y + 25, sideBarWidth * 0.45f, 20);
        if (GUI.Button(editBtn, "编辑角色基础属性"))
        {
            attrEditor = PengActorAttributesEditor.Init(this);
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
                        if (trackName == tracks[i].trackName)
                        {
                            index++;
                            trackName = "Track" + index.ToString();
                        }
                    }
                }
                statesTrack[currentStateName].Add(new PengEditorTrack(PengTrack.ExecTime.Update, "Track", 3, 20, this, true));
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
                            if (trackName == tracks[i].trackName)
                            {
                                index++;
                                trackName = "Track" + index.ToString();
                            }
                        }
                    }
                    PengEditorTrack track = GenerateTrack(copyInfo);
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

                    if (statesTypes.Count > 0)
                    {
                        for (int i = 0; i < statesTypes.Count; i++)
                        {
                            if (statesTypes.ElementAt(i).Key == currentStateName)
                            {
                                statesTypes.Add(stateName, statesTypes.ElementAt(i).Value);
                                statesTypes.Remove(currentStateName);
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
            GUILayout.Space(15);

            try
            {
                statesLength[currentStateName] = EditorGUILayout.IntField(statesLength[currentStateName], GUILayout.Width(65));
                currentFrameLength = statesLength[currentStateName];
            }
            catch
            {

            }

            GUILayout.Space(10);
            GUILayout.Label("状态类型：", GUILayout.Width(65));
            GUILayout.Space(10);

            try
            {
                statesTypes[currentStateName] = (PengActorState.StateType)EditorGUILayout.EnumPopup(statesTypes[currentStateName], GUILayout.Width(65));
                currentStateType = statesTypes[currentStateName];
            }
            catch
            {

            }

            if (!EditorApplication.isPlaying)
            {
                Rect butRe = new Rect(sideBarWidth + 750, 20, 50, 20);
                
                if (editorStatePlaying)
                {
                    if (GUI.Button(butRe, EditorGUIUtility.IconContent("d_PauseButton")))
                    {
                        previewAudios.Clear();
                        editorStatePlaying = false;
                    }
                }
                else
                {
                    if (GUI.Button(butRe, EditorGUIUtility.IconContent("d_PlayButton")))
                    {
                        if (tracks.Count > 0)
                        {
                            for (int i = 0; i < tracks.Count; i++)
                            {
                                if (tracks[i].execTime == PengTrack.ExecTime.Update && tracks[i].nodes.Count > 0)
                                {
                                    for (int j = 0; j < tracks[i].nodes.Count; j++)
                                    {
                                        if (tracks[i].nodes[j].scriptType == PengScriptType.PlayAudio)
                                        {
                                            previewAudios.Add(tracks[i].start, tracks[i].nodes[j] as PlayAudio);
                                        }
                                    }
                                }
                            }
                        }
                        editorStatePlaying = true;
                    }
                }
            }
            else
            {
                editorStatePlaying = false;
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
        if (tracks.Count > 0 && currentSelectedTrack < tracks.Count && !editGlobal)
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
        if (editGlobal)
        {
            if (globalTrack.nodes.Count > 0)
            {
                for (int i = globalTrack.nodes.Count - 1; i >= 0; i--)
                {
                    bool dragged = globalTrack.nodes[i].ProcessEvents(e);
                    if (dragged)
                    {
                        GUI.changed = true;
                    }
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
            if (editGlobal)
            {
                menu.AddItem(new GUIContent("添加节点/按类型分类/" + PengNode.GetCatalogByFunction(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(ref globalTrack, mousePos, scriptType); });
                menu.AddItem(new GUIContent("添加节点/按首字母分类/" + PengNode.GetCatalogByName(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(ref globalTrack, mousePos, scriptType); });
                menu.AddItem(new GUIContent("添加节点/按封装程度分类/" + PengNode.GetCatalogByPackage(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(ref globalTrack, mousePos, scriptType); });
            }
            else
            {
                PengEditorTrack track = tracks[currentSelectedTrack];
                menu.AddItem(new GUIContent("添加节点/按类型分类/" + PengNode.GetCatalogByFunction(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(ref track, mousePos, scriptType); });
                menu.AddItem(new GUIContent("添加节点/按首字母分类/" + PengNode.GetCatalogByName(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(ref track, mousePos, scriptType); });
                menu.AddItem(new GUIContent("添加节点/按封装程度分类/" + PengNode.GetCatalogByPackage(scriptType) + "/" + PengNode.GetDescription(scriptType)), false, () => { ProcessAddNode(ref track, mousePos, scriptType); });
            }
            
        }
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
        if (tracks.Count > 0 && currentSelectedTrack < tracks.Count && !editGlobal)
        {
            if (tracks[currentSelectedTrack].nodes.Count > 0)
            {
                for (int i = 0; i < tracks[currentSelectedTrack].nodes.Count; i++)
                {
                    tracks[currentSelectedTrack].nodes[i].Draw();
                }
            }

            DrawConnectionLines();

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
                tracks[currentSelectedTrack].trackName = GUI.TextField(text1, tracks[currentSelectedTrack].trackName, style1);
            }
            else
            {
                GUI.Box(text1, tracks[currentSelectedTrack].trackName, style2);
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

           
            if (copyNodeInfo != null)
            {
                Rect copyNodeRect = new Rect(debugRect.x + debugRect.width + 10, debugRect.y, 80, 20);
                if (GUI.Button(copyNodeRect, "粘贴脚本"))
                {
                    PengEditorTrack trk = tracks[currentSelectedTrack];
                    PasteNode(ref trk);
                }
            }

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
        if (editGlobal)
        {
            if (globalTrack.nodes.Count > 0)
            {
                for (int i = 0; i < globalTrack.nodes.Count; i++)
                {
                    globalTrack.nodes[i].Draw();
                }
            }
            DrawConnectionLines();

            Rect box = new Rect(sideBarWidth, timelineHeight, position.width - sideBarWidth, 30);
            Rect label1 = new Rect(box.x + 8, box.y + 3, 80, 20);
            Rect text1 = new Rect(box.x + 88, box.y + 3, 120, 20);


            GUI.Box(box, "", style);
            GUI.Box(label1, "轨道名称：", style);
            GUI.Box(text1, "全局节点图", style);

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

            if (copyNodeInfo != null)
            {
                Rect copyNodeRect = new Rect(debugRect.x + debugRect.width + 10, debugRect.y, 80, 20);
                if (GUI.Button(copyNodeRect, "粘贴脚本"))
                {
                    PasteNode(ref globalTrack);
                }
            }
        }
    }

    public void CopyNode(PengNode node)
    {
        node.isSelected = false;
        copyNodeInternalDoc = new XmlDocument();
        XmlDeclaration decl = copyNodeInternalDoc.CreateXmlDeclaration("1.0", "UTF-8", "");
        copyNodeInfo = GeneratePureNodeInfo(ref copyNodeInternalDoc, node);
    }

    public static XmlElement GeneratePureNodeInfo(ref XmlDocument doc, PengNode pengNode)
    {
        XmlElement node = doc.CreateElement("Script");
        node.SetAttribute("Name", pengNode.nodeName);
        node.SetAttribute("ScriptType", pengNode.scriptType.ToString());
        node.SetAttribute("NodeType", pengNode.type.ToString());
        node.SetAttribute("NodePos", PengNode.ParseVector2ToString(pengNode.pos));
        node.SetAttribute("ScriptID", pengNode.nodeID.ToString());
        node.SetAttribute("Position", PengNode.ParseVector2ToString(pengNode.pos));
        node.SetAttribute("ParaNum", pengNode.paraNum.ToString());
        node.SetAttribute("OutID", PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(pengNode.outID.Count)));
        node.SetAttribute("VarOutID", PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(pengNode.varOutID.Count)));
        node.SetAttribute("VarInID", PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(pengNode.varInID.Count)));
        node.SetAttribute("SpecialInfo", pengNode.SpecialParaDescription());
        return node;
    }

    public static XmlElement GenerateNodeInfo(ref XmlDocument doc, PengNode pengNode)
    {
        XmlElement node = doc.CreateElement("Script");
        node.SetAttribute("Name", pengNode.nodeName);
        node.SetAttribute("ScriptType", pengNode.scriptType.ToString());
        node.SetAttribute("NodeType", pengNode.type.ToString());
        node.SetAttribute("NodePos", PengNode.ParseVector2ToString(pengNode.pos));
        node.SetAttribute("ScriptID", pengNode.nodeID.ToString());
        node.SetAttribute("Position", PengNode.ParseVector2ToString(pengNode.pos));
        node.SetAttribute("ParaNum", pengNode.paraNum.ToString());
        node.SetAttribute("OutID", PengNode.ParseDictionaryIntNodeIDConnectionIDToString(pengNode.outID));
        node.SetAttribute("VarOutID", PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(pengNode.varOutID));
        node.SetAttribute("VarInID", PengNode.ParseDictionaryIntNodeIDConnectionIDToString(pengNode.varInID));
        node.SetAttribute("SpecialInfo", pengNode.SpecialParaDescription());
        return node;
    }

    public void PasteNode(ref PengEditorTrack track)
    {
        PengNode node = ReadPengNode(copyNodeInfo, ref track);
        node.master = this;
        node.trackMaster = track;
        node.isSelected = true;
        node.pos = new Vector2(350f, 415f) + gridOffset;
        track.nodes.Add(node);
        Save(false);
    }

    private void DrawConnectionLines()
    {
        if (!editGlobal)
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
        else
        {
            if (globalTrack.nodes.Count > 0)
            {
                for (int i = 0; i < globalTrack.nodes.Count; i++)
                {
                    globalTrack.nodes[i].DrawLines();
                }
            }
        }
    }

    private void DrawPendingConnectionLine(Event e)
    {
        if (selectingPoint != null)
        {
            Vector3 start = (selectingPoint.type == PengScript.ConnectionPointType.In) ? selectingPoint.rect.center : e.mousePosition;
            Vector3 end = (selectingPoint.type == PengScript.ConnectionPointType.In) ? e.mousePosition : selectingPoint.rect.center;

            if (selectingPoint.type == PengScript.ConnectionPointType.In || selectingPoint.type == PengScript.ConnectionPointType.Out) { Handles.DrawBezier(start, end, start + Vector3.left * 40f, end - Vector3.left * 40f, Color.white, null, 3f); }
            else if (selectingPoint.type == PengScript.ConnectionPointType.FlowIn || selectingPoint.type == PengScript.ConnectionPointType.FlowOut) { Handles.DrawBezier(start, end, start + Vector3.left * 40f, end - Vector3.left * 40f, Color.white, null, 6f); }

            GUI.changed = true;

        }
    }

    public void ProcessRemoveNode(PengNode node)
    {
        if (!editGlobal)
        {
            if (tracks[currentSelectedTrack].nodes.Count > 1)
            {
                for (int i = 0; i < tracks[currentSelectedTrack].nodes.Count; i++)
                {
                    if (tracks[currentSelectedTrack].nodes[i].outID.Count > 0)
                    {
                        for (int j = 0; j < tracks[currentSelectedTrack].nodes[i].outID.Count; j++)
                        {
                            if (tracks[currentSelectedTrack].nodes[i].outID.ElementAt(j).Value.nodeID == node.nodeID)
                            {
                                PengNode.NodeIDConnectionID nici = new PengNode.NodeIDConnectionID();
                                nici.nodeID = -1;
                                tracks[currentSelectedTrack].nodes[i].outID[j] = nici;
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
            if (node.scriptType == PengScriptType.PlayEffects)
            {
                PlayEffects pe = node as PlayEffects;
                GameObject.DestroyImmediate(pe.ps.gameObject);
            }
            tracks[currentSelectedTrack].nodes.Remove(node);
        }
        else
        {
            if (globalTrack.nodes.Count > 1)
            {
                for (int i = 0; i < globalTrack.nodes.Count; i++)
                {
                    if (globalTrack.nodes[i].outID.Count > 0)
                    {
                        for (int j = 0; j < globalTrack.nodes[i].outID.Count; j++)
                        {
                            if (globalTrack.nodes[i].outID.ElementAt(j).Value.nodeID == node.nodeID)
                            {
                                PengNode.NodeIDConnectionID nici = new PengNode.NodeIDConnectionID();
                                nici.nodeID = -1;
                                globalTrack.nodes[i].outID[j] = nici;
                            }
                        }
                    }

                    if (globalTrack.nodes[i].varInID.Count > 0)
                    {
                        for (int j = 0; j < globalTrack.nodes[i].varInID.Count; j++)
                        {
                            if (globalTrack.nodes[i].varInID.ElementAt(j).Value.nodeID == node.nodeID)
                            {
                                globalTrack.nodes[i].varInID[j] = PengNode.DefaultNodeIDConnectionID();
                            }
                        }
                    }
                }
            }
            if (node.scriptType == PengScriptType.PlayEffects)
            {
                PlayEffects pe = node as PlayEffects;
                GameObject.DestroyImmediate(pe.ps.gameObject);
            }
            globalTrack.nodes.Remove(node);
        }
        
    }

    private void DragAllNodes(Vector2 change)
    {
        if (!editGlobal)
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
        else
        {
            if (globalTrack.nodes.Count > 0)
            {
                for (int i = 0; i < globalTrack.nodes.Count; i++)
                {
                    
                    globalTrack.nodes[i].ProcessDrag(change);
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

    public void OnCurrentSelectedTrackChanged()
    {

    }

    public void Save(bool showMsg)
    {
        SaveActorData(currentActorID, currentActorCamp, currentActorName, states, statesTrack, statesLength, statesLoop, statesTypes, showMsg, globalTrack);
    }

    public static void SaveActorData(int actorID, int actorCamp, string actorName, Dictionary<string, List<string>> stateGroup, Dictionary<string, List<PengEditorTrack>> stateTrack, Dictionary<string, int> statesLength, Dictionary<string, bool> statesLoop, Dictionary<string, PengActorState.StateType> stateTypes, bool showMsg, PengEditorTrack globalTrack)
    {
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
        if (globalTrack != null)
        {
            info.AppendChild(GenerateTrackInfo(ref doc, globalTrack));
        }

        XmlElement attr = doc.CreateElement("Attribute");
        attr.SetAttribute("MaxHP", globalTrack.master.currentActorMaxHP.ToString());
        attr.SetAttribute("AttackPower", globalTrack.master.currentActorAttackPower.ToString());
        attr.SetAttribute("DefendPower", globalTrack.master.currentActorDefendPower.ToString());
        attr.SetAttribute("CriticalRate", globalTrack.master.currentActorCriticalRate.ToString());
        attr.SetAttribute("CriticalDamageRatio", globalTrack.master.currentActorCriticalDamageRatio.ToString());
        attr.SetAttribute("Resist", globalTrack.master.currentActorResist.ToString());


        info.AppendChild(ID);
        info.AppendChild(camp);
        info.AppendChild(name);
        info.AppendChild(attr);
        
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
                        state.SetAttribute("StateType", stateTypes[stateName].ToString());
                        if (stateTrack[stateName].Count > 0)
                        {
                            for (int k = 0; k < stateTrack[stateName].Count; k++)
                            {
                                PengEditorTrack track = stateTrack[stateName][k];
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
        if (attrEditor != null)
        {
            attrEditor.Close();
            attrEditor = null;
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
            if (ele.Name == "Attribute")
            {
                currentActorMaxHP = float.Parse(ele.GetAttribute("MaxHP"));
                currentActorAttackPower = float.Parse(ele.GetAttribute("AttackPower"));
                currentActorDefendPower = float.Parse(ele.GetAttribute("DefendPower"));
                currentActorCriticalRate = float.Parse(ele.GetAttribute("CriticalRate"));
                currentActorCriticalDamageRatio = float.Parse(ele.GetAttribute("CriticalDamageRatio"));
                currentActorResist = float.Parse(ele.GetAttribute("Resist"));
                continue;
            }
            if (ele.Name == "Track")
            {
                globalTrack = GenerateTrack(ele);
                globalTrack.master = this;
            }
        }

        XmlNodeList stateGroupChild = actorState.ChildNodes;
        states = new Dictionary<string, List<string>>();
        statesFold = new Dictionary<string, bool>();
        statesLength = new Dictionary<string, int>();
        statesLoop = new Dictionary<string, bool>();
        statesTrack = new Dictionary<string, List<PengEditorTrack>>();
        statesTypes = new Dictionary<string, PengActorState.StateType>();
        foreach (XmlElement ele in stateGroupChild)
        {
            List<string> stateNames = new List<string>();
            foreach(XmlElement ele2 in ele.ChildNodes)
            {
                stateNames.Add(ele2.GetAttribute("Name"));
                statesLength.Add(ele2.GetAttribute("Name"), int.Parse(ele2.GetAttribute("Length")));
                statesLoop.Add(ele2.GetAttribute("Name"), int.Parse(ele2.GetAttribute("IsLoop")) > 0);
                if (ele2.HasAttribute("StateType"))
                {
                    statesTypes.Add(ele2.GetAttribute("Name"), (PengActorState.StateType)Enum.Parse(typeof(PengActorState.StateType), ele2.GetAttribute("StateType")));
                }
                else
                {
                    statesTypes.Add(ele2.GetAttribute("Name"), PengActorState.StateType.其他);
                }
                List<PengEditorTrack> pengTracks = new List<PengEditorTrack>();
                foreach(XmlElement trackEle in ele2.ChildNodes)
                {
                    PengEditorTrack track = GenerateTrack(trackEle);
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
        currentStateType = statesTypes[currentStateName];
        currentSelectedTrack = 2;
        editGlobal = false;
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

    public PengEditorTrack GenerateTrack(XmlElement trackEle)
    {
        PengEditorTrack track = new PengEditorTrack((PengTrack.ExecTime)Enum.Parse(typeof(PengTrack.ExecTime), trackEle.GetAttribute("ExecTime")),
                        trackEle.GetAttribute("Name"), int.Parse(trackEle.GetAttribute("Start")), int.Parse(trackEle.GetAttribute("End")), this, false);
        if (trackEle.GetAttribute("Other") != null)
        {
            track.otherInfo = trackEle.GetAttribute("Other");
        }
        
        foreach (XmlElement nodeEle in trackEle.ChildNodes)
        {
            PengNode node = ReadPengNode(nodeEle, ref track);
            track.nodes.Add(node);
        }
        return track;
    }

    public static XmlElement GenerateTrackInfo(ref XmlDocument doc, PengEditorTrack pengTrack)
    {
        XmlElement track = doc.CreateElement("Track");
        track.SetAttribute("Name", pengTrack.trackName);
        track.SetAttribute("Start", pengTrack.start.ToString());
        track.SetAttribute("End", pengTrack.end.ToString());
        track.SetAttribute("ExecTime", pengTrack.execTime.ToString());
        track.SetAttribute("Other", pengTrack.otherInfo.ToString());
        if (pengTrack.nodes.Count > 0)
        {
            for (int l = 0; l < pengTrack.nodes.Count; l++)
            {
                track.AppendChild(GenerateNodeInfo(ref doc, pengTrack.nodes[l]));
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
        tracks = new List<PengEditorTrack>();
        currentActorID = 100425;
        currentActorCamp = 1;
        currentActorName = "";
        currentScale = 1;
        debug = false;
        editorPlaying = false;
        states = new Dictionary<string, List<string>>();
        statesFold = new Dictionary<string, bool>();
        statesTrack = new Dictionary<string, List<PengEditorTrack>>();
        statesLength = new Dictionary<string, int>();
        statesLoop = new Dictionary<string, bool>();
        statesTypes = new Dictionary<string, PengActorState.StateType>();
        currentStateName = "";
        copyInfo = null;
        copyInternalDoc = null;
        copyNodeInfo = null;
        copyNodeInternalDoc = null;
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

    void OnSceneGUI(SceneView sv)
    {
        if (tracks.Count > 0 && Selection.activeTransform != null)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].execTime == PengTrack.ExecTime.Update && currentSelectedFrame >= tracks[i].start && currentSelectedFrame <= tracks[i].end && tracks[i].nodes.Count > 0)
                {
                    for (int j = 0; j < tracks[i].nodes.Count; j++)
                    {
                        if (tracks[i].nodes[j].scriptType == PengScript.PengScriptType.GetTargetsByRange)
                        {
                            GetTargetsByRange gtbr = tracks[i].nodes[j] as GetTargetsByRange;
                            switch (gtbr.rangeType)
                            {
                                case RangeType.Cylinder:
                                    Handles.color = Color.red;
                                    Handles.DrawWireArc(Selection.activeTransform.position + gtbr.pengOffset.value, Vector3.up, Selection.activeTransform.forward, gtbr.pengPara.value.z, gtbr.pengPara.value.x);
                                    Handles.DrawWireArc(Selection.activeTransform.position + gtbr.pengOffset.value, Vector3.up, Selection.activeTransform.forward, - gtbr.pengPara.value.z, gtbr.pengPara.value.x);
                                    Handles.DrawWireArc(Selection.activeTransform.position + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up, Vector3.up, Selection.activeTransform.forward, gtbr.pengPara.value.z, gtbr.pengPara.value.x);
                                    Handles.DrawWireArc(Selection.activeTransform.position + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up, Vector3.up, Selection.activeTransform.forward, - gtbr.pengPara.value.z, gtbr.pengPara.value.x);
                                    Handles.DrawLine(Selection.activeTransform.position + Selection.activeTransform.forward * gtbr.pengPara.value.x + gtbr.pengOffset.value, Selection.activeTransform.position + Selection.activeTransform.forward * gtbr.pengPara.value.x + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up);
                                    Vector3 pos1 = Quaternion.Euler(0, gtbr.pengPara.value.z, 0) * (Selection.activeTransform.forward * gtbr.pengPara.value.x);
                                    pos1 = Selection.activeTransform.position + gtbr.pengOffset.value + pos1;
                                    Vector3 pos3 = pos1 + gtbr.pengPara.value.y * Vector3.up;
                                    Vector3 pos2 = Quaternion.Euler(0, - gtbr.pengPara.value.z, 0) * (Selection.activeTransform.forward * gtbr.pengPara.value.x);
                                    pos2 = Selection.activeTransform.position + gtbr.pengOffset.value + pos2;
                                    Vector3 pos4 = pos2 + gtbr.pengPara.value.y * Vector3.up;
                                    Handles.DrawLine(pos1, pos3);
                                    Handles.DrawLine(pos2, pos4);
                                    Handles.DrawLine(pos1, Selection.activeTransform.position + gtbr.pengOffset.value);
                                    Handles.DrawLine(pos2, Selection.activeTransform.position + gtbr.pengOffset.value);
                                    Handles.DrawLine(pos3, Selection.activeTransform.position + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up);
                                    Handles.DrawLine(pos4, Selection.activeTransform.position + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up);
                                    Handles.DrawLine(Selection.activeTransform.position + gtbr.pengOffset.value, Selection.activeTransform.position + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up);
                                    Handles.DrawLine(Selection.activeTransform.position + gtbr.pengOffset.value, Selection.activeTransform.position + Selection.activeTransform.forward * gtbr.pengPara.value.x + gtbr.pengOffset.value);
                                    Handles.DrawLine(Selection.activeTransform.position + Selection.activeTransform.forward * gtbr.pengPara.value.x + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up, Selection.activeTransform.position + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up);
                                    Handles.DrawWireArc(Selection.activeTransform.position + gtbr.pengOffset.value, Vector3.up, Selection.activeTransform.forward, gtbr.pengPara.value.z, gtbr.pengPara.value.x * 0.5f);
                                    Handles.DrawWireArc(Selection.activeTransform.position + gtbr.pengOffset.value, Vector3.up, Selection.activeTransform.forward, -gtbr.pengPara.value.z, gtbr.pengPara.value.x * 0.5f);
                                    Handles.DrawWireArc(Selection.activeTransform.position + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up, Vector3.up, Selection.activeTransform.forward, gtbr.pengPara.value.z, gtbr.pengPara.value.x * 0.5f);
                                    Handles.DrawWireArc(Selection.activeTransform.position + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up, Vector3.up, Selection.activeTransform.forward, -gtbr.pengPara.value.z, gtbr.pengPara.value.x * 0.5f);
                                    Vector3 pos5 = Quaternion.Euler(0, gtbr.pengPara.value.z, 0) * (Selection.activeTransform.forward * gtbr.pengPara.value.x * 0.5f);
                                    pos5 = Selection.activeTransform.position + gtbr.pengOffset.value + pos5;
                                    Vector3 pos7 = pos5 + gtbr.pengPara.value.y * Vector3.up;
                                    Vector3 pos6 = Quaternion.Euler(0, -gtbr.pengPara.value.z, 0) * (Selection.activeTransform.forward * gtbr.pengPara.value.x * 0.5f);
                                    pos6 = Selection.activeTransform.position + gtbr.pengOffset.value + pos6;
                                    Vector3 pos8 = pos6 + gtbr.pengPara.value.y * Vector3.up;
                                    Handles.DrawLine(pos5, pos7);
                                    Handles.DrawLine(pos6, pos8);
                                    Handles.DrawLine(Selection.activeTransform.position + Selection.activeTransform.forward * gtbr.pengPara.value.x * 0.5f + gtbr.pengOffset.value, Selection.activeTransform.position + Selection.activeTransform.forward * gtbr.pengPara.value.x * 0.5f + gtbr.pengOffset.value + gtbr.pengPara.value.y * Vector3.up);

                                    break;
                                case RangeType.Sphere:
                                    Handles.color = Color.red;
                                    Handles.DrawWireDisc(Selection.activeTransform.position + gtbr.pengOffset.value, Vector3.up, gtbr.pengPara.value.x);
                                    Handles.DrawWireDisc(Selection.activeTransform.position + gtbr.pengOffset.value, Vector3.forward, gtbr.pengPara.value.x);
                                    Handles.DrawWireDisc(Selection.activeTransform.position + gtbr.pengOffset.value, Vector3.right, gtbr.pengPara.value.x);
                                    Handles.DrawWireDisc(Selection.activeTransform.position + gtbr.pengOffset.value, Vector3.forward - Vector3.right, gtbr.pengPara.value.x);
                                    Handles.DrawWireDisc(Selection.activeTransform.position + gtbr.pengOffset.value, Vector3.forward + Vector3.right, gtbr.pengPara.value.x);
                                    break;
                                case RangeType.Box:
                                    Handles.color = Color.red;
                                    Handles.DrawWireCube(Selection.activeTransform.position + gtbr.pengOffset.value, gtbr.pengPara.value);
                                    break;
                            }
                        }
                    }
                }

                if (tracks[i].execTime == PengTrack.ExecTime.Update && tracks[i].nodes.Count > 0)
                {
                    for (int j = 0; j < tracks[i].nodes.Count; j++)
                    {
                        if (tracks[i].nodes[j].scriptType == PengScript.PengScriptType.PlayEffects)
                        {
                            PlayEffects pe = tracks[i].nodes[j] as PlayEffects;
                            if (pe.ps == null)
                            {
                                if (File.Exists(Application.dataPath + "/Resources/Effects/" + pe.effectPath.value + ".prefab"))
                                {
                                    GameObject psPrefab = Resources.Load("Effects/" + pe.effectPath.value) as GameObject;
                                    GameObject psGO = GameObject.Instantiate(psPrefab, Selection.activeTransform);
                                    pe.ps = psGO.GetComponent<ParticleSystem>();
                                    psList.Add(pe.ps);
                                    pe.ps.tag = "Temporary";
                                }
                            }
                            else
                            {
                                if (!pe.ps.gameObject.activeSelf)
                                {
                                    pe.ps.gameObject.SetActive(true);
                                }
                                pe.ps.Simulate((currentSelectedFrame - tracks[i].start) / globalFrameRate);
                                pe.ps.transform.localPosition = pe.posOffset.value;
                                pe.ps.transform.localRotation = Quaternion.Euler(pe.rotOffset.value);
                                pe.ps.transform.localScale = pe.scaleOffset.value;

                                if ((currentSelectedFrame - tracks[i].start) / globalFrameRate >= pe.deleteTime.value)
                                {
                                    pe.ps.Simulate(-1);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void EnterPlayModeClearVFX(PlayModeStateChange obj)
    {
        if (psList.Count > 0)
        {
            for (int i = psList.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(psList[i].gameObject); 
            }
            psList.Clear();
        }
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Temporary");
        if (temp.Length > 0)
        {
            for (int i = temp.Length - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(temp[i]);
            }
        }
    }

    public void OnCurrentStateChanged()
    {
        if (EditorApplication.isPlaying)
        {
            if (!runTimeEdit)
            {
                if (psList.Count > 0)
                {
                    Debug.Log(psList.Count);
                    for (int i = psList.Count - 1; i >= 0; i--)
                    {
                        DestroyImmediate(psList[i].gameObject);
                    }
                    psList.Clear();
                }
            }
        }
        else
        {
            if (psList.Count > 0)
            {
                for (int i = psList.Count - 1 ; i >= 0; i--)
                {
                    DestroyImmediate(psList[i].gameObject); 
                }
                psList.Clear();
            }
        }
    }

    public void UpdateState()
    {
        if (editorStatePlaying)
        {
            editorStatePreviewTimeCount += Time.deltaTime;
            if (editorStatePreviewTimeCount >= 2 / globalFrameRate)
            {
                editorStatePreviewTimeCount -= 2 / globalFrameRate;
                currentSelectedFrame++;
                if (currentSelectedFrame >= currentFrameLength)
                {
                    currentSelectedFrame = 0;
                }
                if (previewAudios.Count > 0 && previewAudios.ContainsKey(currentSelectedFrame))
                {
                    previewAudios[currentSelectedFrame].PlayInEdtior();
                }
                
                Repaint();
            }
        }
    }
}
