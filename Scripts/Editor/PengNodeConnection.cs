using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;




public class PengNodeConnection
{
    public Rect rect;
    public PengScript.ConnectionPointType type;
    public PengEditorVariables.PengVar pengVar;
    public PengNode node;
    public int m_lineNum = 0;
    public int lineNum
    {
        get { return m_lineNum; }
        set
        {
            m_lineNum = value;
            if (m_lineNum <= 0)
            { m_lineNum = 0; }
        }
    }
    public int index;

    public PengNodeConnection(PengScript.ConnectionPointType type, int index, PengNode node, PengEditorVariables.PengVar pengVar)
    {
        this.type = type;
        this.node = node;
        if (type == PengScript.ConnectionPointType.In || type == PengScript.ConnectionPointType.Out)
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
            case PengScript.ConnectionPointType.In:
                this.rect.x = rect.x - this.rect.width;
                this.rect.height = 15f;
                break;
            case PengScript.ConnectionPointType.Out:
                this.rect.x = rect.x + rect.width;
                this.rect.height = 15f;
                break;
            case PengScript.ConnectionPointType.FlowIn:
                this.rect.x = rect.x - this.rect.width + 5f;
                break;
            case PengScript.ConnectionPointType.FlowOut:
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
                    switch(type)
                    {
                        case PengScript.ConnectionPointType.In:
                            if(node.master.selectingPoint.type == PengScript.ConnectionPointType.Out && (node.master.selectingPoint.pengVar.type == pengVar.type || pengVar.type == PengVariables.PengVarType.T))
                            {
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
                        case PengScript.ConnectionPointType.Out:
                            if (node.master.selectingPoint.type == PengScript.ConnectionPointType.In && (node.master.selectingPoint.pengVar.type == pengVar.type || node.master.selectingPoint.pengVar.type == PengVariables.PengVarType.T))
                            {
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
                        case PengScript.ConnectionPointType.FlowIn:
                            if (node.master.selectingPoint.type == PengScript.ConnectionPointType.FlowOut && node.master.selectingPoint.lineNum == 0)
                            {
                                node.master.selectingPoint.node.outID[node.master.selectingPoint.index] = node.nodeID;
                            }
                            break;
                        case PengScript.ConnectionPointType.FlowOut:
                            if (node.master.selectingPoint.type == PengScript.ConnectionPointType.FlowIn && lineNum == 0)
                            {
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
