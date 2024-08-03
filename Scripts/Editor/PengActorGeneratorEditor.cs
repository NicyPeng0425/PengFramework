using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class PengActorGeneratorEditor : EditorWindow
{
    /// <summary>
    /// 一键生成角色的牛逼工具。
    /// </summary>
    /// 

    int actorID;
    string actorName;
    int actorCamp;
    List<string> actorStates = new List<string>();
    GameObject actorModel = null;

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
        actorStates.Add("Idle");
        actorStates.Add("Move");
        actorStates.Add("Dead");
        actorStates.Add("Attack1");
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
            "三、角色状态里必须有Idle状态。", style);

        GUILayout.Space(10);

        PengActorGenerator();

        EditorGUILayout.EndVertical();
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
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30);
        actorStates = PengEditorMain.DrawStringListEditorGUI(actorStates);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();


        if (GUILayout.Button("一键生成角色"))
        {
            if (actorModel != null && actorModel.GetComponent<Animator>() && actorStates.Count > 0)
            {
                bool hasIdle = false;
                for (int i = 0; i <= actorStates.Count; i++)
                {
                    if (actorStates[i] == "Idle")
                    {
                        hasIdle = true;
                    }
                }

                if (!hasIdle)
                {
                    EditorUtility.DisplayDialog("警告", "角色状态没有Idle，无法生成。", "ok");
                    return;
                }

                GameObject actorNew = GameObject.Instantiate(actorModel);

                //添加Animator
                AnimatorController actorAnim = AnimatorController.CreateAnimatorControllerAtPath(Application.dataPath + "/Resources/Animators/"+actorID.ToString()+"/Animator" + actorID.ToString());
                AnimatorControllerLayer layer = actorAnim.layers[0];
                AnimatorStateMachine states = layer.stateMachine;

                AnimatorState idleState = states.AddState("Idle", new Vector3(states.entryPosition.x + 200, states.entryPosition.y, 0));

                int j = 0;
                for (int i = 0; i <= actorStates.Count; i++)
                {
                    if (actorStates[i] != "Idle")
                    {
                        AnimatorState state = states.AddState(actorStates[i], new Vector3(states.entryPosition.x + 200, states.entryPosition.y - 100 * (j + 1), 0));
                        AnimationClip clip = Resources.Load("Animations/" + actorID.ToString() + "/" + actorID + "@" + actorStates[i]) as AnimationClip;
                        if(clip != null)
                        {
                            state.motion = clip;
                        }
                        else
                        {
                            Debug.LogWarning("Actor" + actorID.ToString() + "的" + actorStates[i] + "状态未填入动画片段，请手动放入。");
                        }
                        j++;
                    }
                }

                //生成State文件


                actorModel.GetComponent<Animator>().runtimeAnimatorController = actorAnim;


                bool success = false;
                PrefabUtility.SaveAsPrefabAsset(actorNew, Application.dataPath + "/Resources/Actors/" + actorID.ToString() + "/Actor" + actorID.ToString() +".prefab", out success);
                Debug.Log(string.Format("Actor" + actorID.ToString() + "保存[{0}]", success ? "成功":"失败"));
            }
            else
            {
                EditorUtility.DisplayDialog("警告", "没有放入角色模型，或不存在Animator组件，或没有填入角色状态，无法生成。", "ok");
            }

        }
        EditorGUILayout.EndVertical();
    }


}
