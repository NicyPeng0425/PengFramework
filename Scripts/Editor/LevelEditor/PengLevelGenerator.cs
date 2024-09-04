using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using static PengActorGeneratorEditor;

public class PengLevelGenerator : EditorWindow
{
    public enum LevelTemplate
    {
        出生点,
        战斗点,
    }

    public int levelID;
    public string levelName = "关卡名称";
    public string info = "关卡说明";
    public LevelTemplate lvlTplt = LevelTemplate.出生点;

    [MenuItem("PengFramework/关卡生成器", false, 22)]
    static void Init()
    {
        PengLevelGenerator window = (PengLevelGenerator)EditorWindow.GetWindow(typeof(PengLevelGenerator));
        window.position = new Rect(120, 120, 400, 500);
        window.titleContent = new GUIContent("彭框架关卡生成器");
    }

    private void OnEnable()
    {
        levelID = 200001;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        PengEditorMain.DrawPengFrameworkIcon("关卡生成器");
        GUILayout.Space(20);

        GUIStyle style = new GUIStyle("Box");
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        GUILayout.Box("填写注意：\n" +
            "好像暂时没啥需要注意的。\n" +
            "空气墙请挂在Level的子物体里，并且附加到Level的Air Walls属性里。", style);
        GUILayout.Space(10);

        lvlTplt = (LevelTemplate)EditorGUILayout.EnumPopup(lvlTplt);

        switch (lvlTplt)
        {
            case LevelTemplate.出生点:
                GenerateStart();
                break;
            case LevelTemplate.战斗点:
                GenerateLevel();
                break;
        }
        EditorGUILayout.EndVertical();
    }

    private void GenerateStart()
    {
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

            List<PengLevelEditorNodes.PengLevelEditorNode> nodes = new List<PengLevelEditorNodes.PengLevelEditorNode>();
            nodes.Add(new PengLevelEditorNodes.LevelStart(new Vector2(20, 80), null, 1, "0|2:0", "", "", ""));
            nodes.Add(new PengLevelEditorNodes.GenerateActor(new Vector2(200, 80), null, 2, "0|3:0", "0|-1:-1,3:0", "0|-1:-1", "100001"));
            nodes.Add(new PengLevelEditorNodes.SetMainActor(new Vector2(380, 80), null, 3, "0|4:0", "", "0|2:0", ""));
            nodes.Add(new PengLevelEditorNodes.StartControl(new Vector2(560, 80), null, 4, "0|-1:-1", "", "", ""));

            PengLevelEditor.SaveLevelData(true, levelID, nodes);
            bool success = false;
            PrefabUtility.SaveAsPrefabAsset(lvl, Application.dataPath + "/Resources/Plot/" + levelID.ToString() + "/Level" + levelID.ToString() + ".prefab", out success);
            Debug.Log(string.Format("Level" + levelID.ToString() + "保存[{0}]", success ? "成功" : "失败"));
            DestroyImmediate(lvl);
            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndVertical();
    }

    private void GenerateLevel()
    {
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
        EditorGUILayout.EndVertical();
    }
}
