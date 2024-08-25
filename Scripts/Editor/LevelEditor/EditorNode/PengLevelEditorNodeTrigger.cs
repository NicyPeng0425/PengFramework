using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelStart : PengLevelEditorNode
{
    public LevelStart(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
    {
        InitialDraw(pos, master);
        nodeID = id;
        outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
        varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
        varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
        meaning = "关卡逻辑起始节点。";

        inPoints = new PengLevelNodeConnection[0];
        outPoints = new PengLevelNodeConnection[1];
        outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
        inVars = new PengLevelNodeVariables[0];
        outVars = new PengLevelNodeVariables[0];

        type = PengLevelRuntimeFunction.LevelFunctionType.Start;
        nodeType = LevelNodeType.Trigger;
        name = GetDescription(type);

        paraNum = 1;
    }
}

public class TriggerWaitTime : PengLevelEditorNode
{
    public PengLevelInt waitFrame;
    public TriggerWaitTime(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
    {
        InitialDraw(pos, master);
        nodeID = id;
        outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
        varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
        varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
        meaning = "等待特定时间";

        inPoints = new PengLevelNodeConnection[1];
        inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
        outPoints = new PengLevelNodeConnection[1];
        outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
        inVars = new PengLevelNodeVariables[1];
        waitFrame = new PengLevelInt(this, "帧数", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
        inVars[0] = waitFrame;
        waitFrame.point = null;
        waitFrame.value = 1;
        outVars = new PengLevelNodeVariables[0];

        type = PengLevelRuntimeFunction.LevelFunctionType.TriggerWaitTime;
        nodeType = LevelNodeType.Trigger;
        name = GetDescription(type);

        paraNum = 1;
    }
    public override string SpecialParaDescription()
    {
        return waitFrame.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            waitFrame.value = int.Parse(info);
        }
    }

    public override void DrawInVarValue(int inVarID, Rect field)
    {
        waitFrame.value = EditorGUI.IntField(field, waitFrame.value);
        if (waitFrame.value <= 0)
        {
            waitFrame.value = 1;
        }
        name = "等待" + waitFrame.value + "帧";
    }
}
