using PengScript;
using PengVariables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PengLevelRuntimeLevelScriptVariables;

public class PengLevelNodeVariables
{
    public PengLevelEditorNodes.PengLevelEditorNode node;
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
            varRect = new Rect(node.rectSmall.x + 3f, node.rect.y + node.rect.height + 3f + 18 * index, node.rectSmall.width * 0.5f - 6f, 15);
        }
        else if (connectionType == PengLevelNodeConnection.PengLevelNodeConnectionType.VarOut)
        {
            style.alignment = TextAnchor.MiddleRight;
            varRect = new Rect(node.rectSmall.x + 0.5f * node.rectSmall.width + 3f, node.rect.y + node.rect.height + 3f + 18 * index, node.rectSmall.width * 0.5f - 6f, 15);
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
    public PengLevelPengActor(PengLevelEditorNodes.PengLevelEditorNode node, string name, int index, PengLevelNodeConnection.PengLevelNodeConnectionType pointType)
    {
        this.node = node;
        this.name = name;
        this.index = index;
        connectionType = pointType;
        type = PengLevelRuntimeLevelScriptVariableType.PengActor;
        point = new PengLevelNodeConnection(pointType, index, node, this);
    }
}

public class PengLevelListPengActor : PengLevelNodeVariables
{
    public List<PengActor> value = null;
    public PengLevelListPengActor(PengLevelEditorNodes.PengLevelEditorNode node, string name, int index, PengLevelNodeConnection.PengLevelNodeConnectionType pointType)
    {
        this.node = node;
        this.name = name;
        this.index = index;
        connectionType = pointType;
        type = PengLevelRuntimeLevelScriptVariableType.ListPengActor;
        point = new PengLevelNodeConnection(pointType, index, node, this);
    }
}

public class PengLevelString : PengLevelNodeVariables
{
    public string value = "";
    public PengLevelString(PengLevelEditorNodes.PengLevelEditorNode node, string name, int index, PengLevelNodeConnection.PengLevelNodeConnectionType pointType)
    {
        this.node = node;
        this.name = name;
        this.index = index;
        connectionType = pointType;
        type = PengLevelRuntimeLevelScriptVariableType.String;
        point = new PengLevelNodeConnection(pointType, index, node, this);
    }
}

public class PengLevelInt : PengLevelNodeVariables
{
    public int value;
    public PengLevelInt(PengLevelEditorNodes.PengLevelEditorNode node, string name, int index, PengLevelNodeConnection.PengLevelNodeConnectionType pointType)
    {
        this.node = node;
        this.name = name;
        this.index = index;
        connectionType = pointType;
        type = PengLevelRuntimeLevelScriptVariableType.Int;
        point = new PengLevelNodeConnection(pointType, index, node, this);
    }
}

public class PengLevelVector3 : PengLevelNodeVariables
{
    public Vector3 value;
    public PengLevelVector3(PengLevelEditorNodes.PengLevelEditorNode node, string name, int index, PengLevelNodeConnection.PengLevelNodeConnectionType pointType)
    {
        this.node = node;
        this.name = name;
        this.index = index;
        connectionType = pointType;
        type = PengLevelRuntimeLevelScriptVariableType.Vector3;
        point = new PengLevelNodeConnection(pointType, index, node, this);
    }
}