using PengScript;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OnTrackExecute : PengNode
{
    //Node描述文件
    //nodeName 名字
    //scriptType 脚本类型
    //pos 节点图上的位置
    //paraNum 参数数量
    //nodeID 脚本ID
    //varOutID 参数出信息
    //varInID 参数入信息
    //outID 脚本流出信息
    //参数信息
    public PengEditorVariables.PengInt pengTrackExecuteFrame;
    public PengEditorVariables.PengInt pengStateExecuteFrame;

    public OnTrackExecute(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoints = new PengNodeConnection[1];
        inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[0];
        outVars = new PengEditorVariables.PengVar[2];
        pengTrackExecuteFrame = new PengEditorVariables.PengInt(this, "当前轨道帧", 0, ConnectionPointType.Out);
        pengStateExecuteFrame = new PengEditorVariables.PengInt(this, "当前状态帧", 1, ConnectionPointType.Out);
        outVars[0] = pengTrackExecuteFrame;
        outVars[1] = pengStateExecuteFrame;

        type = NodeType.Event;
        scriptType = PengScript.PengScriptType.OnTrackExecute;
        nodeName = GetDescription(scriptType);

        paraNum = 2;

    }

    public override void Draw()
    {
        base.Draw();
    }
}

public class OnEvent : PengNode
{
    public PengEditorVariables.PengString eventName;

    public PengEditorVariables.PengInt intMessage;
    public PengEditorVariables.PengFloat floatMessage;
    public PengEditorVariables.PengString stringMessage;
    public PengEditorVariables.PengBool boolMessage;

    public OnEvent(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoints = new PengNodeConnection[1]; inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[1];
        outVars = new PengEditorVariables.PengVar[4];
        intMessage = new PengEditorVariables.PengInt(this, "整型参数", 0, ConnectionPointType.Out);
        floatMessage = new PengEditorVariables.PengFloat(this, "浮点参数", 1, ConnectionPointType.Out);
        stringMessage = new PengEditorVariables.PengString(this, "字符串参数", 2, ConnectionPointType.Out);
        boolMessage = new PengEditorVariables.PengBool(this, "布尔参数", 3, ConnectionPointType.Out);
        eventName = new PengEditorVariables.PengString(this, "事件名称", 0, ConnectionPointType.In);

        outVars[0] = intMessage;
        outVars[1] = floatMessage;
        outVars[2] = stringMessage;
        outVars[3] = boolMessage;
        eventName.point = null;
        inVars[0] = eventName;
        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Event;
        scriptType = PengScript.PengScriptType.OnEvent;
        nodeName = GetDescription(scriptType);

        paraNum = 4;
    }

    public override void Draw()
    {
        base.Draw();
        Rect field = new Rect(inVars[0].varRect.x + 45, inVars[0].varRect.y, 65, 18);
        eventName.value = EditorGUI.TextField(field, eventName.value);
    }

    public override string SpecialParaDescription()
    {
        return eventName.value;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        eventName.value = info;
    }
}
