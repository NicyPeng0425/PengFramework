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
    public bool inOccupied = false;

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
                            if (node.editor.selectingPoint.type == AINodeConnectionType.Out && !inOccupied)
                            {
                                node.editor.selectingPoint.node.outID[node.editor.selectingPoint.index] = node.nodeID;
                                inOccupied = true;
                            }
                            break;
                        case AINodeConnectionType.Out:
                            if (node.editor.selectingPoint.type == AINodeConnectionType.In && !node.editor.selectingPoint.inOccupied)
                            {
                                node.outID[index] = node.editor.selectingPoint.node.nodeID;
                                node.editor.selectingPoint.inOccupied = true;
                            }
                            break;
                    }
                    node.editor.selectingPoint = null;
                }
            }
        }
    }
}
