using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PengLevelNodeConnection;

public class PengAIEditorNodeConnection
{
    public enum AINodeConnectionType
    {
        In,
        Out,
    }

    public Rect rect;
    public AINodeConnectionType type;
    public PengAIEditorNode.PengAIEditorNode node;
    public int index;

    public PengAIEditorNodeConnection(AINodeConnectionType type, int index, PengAIEditorNode.PengAIEditorNode node)
    {
        this.type = type;
        this.node = node;
        this.index = index;
        rect = new Rect(0f, 0f, 20f, 15f);
    }

    public void Draw(Rect rect, int total, int index)
    {
        this.rect.x = rect.x + rect.width * ((float)(index + 1) / (float)(total + 1)) - this.rect.width * 0.5f;

        switch (type)
        {
            case AINodeConnectionType.In:
                this.rect.y = rect.y - this.rect.height * 0.5f;
                break;
            case AINodeConnectionType.Out:
                this.rect.y = rect.y + rect.height - this.rect.height * 0.5f;
                break;
        }

        if (GUI.Button(this.rect, total > 1 ? index.ToString():""))
        {
            if (node.editor.selectingPoint == null)
            {
                node.editor.selectingPoint = this;
            }
            else
            {
                if (node.editor.selectingPoint.type != this.type && node.editor.selectingPoint.node != node)
                {
                    switch (type)
                    {
                        case AINodeConnectionType.In:
                            if (node.editor.selectingPoint.type == AINodeConnectionType.Out)
                            {
                                for (int i = 0; i < node.editor.nodes.Count; i++)
                                {
                                    if (node.editor.nodes[i].outID.Count > 0)
                                    {
                                        for (int j = 0; j < node.editor.nodes[i].outID.Count; j++)
                                        {
                                            if (node.editor.nodes[i].outID[j] == node.nodeID)
                                            {
                                                node.editor.nodes[i].outID[j] = -1;
                                            }
                                        }
                                    }
                                }
                                node.editor.selectingPoint.node.outID[node.editor.selectingPoint.index] = node.nodeID;
                            }
                            break;
                        case AINodeConnectionType.Out:
                            if (node.editor.selectingPoint.type == AINodeConnectionType.In)
                            {
                                for (int i = 0; i < node.editor.nodes.Count; i++)
                                {
                                    if (node.editor.nodes[i].outID.Count > 0)
                                    {
                                        for (int j = 0; j < node.editor.nodes[i].outID.Count; j++)
                                        {
                                            if (node.editor.nodes[i].outID[j] == node.editor.selectingPoint.node.nodeID)
                                            {
                                                node.editor.nodes[i].outID[j] = -1;
                                            }
                                        }
                                    }
                                }
                                node.outID[index] = node.editor.selectingPoint.node.nodeID;
                            }
                            break;
                    }
                    node.editor.selectingPoint = null;
                }
            }
        }
    }
}
