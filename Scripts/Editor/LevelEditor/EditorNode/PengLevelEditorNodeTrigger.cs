using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PengLevelEditorNodes
{
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
            ReadSpecialParaDescription(specialInfo);
            name = "等待" + waitFrame.value + "帧";
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

    public class TriggerWaitArrival : PengLevelEditorNode
    {
        public PengScript.GetTargetsByRange.RangeType rangeType;
        public PengLevelInt typeInt;
        public PengLevelVector3 posV;
        public PengLevelVector3 para;

        public TriggerWaitArrival(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "等待主控角色到达特定区域。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[3];
            outVars = new PengLevelNodeVariables[0];

            typeInt = new PengLevelInt(this, "范围类型", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
            typeInt.point = null;
            rangeType = (PengScript.GetTargetsByRange.RangeType)typeInt.value;
            posV = new PengLevelVector3(this, "位置", 1, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
            posV.point = null;
            para = new PengLevelVector3(this, "参数", 2, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
            para.point = null;

            inVars[0] = typeInt;
            inVars[1] = posV;
            inVars[2] = para;

            ReadSpecialParaDescription(specialInfo);

            type = PengLevelRuntimeFunction.LevelFunctionType.TriggerWaitArrival;
            nodeType = LevelNodeType.Trigger;
            name = GetDescription(type);

            paraNum = 3;
        }
        public override string SpecialParaDescription()
        {
            return typeInt.value.ToString() + ";" + PengScript.BaseScript.ParseVector3ToString(posV.value) + ";" + PengScript.BaseScript.ParseVector3ToString(para.value);
        }

        public override void ReadSpecialParaDescription(string info)
        {
            if (info != "")
            {
                string[] str = info.Split(";");
                typeInt.value = int.Parse(str[0]);
                rangeType = (PengScript.GetTargetsByRange.RangeType)typeInt.value;
                posV.value = PengScript.BaseScript.ParseStringToVector3(str[1]);
                para.value = PengScript.BaseScript.ParseStringToVector3(str[2]);
            }
        }

        public override void DrawInVarValue(int inVarID, Rect field)
        {
            switch (inVarID)
            {
                case 0:
                    rangeType = (PengScript.GetTargetsByRange.RangeType)EditorGUI.EnumPopup(field, rangeType);
                    typeInt.value = (int)rangeType;
                    break;
                case 1:
                    posV.value = EditorGUI.Vector3Field(field, "", posV.value);
                    break;
                case 2:
                    para.value = EditorGUI.Vector3Field(field, "", para.value);
                    if (rangeType == PengScript.GetTargetsByRange.RangeType.Cylinder)
                    {
                        para.value.z = 180;
                    }
                    break;
            }
        }
    }
}
