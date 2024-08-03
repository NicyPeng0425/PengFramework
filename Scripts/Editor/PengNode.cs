using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PengNode
{
    public string nodeName = "Ä¬ÈÏ";
    public Rect rect;
    private bool isDragged;
    public bool isSelected;

    public PengNodeConnection inPoint;
    public PengNodeConnection outPoint;
    public PengActorStateEditorWindow master;

    public PengNode(Vector2 pos, string name, PengActorStateEditorWindow master)
    {
        rect = new Rect(pos.x, pos.y, 160, 40);
        nodeName = name;
        this.master = master;
        inPoint = new PengNodeConnection(ConnectionPointType.In, this);
        outPoint = new PengNodeConnection(ConnectionPointType.Out, this);
    }

    public void Draw()
    {
        GUIStyle style = new GUIStyle("flow node 0" + (isSelected? " on" : ""));
        GUI.Box(rect, nodeName, style);

        inPoint.Draw();
        outPoint.Draw();
    }

    public void ProcessDrag(Vector2 change)
    {
        rect.position += change;
    }

    public bool ProcessEvents(Event e) 
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
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
                if (e.button == 1)
                {
                    if (rect.Contains(e.mousePosition) && isSelected)
                    {
                        RightMouseMenu();
                        e.Use();
                        GUI.changed = true;
                    }
                }
                break;
            case EventType.MouseUp:
                isDragged = false;
                break;
            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    ProcessDrag(e.delta);
                    e.Use();
                    return true;
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
}
