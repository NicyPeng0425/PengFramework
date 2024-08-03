using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class PengActorGeneratorEditor : EditorWindow
{
    /// <summary>
    /// һ�����ɽ�ɫ��ţ�ƹ��ߡ�
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
        window.titleContent = new GUIContent("���ܽ�ɫ������");
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


        PengEditorMain.DrawPengFrameworkIcon("��ɫ������");
        GUILayout.Space(20);

        GUIStyle style = new GUIStyle("Box");
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        GUILayout.Box("��дע�⣺\n" +
            "һ����ɫģ����Ҫ��Animator�����\n" +
            "��������ǰ������ɫ����Ƭ�η�����Resources/Animations/��ɫID�����Խ�ɫID+@+״̬���ĸ�ʽ������\n" +
            "���磺Resources/Animations/100001/100001@Idle\n" +
            "������ɫ״̬�������Idle״̬��", style);

        GUILayout.Space(10);

        PengActorGenerator();

        EditorGUILayout.EndVertical();
    }

    public void PengActorGenerator()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("��ɫID��");
        actorID = EditorGUILayout.IntField(actorID, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("��ɫ���ƣ�");
        actorName = EditorGUILayout.TextField(actorName, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("��ɫ��Ӫ��");
        actorCamp = EditorGUILayout.IntField(actorCamp, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("��ɫģ�ͣ�");
        actorModel = (GameObject)EditorGUILayout.ObjectField(actorModel, typeof(GameObject), true, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        GUILayout.Label("��ɫ״̬��");
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30);
        actorStates = PengEditorMain.DrawStringListEditorGUI(actorStates);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();


        if (GUILayout.Button("һ�����ɽ�ɫ"))
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
                    EditorUtility.DisplayDialog("����", "��ɫ״̬û��Idle���޷����ɡ�", "ok");
                    return;
                }

                GameObject actorNew = GameObject.Instantiate(actorModel);

                //���Animator
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
                            Debug.LogWarning("Actor" + actorID.ToString() + "��" + actorStates[i] + "״̬δ���붯��Ƭ�Σ����ֶ����롣");
                        }
                        j++;
                    }
                }

                //����State�ļ�


                actorModel.GetComponent<Animator>().runtimeAnimatorController = actorAnim;


                bool success = false;
                PrefabUtility.SaveAsPrefabAsset(actorNew, Application.dataPath + "/Resources/Actors/" + actorID.ToString() + "/Actor" + actorID.ToString() +".prefab", out success);
                Debug.Log(string.Format("Actor" + actorID.ToString() + "����[{0}]", success ? "�ɹ�":"ʧ��"));
            }
            else
            {
                EditorUtility.DisplayDialog("����", "û�з����ɫģ�ͣ��򲻴���Animator�������û�������ɫ״̬���޷����ɡ�", "ok");
            }

        }
        EditorGUILayout.EndVertical();
    }


}
