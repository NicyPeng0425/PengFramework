using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


public enum ConnectionPointType
{
    In,
    Out,
    FlowIn,
    FlowOut,
}

public class PengNodeConnection
{
    public Rect rect;
    public ConnectionPointType type;
    public PengVariables.PengVar pengVar;
    public PengNode node;
    public int lineNum = 0;
    public int index;

    public PengNodeConnection(ConnectionPointType type, int index, PengNode node, PengVariables.PengVar pengVar)
    {
        this.type = type;
        this.node = node;
        if (type == ConnectionPointType.In || type == ConnectionPointType.Out)
        {
            this.pengVar = pengVar;
        }
        this.index = index;
        rect = new Rect(0f, 0f, 12f, 18f);
    }

    public void Draw(Rect rect)
    {
        this.rect.y = rect.y + rect.height * 0.5f - this.rect.height * 0.5f + 1f;

        switch (type)
        {
            case ConnectionPointType.In:
                this.rect.x = rect.x - this.rect.width;
                this.rect.height = 15f;
                break;
            case ConnectionPointType.Out:
                this.rect.x = rect.x + rect.width;
                this.rect.height = 15f;
                break;
            case ConnectionPointType.FlowIn:
                this.rect.x = rect.x - this.rect.width + 5f;
                break;
            case ConnectionPointType.FlowOut:
                this.rect.x = rect.x + rect.width - 5f;
                break;
        }

        if(GUI.Button(this.rect, ""))
        {
            if (node.master.selectingPoint == null)
            {
                node.master.selectingPoint = this;
            }
            else
            {
                if (node.master.selectingPoint.type != this.type && node.master.selectingPoint.node != node)
                {
                    bool sameLine = false;
                    foreach(PengNodeConnectionLine line in node.trackMaster.lines)
                    {
                        if((line.outPoint == node.master.selectingPoint && line.inPoint == this) || (line.outPoint == this && line.inPoint == node.master.selectingPoint))
                        {
                            sameLine = true;
                        }
                    }
                    if (sameLine)
                        return;
                    switch(type)
                    {
                        case ConnectionPointType.In:
                            if(node.master.selectingPoint.type == ConnectionPointType.Out && lineNum == 0 && node.master.selectingPoint.pengVar.type == pengVar.type)
                            {
                                node.trackMaster.lines.Add(new PengNodeConnectionLine(this, node.master.selectingPoint));

                                PengNode.NodeIDConnectionID thisConnection = new PengNode.NodeIDConnectionID();
                                thisConnection.nodeID = node.nodeID;
                                thisConnection.connectionID = index;

                                PengNode.NodeIDConnectionID selectingConnection = new PengNode.NodeIDConnectionID();
                                selectingConnection.nodeID = node.master.selectingPoint.node.nodeID;
                                selectingConnection.connectionID = node.master.selectingPoint.index;

                                node.varInID[index] = selectingConnection;
                                node.master.selectingPoint.node.varOutID[node.master.selectingPoint.index].Add(thisConnection);
                            }
                            break;
                        case ConnectionPointType.Out:
                            if (node.master.selectingPoint.type == ConnectionPointType.In && node.master.selectingPoint.pengVar.type == pengVar.type)
                            {
                                node.trackMaster.lines.Add(new PengNodeConnectionLine(node.master.selectingPoint, this));

                                PengNode.NodeIDConnectionID thisConnection = new PengNode.NodeIDConnectionID();
                                thisConnection.nodeID = node.nodeID;
                                thisConnection.connectionID = index;

                                PengNode.NodeIDConnectionID selectingConnection = new PengNode.NodeIDConnectionID();
                                selectingConnection.nodeID = node.master.selectingPoint.node.nodeID;
                                selectingConnection.connectionID = node.master.selectingPoint.index;

                                node.varOutID[index].Add(selectingConnection);
                                node.master.selectingPoint.node.varInID[node.master.selectingPoint.index] = thisConnection;
                            }
                            break;
                        case ConnectionPointType.FlowIn:
                            if (node.master.selectingPoint.type == ConnectionPointType.FlowOut && node.master.selectingPoint.lineNum == 0)
                            {
                                node.trackMaster.lines.Add(new PengNodeConnectionLine(this, node.master.selectingPoint));

                                node.master.selectingPoint.node.outID[node.master.selectingPoint.index] = node.nodeID;
                            }
                            break;
                        case ConnectionPointType.FlowOut:
                            if (node.master.selectingPoint.type == ConnectionPointType.FlowIn && lineNum == 0)
                            {
                                node.trackMaster.lines.Add(new PengNodeConnectionLine(node.master.selectingPoint, this));

                                node.outID[index] = node.master.selectingPoint.node.nodeID;
                            }
                            break;
                    }
                    node.master.selectingPoint = null;
                }
            }
        }
    }
}
