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
}
