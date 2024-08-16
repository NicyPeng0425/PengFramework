using PengScript;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimation : PengNode
{
    public PengEditorVariables.PengString pengAnimationName;
    public PengEditorVariables.PengBool pengHardCut;
    public PengEditorVariables.PengFloat pengTransitionNormalizedTime;
    public PengEditorVariables.PengFloat pengStartAtNormalizedTime;
    public PengEditorVariables.PengInt pengAnimationLayer;

    public PlayAnimation(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
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
        inVars = new PengEditorVariables.PengVar[5];
        outVars = new PengEditorVariables.PengVar[0];
        pengAnimationName = new PengEditorVariables.PengString(this, "动画名称", 0, ConnectionPointType.In);
        pengHardCut = new PengEditorVariables.PengBool(this, "是否硬切", 1, ConnectionPointType.In);
        pengTransitionNormalizedTime = new PengEditorVariables.PengFloat(this, "过渡时间", 2, ConnectionPointType.In);
        pengStartAtNormalizedTime = new PengEditorVariables.PengFloat(this, "开始时间", 3, ConnectionPointType.In);
        pengAnimationLayer = new PengEditorVariables.PengInt(this, "动画层", 4, ConnectionPointType.In);
        inVars[0] = pengAnimationName;
        inVars[1] = pengHardCut;
        inVars[2] = pengTransitionNormalizedTime;
        inVars[3] = pengStartAtNormalizedTime;
        inVars[4] = pengAnimationLayer;

        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.PlayAnimation;
        nodeName = GetDescription(scriptType);

        paraNum = 5;
    }

    public override void Draw()
    {
        base.Draw();
    }
}
