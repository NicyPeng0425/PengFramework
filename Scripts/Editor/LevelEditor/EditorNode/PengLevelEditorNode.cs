using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.ComponentModel;
using PengScript;
using System.Linq;
using UnityEditor;
using PengLevelRuntimeFunction;

public class PengLevelEditorNode
{
    public struct NodeIDConnectionID
    {
        public int nodeID;
        public int connectionID;
    }

    public enum LevelNodeType
    {
        Function,
        Trigger,
        Value,
    }

    public string name = "默认";
    public string meaning = "暂无说明";
    public LevelFunctionType type;
    public PengLevelEditor editor;
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
    public int paraNum;
    public Rect rectSmall
    {
        get { return new Rect(rect.x, rect.y + rect.height, rect.width, rect.height * paraNum); }
    }
    private bool isDragged;
    private bool m_isSelected;
    public bool isSelected
    {
        get { return m_isSelected; }
        set { m_isSelected = value; /*OnSelectedChange(value);*/ }
    }
    public LevelNodeType nodeType;

    public PengLevelNodeConnection[] inPoints;
    public PengLevelNodeConnection[] outPoints;
    public PengLevelNodeVariables[] inVars;
    public PengLevelNodeVariables[] outVars;
    public Dictionary<int, NodeIDConnectionID> outID = new Dictionary<int, NodeIDConnectionID>();
    public Dictionary<int, List<NodeIDConnectionID>> varOutID = new Dictionary<int, List<NodeIDConnectionID>>();
    public Dictionary<int, NodeIDConnectionID> varInID = new Dictionary<int, NodeIDConnectionID>();

