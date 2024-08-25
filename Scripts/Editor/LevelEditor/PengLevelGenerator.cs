using Meryel.UnityCodeAssist.YamlDotNet.Core;
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
                GenerateLevel();
                break;
        }
        EditorGUILayout.EndVertical();
    }

    private void GenerateLevel()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("关卡ID：");
        levelID = EditorGUILayout.IntField(levelID, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("关卡名称：");
        levelName = EditorGUILayout.TextField(levelName, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("关卡说明：");
        info = EditorGUILayout.TextArea(info, GUILayout.Width(200), GUILayout.Height(80));
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

            List<PengLevelEditorNode> nodes = new List<PengLevelEditorNode>();
            nodes.Add(new LevelStart(new Vector2(20, 80), null, 1, "0|2:0", "", "", ""));
            nodes.Add(new GenerateActor(new Vector2(200, 80), null, 2, "0|3:0", "0|-1:-1,3:0", "0|-1:-1", "100001"));
            nodes.Add(new SetMainActor(new Vector2(380, 80), null, 3, "0|-1:-1", "", "0|2:0", ""));

            PengLevelEditor.SaveLevelData(true, levelID, nodes);
            bool success = false;
            PrefabUtility.SaveAsPrefabAsset(lvl, Application.dataPath + "/Resources/Plot/" + levelID.ToString() + "/Level" + levelID.ToString() + ".prefab", out success);
            Debug.Log(string.Format("/Level" + levelID.ToString() + "保存[{0}]", success ? "成功" : "失败"));
            DestroyImmediate(lvl);
            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndVertical();
    }
}
