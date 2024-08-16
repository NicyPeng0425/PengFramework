using PengScript;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForIterator : PengNode
{
    public PengEditorVariables.PengInt firstIndex;
    public PengEditorVariables.PengInt lastIndex;

    public PengEditorVariables.PengInt pengIndex;
    public ForIterator(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);
        this.ReadSpecialParaDescription(specialInfo);

        inPoints = new PengNodeConnection[2];
        inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        inPoints[1] = new PengNodeConnection(ConnectionPointType.FlowIn, 1, this, null);
        outPoints = new PengNodeConnection[2];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        outPoints[1] = new PengNodeConnection(ConnectionPointType.FlowOut, 1, this, null);

        inVars = new PengEditorVariables.PengVar[this.varInID.Count];
        paraNum = this.varInID.Count;
        outVars = new PengEditorVariables.PengVar[1];
        firstIndex = new PengEditorVariables.PengInt(this, "首个指数", 0, ConnectionPointType.In);
        lastIndex = new PengEditorVariables.PengInt(this, "末个指数", 1, ConnectionPointType.In);
        pengIndex = new PengEditorVariables.PengInt(this, "指数", 0, ConnectionPointType.Out);
        inVars[0] = firstIndex;
        inVars[1] = lastIndex;
        outVars[0] = pengIndex;

        type = NodeType.Iterator;
        scriptType = PengScript.PengScriptType.ForIterator;
        nodeName = GetDescription(scriptType);
    }

    public override void Draw()
    {
        base.Draw();
        GUIStyle style = new GUIStyle("CN EntryInfo");
        style.fontSize = 12;
        style.alignment = TextAnchor.UpperRight;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        GUIStyle style2 = new GUIStyle("CN EntryInfo");
        style2.fontSize = 12;
        style2.alignment = TextAnchor.UpperLeft;
        style2.fontStyle = FontStyle.Bold;
        style2.normal.textColor = Color.white;

        Rect loopBody = new Rect(outPoints[0].rect.x - 80, outPoints[0].rect.y, 70, 20);
        Rect completed = new Rect(outPoints[1].rect.x - 80, outPoints[1].rect.y, 70, 20);

        Rect enter = new Rect(inPoints[0].rect.x - 10, inPoints[0].rect.y, 70, 20);
        Rect breakin = new Rect(inPoints[1].rect.x - 10, inPoints[1].rect.y, 70, 20);

        GUI.Box(loopBody, "循环体", style);
        GUI.Box(completed, "完成后", style);
        GUI.Box(enter, "进入", style2);
        GUI.Box(breakin, "打断", style2);
    }
}

