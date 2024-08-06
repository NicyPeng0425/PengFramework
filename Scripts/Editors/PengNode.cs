using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.ComponentModel;
using System.Reflection;
using PengVariables;
using static UnityEditor.PlayerSettings;

public class PengNode
{
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

    public PengNodeConnection inPoint;
    public PengNodeConnection outPoint;
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
        rectSmall.height = 2 + 23 * paraNum;
        GUIStyle style3 = new GUIStyle("flow node 0" + (isSelected ? " on" : ""));
        GUI.Box(rectSmall, "", style3);

        switch (type)
        {
            case NodeType.Event:
                GUIStyle style = new GUIStyle("flow node 6" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style);
                outPoint.Draw(rect);
                break;
            case NodeType.Action:
                GUIStyle style1 = new GUIStyle("flow node 1" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style1);
                inPoint.Draw(rect);
                outPoint.Draw(rect);
                break;
            case NodeType.Branch:
                GUIStyle style2 = new GUIStyle("flow node 2" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style2);
                inPoint.Draw(rect);
                outPoint.Draw(rect);
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
        inPoint = new PengNodeConnection(ConnectionPointType.FlowIn, this, null);
        outPoint = new PengNodeConnection(ConnectionPointType.FlowOut, this, null);
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

    public virtual List<string> GetParaName()
    {
        List<string> list = new List<string>();
        return list;
    }

    public virtual List<string> GetParaValue()
    {
        List<string> list = new List<string>();
        return list;
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
}

public class OnExecute : PengNode
{
    public int trackExecuteFrame;
    public int stateExecuteFrame;

    public PengInt pengTrackExecuteFrame;
    public PengInt pengStateExecuteFrame;

    public OnExecute(Vector2 pos, PengActorStateEditorWindow master, PengTrack trackMaster, int nodeID) 
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;

        this.type = NodeType.Event;
        this.scriptType = PengScript.PengScriptType.OnExecute;
        this.nodeName = GetDescription(scriptType);

        paraNum = 2;

        pengTrackExecuteFrame = new PengInt(this, "当前轨道帧", 0, trackExecuteFrame, ConnectionPointType.Out);
        pengStateExecuteFrame = new PengInt(this, "当前状态帧", 1, stateExecuteFrame, ConnectionPointType.Out);
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
    public string animationName;
    public bool hardCut;
    public float transitionNormalizedTime;
    public float startAtNormalizedTime;
    public int animatorLayer;

    public PengString pengAnimationName;
    public PengBool pengHardCut;
    public PengFloat pengTransitionNormalizedTime;
    public PengFloat pengStartAtNormalizedTime;
    public PengInt pengAnimationLayer;


    public PlayAnimation(Vector2 pos, PengActorStateEditorWindow master, PengTrack trackMaster, int nodeID, string animationName, bool hardCut, float transitionNormalizedTime, float startAtNormalizedTime, int animatorLayer)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;

        this.type = NodeType.Action;
        this.scriptType = PengScript.PengScriptType.PlayAnimation;
        this.nodeName = GetDescription(scriptType);


        this.animationName = animationName;
        this.hardCut = hardCut;
        this.transitionNormalizedTime = transitionNormalizedTime;
        this.startAtNormalizedTime = startAtNormalizedTime;
        this.animatorLayer = animatorLayer;

        pengAnimationName = new PengString(this, "动画名称", 0, animationName, ConnectionPointType.In);
        pengHardCut = new PengBool(this, "是否硬切", 1, hardCut, ConnectionPointType.In);
        pengTransitionNormalizedTime = new PengFloat(this, "过渡时间", 2, transitionNormalizedTime, ConnectionPointType.In);
        pengStartAtNormalizedTime = new PengFloat(this, "开始时间", 3, startAtNormalizedTime, ConnectionPointType.In);
        pengAnimationLayer = new PengInt(this, "动画层", animatorLayer, 4, ConnectionPointType.In);

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

