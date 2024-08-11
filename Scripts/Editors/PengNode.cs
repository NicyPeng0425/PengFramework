using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.ComponentModel;
using System.Reflection;
using PengVariables;
using static UnityEditor.PlayerSettings;
using System.Linq;
using static UnityEngine.Rendering.VolumeComponent;
using PengScript;

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

    public PengNodeConnection inPoint;
    public PengNodeConnection[] outPoints;
    public PengVar[] inVars;
    public PengVar[] outVars;

    //第x个FlowOut连接点，链接到哪个节点的FlowIn节点。因为所有节点只有一个FlowIn，所以只需要记载节点ID
    //Value取值为-1时表示该点没有链接
    public Dictionary<int, int> outID = new Dictionary<int, int>();
    //第x个VarOut连接点，链接到哪些节点的哪一VarIn连接点。因为VarOut可以链接多个点，所以用List
    //List.Count为0时表示没有链接
    public Dictionary<int, List<NodeIDConnectionID>> varOutID = new Dictionary<int, List<NodeIDConnectionID>>();
    //第x个VarIn连接点，从哪个节点的哪一VarOut取值。键值对：<节点ID，连接点ID>
    //Value的Key取值为-1时表示该点没有链接
    public Dictionary<int, NodeIDConnectionID> varInID = new Dictionary<int, NodeIDConnectionID>();                     
    
    public PengActorStateEditorWindow master;
    public PengTrack trackMaster;
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


    public virtual void Draw()
    {
        Rect rectScale = new Rect();
        if (type != NodeType.Value)
        {
            rect = new Rect(rect.x, rect.y, rect.width, 26 * outPoints.Length);
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
                inPoint.Draw(rectScale);
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
                inPoint.Draw(rectScale);
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
                inPoint.Draw(rectScale);
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
                if (outID.ElementAt(i).Value > 0)
                {
                    Handles.DrawBezier(GetNodeByNodeID(outID.ElementAt(i).Value).inPoint.rect.center, outPoints[i].rect.center, GetNodeByNodeID(outID.ElementAt(i).Value).inPoint.rect.center + Vector2.left * 40f, outPoints[i].rect.center - Vector2.left * 40f, Color.white, null, 6f);
                    
                    Vector2 buttonSize = new Vector2(20, 20);
                    Vector2 lineCenter = (GetNodeByNodeID(outID.ElementAt(i).Value).inPoint.rect.center + outPoints[i].rect.center) * 0.5f;

                    if (GUI.Button(new Rect(lineCenter - buttonSize / 2, buttonSize), "×"))
                    {
                        outID[i] = -1;
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
            menu.ShowAsContext();
        }
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

    public PengVar GetPengVarByNodeIDPengVarOutID(int nodeID, int VarID)
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

public class OnTrackExecute : PengNode
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
    public PengInt pengTrackExecuteFrame;
    public PengInt pengStateExecuteFrame;

    public OnTrackExecute(Vector2 pos, PengActorStateEditorWindow master, ref PengTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntInt(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);


        inPoint = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengVar[0];
        outVars = new PengVar[2];
        pengTrackExecuteFrame = new PengInt(this, "当前轨道帧", 0, ConnectionPointType.Out);
        pengStateExecuteFrame = new PengInt(this, "当前状态帧", 1, ConnectionPointType.Out);
        outVars[0] = pengTrackExecuteFrame;
        outVars[1] = pengStateExecuteFrame;

        type = NodeType.Event;
        scriptType = PengScript.PengScriptType.OnTrackExecute;
        nodeName = GetDescription(scriptType);


        paraNum = 2;

        
    }

    public override void Draw()
    {
        base.Draw();
    }
}

public class PlayAnimation : PengNode
{
    public PengString pengAnimationName;
    public PengBool pengHardCut;
    public PengFloat pengTransitionNormalizedTime;
    public PengFloat pengStartAtNormalizedTime;
    public PengInt pengAnimationLayer;
    
    public PlayAnimation(Vector2 pos, PengActorStateEditorWindow master, ref PengTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntInt(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoint = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengVar[5];
        outVars = new PengVar[0];
        pengAnimationName = new PengString(this, "动画名称", 0, ConnectionPointType.In);
        pengHardCut = new PengBool(this, "是否硬切", 1, ConnectionPointType.In);
        pengTransitionNormalizedTime = new PengFloat(this, "过渡时间", 2, ConnectionPointType.In);
        pengStartAtNormalizedTime = new PengFloat(this, "开始时间", 3, ConnectionPointType.In);
        pengAnimationLayer = new PengInt(this, "动画层", 4, ConnectionPointType.In);
        inVars[0] = pengAnimationName;
        inVars[1] = pengHardCut;
        inVars[2] = pengTransitionNormalizedTime;
        inVars[3] = pengStartAtNormalizedTime;
        inVars[4] = pengAnimationLayer;

        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.PlayAnimation;
        nodeName = GetDescription(scriptType);

        paraNum = 5;
    }

    public override void Draw()
    {
        base.Draw();
    }
}

public class IfElse: PengNode
{
    public enum IfElseIfElse
    {
        If,
        ElseIf,
        Else
    }

    public List<IfElseIfElse> conditionTypes = new List<IfElseIfElse>();

    public IfElse(Vector2 pos, PengActorStateEditorWindow master, ref PengTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntInt(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);
        this.ReadSpecialParaDescription(specialInfo);


        inPoint = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[this.outID.Count];
        inVars = new PengVar[this.varInID.Count];
        paraNum = this.varInID.Count;
        for (int i = 0; i < this.outID.Count; i++)
        {
            outPoints[i] = new PengNodeConnection(ConnectionPointType.FlowOut, i, this, null);
            PengBool ifInVar = new PengBool(this, "条件"+(i+1).ToString(), i, ConnectionPointType.In);
            inVars[i] = ifInVar;
        }

        outVars = new PengVar[0];
        type = NodeType.Branch;
        scriptType = PengScript.PengScriptType.IfElse;
        nodeName = GetDescription(scriptType);
        
    }

    public override void Draw()
    {
        base.Draw();
        if (inVars.Length > 0)
        {
            for (int i = 0; i < inVars.Length; i++)
            {
                Rect re = new Rect(inVars[i].varRect.x + 100, inVars[i].varRect.y, 60, 18);
                if(i == 0)
                {
                    GUI.Box(re, "If");
                }
                else 
                {
                    if (i == inVars.Length - 1)
                    {
                        GUI.Box(re, conditionTypes[conditionTypes.Count - 1].ToString());
                    } 
                    else
                    {
                        GUI.Box(re, "ElseIf");
                    }
                }
                

            }
            Rect add = new Rect(inVars[inVars.Length - 1].varRect.x, inVars[inVars.Length - 1].varRect.y + 20, 60, 20);
            if (GUI.Button(add, "添加"))
            {
                this.AddConditions();
            }
            if (inVars.Length > 1)
            {
                Rect remove = new Rect(add.x + 70, add.y, 60, 20);
                if (GUI.Button(remove, "移除"))
                {
                    this.RemoveConditions();
                }
                Rect change = new Rect(add.x + 140, add.y, 60, 20);
                if (GUI.Button(change, "更改"))
                {
                    this.ChangeCondition(inVars.Length - 1);
                }
            }
            
        }
    }

    public override string SpecialParaDescription()
    {
        string result = "";
        if(conditionTypes.Count > 0)
        {
            for (int i = 0; i < conditionTypes.Count; i++)
            {
                result += conditionTypes[i].ToString();
                if(i !=  conditionTypes.Count - 1)
                {
                    result += ",";
                }
            }
        }
        return result;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if(info != "")
        {
            string[] str = info.Split(",");
            for (int i = 0; i < str.Length; i++)
            {
                conditionTypes.Add((IfElseIfElse)Enum.Parse(typeof(IfElseIfElse), str[i]));
            }
        }
        else
        {
            conditionTypes.Add(IfElseIfElse.If);
        }
    }

    public void AddConditions()
    {
        PengBool newVar = new PengBool(this, "条件" + (inVars.Length + 1).ToString(), inVars.Length, ConnectionPointType.In);
        PengVar[] newInVars = new PengVar[inVars.Length + 1];
        for(int i = 0;i < inVars.Length;i++)
        {
            newInVars[i] = inVars[i];
        }
        newInVars[newInVars.Length - 1] = newVar;
        inVars = newInVars;

        conditionTypes.Add(IfElseIfElse.ElseIf);

        PengNodeConnection[] newOutPoints = new PengNodeConnection[inVars.Length];
        for (int i = 0; i < outPoints.Length; i++)
        {
            newOutPoints[i] = outPoints[i];
        }
        newOutPoints[newOutPoints.Length - 1] = new PengNodeConnection(ConnectionPointType.FlowOut, inVars.Length - 1, this, null);
        outPoints = newOutPoints;

        outID.Add(newInVars.Length - 1, -1);
        varInID.Add(newInVars.Length - 1, DefaultNodeIDConnectionID());
        paraNum++;
        
    }
    
    public void RemoveConditions()
    {
        if (inVars.Length > 1)
        {
            if (GetNodeByNodeID(varInID[varInID.Count - 1].nodeID).varOutID[varInID[varInID.Count - 1].connectionID].Count > 0)
            {
                for (int j = GetNodeByNodeID(varInID[varInID.Count - 1].nodeID).varOutID[varInID[varInID.Count - 1].connectionID].Count - 1; j >= 0; j--)
                {
                    if (GetNodeByNodeID(varInID[varInID.Count - 1].nodeID).varOutID[varInID[varInID.Count - 1].connectionID][j].nodeID == nodeID)
                    {
                        GetNodeByNodeID(varInID[varInID.Count - 1].nodeID).varOutID[varInID[varInID.Count - 1].connectionID].RemoveAt(j);
                        break;
                    }
                }
            }
            PengVar[] newInVars = new PengVar[inVars.Length - 1];
            PengNodeConnection[] newOutPoints = new PengNodeConnection[inVars.Length - 1];
            for (int i = 0; i < newInVars.Length; i++)
            {
                newInVars[i] = inVars[i];
                newOutPoints[i] = outPoints[i];
            }
            outPoints = newOutPoints;
            inVars = newInVars;

            paraNum--;
            conditionTypes.RemoveAt(inVars.Length);
            outID.Remove(inVars.Length);
            varInID.Remove(inVars.Length);
        }
    }

    public void ChangeCondition (int index)
    {
        if (conditionTypes[index] == IfElseIfElse.ElseIf)
        {
            conditionTypes[index] = IfElseIfElse.Else;
        }
        else if(conditionTypes[index] == IfElseIfElse.Else)
        {
            conditionTypes[index] = IfElseIfElse.ElseIf;
        }
    }
}

public class ValuePengInt : PengNode
{
    public PengInt pengInt;

    public ValuePengInt(Vector2 pos, PengActorStateEditorWindow master, ref PengTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntInt(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        //inPoint = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        //outPoints = new PengNodeConnection[1];
        //outPoints[0] = new PengNodeConnection(ConnectionPointType.Out, 0, this, null);
        inVars = new PengVar[0];
        outVars = new PengVar[1];
        pengInt = new PengInt(this, "值", 0, ConnectionPointType.Out);
        outVars[0] = pengInt;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValuePengInt;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        Rect intField = new Rect(pengInt.varRect.x + 45, pengInt.varRect.y, 65, 18);
        string text = pengInt.value.ToString();
        text = GUI.TextField(intField, text);
        if(text != pengInt.value.ToString())
        {
            if(int.TryParse(text, out pengInt.value))
            {

            }
            else
            {
                text = pengInt.value.ToString();
            }
        }
    }
    public override string SpecialParaDescription()
    {
        return pengInt.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if(info != "")
        {
            pengInt.value = int.Parse(info);
        }
        else
        {
            pengInt.value = 0;
        }
    }
}

public class ValuePengFloat : PengNode
{
    public PengFloat pengFloat;

    public ValuePengFloat(Vector2 pos, PengActorStateEditorWindow master, ref PengTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntInt(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        //inPoint = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        //outPoints = new PengNodeConnection[1];
        //outPoints[0] = new PengNodeConnection(ConnectionPointType.Out, 0, this, null);
        inVars = new PengVar[0];
        outVars = new PengVar[1];
        pengFloat = new PengFloat(this, "值", 0, ConnectionPointType.Out);
        outVars[0] = pengFloat;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValuePengFloat;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        Rect floatField = new Rect(pengFloat.varRect.x + 45, pengFloat.varRect.y, 65, 18);
        string text = pengFloat.value.ToString();
        text = GUI.TextField(floatField, text);
        if (text != pengFloat.value.ToString())
        {
            if (float.TryParse(text, out pengFloat.value))
            {

            }
            else
            {
                text = pengFloat.value.ToString();
            }
        }
    }
    public override string SpecialParaDescription()
    {
        return pengFloat.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            pengFloat.value = float.Parse(info);
        }
        else
        {
            pengFloat.value = 0f;
        }
    }
}

public class ValuePengString : PengNode
{
    public PengString pengString;

    public ValuePengString(Vector2 pos, PengActorStateEditorWindow master, ref PengTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntInt(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        //inPoint = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        //outPoints = new PengNodeConnection[1];
        //outPoints[0] = new PengNodeConnection(ConnectionPointType.Out, 0, this, null);
        inVars = new PengVar[0];
        outVars = new PengVar[1];
        pengString = new PengString(this, "值", 0, ConnectionPointType.Out);
        outVars[0] = pengString;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValuePengString;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        Rect field = new Rect(pengString.varRect.x + 45, pengString.varRect.y, 65, 18);
        pengString.value = GUI.TextField(field, pengString.value);

    }
    public override string SpecialParaDescription()
    {
        return pengString.value;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        pengString.value = info;
    }
}

public class ValuePengBool : PengNode
{
    public PengBool pengBool;

    public ValuePengBool(Vector2 pos, PengActorStateEditorWindow master, ref PengTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntInt(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        //inPoint = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        //outPoints = new PengNodeConnection[1];
        //outPoints[0] = new PengNodeConnection(ConnectionPointType.Out, 0, this, null);
        inVars = new PengVar[0];
        outVars = new PengVar[1];
        pengBool = new PengBool(this, "值", 0, ConnectionPointType.Out);
        outVars[0] = pengBool;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValuePengBool;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        Rect field = new Rect(pengBool.varRect.x + 45, pengBool.varRect.y, 65, 18);
        pengBool.value = GUI.Toggle(field, pengBool.value, "");

    }
    public override string SpecialParaDescription()
    {
        return pengBool.value? "1" : "0";
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if(info != "")
        {
            pengBool.value = int.Parse(info) > 0;
        }
        else
        {
            pengBool.value = false;
        }
    }
}


public class GetTargetsByRange : PengNode
{
    public enum RangeType
    {
        [Description("圆柱体")]
        Cylinder,
        [Description("球体")]
        Sphere,
        [Description("盒形")]
        Box,
    }

    public PengListPengActor result;
    public RangeType rangeType = RangeType.Cylinder;
    public PengInt typeNum;
    public PengInt pengCamp;
    public PengVector3 pengPara;
    public PengVector3 pengOffset;

    public GetTargetsByRange(Vector2 pos, PengActorStateEditorWindow master, ref PengTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntInt(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoint = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);

        inVars = new PengVar[4];
        outVars = new PengVar[1];

        typeNum = new PengInt(this, "范围类型", 0, ConnectionPointType.In);
        typeNum.point = null;
        result = new PengListPengActor(this, "获取到的目标", 0, ConnectionPointType.Out);
        pengCamp = new PengInt(this, "阵营", 1, ConnectionPointType.In);
        pengPara = new PengVector3(this, "参数", 2, ConnectionPointType.In);
        pengOffset = new PengVector3(this, "偏移", 3, ConnectionPointType.In);

        outVars[0] = result;
        inVars[0] = typeNum;
        inVars[1] = pengCamp;
        inVars[2] = pengPara;
        inVars[3] = pengOffset;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.GetTargetsByRange;
        nodeName = GetDescription(scriptType);

        paraNum = 4;
    }

    public override void Draw()
    {
        base.Draw();
        Rect field = new Rect(typeNum.varRect.x + 40, typeNum.varRect.y, 70, 18);
        rangeType = (RangeType)EditorGUI.EnumPopup(field, rangeType);

    }
    public override string SpecialParaDescription()
    {
        switch (rangeType)
        {
            default:
                return "";
            case RangeType.Cylinder:
                return "1";
            case RangeType.Sphere:
                return "2";
            case RangeType.Box:
                return "3";
        }
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            switch (int.Parse(info))
            {
                case 1:
                    rangeType = RangeType.Cylinder;
                    break;
                case 2:
                    rangeType = RangeType.Sphere;
                    break;
                case 3:
                    rangeType = RangeType.Box;
                    break;
            }
        }
    }
}