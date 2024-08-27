using PengLevelRuntimeFunction;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace PengLevelEditorNodes
{
    public class GenerateBlack : PengLevelEditorNode
    {
        public GenerateBlack(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "生成一个黑屏。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[0];
            outVars = new PengLevelNodeVariables[0];

            type = LevelFunctionType.GenerateBlack;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);

            paraNum = 1;
        }
    }

    public class EaseInBlack : PengLevelEditorNode
    {
        public PengLevelInt frame;
        public EaseInBlack(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "黑屏渐入（屏幕逐渐变黑）。参数为该过程的帧数。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[1];
            frame = new PengLevelInt(this, "帧数", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
            inVars[0] = frame;
            frame.point = null;
            frame.value = 1;
            outVars = new PengLevelNodeVariables[0];
            ReadSpecialParaDescription(specialInfo);
            type = PengLevelRuntimeFunction.LevelFunctionType.EaseInBlack;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);

            paraNum = 1;
        }
        public override string SpecialParaDescription()
        {
            return frame.value.ToString();
        }

        public override void ReadSpecialParaDescription(string info)
        {
            if (info != "")
            {
                frame.value = int.Parse(info);
            }
        }

        public override void DrawInVarValue(int inVarID, Rect field)
        {
            frame.value = EditorGUI.IntField(field, frame.value);
            if (frame.value <= 0)
            {
                frame.value = 1;
            }
        }
    }

    public class EaseOutBlack : PengLevelEditorNode
    {
        public PengLevelInt frame;
        public EaseOutBlack(Vector2 pos, PengLevelEditor master, int id, string flowOut, string varOut, string varIn, string specialInfo)
        {
            InitialDraw(pos, master);
            nodeID = id;
            outID = ParseStringToDictionaryIntNodeIDConnectionID(flowOut);
            varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOut);
            varInID = ParseStringToDictionaryIntNodeIDConnectionID(varIn);
            meaning = "黑屏渐出（屏幕逐渐由黑变透明）。参数为该过程的帧数。";

            inPoints = new PengLevelNodeConnection[1];
            inPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowIn, 0, this, null);
            outPoints = new PengLevelNodeConnection[1];
            outPoints[0] = new PengLevelNodeConnection(PengLevelNodeConnection.PengLevelNodeConnectionType.FlowOut, 0, this, null);
            inVars = new PengLevelNodeVariables[1];
            frame = new PengLevelInt(this, "帧数", 0, PengLevelNodeConnection.PengLevelNodeConnectionType.VarIn);
            inVars[0] = frame;
            frame.point = null;
            frame.value = 1;
            outVars = new PengLevelNodeVariables[0];
            ReadSpecialParaDescription(specialInfo);
            type = PengLevelRuntimeFunction.LevelFunctionType.EaseOutBlack;
            nodeType = LevelNodeType.Function;
            name = GetDescription(type);

            paraNum = 1;
        }
        public override string SpecialParaDescription()
        {
            return frame.value.ToString();
        }

        public override void ReadSpecialParaDescription(string info)
        {
            if (info != "")
            {
                frame.value = int.Parse(info);
            }
        }

        public override void DrawInVarValue(int inVarID, Rect field)
        {
            frame.value = EditorGUI.IntField(field, frame.value);
            if (frame.value <= 0)
            {
                frame.value = 1;
            }
        }
    }
}
