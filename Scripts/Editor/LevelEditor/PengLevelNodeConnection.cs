using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengLevelNodeConnection
{
    public enum PengLevelNodeConnectionType
    {
        FlowIn,
        FlowOut,
        VarIn,
        VarOut,
    }

    public Rect rect;
    public PengLevelNodeConnectionType connectionType;
    public PengLevelNodeVariables var;
    public PengLevelEditorNodes.PengLevelEditorNode node;
    public int index;

    public PengLevelNodeConnection(PengLevelNodeConnectionType type, int index, PengLevelEditorNodes.PengLevelEditorNode node, PengLevelNodeVariables pengVar)
    {
        this.connectionType = type;
        this.node = node;
        if (type == PengLevelNodeConnectionType.VarIn || type == PengLevelNodeConnectionType.VarOut)
        {
            this.var = pengVar;
        }
        this.index = index;
        rect = new Rect(0f, 0f, 12f, 12f);
    }

    public void Draw(Rect rect)
    {
        this.rect.y = rect.y + rect.height * 0.5f - this.rect.height * 0.5f + 1f;

        switch (connectionType)
        {
            case PengLevelNodeConnectionType.VarIn:
                this.rect.x = rect.x - this.rect.width;
                this.rect.height = 12f;
                break;
            case PengLevelNodeConnectionType.VarOut:
                this.rect.x = rect.x + rect.width;
                this.rect.height = 12f;
                break;
            case PengLevelNodeConnectionType.FlowIn:
                this.rect.x = rect.x - this.rect.width + 5f;
                break;
            case PengLevelNodeConnectionType.FlowOut:
                this.rect.x = rect.x + rect.width - 5f;
                break;
        }

        if (GUI.Button(this.rect, ""))
        {
            if (node.editor.selectingPoint == null)
            {
                node.editor.selectingPoint = this;
            }
            else
            {
                if (node.editor.selectingPoint.connectionType != this.connectionType && node.editor.selectingPoint.node != node)
                {
                    switch (connectionType)
                    {
                        case PengLevelNodeConnectionType.VarIn:
                            if (node.editor.selectingPoint.connectionType == PengLevelNodeConnectionType.VarOut &&
                                (node.editor.selectingPoint.var.type == var.type))
                            {
                                PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID thisConnection = new PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID();
                                thisConnection.nodeID = node.nodeID;
                                thisConnection.connectionID = index;

                                PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID selectingConnection = new PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID();
                                selectingConnection.nodeID = node.editor.selectingPoint.node.nodeID;
                                selectingConnection.connectionID = node.editor.selectingPoint.index;

                                node.varInID[index] = selectingConnection;
                                node.editor.selectingPoint.node.varOutID[node.editor.selectingPoint.index].Add(thisConnection);
                            }
                            break;
                        case PengLevelNodeConnectionType.VarOut:
                            if (node.editor.selectingPoint.connectionType == PengLevelNodeConnectionType.VarIn && 
                                (node.editor.selectingPoint.var.type == var.type))
                            {
                                PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID thisConnection = new PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID();
                                thisConnection.nodeID = node.nodeID;
                                thisConnection.connectionID = index;

                                PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID selectingConnection = new PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID();
                                selectingConnection.nodeID = node.editor.selectingPoint.node.nodeID;
                                selectingConnection.connectionID = node.editor.selectingPoint.index;

                                node.varOutID[index].Add(selectingConnection);
                                node.editor.selectingPoint.node.varInID[node.editor.selectingPoint.index] = thisConnection;
                            }
                            break;
                        case PengLevelNodeConnectionType.FlowIn:
                            if (node.editor.selectingPoint.connectionType == PengLevelNodeConnectionType.FlowOut)
                            {
                                PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID nici = new PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID();
                                nici.nodeID = node.nodeID;
                                nici.connectionID = index;
                                node.editor.selectingPoint.node.outID[node.editor.selectingPoint.index] = nici;
                            }
                            break;
                        case PengLevelNodeConnectionType.FlowOut:
                            if (node.editor.selectingPoint.connectionType == PengLevelNodeConnectionType.FlowIn)
                            {
                                PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID nici = new PengLevelEditorNodes.PengLevelEditorNode.NodeIDConnectionID();
                                nici.nodeID = node.editor.selectingPoint.node.nodeID;
                                nici.connectionID = node.editor.selectingPoint.index;
                                node.outID[index] = nici;
                            }
                            break;
                    }
                    node.editor.selectingPoint = null;
                }
            }
        }
    }
}
