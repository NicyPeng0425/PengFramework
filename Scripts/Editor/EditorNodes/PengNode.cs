using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using PengScript;
using static PengScript.MathCompare;

public class PengNode
{
    //Node描述文件
    //nodeName 名字
    //scriptType 脚本类型
    //pos 节点图上的位置
    //paraNum 参数数量
    //nodeID 脚本ID
    //varOutID 参数出信息
    //varInID 参数入信息
    //outID 脚本流出信息
    //参数信息
    public string nodeName = "默认";
    public PengScript.PengScriptType scriptType;
    public Vector2 pos;
    public Vector2 posChange = Vector2.zero;
    Rect m_Rect;
    public Rect rect
    { 
        get
        {
            return m_Rect;
        }
        set
        {
            m_Rect = value;
            pos = m_Rect.position;
            posChange = pos;
        }
    }
    public Rect rectSmall;
    private bool isDragged;
    private bool m_isSelected;
    public int paraNum = 1;
    public bool isSelected
    {
        get { return m_isSelected; }
        set { m_isSelected = value; OnSelectedChange(value); }
    }

    public struct NodeIDConnectionID
    {
        public int nodeID;
        public int connectionID;
    }

    public PengNodeConnection[] inPoints;
    public PengNodeConnection[] outPoints;
    public PengEditorVariables.PengVar[] inVars;
    public PengEditorVariables.PengVar[] outVars;

    //第x个FlowOut连接点，链接到哪个节点的FlowIn节点
    //Value取值为-1时表示该点没有链接
    public Dictionary<int, NodeIDConnectionID> outID = new Dictionary<int, NodeIDConnectionID>();
    //第x个VarOut连接点，链接到哪些节点的哪一VarIn连接点。因为VarOut可以链接多个点，所以用List
    //List.Count为0时表示没有链接
    public Dictionary<int, List<NodeIDConnectionID>> varOutID = new Dictionary<int, List<NodeIDConnectionID>>();
    //第x个VarIn连接点，从哪个节点的哪一VarOut取值。键值对：<节点ID，连接点ID>
    //Value的Key取值为-1时表示该点没有链接
    public Dictionary<int, NodeIDConnectionID> varInID = new Dictionary<int, NodeIDConnectionID>();                     
    
    public PengActorStateEditorWindow master;
    public PengEditorTrack trackMaster;
    public int nodeID;

    public enum NodeType
    {
        Event,
        Action,
        Branch,
        Value,
        Iterator,
    }

