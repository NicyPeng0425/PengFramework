using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PengNodeConnectionLine
{
    public PengNodeConnection inPoint;
    public PengNodeConnection outPoint;

    public PengNodeConnectionLine(PengNodeConnection inPoint, PengNodeConnection outPoint)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        inPoint.lineNum++;
        outPoint.lineNum++;
    }

    public void Draw()
    {
        if (inPoint.type == ConnectionPointType.In) { Handles.DrawBezier(inPoint.rect.center, outPoint.rect.center, inPoint.rect.center + Vector2.left * 40f, outPoint.rect.center - Vector2.left * 40f, Color.white, null, 3f); }
        else if (inPoint.type == ConnectionPointType.FlowIn) { Handles.DrawBezier(inPoint.rect.center, outPoint.rect.center, inPoint.rect.center + Vector2.left * 40f, outPoint.rect.center - Vector2.left * 40f, Color.white, null, 6f); }
        
        Vector2 buttonSize = new Vector2(20, 20);
        Vector2 lineCenter = (inPoint.rect.center + outPoint.rect.center) * 0.5f;

        if(GUI.Button(new Rect(lineCenter - buttonSize/2, buttonSize), "¡Á"))
        {
            inPoint.node.trackMaster.lines.Remove(this);
            inPoint.lineNum--;
            outPoint.lineNum--;
        }
    }
}
