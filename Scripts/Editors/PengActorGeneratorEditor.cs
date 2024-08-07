using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using static UnityEditor.VersionControl.Asset;

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

    [MenuItem("PengFramework/Actor Generator")]
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

        if(WhetherHaveGlobalConfiguration())
        {
            PengActorGenerator();
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
                bool hasIdle = statesLength.ContainsKey("Idle");

                if (!hasIdle)
                {
                    EditorUtility.DisplayDialog("警告", "角色状态没有Idle，无法生成。", "ok");
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



                Dictionary<string, List<PengTrack>> stateTrack = new Dictionary<string, List<PengTrack>>();

                AnimatorState idleState = states.AddState("Idle", new Vector3(states.entryPosition.x + 200, states.entryPosition.y, 0));
                AnimationClip clip1 = Resources.Load("Animations/" + actorID.ToString() + "/" + actorID + "@Idle") as AnimationClip;
                List<PengTrack> track = new List<PengTrack>();
                PengTrack enterTrack1 = new PengTrack(PengTrack.ExecTime.Enter, "OnEnter", 0, 0, null, true);
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
                track.Add(new PengTrack(PengTrack.ExecTime.Exit, "OnExit", 0, 0, null, true));
                track.Add(new PengTrack(PengTrack.ExecTime.Update, "Track", 3, 20, null, true));
                stateTrack.Add("Idle", track);


                int j = 0;
                for (int i = 0; i < statesLength.Count; i++)
                {
                    if(statesLength.ElementAt(i).Key != "Idle")
                    {
                        AnimatorState state = states.AddState(statesLength.ElementAt(i).Key, new Vector3(states.entryPosition.x + 200, states.entryPosition.y + 50 * (j + 1), 0));
                        AnimationClip clip = Resources.Load("Animations/" + actorID.ToString() + "/" + actorID + "@" + statesLength.ElementAt(i).Key) as AnimationClip;

                        List<PengTrack> track1 = new List<PengTrack>();
                        PengTrack enterTrack = new PengTrack(PengTrack.ExecTime.Enter, "OnEnter", 0, 0, null, true);
                        if (clip != null)
                        {
                            //如果能读取到片段，可以直接给enterTrack附加一个播放动画的节点，并和轨道执行连接起来
                            state.motion = clip;
                            clip.frameRate = globalFrameRate;
                            statesLength[statesLength.ElementAt(i).Key] = Mathf.FloorToInt(clip.length * globalFrameRate);
                        }
                        else
                        {
                            Debug.LogWarning("Actor" + actorID.ToString() + "的" + statesLength.ElementAt(i).Key + "状态未填入动画片段，请手动在状态中放入Clip。");
                        }
                        track1.Add(enterTrack);
                        track1.Add(new PengTrack(PengTrack.ExecTime.Exit, "OnExit", 0, 0, null, true));
                        track1.Add(new PengTrack(PengTrack.ExecTime.Update, "Track", 3, 20, null, true));
                        stateTrack.Add(statesLength.ElementAt(i).Key, track1);
                        j++;
                    }
                }

                //AssetDatabase.CreateAsset(anim, Application.dataPath + "/Resources/Animators/" + actorID.ToString() + "/Animator" + actorID.ToString() + ".controller");
                actorNew.GetComponent<Animator>().runtimeAnimatorController = anim;
                PengActor pa = actorNew.AddComponent<PengActor>();
                actorNew.AddComponent<CharacterController>();
                pa.actorID = actorID;
                actorNew.tag = "PengActor";
                actorNew.layer = LayerMask.NameToLayer("PengActor");
                actorNew.name = "Actor" + actorID.ToString();


                PengActorStateEditorWindow.SaveActorData(actorID, actorCamp, actorName, stateGroup, stateTrack, statesLength, statesLoop);

                if(!Directory.Exists(Application.dataPath + "/Resources/Actors/" + actorID.ToString()))
                {
                    Directory.CreateDirectory(Application.dataPath + "/Resources/Actors/" + actorID.ToString());
                }
                bool success = false;
                PrefabUtility.SaveAsPrefabAsset(actorNew, Application.dataPath + "/Resources/Actors/" + actorID.ToString() + "/Actor" + actorID.ToString() +".prefab", out success);
                Debug.Log(string.Format("Actor" + actorID.ToString() + "保存[{0}]", success ? "成功":"失败"));
                AssetDatabase.Refresh();
            }
            else
            {
                EditorUtility.DisplayDialog("警告", "没有放入角色模型，或不存在Animator组件，或没有填入角色状态，无法生成。", "ok");
            }

        }
        EditorGUILayout.EndVertical();
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
}
