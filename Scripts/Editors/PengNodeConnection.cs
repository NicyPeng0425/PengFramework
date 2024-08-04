using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ConnectionPointType
{
    In,
    Out,
}

public class PengNodeConnection
{
    public Rect rect;
    public ConnectionPointType type;
    public PengNode node;

    public PengNodeConnection(ConnectionPointType type, PengNode node)
    {
        this.type = type;
        this.node = node;
        rect = new Rect(0f, 0f, 12f, 18f);
    }

    public void Draw()
    {
        rect.y = node.rect.y + node.rect.height * 0.5f - rect.height * 0.5f + 1f;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = node.rect.x - rect.width + 5f;
                break;
            case ConnectionPointType.Out:
                rect.x = node.rect.x + node.rect.width - 7f;
                break;
        }

        if(GUI.Button(rect, ""))
        {
            if (node.master.selectingPoint == null)
            {
                node.master.selectingPoint = this;
            }
            else
            {
                if (node.master.selectingPoint.type != this.type)
                {
                    if (type == ConnectionPointType.In)
                    {
                        node.master.lines.Add(new PengNodeConnectionLine(this, node.master.selectingPoint));
                    }
                    else
                    {
                        node.master.lines.Add(new PengNodeConnectionLine(node.master.selectingPoint, this));
                    }
                    node.master.selectingPoint = null;
                }
            }
        }
    }
}
