using PengScript;
using PengVariables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PengLevelRuntimeLevelScriptVariables;

public class PengLevelNodeVariables
{
    public PengLevelEditorNode node;
    public string name;
    public PengLevelRuntimeLevelScriptVariables.PengLevelRuntimeLevelScriptVariableType type;
    public Rect varRect;
    public PengLevelNodeConnection point;
    public int index;
    public PengLevelNodeConnection.PengLevelNodeConnectionType connectionType;

    public virtual void DrawVar()
    {
        GUIStyle style = new GUIStyle("DD Background");
        style.fontSize = 9;
        if (connectionType == PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn)
        {
            style.alignment = TextAnchor.MiddleLeft;
            varRect = new Rect(node.rectSmall.x + 3f, node.rect.y + node.rect.height + 3f + 18 * index, node.rectSmall.width - 6f, 15);
        }
        else if (connectionType == PengLevelNodeConnection.PengLevelNodeConnectionType.VarOut)
        {
            style.alignment = TextAnchor.MiddleRight;
            varRect = new Rect(node.rectSmall.x + 0.5f * node.rectSmall.width + 3f, node.rect.y + node.rect.height + 3f + 18 * index, node.rectSmall.width - 6f, 15);
        }
        GUI.Box(varRect, " " + name + "(" + type.ToString() + ")", style);
        if (point != null)
        {
            point.Draw(varRect);
        }
    }
}

public class PengLevelPengActor : PengLevelNodeVariables
{
    public PengActor value = null;
    public PengLevelPengActor(PengLevelEditorNode node, string name, int index, PengLevelNodeConnection.PengLevelNodeConnectionType pointType)
    {
        this.node = node;
        this.name = name;
        this.index = index;
        connectionType = pointType;
        type = PengLevelRuntimeLevelScriptVariableType.PengActor;
        point = new PengLevelNodeConnection(pointType, index, node, this);
    }
}