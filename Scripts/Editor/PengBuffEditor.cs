using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.IO;
using System;

public class PengBuffEditor : EditorWindow
{
    XmlDocument buffDoc;
    XmlElement root;
    Vector2 scrollPos = Vector2.zero;
    XmlElement editEle = null;
    [MenuItem("PengFramework/Buff编辑器", false, 5)]
    static void Init()
    {
        PengBuffEditor window = (PengBuffEditor)EditorWindow.GetWindow(typeof(PengBuffEditor));
        window.position = new Rect(150, 120, 800, 400);
        window.titleContent = new GUIContent("彭框架Buff编辑器");
        CreateBuffXML();
    }

    private void OnEnable()
    {
        buffDoc = ReadBuffData();
        root = buffDoc.DocumentElement;
    }

    private void OnGUI()
    {
        if (buffDoc == null)
            return;
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width - 10), GUILayout.Height(position.height - 10));

        EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
        if (GUILayout.Button("+", GUILayout.Width(50), GUILayout.Height(23)))
        {
            XmlElement newEle = buffDoc.CreateElement("Buff");
            int id = 1000;
            if (root.ChildNodes.Count > 0)
            {
                bool same = true;
                while (same)
                {
                    same = false;
                    for (int i = 0; i < root.ChildNodes.Count; i++)
                    {
                        XmlElement ele = root.ChildNodes[i] as XmlElement;
                        if (id == int.Parse(ele.GetAttribute("ID")))
                        {
                            same = true;
                            id++;
                            break;
                        }
                    }
                }
            }
            newEle.SetAttribute("ID", id.ToString());
            newEle.SetAttribute("Name", "NewBuff");
            newEle.SetAttribute("Description", "Buff描述");
            newEle.SetAttribute("IsDebuff", "0");
            newEle.SetAttribute("ExistTime", "5");
            newEle.SetAttribute("RemoveCondition", PengBuff.RemoveConditionType.Time.ToString());
            newEle.SetAttribute("StackLimit", "1");
            newEle.SetAttribute("RemoveOnceForAll", "1");
            newEle.SetAttribute("AttackPowerValue", "0");
            newEle.SetAttribute("AttackPowerPercent", "0");
            newEle.SetAttribute("DefendPowerValue", "0");
            newEle.SetAttribute("DefendPowerPercent", "0");
            newEle.SetAttribute("CriticalRateValue", "0");
            newEle.SetAttribute("CriticalDamageRatioValue", "0");
            newEle.SetAttribute("NotEffectedByGravity", "0");
            newEle.SetAttribute("Unbreakable", "0");
            newEle.SetAttribute("Invicible", "0");

            root.AppendChild(newEle);
        }

        if (GUILayout.Button("保存", GUILayout.Width(50), GUILayout.Height(23)))
        {
            buffDoc.Save(Application.dataPath + "/Resources/BuffData/Universal/BuffTable.xml");
            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width - 10), GUILayout.Height(position.height - 35));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("ID", GUILayout.Width(20));
        GUILayout.Space(47);

        GUILayout.Label("名字", GUILayout.Width(30));
        GUILayout.Space(77);

        GUILayout.Label("描述", GUILayout.Width(30));
        GUILayout.Space(127);
        EditorGUILayout.EndHorizontal();
        GUILayout.Box("", GUILayout.Width(400), GUILayout.Height(2));
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(420), GUILayout.Height(position.height - 35));
        
        if (root.ChildNodes.Count > 0)
        {
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlElement ele = root.ChildNodes[i] as XmlElement;
                EditorGUILayout.BeginVertical(GUILayout.Height(25));
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Label(ele.GetAttribute("ID"), GUILayout.Width(62));
                GUILayout.Space(5);

                GUILayout.Label(ele.GetAttribute("Name"), GUILayout.Width(102));
                GUILayout.Space(5);

                GUILayout.Label(ele.GetAttribute("Description"), GUILayout.Width(152));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("编辑"))
                {
                    editEle = ele;
                }

                //信息
                EditorGUILayout.EndHorizontal();
                GUILayout.Box("", GUILayout.Width(400), GUILayout.Height(2));
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();

        if (editEle != null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("ID", GUILayout.Width(20));
            GUILayout.Space(3);
            int origin = int.Parse(editEle.GetAttribute("ID"));
            int newID = origin;
            newID = EditorGUILayout.IntField(newID, GUILayout.Width(100));
            if (newID != origin)
            {
                if (root.ChildNodes.Count > 0)
                {
                    bool same = true;
                    while (same)
                    {
                        same = false;
                        for (int i = 0; i < root.ChildNodes.Count; i++)
                        {
                            XmlElement ele = root.ChildNodes[i] as XmlElement;
                            if (newID == int.Parse(ele.GetAttribute("ID")))
                            {
                                same = true;
                                newID++;
                                break;
                            }
                        }
                    }
                }
                editEle.SetAttribute("ID", newID.ToString());
            }
            GUILayout.Space(3);
            GUILayout.Label("名字", GUILayout.Width(35));
            GUILayout.Space(3);
            string name = editEle.GetAttribute("Name");
            name = EditorGUILayout.TextField(name, GUILayout.Width(100));
            editEle.SetAttribute("Name", name);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("描述", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            string descri = editEle.GetAttribute("Description");
            descri = EditorGUILayout.TextArea(descri, GUILayout.Height(80));
            editEle.SetAttribute("Description", descri);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("移除条件", GUILayout.Width(50));
            GUILayout.Space(3);
            PengBuff.RemoveConditionType rct = (PengBuff.RemoveConditionType)Enum.Parse(typeof(PengBuff.RemoveConditionType), editEle.GetAttribute("RemoveCondition"));
            rct = (PengBuff.RemoveConditionType)EditorGUILayout.EnumPopup(rct, GUILayout.Width(80));
            editEle.SetAttribute("RemoveCondition", rct.ToString());
            GUILayout.Space(3);
            GUILayout.Label("移除时移除所有层", GUILayout.Width(100));
            GUILayout.Space(3);
            bool removeAll = int.Parse(editEle.GetAttribute("RemoveOnceForAll")) > 0;
            removeAll = EditorGUILayout.Toggle(removeAll, GUILayout.Width(30));
            editEle.SetAttribute("RemoveOnceForAll", removeAll ? "1" : "0");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("最大层数", GUILayout.Width(50));
            GUILayout.Space(3);
            int stack = int.Parse(editEle.GetAttribute("StackLimit"));
            stack = EditorGUILayout.IntField(stack, GUILayout.Width(80));
            editEle.SetAttribute("StackLimit", stack.ToString());
            GUILayout.Space(3);
            GUILayout.Label("持续时间", GUILayout.Width(100));
            GUILayout.Space(3);
            float exist = float.Parse(editEle.GetAttribute("ExistTime"));
            exist = EditorGUILayout.FloatField(exist, GUILayout.Width(30));
            editEle.SetAttribute("ExistTime", exist.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("是Debuff", GUILayout.Width(60));
            GUILayout.Space(3);
            bool isDebuff = int.Parse(editEle.GetAttribute("IsDebuff")) > 0;
            isDebuff = EditorGUILayout.Toggle(isDebuff, GUILayout.Width(70));
            editEle.SetAttribute("IsDebuff", isDebuff ? "1" : "0");
            GUILayout.Space(3);

            GUILayout.Label("不受重力控制", GUILayout.Width(80));
            GUILayout.Space(3);
            bool notEffectedByGravity = int.Parse(editEle.GetAttribute("NotEffectedByGravity")) > 0;
            notEffectedByGravity = EditorGUILayout.Toggle(notEffectedByGravity, GUILayout.Width(50));
            editEle.SetAttribute("NotEffectedByGravity", notEffectedByGravity ? "1" : "0");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("无敌", GUILayout.Width(60));
            GUILayout.Space(3);
            bool invicible = int.Parse(editEle.GetAttribute("Invicible")) > 0;
            invicible = EditorGUILayout.Toggle(invicible, GUILayout.Width(70));
            editEle.SetAttribute("Invicible", invicible ? "1" : "0");
            GUILayout.Space(3);

            GUILayout.Label("霸体", GUILayout.Width(80));
            GUILayout.Space(3);
            bool unbreakable = int.Parse(editEle.GetAttribute("Unbreakable")) > 0;
            unbreakable = EditorGUILayout.Toggle(unbreakable, GUILayout.Width(50));
            editEle.SetAttribute("Unbreakable", unbreakable ? "1" : "0");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("攻击力值", GUILayout.Width(70));
            GUILayout.Space(3);
            float attackPV = float.Parse(editEle.GetAttribute("AttackPowerValue"));
            attackPV = EditorGUILayout.FloatField(attackPV, GUILayout.Width(60));
            editEle.SetAttribute("AttackPowerValue", attackPV.ToString());
            GUILayout.Space(3);

            GUILayout.Label("攻击力百分比", GUILayout.Width(90));
            GUILayout.Space(3);
            float attackPP = float.Parse(editEle.GetAttribute("AttackPowerPercent"));
            attackPP = EditorGUILayout.FloatField(attackPP, GUILayout.Width(40));
            editEle.SetAttribute("AttackPowerPercent", attackPP.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("防御力值", GUILayout.Width(70));
            GUILayout.Space(3);
            float dpv = float.Parse(editEle.GetAttribute("DefendPowerValue"));
            dpv = EditorGUILayout.FloatField(dpv, GUILayout.Width(60));
            editEle.SetAttribute("DefendPowerValue", dpv.ToString());
            GUILayout.Space(3);

            GUILayout.Label("防御力百分比", GUILayout.Width(90));
            GUILayout.Space(3);
            float dpp = float.Parse(editEle.GetAttribute("DefendPowerPercent"));
            dpp = EditorGUILayout.FloatField(dpp, GUILayout.Width(40));
            editEle.SetAttribute("DefendPowerPercent", dpp.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("暴击率增加", GUILayout.Width(70));
            GUILayout.Space(3);
            float crv = float.Parse(editEle.GetAttribute("CriticalRateValue"));
            crv = EditorGUILayout.FloatField(crv, GUILayout.Width(60));
            editEle.SetAttribute("CriticalRateValue", crv.ToString());
            GUILayout.Space(3);

            GUILayout.Label("暴伤增加", GUILayout.Width(90));
            GUILayout.Space(3);
            float cdrv = float.Parse(editEle.GetAttribute("CriticalDamageRatioValue"));
            cdrv = EditorGUILayout.FloatField(cdrv, GUILayout.Width(40));
            editEle.SetAttribute("CriticalDamageRatioValue", cdrv.ToString());
            EditorGUILayout.EndHorizontal();
        }
        //面板
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    static void CreateBuffXML()
    {
        if (!Directory.Exists(Application.dataPath + "/Resources/BuffData/Universal"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources/BuffData/Universal");
        }

        if (!File.Exists(Application.dataPath + "/Resources/BuffData/Universal/BuffTable.xml"))
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", "");
            XmlElement root = doc.CreateElement("Root");
            doc.AppendChild(root);
            doc.Save(Application.dataPath + "/Resources/BuffData/Universal/BuffTable.xml");
            AssetDatabase.Refresh();
        }
    }

    static XmlDocument ReadBuffData()
    {
        TextAsset ta = Resources.Load("BuffData/Universal/BuffTable") as TextAsset;
        XmlDocument result = new XmlDocument();
        if (ta == null)
            return null;

        result.LoadXml(ta.text);
        return result;
    }
}
