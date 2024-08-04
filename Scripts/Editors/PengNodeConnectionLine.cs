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
    }

    public void Draw()
    {
        Handles.DrawBezier(inPoint.rect.center, outPoint.rect.center, inPoint.rect.center + Vector2.left * 40f, outPoint.rect.center - Vector2.left * 40f, Color.white, null, 3f);
        Vector2 buttonSize = new Vector2(20, 20);
        Vector2 lineCenter = (inPoint.rect.center + outPoint.rect.center) * 0.5f;

        if(GUI.Button(new Rect(lineCenter - buttonSize/2, buttonSize), "¡Á"))
        {
            inPoint.node.master.lines.Remove(this);
        }
    }
}
