using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PengNode
{
    public string nodeName = "Ä¬ÈÏ";
    public Rect rect;
    public Rect rectSmall;
    private bool isDragged;
    private bool m_isSelected;
    public bool isSelected
    {
        get { return m_isSelected; }
        set { m_isSelected = value; OnSelectedChange(value); }
    }

    public PengNodeConnection inPoint;
    public PengNodeConnection outPoint;
    public PengActorStateEditorWindow master;

    public enum NodeType
    {
        Event,
        Action,
        Conditional,
    }

    public NodeType type;


    public PengNode(Vector2 pos, string name, PengActorStateEditorWindow master, NodeType type)
    {
        rect = new Rect(pos.x, pos.y, 160, 26);
        rectSmall = new Rect(pos.x, pos.y + 26, 160, 80);
        nodeName = name;
        this.master = master;
        inPoint = new PengNodeConnection(ConnectionPointType.In, this);
        outPoint = new PengNodeConnection(ConnectionPointType.Out, this);
        this.type = type;
    }

    public void Draw()
    {
        GUIStyle style3 = new GUIStyle("flow node 0" + (isSelected ? " on" : ""));
        GUI.Box(rectSmall, nodeName, style3);

        switch (type)
        {
            case NodeType.Event:
                GUIStyle style = new GUIStyle("flow node 6" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style);
                outPoint.Draw();
                break;
            case NodeType.Action:
                GUIStyle style1 = new GUIStyle("flow node 1" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style1);
                inPoint.Draw();
                outPoint.Draw();
                break;
            case NodeType.Conditional:
                GUIStyle style2 = new GUIStyle("flow node 2" + (isSelected ? " on" : ""));
                GUI.Box(rect, nodeName, style2);
                inPoint.Draw();
                outPoint.Draw();
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
                    if(!master.timeLineRect.Contains(e.mousePosition))
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
                    if (!master.timeLineRect.Contains(e.mousePosition))
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
                if (!master.timeLineRect.Contains(e.mousePosition))
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
        }
        return false;
    }

    private void RightMouseMenu()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("É¾³ý½Úµã"), false, ()=> master.ProcessRemoveNode(this));
        menu.ShowAsContext();
    }

    private void OnSelectedChange(bool changeTo)
    {

    }
}
