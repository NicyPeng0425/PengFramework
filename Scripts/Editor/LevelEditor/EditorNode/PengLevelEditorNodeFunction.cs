using PengScript;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

public class GenerateActor : PengLevelEditorNode
{
    public PengLevelInt actorID;
    public GenerateActor(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
    {
        InitialDraw(pos, master);
        nodeID = id;
        outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
        varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
        varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
        meaning = "生成Actor。";

        inPoints = new PengLevelNodeConnection[1];
        inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
        outPoints = new PengLevelNodeConnection[1];
        outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
        inVars = new PengLevelNodeVariables[1];
        actorID = new PengLevelInt(this, "角色ID", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
        inVars[0] = actorID;
        actorID.point = null;
        outVars = new PengLevelNodeVariables[1];
        outVars[0] = new PengLevelPengActor(this, "角色", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarOut);

        type = PengLevelRuntimeFunction.LevelFunctionType.GenerateActor;
        nodeType = LevelNodeType.Function;
        name = GetDescription(type);
        ReadSpecialParaDescription(specialInfo);
        paraNum = 1;
    }

    public override string SpecialParaDescription()
    {
        return actorID.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            actorID.value = int.Parse(info);
        }
    }

    public override void DrawInVarValue(int inVarID, Rect field)
    {
        actorID.value = EditorGUI.IntField(field, actorID.value);
    }
}

public class SetMainActor : PengLevelEditorNode
{
    public PengLevelPengActor actor;
    public SetMainActor(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
    {
        InitialDraw(pos, master);
        nodeID = id;
        outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
        varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
        varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
        meaning = "生成Actor。";

        inPoints = new PengLevelNodeConnection[1];
        inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
        outPoints = new PengLevelNodeConnection[1];
        outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
        inVars = new PengLevelNodeVariables[1];
        actor = new PengLevelPengActor(this, "角色", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
        inVars[0] = actor;
        outVars = new PengLevelNodeVariables[0];

        type = PengLevelRuntimeFunction.LevelFunctionType.SetMainActor;
        nodeType = LevelNodeType.Function;
        name = GetDescription(type);
        ReadSpecialParaDescription(specialInfo);
        paraNum = 1;
    }
}