using System.Collections;
using System.Collections.Generic;
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

    public PengNodeConnection(ConnectionPointType type, PengNode node, PengVariables.PengVar pengVar)
    {
        this.type = type;
        this.node = node;
        this.pengVar = pengVar;
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
                    switch(type)
                    {
                        case ConnectionPointType.In:
                            if(node.master.selectingPoint.type == ConnectionPointType.Out && lineNum == 0 && node.master.selectingPoint.pengVar.type == pengVar.type)
                            {
                                node.trackMaster.lines.Add(new PengNodeConnectionLine(this, node.master.selectingPoint));
                            }
                            break;
                        case ConnectionPointType.Out:
                            if (node.master.selectingPoint.type == ConnectionPointType.In && node.master.selectingPoint.pengVar.type == pengVar.type)
                            {
                                node.trackMaster.lines.Add(new PengNodeConnectionLine(node.master.selectingPoint, this));
                            }
                            break;
                        case ConnectionPointType.FlowIn:
                            if (node.master.selectingPoint.type == ConnectionPointType.FlowOut && node.master.selectingPoint.lineNum == 0)
                            {
                                node.trackMaster.lines.Add(new PengNodeConnectionLine(this, node.master.selectingPoint));
                            }
                            break;
                        case ConnectionPointType.FlowOut:
                            if (node.master.selectingPoint.type == ConnectionPointType.FlowIn && lineNum == 0)
                            {
                                node.trackMaster.lines.Add(new PengNodeConnectionLine(node.master.selectingPoint, this));
                            }
                            break;
                    }
                    node.master.selectingPoint = null;
                }
            }
        }
    }
}
