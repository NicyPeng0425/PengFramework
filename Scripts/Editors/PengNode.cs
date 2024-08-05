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
    public Rect rect;
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

    public enum NodeType
    {
        Event,
        Action,
        Conditional,
        Value,
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
            case NodeType.Conditional:
                GUIStyle style2 = new GUIStyle("flow node 2" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style2);
                inPoint.Draw(rect);
                outPoint.Draw(rect);
                break;
        }
    }

    public void ProcessDrag(Vector2 change)
    {
        rect.position += change;
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
        menu.AddItem(new GUIContent("删除节点"), false, ()=> master.ProcessRemoveNode(this));
        menu.ShowAsContext();
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
}

public class OnExecute : PengNode
{
    public int trackExecuteFrame;
    public int stateExecuteFrame;

    public PengInt pengTrackExecuteFrame;
    public PengInt pengStateExecuteFrame;

    public OnExecute(Vector2 pos, PengActorStateEditorWindow master, PengTrack trackMaster) 
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;

        this.type = NodeType.Event;
        this.scriptType = PengScript.PengScriptType.OnExecute;
        this.nodeName = GetDescription(scriptType);

        paraNum = 2;

        pengTrackExecuteFrame = new PengInt(this, "当前轨道帧", trackExecuteFrame, ConnectionPointType.Out);
        pengStateExecuteFrame = new PengInt(this, "当前状态帧", stateExecuteFrame, ConnectionPointType.Out);
    }

    public override void Draw()
    {
        base.Draw();

        pengTrackExecuteFrame.DrawVar(false, 0);
        pengStateExecuteFrame.DrawVar(false, 1);
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


    public PlayAnimation(Vector2 pos, PengActorStateEditorWindow master, PengTrack trackMaster, string animationName, bool hardCut, float transitionNormalizedTime, float startAtNormalizedTime, int animatorLayer)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;

        this.type = NodeType.Action;
        this.scriptType = PengScript.PengScriptType.PlayAnimation;
        this.nodeName = GetDescription(scriptType);


        this.animationName = animationName;
        this.hardCut = hardCut;
        this.transitionNormalizedTime = transitionNormalizedTime;
        this.startAtNormalizedTime = startAtNormalizedTime;
        this.animatorLayer = animatorLayer;

        pengAnimationName = new PengString(this, "动画名称", animationName, ConnectionPointType.In);
        pengHardCut = new PengBool(this, "是否硬切", hardCut, ConnectionPointType.In);
        pengTransitionNormalizedTime = new PengFloat(this, "过渡时间", transitionNormalizedTime, ConnectionPointType.In);
        pengStartAtNormalizedTime = new PengFloat(this, "开始时间", startAtNormalizedTime, ConnectionPointType.In);
        pengAnimationLayer = new PengInt(this, "动画层", animatorLayer, ConnectionPointType.In);

        paraNum = 5;
    }

    public override void Draw()
    {
        base.Draw();

        pengAnimationName.DrawVar(true, 0);
        pengHardCut.DrawVar(true, 1);
        pengTransitionNormalizedTime.DrawVar(true, 2);
        pengStartAtNormalizedTime.DrawVar(true, 3);
        pengAnimationLayer.DrawVar(true, 4);
    }
}

