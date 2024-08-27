using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.ComponentModel;

public class PengActorGeneratorEditor : EditorWindow
{
    /// <summary>
    /// 一键生成角色的牛逼工具。
    /// </summary>
    /// 

    int actorID;
    string actorName;
    int actorCamp;
    GameObject actorModel = null;

    Vector2 stateScrollPos = Vector2.zero;
    Dictionary<string, List<string>> stateGroup; 
    Dictionary<string, int> statesLength;
    Dictionary<string, bool> statesLoop;

    float globalFrameRate;
    XmlDocument globalConfiguration = null;

    public enum GenerateMode
    {
        [Description("创建新角色")]
        New,
        [Description("复制已有角色")]
        Copy,
    }

    public GenerateMode generateMode = GenerateMode.New;
    public int copyID;
    public int pasteID;
    public int pasteCamp;
    public string pasteName;


    [MenuItem("PengFramework/角色生成器", false, 3)]
    static void Init()
    {
        PengActorGeneratorEditor window = (PengActorGeneratorEditor)EditorWindow.GetWindow(typeof(PengActorGeneratorEditor));
        window.position = new Rect(120, 120, 400, 800);
        window.titleContent = new GUIContent("彭框架角色生成器");
    }

    private void OnEnable()
    {
        actorID = 100001;
        actorName = "Actor Name";
        actorCamp = 1;

        stateGroup = new Dictionary<string, List<string>>();
        statesLength = new Dictionary<string, int>();
        statesLoop = new Dictionary<string, bool>();

        List<string> group1 = new List<string>();
        group1.Add("Idle");
        group1.Add("Intro");
        group1.Add("Dead");
        List<string> group2 = new List<string>();
        group2.Add("Move");
        group2.Add("Run");
        group2.Add("Boost");
        List<string> group3 = new List<string>();
        group3.Add("Attack_1");
        group3.Add("Attack_2");
        List<string> group4 = new List<string>();
        group4.Add("Skill_1");
        group4.Add("Skill_2");

        stateGroup.Add("Idle", group1);
        stateGroup.Add("Move", group2);
        stateGroup.Add("Attack", group3);
        stateGroup.Add("Skill", group4);

        statesLoop.Add("Idle", true);
        statesLoop.Add("Intro", false);
        statesLoop.Add("Dead", false);
        statesLoop.Add("Move", true);
        statesLoop.Add("Run", true);
        statesLoop.Add("Boost", false);
        statesLoop.Add("Attack_1", false);
        statesLoop.Add("Attack_2", false);
        statesLoop.Add("Skill_1", false);
        statesLoop.Add("Skill_2", false);

        for(int i = 0; i < statesLoop.Count; i++)
        {
            statesLength.Add(statesLoop.ElementAt(i).Key, 60);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();


        PengEditorMain.DrawPengFrameworkIcon("角色生成器");
        GUILayout.Space(20);

        GUIStyle style = new GUIStyle("Box");
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        GUILayout.Box("填写注意：\n" +
            "一、角色模型需要带Animator组件。\n" +
            "二、创建前，将角色动画片段放置于Resources/Animations/角色ID，并以角色ID+@+状态名的格式命名。\n" +
            "例如：Resources/Animations/100001/100001@Idle\n" +
            "三、角色状态里必须有Idle状态。\n" +
            "四、状态名作为状态的唯一识别符，原则上不允许频繁更改、不允许重名，请慎重制定命名规范，出现因更改状态名、状态重名而导致的bug，后果自负！\n" +
            "五、想修改生成角色的状态名时，建议把已有的想改的名字删掉，再在状态组里添加一个新的字段。", style);

        GUILayout.Space(10);

        generateMode = (GenerateMode)EditorGUILayout.EnumPopup(generateMode);
        if (WhetherHaveGlobalConfiguration())
        {
            switch (generateMode)
            {
                case GenerateMode.New:
                    PengActorGenerator();
                break;
            case GenerateMode.Copy:
                CopyActor();
                break;
            }
        }
        EditorGUILayout.EndVertical();
    }

    public bool WhetherHaveGlobalConfiguration()
    {
        if (globalConfiguration == null)
        {
            if (File.Exists(Application.dataPath + "/Resources/GlobalConfiguration/GlobalSetting.xml"))
            {
                TextAsset textAsset = null;
                textAsset = (TextAsset)Resources.Load("GlobalConfiguration/GlobalSetting");
                if (textAsset != null)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(textAsset.text);
                    globalConfiguration = xml;
                    return true;
                }
                else
                {
                    Debug.LogWarning("意外：Resources/GlobalConfiguration/GlobalSetting.xml存在，但却读不出TextAsset？");
                    return false;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("暂不存在全局配置文件，请使用上方按钮来生成！", MessageType.Warning);
                return false;
            }
        }
        else
        {
            XmlElement frameSettingElement = (XmlElement)globalConfiguration.SelectSingleNode("FrameSetting");
            if (frameSettingElement != null)
            {
                globalFrameRate = float.Parse(frameSettingElement.GetAttribute("ActionFrameRate"));
                return true;
            }
            else
            {
                EditorGUILayout.HelpBox("全局配置中不存在动作帧帧率信息，请使用启动器来修复！", MessageType.Warning);
                return false;
            }
        }
    }

    public void PengActorGenerator()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("角色ID：");
        actorID = EditorGUILayout.IntField(actorID, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("角色名称：");
        actorName = EditorGUILayout.TextField(actorName, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("角色阵营：");
        actorCamp = EditorGUILayout.IntField(actorCamp, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("角色模型：");
        actorModel = (GameObject)EditorGUILayout.ObjectField(actorModel, typeof(GameObject), true, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        GUILayout.Label("角色状态：");
        DrawStates();
        EditorGUILayout.EndVertical();


        if (GUILayout.Button("一键生成角色"))
        {
            if(File.Exists(Application.dataPath + "/Resources/ActorData/" + actorID.ToString() + "/" + actorID.ToString() + ".xml"))
            {
                EditorUtility.DisplayDialog("警告", "已存在Actor数据！", "ok");
                EditorGUILayout.EndVertical();
                return;
            }
            if (actorModel != null && actorModel.GetComponent<Animator>()  && statesLength.Count > 0)
            {
                bool hasIdle = statesLength.ContainsKey("Idle") && statesLength.ContainsKey("Dead");

                if (!hasIdle)
                {
                    EditorUtility.DisplayDialog("警告", "角色状态没有Idle或Dead，无法生成。", "ok");
                    return;
                }

                GameObject actorNew = GameObject.Instantiate(actorModel);

                //添加Animator
                if (!Directory.Exists(Application.dataPath + "/Resources/Animators/" + actorID.ToString()))
                {
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Animators/" + actorID.ToString());
                }

                //AnimatorController anim = new AnimatorController();
                AnimatorController anim = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/Resources/Animators/" + actorID.ToString() + "/Animator" + actorID.ToString() + ".controller");


                anim.name = "Animator" + actorID.ToString();
                AnimatorControllerLayer layer = anim.layers[0];
                AnimatorStateMachine states = layer.stateMachine;



                Dictionary<string, List<PengEditorTrack>> stateTrack = new Dictionary<string, List<PengEditorTrack>>();

                AnimatorState idleState = states.AddState("Idle", new Vector3(states.entryPosition.x + 200, states.entryPosition.y, 0));
                AnimationClip clip1 = Resources.Load("Animations/" + actorID.ToString() + "/" + actorID + "@Idle") as AnimationClip;
                List<PengEditorTrack> track = new List<PengEditorTrack>();
                PengEditorTrack enterTrack1 = new PengEditorTrack(PengTrack.ExecTime.Enter, "OnEnter", 0, 0, null, true);
                InitialAnimation(ref enterTrack1, "Idle");
                if (clip1 != null)
                {
                    //如果能读取到片段，可以直接给enterTrack1附加一个播放动画的节点，并和轨道执行连接起来
                    idleState.motion = clip1;
                    clip1.frameRate = globalFrameRate;
                    statesLength["Idle"] = Mathf.FloorToInt(clip1.length * globalFrameRate);
                }
                else
                {
                    Debug.LogWarning("Actor" + actorID.ToString() + "的Idle状态未填入动画片段，请手动在状态中放入Clip。");
                }
                track.Add(enterTrack1);
                track.Add(new PengEditorTrack(PengTrack.ExecTime.Exit, "OnExit", 0, 0, null, true));
                track.Add(new PengEditorTrack(PengTrack.ExecTime.Update, "Track", 3, 20, null, true));
                stateTrack.Add("Idle", track);


                int j = 0;
                for (int i = 0; i < statesLength.Count; i++)
                {
                    if(statesLength.ElementAt(i).Key != "Idle")
                    {
                        AnimatorState state = states.AddState(statesLength.ElementAt(i).Key, new Vector3(states.entryPosition.x + 200, states.entryPosition.y + 50 * (j + 1), 0));
                        AnimationClip clip = Resources.Load("Animations/" + actorID.ToString() + "/" + actorID.ToString() + "@" + statesLength.ElementAt(i).Key) as AnimationClip;

                        List<PengEditorTrack> track1 = new List<PengEditorTrack>();
                        PengEditorTrack enterTrack = new PengEditorTrack(PengTrack.ExecTime.Enter, "OnEnter", 0, 0, null, true);
                        InitialAnimation(ref enterTrack, statesLength.ElementAt(i).Key);

                        if (clip != null)
                        {
                            state.motion = clip;
                            clip.frameRate = globalFrameRate;
                            statesLength[statesLength.ElementAt(i).Key] = Mathf.FloorToInt(clip.length * globalFrameRate);
                        }
                        else
                        {
                            Debug.LogWarning("Actor" + actorID.ToString() + "的" + statesLength.ElementAt(i).Key + "状态未填入动画片段，请手动在状态中放入Clip。");
                        }
                        track1.Add(enterTrack);
                        track1.Add(new PengEditorTrack(PengTrack.ExecTime.Exit, "OnExit", 0, 0, null, true));
                        track1.Add(new PengEditorTrack(PengTrack.ExecTime.Update, "Track", 3, 20, null, true));
                        stateTrack.Add(statesLength.ElementAt(i).Key, track1);
                        j++;
                    }
                }

                //AssetDatabase.CreateAsset(anim, Application.dataPath + "/Resources/Animators/" + actorID.ToString() + "/Animator" + actorID.ToString() + ".controller");
                actorNew.GetComponent<Animator>().runtimeAnimatorController = anim;
                PengActor pa = actorNew.AddComponent<PengActor>();
                actorNew.AddComponent<CharacterController>();
                actorNew.GetComponent<CharacterController>().center = new Vector3(0,1.1f,0);
                pa.actorID = actorID;
                actorNew.tag = "PengActor";
                actorNew.layer = LayerMask.NameToLayer("PengActor");
                actorNew.name = "Actor" + actorID.ToString();
                foreach (SkinnedMeshRenderer smr in actorNew.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    pa.smrs.Add(smr);
                }
                foreach (MeshRenderer mr in actorNew.GetComponentsInChildren<MeshRenderer>())
                {
                    pa.mrs.Add(mr);
                }
                PengEditorTrack globalTrack = new PengEditorTrack(PengTrack.ExecTime.Global, "Global", 0, 0, null, true);
                Dictionary<string, PengActorState.StateType> stateTypes = new Dictionary<string, PengActorState.StateType>();
                for (int i = 0; i < statesLength.Count; i++)
                {
                    stateTypes.Add(statesLength.ElementAt(i).Key, PengActorState.StateType.其他);
                }

                PengActorStateEditorWindow.SaveActorData(actorID, actorCamp, actorName, stateGroup, stateTrack, statesLength, statesLoop, stateTypes, true, globalTrack);

                if(!Directory.Exists(Application.dataPath + "/Resources/Actors/" + actorID.ToString()))
                {
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Actors/" + actorID.ToString());
                }
                bool success = false;
                PrefabUtility.SaveAsPrefabAsset(actorNew, Application.dataPath + "/Resources/Actors/" + actorID.ToString() + "/Actor" + actorID.ToString() +".prefab", out success);
                Debug.Log(string.Format("Actor" + actorID.ToString() + "保存[{0}]", success ? "成功":"失败"));
                DestroyImmediate(actorNew);
                AssetDatabase.Refresh();
            }
            else
            {
                EditorUtility.DisplayDialog("警告", "没有放入角色模型，或不存在Animator组件，或没有填入角色状态，无法生成。", "ok");
            }

        }
        EditorGUILayout.EndVertical();
    }

    public void InitialAnimation(ref PengEditorTrack enterTrack, string stateName)
    {
        PengNode animNode = new PlayAnimation(enterTrack.nodes[0].pos + new Vector2(300, 0), null, ref enterTrack, 2,
                            PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                            PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                            PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(5)), "");
        PengNode.NodeIDConnectionID nici1 = PengNode.DefaultNodeIDConnectionID();
        nici1.nodeID = 3;
        nici1.connectionID = 0;
        PengNode.NodeIDConnectionID nici2 = PengNode.DefaultNodeIDConnectionID();
        nici2.nodeID = 4;
        nici2.connectionID = 0;
        PengNode.NodeIDConnectionID nici3 = PengNode.DefaultNodeIDConnectionID();
        nici3.nodeID = 5; 
        nici3.connectionID = 0;
        animNode.varInID[0] = nici1;
        animNode.varInID[1] = nici2;
        animNode.varInID[4] = nici3;

        ValuePengString stringNode = new ValuePengString(enterTrack.nodes[0].pos + new Vector2(0, 80), null, ref enterTrack, 3,
            PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
            PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
            PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), "");
        stringNode.pengString.value = stateName;

        ValuePengBool boolNode = new ValuePengBool(enterTrack.nodes[0].pos + new Vector2(0, 160), null, ref enterTrack, 4,
            PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
            PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
            PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), "");
        boolNode.pengBool.value = true;

        ValuePengInt intNode = new ValuePengInt(enterTrack.nodes[0].pos + new Vector2(0, 240), null, ref enterTrack, 5,
            PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
            PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
            PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), "");
        intNode.pengInt.value = 0;
        PengNode.NodeIDConnectionID sivi = new PengNode.NodeIDConnectionID();
        sivi.nodeID = 2;
        sivi.connectionID = 0;
        enterTrack.nodes[0].outID[0] = sivi;
        enterTrack.nodes.Add(animNode);
        enterTrack.nodes.Add(stringNode);
        enterTrack.nodes.Add(boolNode);
        enterTrack.nodes.Add(intNode);
    }

