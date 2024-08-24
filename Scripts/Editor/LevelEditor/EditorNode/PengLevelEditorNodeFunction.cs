using PengScript;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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