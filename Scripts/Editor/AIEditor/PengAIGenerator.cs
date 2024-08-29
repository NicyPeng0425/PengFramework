using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class PengAIGenerator : EditorWindow
{
    public enum AITemplate
    {
        新角色,
        复制角色,
    }

    public int actorID;
    public AITemplate aiTplt = AITemplate.新角色;

    [MenuItem("PengFramework/AI生成器", false, 34)]
    static void Init()
    {
        PengAIGenerator window = (PengAIGenerator)GetWindow(typeof(PengAIGenerator));
        window.position = new Rect(150, 150, 300, 300);
        window.minSize = new Vector2(300, 300);
        window.maxSize = new Vector2(500, 500);
        window.titleContent = new GUIContent("彭框架AI生成器");
    }

    private void OnEnable()
    {
        actorID = 100001;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        PengEditorMain.DrawPengFrameworkIcon("AI生成器");
        GUILayout.Space(20);

        GUIStyle style = new GUIStyle("Box");
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        GUILayout.Box("填写注意：\n" +
            "好像暂时没啥需要注意的。\n", style);
        GUILayout.Space(10);

        aiTplt = (AITemplate)EditorGUILayout.EnumPopup(aiTplt);

        switch (aiTplt)
        {
            case AITemplate.新角色:
                GenerateNew();
                break;
            case AITemplate.复制角色:
                CopyActor();
                break;
        }
        EditorGUILayout.EndVertical();
    }

    private void GenerateNew()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("角色ID：");
        actorID = EditorGUILayout.IntField(actorID, GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("一键生成AI"))
        {
            if (File.Exists(Application.dataPath + "/Resources/AIs/" + actorID.ToString() + "/" + actorID.ToString() + ".xml"))
            {
                EditorUtility.DisplayDialog("警告", "已存在AI数据！", "ok");
                EditorGUILayout.EndVertical();
                return;
            }
            List<PengAIEditorNode.PengAIEditorNode> nodes = new List<PengAIEditorNode.PengAIEditorNode>();

            nodes.Add(new PengAIEditorNode.EventDecide(new Vector2(270, 40), null, 0, "0:-1", ""));

            PengAIEditor.SaveActorAIData(true, actorID, nodes);
            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndVertical();
    }

    private void CopyActor()
    {/*
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("关卡ID：");
        levelID = EditorGUILayout.IntField(levelID, GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("关卡名称：");
        levelName = EditorGUILayout.TextField(levelName, GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("关卡说明：");
        info = EditorGUILayout.TextArea(info, GUILayout.Width(300), GUILayout.Height(80));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("一键生成关卡"))
        {
            if (File.Exists(Application.dataPath + "/Resources/Plot/" + levelID.ToString() + "/" + levelID.ToString() + ".xml"))
            {
                EditorUtility.DisplayDialog("警告", "已存在关卡数据！", "ok");
                EditorGUILayout.EndVertical();
                return;
            }

            GameObject lvl = new GameObject();
            PengLevel lv = lvl.AddComponent<PengLevel>();
            lvl.tag = "PengLevel";
            lvl.name = "Level" + levelID.ToString();
            lv.levelID = levelID;
            lv.levelName = levelName;
            lv.info = info;
            if (!Directory.Exists(Application.dataPath + "/Resources/Plot/" + levelID.ToString()))
            {
                Directory.CreateDirectory(Application.dataPath + "/Resources/Plot/" + levelID.ToString());
            }
            string path = Application.dataPath + "/PengFramework/Prefab/Airwall.prefab";

            GameObject airwall1 = PrefabUtility.LoadPrefabContents(path);
            airwall1.transform.position = new Vector3(0, 0, -3);
            airwall1.transform.localScale = new Vector3(6, 6, 0.5f);

            GameObject airwall2 = PrefabUtility.LoadPrefabContents(path);
            airwall2.transform.position = new Vector3(3, 0, 0);
            airwall2.transform.localScale = new Vector3(0.5f, 6, 6);

            GameObject airwall3 = PrefabUtility.LoadPrefabContents(path);
            airwall3.transform.position = new Vector3(-3, 0, 0);
            airwall3.transform.localScale = new Vector3(0.5f, 6, 6);

            GameObject airwall4 = PrefabUtility.LoadPrefabContents(path);
            airwall4.transform.position = new Vector3(0, 0, 3);
            airwall4.transform.localScale = new Vector3(6, 6, 0.5f);

            airwall1.transform.SetParent(lvl.transform);
            airwall2.transform.SetParent(lvl.transform);
            airwall3.transform.SetParent(lvl.transform);
            airwall4.transform.SetParent(lvl.transform);

            List<PengLevelEditorNodes.PengLevelEditorNode> nodes = new List<PengLevelEditorNodes.PengLevelEditorNode>();
            nodes.Add(new PengLevelEditorNodes.LevelStart(new Vector2(20, 80), null, 1, "0|2:0", "", "", ""));
            nodes.Add(new PengLevelEditorNodes.CloseAirWall(new Vector2(200, 80), null, 2, "0|3:0", "", "", ""));
            nodes.Add(new PengLevelEditorNodes.GenerateEnemy(new Vector2(380, 80), null, 3, "0|4:0", "", "", "0;"));
            nodes.Add(new PengLevelEditorNodes.TriggerWaitArrival(new Vector2(560, 80), null, 4, "0|5:0", "", "0|-1:-1;1|-1:-1;2|-1:-1", "2;(0,0,0);(4,2,4)"));
            nodes.Add(new PengLevelEditorNodes.ActiveActor(new Vector2(740, 80), null, 5, "0|6:0", "", "", ""));
            nodes.Add(new PengLevelEditorNodes.SetAirWall(new Vector2(920, 80), null, 6, "0|7:0", "", "", ""));
            nodes.Add(new PengLevelEditorNodes.TriggerWaitEnemyDie(new Vector2(1100, 80), null, 7, "0|8:0", "", "", ""));
            nodes.Add(new PengLevelEditorNodes.CloseAirWall(new Vector2(1280, 80), null, 8, "0|-1:-1", "", "", ""));

            PengLevelEditor.SaveLevelData(true, levelID, nodes);
            bool success = false;
            PrefabUtility.SaveAsPrefabAsset(lvl, Application.dataPath + "/Resources/Plot/" + levelID.ToString() + "/Level" + levelID.ToString() + ".prefab", out success);
            Debug.Log(string.Format("Level" + levelID.ToString() + "保存[{0}]", success ? "成功" : "失败"));
            DestroyImmediate(lvl);
            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndVertical();*/
    }
}