    public void DrawStates()
    {
        stateScrollPos = EditorGUILayout.BeginScrollView(stateScrollPos, GUILayout.Height(position.height - 500), GUILayout.Width(position.width));
        if (stateGroup.Count > 0)
        {
            string stateGroupNameToEdit = null;
            string stateGroupNameNew = null;
            for (int i = 0; i < stateGroup.Count; i++)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.Label("状态组：");
                GUILayout.Space(10);
                string stateGroupName = stateGroup.ElementAt(i).Key;
                stateGroupName = GUILayout.TextField(stateGroupName, GUILayout.Width(150));
                if(stateGroupName != stateGroup.ElementAt(i).Key && !StateGroupNameRepeat(stateGroupName))
                {
                    stateGroupNameToEdit = stateGroup.ElementAt(i).Key;
                    stateGroupNameNew = stateGroupName;
                }
                GUILayout.Space(10);
                if(GUILayout.Button("删除"))
                {
                    stateGroupNameToEdit = stateGroup.ElementAt(i).Key;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }
                GUILayout.Space(10);
                if (GUILayout.Button("添加"))
                {
                    string sn = ReturnANewStateName();
                    stateGroup.ElementAt(i).Value.Add(sn);
                    statesLength.Add(sn, 60);
                    statesLoop.Add(sn, false);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                if (stateGroup.ElementAt(i).Value.Count > 0)
                {
                    string stateNameToEdit = null;
                    string stateNameNew = null;
                    for(int j = 0; j < stateGroup.ElementAt(i).Value.Count ; j++)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(60);
                        GUILayout.Label("状态名：");
                        GUILayout.Space(10);
                        string stateName = stateGroup.ElementAt(i).Value[j];
                        stateName = GUILayout.TextField(stateName, GUILayout.Width(120));
                        if(stateName != stateGroup.ElementAt(i).Value[j] && !StateNameRepeat(stateName))
                        {
                            stateNameToEdit = stateGroup.ElementAt(i).Value[j];
                            stateNameNew = stateName;
                        }
                        GUILayout.Space(10);
                        GUILayout.Label("循环：");
                        GUILayout.Space(10);
                        bool stateLoop = statesLoop[stateGroup.ElementAt(i).Value[j]];
                        stateLoop = EditorGUILayout.Toggle(stateLoop);
                        GUILayout.Space(10);
                        if (GUILayout.Button("删除"))
                        {
                            stateNameToEdit = stateGroup.ElementAt(i).Value[j];

                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            continue;
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                    if(stateNameToEdit != null)
                    {
                        if(stateNameNew == null)
                        {
                            stateGroup.ElementAt(i).Value.Remove(stateNameToEdit);
                            statesLength.Remove(stateNameToEdit);
                            statesLoop.Remove(stateNameToEdit);
                        }
                        else
                        {
                            stateGroup.ElementAt(i).Value.Add(stateNameNew);
                            statesLength.Add(stateNameNew, statesLength[stateNameToEdit]);
                            statesLoop.Add(stateNameNew, statesLoop[stateNameToEdit]);
                            stateGroup.ElementAt(i).Value.Remove(stateNameToEdit);
                            statesLength.Remove(stateNameToEdit);
                            statesLoop.Remove(stateNameToEdit);
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            if(stateGroupNameToEdit != null)
            {
                if(stateGroupNameNew == null)
                {
                    if (stateGroup[stateGroupNameToEdit].Count > 0)
                    {
                        for (int i = 0; i < stateGroup[stateGroupNameToEdit].Count; i++)
                        {
                            statesLength.Remove(stateGroup[stateGroupNameToEdit][i]);
                            statesLoop.Remove(stateGroup[stateGroupNameToEdit][i]);
                        }
                    }
                    stateGroup.Remove(stateGroupNameToEdit);
                }
                else
                {
                    List<string> states = new List<string>();
                    if (stateGroup[stateGroupNameToEdit].Count > 0)
                    {
                        for (int i = 0; i < stateGroup[stateGroupNameToEdit].Count; i++)
                        {
                            states.Add(stateGroup[stateGroupNameToEdit][i]);
                        }
                    }
                    stateGroup.Remove(stateGroupNameToEdit);
                    stateGroup.Add(stateGroupNameNew, states);
                }
            }
        }
        if (GUILayout.Button("添加状态组"))
        {
            List<string> group1 = new List<string>();
            string sgn = ReturnANewStateGroupName();
            string sn = ReturnANewStateName();
            group1.Add(sn);
            stateGroup.Add(sgn, group1);
            statesLength.Add(sn, 60);
            statesLoop.Add(sn, false);
        }
        
        EditorGUILayout.EndScrollView();
        GUILayout.Space(30);
    }

    public string ReturnANewStateName()
    {
        string result = "NewState";
        int i = 1;
        while(StateNameRepeat(result))
        {
            result = "NewState" + i.ToString();
            i++;
        }
        return result;
    }

    public string ReturnANewStateGroupName()
    {
        string result = "NewStateGroup";
        int i = 1;
        while (StateGroupNameRepeat(result))
        {
            result = "NewStateGroup" + i.ToString();
            i++;
        }
        return result;
    }

    public bool StateNameRepeat(string str)
    {
        if (statesLength.Count > 0 && statesLength.ContainsKey(str) && str != "")
        {
            return true;
        }
        return false;
    }

    public bool StateGroupNameRepeat(string str)
    {
        if (stateGroup.Count > 0 && stateGroup.ContainsKey(str) && str != "")
        {
            return true;
        }
        return false;
    }

    public void CopyActor()
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

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("目标对象阵营：");
        pasteCamp = EditorGUILayout.IntField(pasteCamp);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("目标对象名称：");
        pasteName = EditorGUILayout.TextField(pasteName);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("一键复制"))
        {
            if(File.Exists(Application.dataPath+"/Resources/ActorData/" + copyID.ToString() + "/" + copyID.ToString() + ".xml"))
            {
                TextAsset textAsset = (TextAsset)Resources.Load("ActorData/" + copyID.ToString() + "/" + copyID.ToString());
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
                XmlElement states = null;
                foreach (XmlElement node in root.ChildNodes)
                {
                    if(node.Name == "ActorInfo")
                    {
                        info = node;
                    }
                    if (node.Name == "ActorState")
                    {
                        states = node;
                    }
                }
                foreach (XmlElement ele in info.ChildNodes)
                {
                    if (ele.Name == "ActorID")
                    {
                        ele.SetAttribute("ActorID", pasteID.ToString());
                        continue;
                    }
                    if (ele.Name == "Camp")
                    {
                        ele.SetAttribute("Camp", pasteCamp.ToString());
                        continue;
                    }
                    if (ele.Name == "ActorName")
                    {
                        ele.SetAttribute("ActorName", pasteName);
                        continue;
                    }
                }

                if (!Directory.Exists(Application.dataPath + "/Resources/ActorData/" + pasteID.ToString()))
                {
                    Directory.CreateDirectory(Application.dataPath + "/Resources/ActorData/" + pasteID.ToString());
                }
                doc.Save(Application.dataPath + "/Resources/ActorData/" + pasteID.ToString() + "/" + pasteID.ToString() + ".xml");

                //复制：物体（改ID）；物体上的PengActor组件；Animator；所有的动画片段
                GameObject actorOld = Resources.Load("Actors/" + copyID.ToString() + "/Actor" + copyID.ToString()) as GameObject;
                GameObject actorNew = GameObject.Instantiate(actorOld);

                if (!Directory.Exists(Application.dataPath + "/Resources/Animators/" + pasteID.ToString()))
                {
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Animators/" + pasteID.ToString());
                }
                AnimatorController animNew = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/Resources/Animators/" + pasteID.ToString() + "/Animator" + pasteID.ToString() + ".controller");
                AnimatorController animOld = Resources.Load("Animators/" + copyID.ToString() + "/Animator" + copyID.ToString()) as AnimatorController;

                for (int i = 0; i < animOld.layers.Length; i++)
                {
                    if(i >= animNew.layers.Length)
                    {
                        animNew.AddLayer(animOld.layers[i].name);
                    }
                    for (int j = 0; j < animOld.layers[i].stateMachine.states.Length; j++)
                    {
                        AnimatorState state = animOld.layers[i].stateMachine.states[j].state;
                        AnimatorState newState = new AnimatorState();
                        EditorUtility.CopySerialized(state, newState);
                        if ((File.Exists(Application.dataPath + "/Resources/Animations/" + copyID.ToString() + "/" + copyID.ToString() + "@" + state.name + ".anim")))
                        {
                            AnimationClip clip = Resources.Load("Animations/" + copyID.ToString() + "/" + copyID.ToString() + "@" + state.name) as AnimationClip;
                            //string path = Application.dataPath + "/Resources/Animations/" + copyID.ToString() + "/" + copyID.ToString() + "@" + state.name + ".anim";
                            //string newPath = Application.dataPath + "/Resources/Animations/" + pasteID.ToString() + "/" + pasteID.ToString() + "@" + state.name + ".anim";
                            AnimationClip newClip = new AnimationClip();
                            EditorUtility.CopySerialized(clip, newClip);

                            if (!Directory.Exists(Application.dataPath + "/Resources/Animations/" + pasteID.ToString()))
                            {
                                Directory.CreateDirectory(Application.dataPath + "/Resources/Animations/" + pasteID.ToString());
                            }
                            AssetDatabase.CreateAsset(newClip, "Assets/Resources/Animations/" + pasteID.ToString() + "/" + pasteID.ToString() + "@" + state.name + ".anim");
                            AnimationClip clip1 = Resources.Load("Animations/" + pasteID.ToString() + "/" + pasteID.ToString() + "@" + state.name) as AnimationClip;
                            newState.motion = clip1;
                        }
                        animNew.layers[i].stateMachine.AddState(newState, animOld.layers[i].stateMachine.states[j].position);
                    }
                }

                actorNew.name = "Actor" + pasteID.ToString();
                actorNew.GetComponent<PengActor>().actorID = pasteID;
                actorNew.GetComponent<Animator>().runtimeAnimatorController = animNew;

                if (!Directory.Exists(Application.dataPath + "/Resources/Actors/" + pasteID.ToString()))
                {
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Actors/" + pasteID.ToString());
                }
                bool success = false;
                PrefabUtility.SaveAsPrefabAsset(actorNew, Application.dataPath + "/Resources/Actors/" + pasteID.ToString() + "/Actor" + pasteID.ToString() + ".prefab", out success);
                DestroyImmediate(actorNew);
                AssetDatabase.Refresh();
            }
        }

        EditorGUILayout.EndVertical();
    }
}
