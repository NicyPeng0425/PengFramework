using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class PengOtherInfoEditor : EditorWindow
{
    public PengActorStateEditorWindow master;
    public int index;
    public static PengOtherInfoEditor Init(PengActorStateEditorWindow master, int index)
    {
        PengOtherInfoEditor window = (PengOtherInfoEditor)EditorWindow.GetWindow(typeof(PengOtherInfoEditor));
        window.position = new Rect(100, 100, 300, 300);
        window.titleContent = new GUIContent("编辑备注");
        window.master = master;
        window.index = index;
        return window;
    }

    private void OnEnable()
    {
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        master.tracks[index].otherInfo = EditorGUILayout.TextArea(master.tracks[index].otherInfo, GUILayout.Width(position.width), GUILayout.Height(position.height));
        master.Repaint();
        EditorGUILayout.EndVertical();
    }
}
