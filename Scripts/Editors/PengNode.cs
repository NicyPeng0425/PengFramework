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
        rect = new Rect(rect.x, rect.y , rect.width, 26 * outPoints.Length);
        rectSmall.y += 26 * (outPoints.Length - 1);
        rectSmall.height = 2 + 23 * paraNum;
        GUIStyle style3 = new GUIStyle("flow node 0" + (isSelected ? " on" : ""));
        GUI.Box(rectSmall, "", style3);
        Rect rectMulti = new Rect(rect.x, rect.y, rect.width,rect.height);  
        switch (type)
        {
            case NodeType.Event:
                GUIStyle style = new GUIStyle("flow node 6" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style);
                if(outPoints.Length > 0)
                {
                    for(int i = 0; i < outPoints.Length; i++)
                    {
                        rectMulti = new Rect(rect.x, rect.y, rect.width, 26 + 52 * i);
                        outPoints[i].Draw(rectMulti);
                    }
                }
                break;
            case NodeType.Action:
                GUIStyle style1 = new GUIStyle("flow node 1" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style1);
                inPoint.Draw(rect);
                if (outPoints.Length > 0)
                {
                    for (int i = 0; i < outPoints.Length; i++)
                    {
                        rectMulti = new Rect(rect.x, rect.y, rect.width, rect.height + 52 * i);
                        outPoints[i].Draw(rectMulti);
                    }
                }
                break;
            case NodeType.Branch:
                GUIStyle style2 = new GUIStyle("flow node 2" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style2);
                inPoint.Draw(rect);
                if (outPoints.Length > 0)
                {
                    for (int i = 0; i < outPoints.Length; i++)
                    {
                        rectMulti = new Rect(rect.x, rect.y, rect.width, rect.height + 52 * i);
                        outPoints[i].Draw(rectMulti);
                    }
                }
                break;
        }
    }

    public void ProcessDrag(Vector2 change)
    {
        rect = new Rect(rect.x + change.x, rect.y +  change.y, rect.width, rect.height);
        rectSmall.position += change;
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
                if(e.keyCode == KeyCode.Delete && isSelected)
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
        if(scriptType != PengScript.PengScriptType.OnExecute)
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
        rectSmall = new Rect(pos.x, pos.y + 26, 240, 5 + 23 * paraNum);
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

public class OnExecute : PengNode
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

    public OnExecute(Vector2 pos, PengActorStateEditorWindow master, ref PengTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
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
        scriptType = PengScript.PengScriptType.OnExecute;
        nodeName = GetDescription(scriptType);


        paraNum = 2;

        
    }

    public override void Draw()
    {
        base.Draw();

        pengTrackExecuteFrame.DrawVar();
        pengStateExecuteFrame.DrawVar();
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

        pengAnimationName.DrawVar();
        pengHardCut.DrawVar();
        pengTransitionNormalizedTime.DrawVar();
        pengStartAtNormalizedTime.DrawVar();
        pengAnimationLayer.DrawVar();
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

    public List<PengBool> conditions = new List<PengBool>();
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
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengVar[1];
        outVars = new PengVar[0];

        type = NodeType.Branch;
        scriptType = PengScript.PengScriptType.IfElse;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        if (conditions.Count > 0)
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                conditions[i].DrawVar();
            }
        }
    }

    public override string SpecialParaDescription()
    {
        string result = "";
        if(conditions.Count > 0)
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                //result += 
            }
        }
        return result;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if(info != "")
        {

        }
    }

    public void AddConditions()
    {
        conditions.Add(new PengBool(this, "条件" + (conditions.Count + 1).ToString(), conditions.Count, ConnectionPointType.In));
        conditionTypes.Add(IfElseIfElse.ElseIf);
        PengVar[] newInVars = new PengVar[conditions.Count];
        for (int i = 0; i < conditions.Count - 1; i++)
        {
            newInVars[i] = inVars[i];
        }
        newInVars[conditions.Count - 1] = conditions[conditions.Count - 1];
        inVars = newInVars;
        PengNodeConnection[] newOutPoints = new PengNodeConnection[conditions.Count];
        for (int i = 0; i < conditions.Count - 1; i++)
        {
            newOutPoints[i] = outPoints[i];
        }
        newOutPoints[conditions.Count - 1] = new PengNodeConnection(ConnectionPointType.FlowOut, conditions.Count - 1, this, null);
        outPoints = newOutPoints;
        paraNum++;
    }
    
    public void RemoveConditions(int index)
    {
        if (conditions.Count > 1 && index > 0)
        {
            List<PengNodeConnectionLine> lines = new List<PengNodeConnectionLine>();

            PengVar[] newInVars = new PengVar[conditions.Count - 1];
            int j = 0;
            for (int i = 0; i < inVars.Length; i++)
            {
                if(i != index)
                {
                    newInVars[j] = inVars[i];
                    j++;
                }
            }
            for(int i = 0; i < trackMaster.lines.Count; i ++)
            {
                if(trackMaster.lines[i].inPoint == inVars[index].point && !lines.Contains(trackMaster.lines[i]))
                {
                    lines.Add(trackMaster.lines[i]);
                }
            }
            inVars = newInVars;

            PengNodeConnection[] newOutPoints = new PengNodeConnection[conditions.Count - 1];
            int k = 0;
            for (int i = 0; i < outPoints.Length; i++)
            {
                if(i != index)
                {
                    newOutPoints[k] = outPoints[i];
                    k++;
                }
            }
            for (int i = 0; i < trackMaster.lines.Count; i++)
            {
                if (trackMaster.lines[i].outPoint == outPoints[index] && !lines.Contains(trackMaster.lines[i]))
                {
                    lines.Add(trackMaster.lines[i]);
                }
            }
            outPoints = newOutPoints;

            for(int i = 0; i < lines.Count; i++)
            {
                trackMaster.lines.Remove(lines[i]);
            }

            paraNum--;
            conditions.RemoveAt(index);
            conditionTypes.RemoveAt(index);
        }
    }
}