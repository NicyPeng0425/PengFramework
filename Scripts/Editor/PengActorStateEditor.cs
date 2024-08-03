using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class PengActorStateEditorWindow : EditorWindow
{
    /// <summary>
    /// 节点图部分主要参考：
    /// https://blog.csdn.net/qq_31967569/article/details/81025419
    /// https://blog.csdn.net/u013412391/article/details/120873714
    /// 感谢两位大佬！
    /// </summary>
    public List<PengNode> nodes = new List<PengNode>();
    public List<PengNodeConnectionLine> lines = new List<PengNodeConnectionLine>();
    public PengNodeConnection selectingPoint;
    private Vector2 gridOffset;

    [MenuItem("PengFramework/StateEditor")]
    static void Init()
    {
        PengActorStateEditorWindow window = (PengActorStateEditorWindow)EditorWindow.GetWindow(typeof(PengActorStateEditorWindow));
        window.position = new Rect(100, 100, 1000, 800);
        window.titleContent = new GUIContent("彭框架角色状态编辑器");
    }

    private void OnEnable()
    {
        nodes.Add(new PengNode(new Vector2(0, 0), "默认节点", this));
    }

    private void OnGUI()
    {
        DrawNodeGraph(nodes);
        ProcessEvents(Event.current);
        DrawPendingConnectionLine(Event.current);

        if (GUI.changed)
        {
            Repaint();
        }
    }

    public static void CreateStateXML(string id, string stateName, int length)
    {
        XmlDocument xml = new XmlDocument();
        XmlElement data = xml.CreateElement("Data");
        XmlElement info = xml.CreateElement("Info");
        XmlElement scripts = xml.CreateElement("Script");

        info.SetAttribute("Name", stateName);
        info.SetAttribute("Loop", stateName == "Idle" ? "1" : "0");
        info.SetAttribute("Length", length.ToString());

        //存一个播放动画的脚本

        data.AppendChild(info);
        data.AppendChild(scripts);
        xml.AppendChild(data);
        xml.Save(Application.dataPath + "Resources/ActorData/" + id + "/" + id + "@" + stateName + ".xml");
    }

    public static void SaveStateXML(string id, string stateName)
    {

    }

    public void DrawNodeGraph(List<PengNode> nodes)
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);
        DrawNodes();
        DrawConnectionLines();
    }

    private void ProcessEvents(Event e)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            bool dragged = nodes[i].ProcessEvents(e);
            if (dragged)
            {
                GUI.changed = true;
            }
        }

        switch (e.type)
        {
            case EventType.MouseDown:
                if(e.button == 1)
                {
                    RightMouseMenu(e.mousePosition);
                }
                if (e.button == 0)
                {
                    selectingPoint = null;
                }
                break;
            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    DragAllNodes(e.delta);
                    gridOffset += e.delta;
                    GUI.changed = true;
                }
                break;
        }
    }

    private void RightMouseMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("添加节点"), false, () => { ProcessAddNode(mousePos); });
        menu.ShowAsContext();
    }

    private void ProcessAddNode(Vector2 mousePos)
    {
        nodes.Add(new PengNode(mousePos, "默认节点", this));
    }

    private void DrawNodes()
    {
        if (nodes.Count > 0)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
    }

    private void DrawConnectionLines()
    {
        if (lines.Count > 0)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].Draw();
            }
        }
    }

    private void DrawPendingConnectionLine(Event e)
    {
        if (selectingPoint != null)
        {
            Vector3 start = (selectingPoint.type == ConnectionPointType.In) ? selectingPoint.rect.center : e.mousePosition;
            Vector3 end = (selectingPoint.type == ConnectionPointType.In) ? e.mousePosition : selectingPoint.rect.center;
            Handles.DrawBezier(start, end, start + Vector3.left * 40f, end - Vector3.left * 40f, Color.white, null, 3f);

            GUI.changed = true;

        }
    }

    public void ProcessRemoveNode(PengNode node)
    {
        List<PengNodeConnectionLine> toRemove = new List<PengNodeConnectionLine>();
        if (lines.Count > 0)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].inPoint == node.inPoint || lines[i].outPoint == node.outPoint)
                {  toRemove.Add(lines[i]); }
            }

            for(int i = 0; i < toRemove.Count;i++)
            {
                lines.Remove(toRemove[i]);
            }
        }
        toRemove = null;
        nodes.Remove(node);
    }

    private void DragAllNodes(Vector2 change)
    {
        if (nodes.Count > 0)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].ProcessDrag(change);
            }
        }
    }
    
    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDiv = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDiv = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        {
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
            Vector3 go = new Vector3(gridOffset.x % gridSpacing, gridOffset.y % gridSpacing, 0);

            for (int i = 0; i < widthDiv; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, - gridSpacing, 0) + go, new Vector3(gridSpacing * i, position.height + gridSpacing, 0f) + go);
            }
            for (int i = 0; i < widthDiv; i++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * i, 0) + go, new Vector3(position.width + gridSpacing, gridSpacing * i, 0f) + go);
            }
            Handles.color = Color.white;
        }
        Handles.EndGUI();
    }

}
