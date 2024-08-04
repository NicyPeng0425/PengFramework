using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PengEditorGUIStyleViewer : EditorWindow
{
    /// <summary>
    /// 直接抄的：https://www.cnblogs.com/fengxing999/p/11096049.html
    /// 逼养的Unity，没个地儿看有哪些内置的GUIStyle
    /// </summary>
    
    private Vector2 scrollVector2 = Vector2.zero;
    private string search = "";

    [MenuItem("PengFramework/开发用：GUIStyle查看器")]
    public static void InitWindow()
    {
        EditorWindow.GetWindow(typeof(PengEditorGUIStyleViewer));
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        GUILayout.Space(30);
        search = EditorGUILayout.TextField("", search, "SearchTextField", GUILayout.MaxWidth(position.x / 3));
        GUILayout.Label("", "SearchCancelButtonEmpty");
        GUILayout.EndHorizontal();
        scrollVector2 = GUILayout.BeginScrollView(scrollVector2);
        foreach (GUIStyle style in GUI.skin.customStyles)
        {
            if (style.name.ToLower().Contains(search.ToLower()))
            {
                DrawStyleItem(style);
            }
        }
        GUILayout.EndScrollView();
    }

    void DrawStyleItem(GUIStyle style)
    {
        GUILayout.BeginHorizontal("box");
        GUILayout.Space(40);
        EditorGUILayout.SelectableLabel(style.name);
        GUILayout.FlexibleSpace();
        EditorGUILayout.SelectableLabel(style.name, style);
        GUILayout.Space(40);
        EditorGUILayout.SelectableLabel("", style, GUILayout.Height(40), GUILayout.Width(40));
        GUILayout.Space(50);
        if (GUILayout.Button("复制GUIStyle名字"))
        {
            TextEditor textEditor = new TextEditor();
            textEditor.text = style.name;
            textEditor.OnFocus();
            textEditor.Copy();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }
}