    public NodeType type;
    public string meaning = "暂无说明";
    public virtual void Draw()
    {
        Rect rectScale = new Rect();
        if (type != NodeType.Value)
        {
            int i = outPoints.Length;
            if (inPoints.Length > outPoints.Length)
            {
                i = inPoints.Length;
            }
            rect = new Rect(rect.x, rect.y, rect.width, 26 * i);
            rectScale = new Rect(rect.x, rect.y, rect.width * master.currentScale, rect.height * master.currentScale);
        }
        else
        {
            rect = new Rect(rect.x, rect.y, rect.width, 26);
            rectScale = new Rect(rect.x, rect.y, rect.width * master.currentScale, rect.height * master.currentScale);
        }
        Rect rectSmallScale = new Rect(rectSmall.x, rectSmall.y, rectSmall.width * master.currentScale, (2 + 23 * paraNum) * master.currentScale);
        rectSmallScale.y = rectScale.y + rectScale.height;
        GUIStyle style3 = new GUIStyle("flow node 0" + (isSelected ? " on" : ""));
        GUI.Box(rectSmallScale, "", style3);
        Rect rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, rectScale.height);  
        switch (type)
        {
            case NodeType.Event:
                GUIStyle style = new GUIStyle("flow node 6" + (isSelected ? " on" : ""));
                style.fontStyle = FontStyle.Bold;
                GUI.Box(rectScale, nodeName, style);
                if(outPoints.Length > 0)
                {
                    for(int i = 0; i < outPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (26 + 52 * i) * master.currentScale);
                        outPoints[i].Draw(rectMulti);
                    }
                }
                break;
            case NodeType.Action:
                GUIStyle style1 = new GUIStyle("flow node 1" + (isSelected ? " on" : ""));
                style1.fontStyle = FontStyle.Bold;
                GUI.Box(rectScale, nodeName, style1);
                if (inPoints.Length > 0)
                {
                    for (int i = 0; i < inPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (26 + 52 * i) * master.currentScale);
                        inPoints[i].Draw(rectMulti);
                    }
                }
                if (outPoints.Length > 0)
                {
                    for (int i = 0; i < outPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (26 + 52 * i) * master.currentScale);
                        outPoints[i].Draw(rectMulti);
                    }
                }
                break;
            case NodeType.Branch:
                GUIStyle style2 = new GUIStyle("flow node 2" + (isSelected ? " on" : ""));
                style2.fontStyle = FontStyle.Bold;
                GUI.Box(rectScale, nodeName, style2);
                if (inPoints.Length > 0)
                {
                    for (int i = 0; i < inPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (26 + 52 * i) * master.currentScale);
                        inPoints[i].Draw(rectMulti);
                    }
                }
                if (outPoints.Length > 0)
                {
                    for (int i = 0; i < outPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (26 + 52 * i) * master.currentScale);
                        outPoints[i].Draw(rectMulti);
                    }
                }
                break;
            case NodeType.Iterator:
                GUIStyle style5 = new GUIStyle("flow node 4" + (isSelected ? " on" : ""));
                style5.fontStyle = FontStyle.Bold;
                GUI.Box(rectScale, nodeName, style5);
                if (inPoints.Length > 0)
                {
                    for (int i = 0; i < inPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (26 + 52 * i) * master.currentScale);
                        inPoints[i].Draw(rectMulti);
                    }
                }
                if (outPoints.Length > 0)
                {
                    for (int i = 0; i < outPoints.Length; i++)
                    {
                        rectMulti = new Rect(rectScale.x, rectScale.y, rectScale.width, (26 + 52 * i) * master.currentScale);
                        outPoints[i].Draw(rectMulti);
                    }
                }
                break;
            case NodeType.Value:
                GUIStyle style4 = new GUIStyle("flow node 3" + (isSelected ? " on" : ""));
                style4.fontStyle = FontStyle.Bold;
                GUI.Box(rectScale, nodeName, style4);
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

        if(master.debug)
        {
            GUIStyle styleNum = new GUIStyle("dockHeader");
            styleNum.fontStyle = FontStyle.Bold;
            Rect num = new Rect(rectScale.x, rectScale.y - 20, 20, 20);
            GUI.Box(num, nodeID.ToString(), styleNum);
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

        if(varInID.Count > 0)
        {
            for(int i = 0;i < varInID.Count;i++)
            {
                if(varInID.ElementAt(i).Value.nodeID > 0)
                {
                    Handles.DrawBezier(inVars[i].point.rect.center, GetPengVarByNodeIDPengVarOutID(varInID.ElementAt(i).Value.nodeID,
                        varInID.ElementAt(i).Value.connectionID).point.rect.center,
                        inVars[i].point.rect.center + Vector2.left * 40f,
                        GetPengVarByNodeIDPengVarOutID(varInID.ElementAt(i).Value.nodeID, varInID.ElementAt(i).Value.connectionID).point.rect.center - Vector2.left * 40f, Color.white, null, 3f);

                    Vector2 buttonSize = new Vector2(20, 20);
                    Vector2 lineCenter = (inVars[i].point.rect.center + GetPengVarByNodeIDPengVarOutID(varInID.ElementAt(i).Value.nodeID, varInID.ElementAt(i).Value.connectionID).point.rect.center) * 0.5f;

                    if (GUI.Button(new Rect(lineCenter - buttonSize / 2, buttonSize), "×"))
                    {
                        for(int j = GetNodeByNodeID(varInID[i].nodeID).varOutID[varInID[i].connectionID].Count - 1; j >= 0; j--)
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
        if(master != null)
        {
            pos += change / master.currentScale;
        }
        else
        {
            pos += change;
        }

        rect = new Rect(pos.x, pos.y, rect.width, rect.height);
        rectSmall = new Rect(rect.x, rect.y + rect.height, rect.width, rect.height);
    }

    public bool ProcessEvents(Event e) 
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if(master.nodeMapRect.Contains(e.mousePosition))
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
                    if (master.nodeMapRect.Contains(e.mousePosition))
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
                if (master.nodeMapRect.Contains(e.mousePosition))
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
                if(e.keyCode == KeyCode.Delete && isSelected && scriptType != PengScriptType.OnTrackExecute)
                {
                    master.ProcessRemoveNode(this);
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
        if(scriptType != PengScript.PengScriptType.OnTrackExecute)
        {
            menu.AddItem(new GUIContent("删除节点"), false, () => master.ProcessRemoveNode(this));
        }
        menu.AddItem(new GUIContent("脚本说明"), false, () => DisplayMeaning());
        menu.ShowAsContext();
    }

    public void DisplayMeaning()
    {
        EditorUtility.DisplayDialog("脚本说明", this.meaning, "确认");
    }

    private void OnSelectedChange(bool changeTo)
    {

    }

    public void InitialDraw(Vector2 pos, PengActorStateEditorWindow master)
    {
        rect = new Rect(pos.x, pos.y, 240, 26);
        rectSmall = new Rect(pos.x, (pos.y + 26), 240, (5 + 23 * paraNum));
        this.master = master;
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
                    if(int.Parse(attr.Description.Split(",")[0]) > 0)
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

    public static string GetCatalogByFunction(Enum value)
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

    public static string GetCatalogByName(Enum value)
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
                    return attr.Description.Split(",")[3];
                }
            }
        }
        return null;
    }

    public static string GetCatalogByPackage(Enum value)
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
                    return attr.Description.Split(",")[4];
                }
            }
        }
        return null;
    }

    public static string GetCNName(Enum value)
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
                    return attr.Description;
                }
            }
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

    public PengNode GetNodeByNodeID(int id)
    {
        if (trackMaster.nodes.Count > 0)
        {
            for (int i = 0; i < trackMaster.nodes.Count; i++)
            {
                if (trackMaster.nodes[i].nodeID == id)
                {
                    return trackMaster.nodes[i];
                }
            }
            return null;
        }
        return null;
    }

    public PengEditorVariables.PengVar GetPengVarByNodeIDPengVarOutID(int nodeID, int VarID)
    {
        return GetNodeByNodeID(nodeID).outVars[VarID];
    }

    public static string ParseRectToString(Rect rect)
    {
        string result = "";
        result += rect.x.ToString() + ",";
        result += rect.y.ToString() + ",";
        result += rect.width.ToString() + ",";
        result += rect.height.ToString();
        return result;
    }

    public static Rect ParseStringToRect(string str)
    {
        string[] s = str.Split(",");
        if (s.Length == 4)
        {
            return new Rect(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
        }
        else
        {
            Debug.LogError("字符串格式不正确，无法转成Rect！");
            return new Rect(0,0,0,0);
        }
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

    public static List<NodeIDConnectionID> DefaultNodeIDConnectionIDList()
    {
        List<NodeIDConnectionID> list = new List<NodeIDConnectionID>();
        list.Add(DefaultNodeIDConnectionID());
        return list;
    }

    public static NodeIDConnectionID DefaultNodeIDConnectionID()
    {
        NodeIDConnectionID nici = new NodeIDConnectionID();
        nici.nodeID = -1;
        nici.connectionID = -1;
        return nici;
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

    public static string ParseDictionaryIntIntToString(Dictionary<int, int> dic)
    {
        string result = "";
        if (dic.Count > 0)
        {
            for (int i = 0; i < dic.Count; i++)
            {
                result += dic.ElementAt(i).Key.ToString() + ":" + dic.ElementAt(i).Value.ToString();
                if(i != dic.Count - 1)
                {
                    result += ",";
                }
            }
        }
        return result;
    }

    public static Dictionary<int, int> ParseStringToDictionaryIntInt(string str)
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        if (str == "")
            return result;
        string[] strings = str.Split(",");
        if (strings.Length > 0)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                string[] s = strings[i].Split(":");
                result.Add(int.Parse(s[0]), int.Parse(s[1]));
            }
        }
        return result;
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

    public static string ParseDictionaryIntNodeIDConnectionIDToString(Dictionary<int, NodeIDConnectionID> dic)
    {
        string result = "";
        if (dic.Count > 0)
        {
            for (int i = 0; i < dic.Count; i++)
            {
                result += dic.ElementAt(i).Key.ToString() + "|" + dic.ElementAt(i).Value.nodeID.ToString() + ":" + dic.ElementAt(i).Value.connectionID.ToString();
                if(i != dic.Count - 1)
                {
                    result += ";";
                }
            }
        }
        return result;
    }

    public static Dictionary<int, NodeIDConnectionID> ParseStringToDictionaryIntNodeIDConnectionID(string str)
    {
        Dictionary<int, NodeIDConnectionID> result = new Dictionary<int, NodeIDConnectionID> ();
        if(str == "")
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

    public static Dictionary<int, int> DefaultDictionaryIntInt(int flowOutNum)
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        if (flowOutNum > 0)
        {
            for (int i = 0; i < flowOutNum; i++)
            {
                result.Add(i, -1);
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
}

public class DebugLog : PengNode
{
    public PengEditorVariables.PengT obj;

    public DebugLog(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);
        meaning = "输出一个对象的值，一般是一个浮点、整型、布尔或字符串类型的值。";

        inPoints = new PengNodeConnection[1];inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[1];
        obj = new PengEditorVariables.PengT(this, "对象", 0, ConnectionPointType.In);
        inVars[0] = obj;
        outVars = new PengEditorVariables.PengVar[0];

        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.DebugLog;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
    }
}

public class BreakPoint : PengNode
{
    public BreakPoint(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);
        meaning = "设置一个断点，运行时此脚本被执行时将会暂停游戏。";

        inPoints = new PengNodeConnection[1];inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[0];
        outVars = new PengEditorVariables.PengVar[0];

        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.BreakPoint;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
    }
}