    public virtual void Draw()
    {
        Rect rectScale = new Rect();
        if (GetCatalog(nodeType) != "值")
        {
            int i = outPoints.Length;
            if (inPoints.Length > outPoints.Length)
            {
                i = inPoints.Length;
            }
            rect = new Rect(rect.x, rect.y, rect.width, 21 * i);
            rectScale = new Rect(rect.x, rect.y, rect.width * editor.currentScale, rect.height * editor.currentScale);
        }
        else
        {
            rect = new Rect(rect.x, rect.y, rect.width, 21);
            rectScale = new Rect(rect.x, rect.y, rect.width * editor.currentScale, rect.height * editor.currentScale);
        }
        Rect rectSmallScale = new Rect(rectSmall.x, rectSmall.y, rectSmall.width * editor.currentScale, (3 + 18 * paraNum) * editor.currentScale);
        rectSmallScale.y = rectScale.y + rectScale.height;
        GUIStyle style3 = new GUIStyle("flow node 0" + (isSelected ? " on" : ""));
        GUI.Box(rectSmallScale, "", style3);
        Rect rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, rectScale.height);
        switch (nodeType)
        {
            case LevelNodeType.Trigger:
                GUIStyle style = new GUIStyle("flow node 6" + (isSelected ? " on" : ""));
                style.fontStyle = FontStyle.Bold;
                GUI.Box(rectScale, name, style);
                if (outPoints.Length > 0)
                {
                    for (int i = 0; i < outPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (18 + 36 * i) * editor.currentScale);
                        outPoints[i].Draw(rectMulti);
                    }
                }
                break;
            case LevelNodeType.Function:
                GUIStyle style1 = new GUIStyle("flow node 1" + (isSelected ? " on" : ""));
                style1.fontStyle = FontStyle.Bold;
                GUI.Box(rectScale, name, style1);
                if (inPoints.Length > 0)
                {
                    for (int i = 0; i < inPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (18 + 36 * i) * editor.currentScale);
                        inPoints[i].Draw(rectMulti);
                    }
                }
                if (outPoints.Length > 0)
                {
                    for (int i = 0; i < outPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (18 + 36 * i) * editor.currentScale);
                        outPoints[i].Draw(rectMulti);
                    }
                }
                break;
            case LevelNodeType.Value:
                GUIStyle style4 = new GUIStyle("flow node 3" + (isSelected ? " on" : ""));
                style4.fontStyle = FontStyle.Bold;
                GUI.Box(rectScale, name, style4);
                break;
        }

        if (inVars.Length > 0)
        {
            for (int i = 0; i < inVars.Length; i++)
            {
                inVars[i].DrawVar();
            }
        }
        if (outVars.Length > 0)
        {
            for (int i = 0; i < outVars.Length; i++)
            {
                outVars[i].DrawVar();
            }
        }
    }

    public void DrawLines()
    {
        if (outID.Count > 0)
        {
            for (int i = 0; i < outID.Count; i++)
            {
                if (outID.ElementAt(i).Value.nodeID > 0)
                {
                    Handles.DrawBezier(GetNodeByNodeID(outID.ElementAt(i).Value.nodeID).inPoints[outID.ElementAt(i).Value.connectionID].rect.center, outPoints[i].rect.center, GetNodeByNodeID(outID.ElementAt(i).Value.nodeID).inPoints[outID.ElementAt(i).Value.connectionID].rect.center + Vector2.left * 40f, outPoints[i].rect.center - Vector2.left * 40f, Color.white, null, 6f);

                    Vector2 buttonSize = new Vector2(20, 20);
                    Vector2 lineCenter = (GetNodeByNodeID(outID.ElementAt(i).Value.nodeID).inPoints[outID.ElementAt(i).Value.connectionID].rect.center + outPoints[i].rect.center) * 0.5f;

                    if (GUI.Button(new Rect(lineCenter - buttonSize / 2, buttonSize), "×"))
                    {
                        NodeIDConnectionID nici = new NodeIDConnectionID();
                        nici.nodeID = -1;
                        outID[i] = nici;
                    }
                }
            }
        }

        if (varInID.Count > 0)
        {
            for (int i = 0; i < varInID.Count; i++)
            {
                if (varInID.ElementAt(i).Value.nodeID > 0)
                {
                    Handles.DrawBezier(inVars[i].point.rect.center, GetPengVarByNodeIDPengVarOutID(varInID.ElementAt(i).Value.nodeID,
                        varInID.ElementAt(i).Value.connectionID).point.rect.center,
                        inVars[i].point.rect.center + Vector2.left * 40f,
                        GetPengVarByNodeIDPengVarOutID(varInID.ElementAt(i).Value.nodeID, varInID.ElementAt(i).Value.connectionID).point.rect.center - Vector2.left * 40f, Color.white, null, 3f);

                    Vector2 buttonSize = new Vector2(20, 20);
                    Vector2 lineCenter = (inVars[i].point.rect.center + GetPengVarByNodeIDPengVarOutID(varInID.ElementAt(i).Value.nodeID, varInID.ElementAt(i).Value.connectionID).point.rect.center) * 0.5f;

                    if (GUI.Button(new Rect(lineCenter - buttonSize / 2, buttonSize), "×"))
                    {
                        for (int j = GetNodeByNodeID(varInID[i].nodeID).varOutID[varInID[i].connectionID].Count - 1; j >= 0; j--)
                        {
                            if (GetNodeByNodeID(varInID[i].nodeID).varOutID[varInID[i].connectionID][j].nodeID == nodeID)
                            {
                                GetNodeByNodeID(varInID[i].nodeID).varOutID[varInID[i].connectionID].RemoveAt(j);
                                break;
                            }
                        }

                        varInID[i] = DefaultNodeIDConnectionID();
                    }
                }
            }
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
                if (e.keyCode == KeyCode.Delete && isSelected && type != LevelFunctionType.Start)
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
        if (type != LevelFunctionType.Start)
        {
            menu.AddItem(new GUIContent("删除节点"), false, () => editor.ProcessRemoveNode(this));
            //menu.AddItem(new GUIContent("复制节点"), false, () => editor.CopyNode(this));
        }
        //menu.AddItem(new GUIContent("脚本说明"), false, () => DisplayMeaning());
        menu.ShowAsContext();
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

    public static NodeIDConnectionID DefaultNodeIDConnectionID()
    {
        NodeIDConnectionID nici = new NodeIDConnectionID();
        nici.nodeID = -1;
        nici.connectionID = -1;
        return nici;
    }

    public void InitialDraw(Vector2 pos, PengLevelEditor master)
    {
        this.pos = pos;
        rect = new Rect(pos.x, pos.y, 120, 18);
        this.editor = master;
    }

    public virtual string SpecialParaDescription()
    {
        return "";
    }

    public virtual void DrawMoreInfo(Rect moreInfoRect)
    {
        Rect paraPanel = new Rect(moreInfoRect.x + 200, moreInfoRect.y + 20, moreInfoRect.width - 240, moreInfoRect.height - 40);
        int line = Mathf.FloorToInt(paraPanel.height / 25f);
        int col = 1;
        if (inVars.Length > 0)
        {
            col = Mathf.FloorToInt(inVars.Length / line) + 1;
        }
            
        if (inVars.Length > 0)
        {
            int index = 0;
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < line; j++)
                {
                    if (index < inVars.Length)
                    {
                        Rect field = new Rect(paraPanel.x + 250 * i, paraPanel.y + 25 * j, 70, 20);
                        Rect fieldValue = new Rect(field.x + 80, field.y, 160, 20);
                        GUI.Box(field, inVars[index].name + "：");
                        DrawInVarValue(index, fieldValue);
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    public virtual void DrawInVarValue(int inVarID, Rect field)
    {

    }

    public virtual void ReadSpecialParaDescription(string info)
    {
    }

    public PengLevelEditorNode GetNodeByNodeID(int id)
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

    public PengLevelNodeVariables GetPengVarByNodeIDPengVarOutID(int nodeID, int VarID)
    {
        return GetNodeByNodeID(nodeID).outVars[VarID];
    }

    public static string ParseDictionaryIntListNodeIDConnectionIDToString(Dictionary<int, List<NodeIDConnectionID>> dic)
    {
        string result = "";
        if (dic.Count > 0)
        {
            for (int i = 0; i < dic.Count; i++)
            {
                result += dic.ElementAt(i).Key.ToString() + "|" + ParseNodeIDConnectionIDListToString(dic.ElementAt(i).Value);
                if (i != dic.Count - 1)
                {
                    result += ";";
                }
            }
        }
        return result;
    }

    public static string ParseDictionaryIntNodeIDConnectionIDToString(Dictionary<int, NodeIDConnectionID> dic)
    {
        string result = "";
        if (dic.Count > 0)
        {
            for (int i = 0; i < dic.Count; i++)
            {
                result += dic.ElementAt(i).Key.ToString() + "|" + dic.ElementAt(i).Value.nodeID.ToString() + ":" + dic.ElementAt(i).Value.connectionID.ToString();
                if (i != dic.Count - 1)
                {
                    result += ";";
                }
            }
        }
        return result;
    }

    public static Dictionary<int, List<NodeIDConnectionID>> ParseStringToDictionaryIntListNodeIDConnectionID(string str)
    {
        Dictionary<int, List<NodeIDConnectionID>> result = new Dictionary<int, List<NodeIDConnectionID>>();
        if (str == "")
            return result;
        string[] elements = str.Split(";");
        if (elements.Length > 0)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                string[] eles2 = elements[i].Split("|");
                result.Add(int.Parse(eles2[0]), ParseStringToNodeIDConnectionIDList(eles2[1]));
            }
        }
        return result;
    }

    public static Dictionary<int, NodeIDConnectionID> ParseStringToDictionaryIntNodeIDConnectionID(string str)
    {
        Dictionary<int, NodeIDConnectionID> result = new Dictionary<int, NodeIDConnectionID>();
        if (str == "")
            return result;
        if (str.Contains("|"))
        {
            string[] strings = str.Split(";");
            if (strings.Length > 0)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    string[] s1 = strings[i].Split("|");
                    string[] s2 = s1[1].Split(":");
                    NodeIDConnectionID nici = new NodeIDConnectionID();
                    nici.nodeID = int.Parse(s2[0]);
                    nici.connectionID = int.Parse(s2[1]);
                    result.Add(int.Parse(s1[0]), nici);
                }
            }
        }
        else
        {
            string[] strings = str.Split(",");
            if (strings.Length > 0)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    string[] s1 = strings[i].Split(":");

                    NodeIDConnectionID nici = new NodeIDConnectionID();
                    nici.nodeID = int.Parse(s1[1]);
                    nici.connectionID = 0;
                    result.Add(int.Parse(s1[0]), nici);
                }
            }
        }
        return result;
    }

    public static Dictionary<int, List<NodeIDConnectionID>> DefaultDictionaryIntListNodeIDConnectionID(int varOutNum)
    {
        Dictionary<int, List<NodeIDConnectionID>> result = new Dictionary<int, List<NodeIDConnectionID>>();
        if (varOutNum > 0)
        {
            for (int i = 0; i < varOutNum; i++)
            {
                result.Add(i, DefaultNodeIDConnectionIDList());
            }
        }
        return result;
    }

    public static Dictionary<int, NodeIDConnectionID> DefaultDictionaryIntNodeIDConnectionID(int varInNum)
    {
        Dictionary<int, NodeIDConnectionID> result = new Dictionary<int, NodeIDConnectionID>();
        if (varInNum > 0)
        {
            for (int i = 0; i < varInNum; i++)
            {
                result.Add(i, DefaultNodeIDConnectionID());
            }
        }
        return result;
    }

    public static List<NodeIDConnectionID> DefaultNodeIDConnectionIDList()
    {
        List<NodeIDConnectionID> list = new List<NodeIDConnectionID>();
        list.Add(DefaultNodeIDConnectionID());
        return list;
    }

    public static List<NodeIDConnectionID> ParseStringToNodeIDConnectionIDList(string s)
    {
        List<NodeIDConnectionID> result = new List<NodeIDConnectionID>();
        if (s == "")
            return result;
        string[] str = s.Split(',');

        for (int i = 0; i < str.Length; i++)
        {
            string[] str2 = str[i].Split(":");
            NodeIDConnectionID ele = new NodeIDConnectionID();
            ele.nodeID = int.Parse(str2[0]);
            ele.connectionID = int.Parse(str2[1]);
            result.Add(ele);
        }
        return result;
    }

    public static string ParseNodeIDConnectionIDListToString(List<NodeIDConnectionID> list)
    {
        string result = "";
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                result += list[i].nodeID.ToString() + ":" + list[i].connectionID.ToString();
                if (i != list.Count - 1)
                {
                    result += ",";
                }
            }
        }
        return result;
    }

    public static string ParseVector2ToString(Vector2 vec)
    {
        string result = "";
        result += vec.x.ToString() + ",";
        result += vec.y.ToString();
        return result;
    }

    public static Vector2 ParseStringToVector2(string str)
    {
        string[] s = str.Split(",");
        if (s.Length == 2)
        {
            return new Vector2(float.Parse(s[0]), float.Parse(s[1]));
        }
        else
        {
            Debug.LogError("字符串格式不正确，无法转成Vector2！");
            return Vector2.zero;
        }
    }
}
