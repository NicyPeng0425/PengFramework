using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using System.ComponentModel;
using PengLevelRuntimeFunction;
using UnityEditor;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using PengLevelEditorNodes;

namespace PengAIEditorNode
{
    public enum PengAIEditorNodeType
    {
        Event,
        Condition,
        Action
    }

    public class PengAIEditorNode
    {
        public string name = "默认";
        public string meaning = "暂无说明";
        public PengAIScript.AIScriptType type;
        public PengAIEditor editor;
        public int nodeID;
        public Vector2 pos;
        Rect m_rect;
        public Rect rect
        {
            get
            {
                if (editor != null)
                {
                    return new Rect(pos.x, pos.y, m_rect.width * editor.currentScale, m_rect.height * editor.currentScale);
                }
                else
                {
                    return m_rect;
                }
            }
            set { m_rect = value; }
        }
        private bool isDragged;
        private bool m_isSelected;
        public bool isSelected
        {
            get { return m_isSelected; }
            set { m_isSelected = value; /*OnSelectedChange(value);*/ }
        }
        public PengAIEditorNodeType nodeType;
        public PengAIEditorNodeConnection inPoint;
        public PengAIEditorNodeConnection[] outPoints;
        public Dictionary<int, int> outID = new Dictionary<int, int>();

        public virtual void Draw()
        {
            Rect rectScale = new Rect();
            if (GetCatalog(nodeType) != "Event")
            {
                int i = outPoints.Length;
                rect = new Rect(rect.x, rect.y, 40 + i * 40, rect.height);
                rectScale = new Rect(rect.x, rect.y, rect.width * editor.currentScale, rect.height * editor.currentScale);
            }
            else
            {
                rect = new Rect(rect.x, rect.y, rect.width, 21);
                rectScale = new Rect(rect.x, rect.y, rect.width * editor.currentScale, rect.height * editor.currentScale);
            }
            switch (nodeType)
            {
                case PengAIEditorNodeType.Event:
                    GUIStyle style = new GUIStyle("flow node 5" + (isSelected ? " on" : ""));
                    style.fontStyle = FontStyle.Bold;
                    GUI.Box(rectScale, name, style);
                    if (outPoints.Length > 0)
                    {
                        for (int i = 0; i < outPoints.Length; i++)
                        {
                            outPoints[i].Draw(rectScale, outPoints.Length, i);
                        }
                    }
                    break;
                case PengAIEditorNodeType.Action:
                    GUIStyle style1 = new GUIStyle("flow node 0" + (isSelected ? " on" : ""));
                    style1.fontStyle = FontStyle.Bold;
                    GUI.Box(rectScale, name, style1);
                    inPoint.Draw(rectScale, 1, 0);
                    if (outPoints.Length > 0)
                    {
                        for (int i = 0; i < outPoints.Length; i++)
                        {
                            outPoints[i].Draw(rectScale, outPoints.Length, i);
                        }
                    }
                    break;
                case PengAIEditorNodeType.Condition:
                    GUIStyle style4 = new GUIStyle("flow node 2" + (isSelected ? " on" : ""));
                    style4.fontStyle = FontStyle.Bold;
                    GUI.Box(rectScale, name, style4);
                    inPoint.Draw(rectScale, 1, 0);
                    if (outPoints.Length > 0)
                    {
                        for (int i = 0; i < outPoints.Length; i++)
                        {
                            outPoints[i].Draw(rectScale, outPoints.Length, i);
                        }
                    }
                    break;
            }
            GUIStyle styleNum = new GUIStyle("dockHeader");
            styleNum.fontStyle = FontStyle.Bold;
            styleNum.fontSize = 10;
            Rect num = new Rect(rectScale.x, rectScale.y - 14, 30, 14);
            GUI.Box(num, nodeID.ToString(), styleNum);
        }

        public virtual void DrawSideBar(Rect sidebar)
        {
            DrawMeaning(sidebar);
        }

        public void DrawMeaning(Rect sidebar)
        {
            if (isSelected)
            {
                Rect box = new Rect(sidebar.x + 10, sidebar.y + 10, sidebar.width - 20, 40);
                GUI.Box(box, meaning);
            }
        }

