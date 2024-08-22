using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PengActorAttributesEditor : EditorWindow
{
    PengActorStateEditorWindow master;
    public static PengActorAttributesEditor Init(PengActorStateEditorWindow master)
    {
        PengActorAttributesEditor window = (PengActorAttributesEditor)EditorWindow.GetWindow(typeof(PengActorAttributesEditor));
        window.position = new Rect(100, 100, 400, 600);
        window.titleContent = new GUIContent("角色属性编辑器");
        window.master = master;
        return window;
    }

    private void OnDisable()
    {
        master.attrEditor = null;
    }

    private void OnLostFocus()
    {
        master.attrEditor = null;
        this.Close();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("角色ID：" + master.currentActorID.ToString(), GUILayout.Width(250));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("角色阵营：" + master.currentActorCamp.ToString(), GUILayout.Width(250));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("基础最大生命值：", GUILayout.Width(150));
        master.currentActorMaxHP = EditorGUILayout.FloatField(master.currentActorMaxHP, GUILayout.Width(150));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("基础攻击力：", GUILayout.Width(150));
        master.currentActorAttackPower = EditorGUILayout.FloatField(master.currentActorAttackPower, GUILayout.Width(150));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("基础防御力：", GUILayout.Width(150));
        master.currentActorDefendPower = EditorGUILayout.FloatField(master.currentActorDefendPower, GUILayout.Width(150));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("基础暴击率：", GUILayout.Width(150));
        master.currentActorCriticalRate = EditorGUILayout.FloatField(master.currentActorCriticalRate, GUILayout.Width(150));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("基础暴击伤害：", GUILayout.Width(150));
        master.currentActorCriticalDamageRatio = EditorGUILayout.FloatField(master.currentActorCriticalDamageRatio, GUILayout.Width(150));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("基础抗打断：", GUILayout.Width(150));
        master.currentActorResist = EditorGUILayout.FloatField(master.currentActorResist, GUILayout.Width(150));
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();
    }
}
