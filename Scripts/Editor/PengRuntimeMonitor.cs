using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PengRuntimeMonitor : EditorWindow
{
    public PengGameManager game;
    public Vector2 scroll;
    public Vector2 scrollLevel;
    public float actorWidth = 350;
    public float levelWidth = 250;
    [MenuItem("PengFramework/运行时数据监控", false, 45)]
    static void Init()
    {
        PengRuntimeMonitor window = (PengRuntimeMonitor)EditorWindow.GetWindow(typeof(PengRuntimeMonitor));
        window.position = new Rect(100, 100, 600, 700);
        window.minSize = new Vector2(600, 700);
        window.titleContent = new GUIContent("彭框架运行时数据监控");
    }

    private void OnEnable()
    {
        
    }

    private void OnGUI()
    {
        PengEditorMain.DrawPengFrameworkIcon("运行时数据监控");
        if (!EditorApplication.isPlaying)
        {
            Rect helpBox = new Rect(5, 60, position.width-5, 50);
            EditorGUI.HelpBox(helpBox, "仅在运行时下可用", MessageType.Warning);
            return;
        }
        if (!EditorApplication.isPlaying)
        {
            Rect helpBox = new Rect(5, 60, position.width - 5, 50);
            EditorGUI.HelpBox(helpBox, "仅在运行时下可用", MessageType.Warning);
            return;
        }

        if (game == null)
        {
            game = GameObject.FindWithTag("PengGameManager").GetComponent<PengGameManager>();
        }

        EditorGUILayout.BeginHorizontal();
        if (game.level.levels.Count > 0)
        {
            GUIStyle buffStyle = new GUIStyle("AppToolbar");
            GUIStyle bbKeyStyle = new GUIStyle("ContentToolbar");
            GUIStyle bbValueStyle = new GUIStyle("ContentToolbar");
            bbKeyStyle.alignment = TextAnchor.MiddleLeft;
            bbValueStyle.alignment = TextAnchor.MiddleLeft;

            EditorGUILayout.BeginVertical(GUILayout.Width(levelWidth), GUILayout.Height(position.height - 10));
            GUILayout.Space(20);
            scrollLevel = EditorGUILayout.BeginScrollView(scrollLevel, GUILayout.Width(levelWidth), GUILayout.Height(position.height - 70));
            for (int i = 0; i < game.level.levels.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUILayout.Height(150));

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("关卡ID：", GUILayout.Width(110));
                GUILayout.Label(game.level.levels[i].levelID.ToString(), GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("运行中节点ID：", GUILayout.Width(110));
                GUILayout.Label(game.level.levels[i].current.ID.ToString(), GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("运行中功能：", GUILayout.Width(110));
                GUILayout.Label(game.level.levels[i].current.type.ToString(), GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("选中关卡" + game.level.levels[i].levelID.ToString()))
                {
                    Selection.activeGameObject = game.level.levels[i].gameObject;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        if (game.actors.Count > 0)
        {
            GUIStyle buffStyle = new GUIStyle("AppToolbar");
            GUIStyle bbKeyStyle = new GUIStyle("ContentToolbar");
            GUIStyle bbValueStyle = new GUIStyle("ContentToolbar");
            bbKeyStyle.alignment = TextAnchor.MiddleLeft;
            bbValueStyle.alignment = TextAnchor.MiddleLeft;

            EditorGUILayout.BeginVertical(GUILayout.Width(position.width - levelWidth - 10), GUILayout.Height(position.height - 10));
            GUILayout.Space(20);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(position.width - levelWidth - 10), GUILayout.Height(position.height - 70));
            for (int i = 0; i < game.actors.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUILayout.Height(200));

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("角色ID：", GUILayout.Width(80));
                GUILayout.Label(game.actors[i].actorID.ToString(), GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                GUILayout.Label("角色阵营：", GUILayout.Width(80));
                GUILayout.Label(game.actors[i].actorCamp.ToString(), GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("角色名称：", GUILayout.Width(80));
                GUILayout.Label(game.actors[i].actorName, GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (game.actors[i].input.aiCtrl)
                {
                    if (GUILayout.Button("设为主控", GUILayout.Width(80)))
                    {
                        game.SetMainActor(game.actors[i], true);
                    }
                }
                else
                {
                    if (game.actors[i].input.active)
                    {
                        GUILayout.Label("下次决策：" + (game.actors[i].input.decideCD - game.actors[i].input.decideCDTimeCount).ToString() + "s");
                    }
                    else
                    {
                        GUILayout.Label("AI未激活");
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("角色状态：", GUILayout.Width(80));
                GUILayout.Label(game.actors[i].currentName, GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                GUILayout.Label("帧数信息：", GUILayout.Width(80));
                GUILayout.Label(game.actors[i].currentStateFrame.ToString() + "/" + game.actors[i].currentStateLength.ToString(), GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();
                if (game.actors[i].bb.intBB.Count > 0)
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("整型黑板：", GUILayout.Width(80));
                    for (int j = 0; j < game.actors[i].bb.intBB.Count; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(game.actors[i].bb.intBB.ElementAt(j).Key, bbKeyStyle, GUILayout.Width(80));
                        GUILayout.Label(game.actors[i].bb.intBB.ElementAt(j).Value.ToString(), bbValueStyle, GUILayout.Width(80));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
                if (game.actors[i].bb.floatBB.Count > 0)
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("浮点黑板：", GUILayout.Width(80));
                    for (int j = 0; j < game.actors[i].bb.floatBB.Count; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(game.actors[i].bb.floatBB.ElementAt(j).Key, bbKeyStyle, GUILayout.Width(80));
                        GUILayout.Label(game.actors[i].bb.floatBB.ElementAt(j).Value.ToString(), bbValueStyle, GUILayout.Width(80));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
                if (game.actors[i].bb.stringBB.Count > 0)
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("字符串黑板：", GUILayout.Width(80));
                    for (int j = 0; j < game.actors[i].bb.stringBB.Count; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(game.actors[i].bb.stringBB.ElementAt(j).Key, bbKeyStyle, GUILayout.Width(80));
                        GUILayout.Label(game.actors[i].bb.stringBB.ElementAt(j).Value.ToString(), bbValueStyle, GUILayout.Width(80));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
                if (game.actors[i].bb.boolBB.Count > 0)
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("布尔黑板：", GUILayout.Width(80));
                    for (int j = 0; j < game.actors[i].bb.boolBB.Count; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(game.actors[i].bb.boolBB.ElementAt(j).Key, bbKeyStyle, GUILayout.Width(80));
                        GUILayout.Label(game.actors[i].bb.boolBB.ElementAt(j).Value.ToString(), bbValueStyle, GUILayout.Width(80));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
                GUILayout.Label("角色Buff：", GUILayout.Width(80));
                string buffList = "";
                if (game.actors[i].buff.buffs.Count > 0)
                {
                    for (int j = 0; j < game.actors[i].buff.buffs.Count; j++)
                    {
                        buffList += game.actors[i].buff.buffs[j].ID.ToString();
                        if (j != game.actors[i].buff.buffs.Count - 1)
                        {
                            buffList += ",";
                        }
                    }
                }
                GUILayout.Label(buffList, buffStyle, GUILayout.Width(80));
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void OnInspectorUpdate()
    {
        this.Repaint();
    }
}