        public void ProcessDrag(Vector2 change)
        {
            if (editor != null)
            {
                pos += change / editor.currentScale;
            }
            else
            {
                pos += change;
            }

            rect = new Rect(pos.x, pos.y, rect.width, rect.height);
        }

        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (editor.nodeMapRect.Contains(e.mousePosition))
                        {
                            if (rect.Contains(e.mousePosition))
                            {
                                isDragged = true;
                                isSelected = true;
                                GUI.changed = true;
                            }
                            else
                            {
                                isSelected = false;
                                GUI.changed = true;
                            }
                        }

                    }
                    if (e.button == 1)
                    {
                        if (editor.nodeMapRect.Contains(e.mousePosition))
                        {
                            if (rect.Contains(e.mousePosition) && isSelected)
                            {
                                RightMouseMenu();
                                e.Use();
                                GUI.changed = true;
                            }
                        }
                    }
                    break;
                case EventType.MouseUp:
                    isDragged = false;
                    break;
                case EventType.MouseDrag:
                    if (editor.nodeMapRect.Contains(e.mousePosition))
                    {
                        if (e.button == 0 && isDragged)
                        {
                            ProcessDrag(e.delta);
                            e.Use();
                            return true;
                        }
                    }
                    else
                    {
                        isDragged = false;
                    }
                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Delete && isSelected && type != PengAIScript.AIScriptType.EventDecide)
                    {
                        editor.ProcessRemoveNode(this);
                        e.Use();
                        GUI.changed = true;
                    }
                    break;
            }
            return false;
        }

        private void RightMouseMenu()
        {
            GenericMenu menu = new GenericMenu();
            if (type != PengAIScript.AIScriptType.EventDecide)
            {
                menu.AddItem(new GUIContent("删除节点"), false, () => editor.ProcessRemoveNode(this));
                //menu.AddItem(new GUIContent("复制节点"), false, () => editor.CopyNode(this));
            }
            //menu.AddItem(new GUIContent("脚本说明"), false, () => DisplayMeaning());
            menu.ShowAsContext();
        }

        public void DrawLines()
        {
            if (outID.Count > 0)
            {
                for (int i = 0; i < outID.Count; i++)
                {
                    if (outID.ElementAt(i).Value > 0)
                    {
                        Handles.DrawBezier(
                            GetNodeByNodeID(outID.ElementAt(i).Value).inPoint.rect.center,
                            outPoints[i].rect.center,
                            GetNodeByNodeID(outID.ElementAt(i).Value).inPoint.rect.center - Vector2.up * 40f,
                            outPoints[i].rect.center + Vector2.up * 40f,
                            Color.white,
                            null,
                            6f);

                        Vector2 buttonSize = new Vector2(20, 20);
                        Vector2 lineCenter = (GetNodeByNodeID(outID.ElementAt(i).Value).inPoint.rect.center + outPoints[i].rect.center) * 0.5f;

                        if (GUI.Button(new Rect(lineCenter - buttonSize / 2, buttonSize), "×"))
                        {
                            editor.nodes[outID[i]].inPoint.inOccupied = false;
                            outID[i] = -1;
                        }
                    }
                }
            }
        }

        public PengAIEditorNode GetNodeByNodeID(int id)
        {
            if (editor.nodes.Count > 0)
            {
                for (int i = 0; i < editor.nodes.Count; i++)
                {
                    if (editor.nodes[i].nodeID == id)
                    {
                        return editor.nodes[i];
                    }
                }
                return null;
            }
            return null;
        }

        public virtual string SpecialParaDescription()
        {
            return "";
        }

        public virtual void ReadSpecialParaDescription(string info)
        {
        }

        public void InitialDraw(Vector2 pos, PengAIEditor master)
        {
            this.pos = pos;
            rect = new Rect(pos.x, pos.y, 120, 40);
            this.editor = master;
        }

        public static string GetDescription(Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description.Split(",")[1];
                    }
                }
            }
            return null;
        }

        public static bool GetCodedDown(Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        if (int.Parse(attr.Description.Split(",")[0]) > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }
                }
            }
            return false;
        }

        public static string GetCatalog(Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description.Split(",")[2];
                    }
                }
            }
            return null;
        }
    }

    public class EventDecide : PengAIEditorNode
    {
        public EventDecide(Vector2 pos, PengAIEditor editor, int id, string flowOut, string info)
        {
            InitialDraw(pos, editor);
            nodeID = id;
            outID = PengGameManager.ParseStringToDictionaryIntInt(flowOut);
            meaning = "决策树根节点";

            inPoint = new PengAIEditorNodeConnection(PengAIEditorNodeConnection.AINodeConnectionType.In, 0, this);
            outPoints = new PengAIEditorNodeConnection[1];
            outPoints[0] = new PengAIEditorNodeConnection(PengAIEditorNodeConnection.AINodeConnectionType.Out, 0, this);

            type = PengAIScript.AIScriptType.EventDecide;
            nodeType = PengAIEditorNodeType.Event;
            name = GetDescription(type);
        }
    }


    public class Condition : PengAIEditorNode
    {
        public List<PengAIScript.ConditionVar> conditions = new List<PengAIScript.ConditionVar>();
        public Condition(Vector2 pos, PengAIEditor editor, int id, string flowOut, string info)
        {
            InitialDraw(pos, editor);
            nodeID = id;
            outID = PengGameManager.ParseStringToDictionaryIntInt(flowOut);
            meaning = "条件节点";

            inPoint = new PengAIEditorNodeConnection(PengAIEditorNodeConnection.AINodeConnectionType.In, 0, this);
            outPoints = new PengAIEditorNodeConnection[outID.Count];
            for (int i = 0; i < outID.Count; i++)
            {
                outPoints[i] = new PengAIEditorNodeConnection(PengAIEditorNodeConnection.AINodeConnectionType.Out, i, this);
            }

            ReadSpecialParaDescription(info);

            type = PengAIScript.AIScriptType.Condition;
            nodeType = PengAIEditorNodeType.Condition;
            name = GetDescription(type);
        }

        public override string SpecialParaDescription()
        {
            string result = "";
            for (int i = 0; i < conditions.Count; i++)
            {
                if (conditions[i].judge.strVal1 == "")
                {
                    conditions[i].judge.strVal1 = "null";
                }
                if (conditions[i].judge.strVal2 == "")
                {
                    conditions[i].judge.strVal2 = "null";
                }
                result += conditions[i].type.ToString() + "|" +
                    conditions[i].judge.type.ToString() + "|" +
                    conditions[i].judge.compareType.ToString() + "|" +
                    conditions[i].judge.sensor1.ToString() + "|" +
                    conditions[i].judge.sensor2.ToString() + "|" +
                    conditions[i].judge.floatVal1.ToString() + "|" +
                    conditions[i].judge.floatVal2.ToString() + "|" +
                    conditions[i].judge.intVal1.ToString() + "|" +
                    conditions[i].judge.intVal2.ToString() + "|" +
                    conditions[i].judge.strVal1 + "|" +
                    conditions[i].judge.strVal2 + "|" +
                    conditions[i].judge.singleSensor.ToString() + "|" +
                    (conditions[i].judge.singleSensorTrue ? "1" : "0");
                if (i != conditions.Count - 1)
                {
                    result += ";";
                }
            }
            return result;
        }

        public override void ReadSpecialParaDescription(string info)
        {
            if (info != "")
            {
                string[] strings = info.Split(";");
                if (strings.Length > 0)
                {
                    for (int i = 0; i < strings.Length; i++)
                    {
                        if (strings[i] != "")
                        {
                            string[] str = strings[i].Split("|");
                            if (str.Length > 0)
                            {
                                PengAIScript.ConditionVar cond = new PengAIScript.ConditionVar();
                                cond.type = (PengAIScript.ConditionVar.JudgmentType)Enum.Parse(typeof(PengAIScript.ConditionVar.JudgmentType), str[0]);
                                cond.judge.type = (PengAIScript.ConditionVar.JudgmentEntryType)Enum.Parse(typeof(PengAIScript.ConditionVar.JudgmentEntryType), str[1]);
                                cond.judge.compareType = (PengScript.MathCompare.CompareTypeCN)Enum.Parse(typeof(PengScript.MathCompare.CompareTypeCN), str[2]);
                                cond.judge.sensor1 = (PengAIScript.AISensor)Enum.Parse(typeof(PengAIScript.AISensor), str[3]);
                                cond.judge.sensor2 = (PengAIScript.AISensor)Enum.Parse(typeof(PengAIScript.AISensor), str[4]);
                                cond.judge.floatVal1 = float.Parse(str[5]);
                                cond.judge.floatVal2 = float.Parse(str[6]);
                                cond.judge.intVal1 = int.Parse(str[7]);
                                cond.judge.intVal2 = int.Parse(str[8]);
                                cond.judge.strVal1 = str[9] == "null" ? "" : str[9];
                                cond.judge.strVal2 = str[10] == "null" ? "" : str[10];
                                cond.judge.singleSensor = (PengAIScript.AISingleSensor)Enum.Parse(typeof(PengAIScript.AISingleSensor), str[11]);
                                cond.judge.singleSensorTrue = int.Parse(str[12]) > 0;
                                conditions.Add(cond);
                            }
                        }
                    }
                }
            }
        }

        public override void DrawSideBar(Rect sidebar)
        {
            base.DrawSideBar(sidebar);
            Rect addConditionButton = new Rect(sidebar.x + 10, sidebar.y + 60, 80, 20);
            if (GUI.Button(addConditionButton, "添加分支"))
            {
                AddCondition();
            }

            if (conditions.Count > 0)
            {
                int remove = -1;
                for (int i = 0; i < conditions.Count; i++)
                {
                    Rect box = new Rect(sidebar.x + 10, sidebar.y + 90 + 50 * i, sidebar.width - 20, 45);
                    Rect enum1 = new Rect(box.x + 5, box.y, (box.width - 10) * 0.25f - 2.5f, 20);
                    Rect enum2 = new Rect(enum1.x + enum1.width + 5, enum1.y, (box.width - 10) * 0.35f - 2.5f, 20);
                    Rect enum3 = new Rect(enum2.x + enum2.width + 5, enum2.y, (box.width - 10) * 0.35f - 5f, 20);
                    Rect enum4 = new Rect(enum1.x, enum1.y, enum1.width + enum2.width + 5f, 20);
                    //conditions[i].type = (PengAIScript.ConditionVar.JudgmentType)EditorGUI.EnumPopup(enum1, conditions[i].type);
                    conditions[i].judge.type = (PengAIScript.ConditionVar.JudgmentEntryType)EditorGUI.EnumPopup(enum4, conditions[i].judge.type);
                    if (i > 0)
                    {
                        if (GUI.Button(enum3, "删除"))
                        {
                            remove = i;
                        }
                    }
                    Rect box2 = new Rect(enum1.x, enum1.y + 25, box.width - 5, 20);
                    switch (conditions[i].judge.type)
                    {
                        case PengAIScript.ConditionVar.JudgmentEntryType.单目运算式:
                            Rect boolCal = new Rect(box2.x, box2.y, box2.width - 50, box2.height);
                            Rect equal = new Rect(boolCal.x + boolCal.width + 5, boolCal.y, 20, boolCal.height);
                            Rect toggle = new Rect(equal.x + equal.width + 5, equal.y, 20, equal.height);
                            conditions[i].judge.singleSensor = (PengAIScript.AISingleSensor)EditorGUI.EnumPopup(boolCal, conditions[i].judge.singleSensor);
                            GUI.Box(equal, "=");
                            conditions[i].judge.singleSensorTrue = EditorGUI.Toggle(toggle, conditions[i].judge.singleSensorTrue);
                            break;
                        case PengAIScript.ConditionVar.JudgmentEntryType.双目运算式:
                            Rect var1 = new Rect(box2.x, box2.y, box2.width * 0.3f - 2.5f, box2.height);
                            Rect compare = new Rect(var1.x + var1.width + 5f, var1.y, box2.width * 0.2f - 2.5f, box2.height);
                            Rect var3 = new Rect(compare.x + compare.width + 5f, compare.y, box2.width * 0.3f - 2.5f, box2.height);
                            Rect var2 = new Rect(var3.x + var3.width + 5f, var3.y, box2.width - var1.width - var3.width - compare.width - 10, box2.height);
                            PengAIScript.AISensor sensor1 = conditions[i].judge.sensor1;
                            sensor1 = (PengAIScript.AISensor)EditorGUI.EnumPopup(var1, sensor1);
                            if(sensor1 != PengAIScript.AISensor.Value)
                            {
                                conditions[i].judge.sensor1 = sensor1;
                            }
                            conditions[i].judge.compareType = (PengScript.MathCompare.CompareTypeCN)EditorGUI.EnumPopup(compare, conditions[i].judge.compareType);
                            conditions[i].judge.sensor2 = (PengAIScript.AISensor)EditorGUI.EnumPopup(var3, conditions[i].judge.sensor2);
                            switch (conditions[i].judge.sensor2)
                            {
                                case PengAIScript.AISensor.Value:
                                    if (conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetDistance || conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetRelativeDirection || conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetCurrentHP ||
                                        conditions[i].judge.sensor1 == PengAIScript.AISensor.SelfWanderTime || conditions[i].judge.sensor1 == PengAIScript.AISensor.SelfDecideGap)
                                    {
                                        conditions[i].judge.floatVal2 = EditorGUI.FloatField(var2, conditions[i].judge.floatVal2);
                                    }
                                    else if(conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetCurrentState || conditions[i].judge.sensor1 == PengAIScript.AISensor.SelfCurrentState)
                                    {
                                        conditions[i].judge.strVal2 = EditorGUI.TextField(var2, conditions[i].judge.strVal2);
                                    }   
                                    else if (conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetCurrentStateFrame)
                                    {
                                        conditions[i].judge.intVal2 = EditorGUI.IntField(var2, conditions[i].judge.intVal2);
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                }

                if (remove >= 0)
                {
                    conditions.RemoveAt(remove);
                    PengAIEditorNodeConnection[] newPAENC = new PengAIEditorNodeConnection[outPoints.Length - 1];
                    int j = 0;
                    for (int i = 0; i < outPoints.Length; i++)
                    {
                        if (i != remove)
                        {
                            newPAENC[j] = outPoints[i];
                        }
                        if (i > j)
                        {
                            outID[j] = outID.ElementAt(i).Value;
                        }
                        if (i != remove)
                        {
                            j++;
                        }
                    }
                    outID.Remove(outID.ElementAt(outID.Count - 1).Key);
                    outPoints = newPAENC;
                }
            }
        }

        public void AddCondition()
        {
            PengAIEditorNodeConnection[] newPAENC = new PengAIEditorNodeConnection[outPoints.Length + 1];
            outID.Add(outPoints.Length, -1);
            for (int i = 0; i < outPoints.Length; i++)
            {
                newPAENC[i] = outPoints[i];
            }
            newPAENC[outPoints.Length] = new PengAIEditorNodeConnection(PengAIEditorNodeConnection.AINodeConnectionType.Out, outPoints.Length, this);
            outPoints = newPAENC;
            PengAIScript.ConditionVar cond = new PengAIScript.ConditionVar();
            cond.type = PengAIScript.ConditionVar.JudgmentType.ElseIf;
            PengAIScript.Judgement judge = new PengAIScript.Judgement();
            judge.type = PengAIScript.ConditionVar.JudgmentEntryType.双目运算式;
            judge.compareType = PengScript.MathCompare.CompareTypeCN.不小于;
            judge.sensor1 = PengAIScript.AISensor.TargetDistance;
            judge.sensor2 = PengAIScript.AISensor.Value;
            cond.judge = judge;
            conditions.Add(cond);
        }
    }
}
