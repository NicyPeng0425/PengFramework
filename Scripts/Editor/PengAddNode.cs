using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public partial class PengActorStateEditorWindow : EditorWindow
{
    public static PengNode ReadPengNode(XmlElement ele, ref PengEditorTrack track)
    {
        PengScript.PengScriptType type;
        if (ele.GetAttribute("ScriptType") == "OnExecute")
        {
            type = PengScript.PengScriptType.OnTrackExecute;
        }
        else
        {
            type = (PengScript.PengScriptType)Enum.Parse(typeof(PengScript.PengScriptType), ele.GetAttribute("ScriptType"));
        }
        int ID = int.Parse(ele.GetAttribute("ScriptID"));
        string outID = ele.GetAttribute("OutID");
        string varOutID = ele.GetAttribute("VarOutID");
        string varInID = ele.GetAttribute("VarInID");
        string specialInfo = ele.GetAttribute("SpecialInfo");
        switch (type)
        {
            default:
                return null;
            case PengScript.PengScriptType.OnTrackExecute:
                return new OnTrackExecute(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.PlayAnimation:
                return new PlayAnimation(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.IfElse:
                return new IfElse(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengInt:
                return new ValuePengInt(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengFloat:
                return new ValuePengFloat(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengString:
                return new ValuePengString(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengBool:
                return new ValuePengBool(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.GetTargetsByRange:
                return new GetTargetsByRange(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ForIterator:
                return new ForIterator(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengVector3:
                return new ValuePengVector3(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValueFloatToString:
                return new ValueFloatToString(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.DebugLog:
                return new DebugLog(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.TransState:
                return new TransState(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.BreakPoint:
                return new BreakPoint(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.GlobalTimeScale:
                return new GlobalTimeScale(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.MathCompare:
                return new MathCompare(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValueIntToFloat:
                return new ValueIntToFloat(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.SetBlackBoardVariables:
                return new SetBlackBoardVariables(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.OnEvent:
                return new OnEvent(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.CustomEvent:
                return new CustomEvent(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.GetVariables:
                return new GetVariables(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.MathBool:
                return new MathBool(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValueGetListCount:
                return new ValueGetListCount(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.PlayEffects:
                return new PlayEffects(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.PlayAudio:
                return new PlayAudio(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.GetInput:
                return new GetInput(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.MathStringEqual:
                return new MathStringEqual(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.OnGround:
                return new OnGround(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.AllowChangeDirection:
                return new AllowChangeDirection(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.JumpForce:
                return new JumpForce(PengNode.ParseStringToVector2(ele.GetAttribute("Position")), null, ref track, ID, outID, varOutID, varInID, specialInfo);
        }
    }

    private void ProcessAddNode(ref PengEditorTrack track, Vector2 mousePos, PengScript.PengScriptType type)
    {
        int id = 1;
        bool idSame = true;

        while (idSame)
        {
            idSame = false;
            for (int i = 0; i < track.nodes.Count; i++)
            {
                if (id == track.nodes[i].nodeID)
                {
                    idSame = true;
                    id++;
                }
            }
        }
        switch (type)
        {
            case PengScript.PengScriptType.PlayAnimation:
                track.nodes.Add(new PlayAnimation(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(5)), ""));
                break;
            case PengScript.PengScriptType.IfElse:
                track.nodes.Add(new IfElse(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.ValuePengInt:
                track.nodes.Add(new ValuePengInt(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.ValuePengFloat:
                track.nodes.Add(new ValuePengFloat(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.ValuePengString:
                track.nodes.Add(new ValuePengString(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.ValuePengBool:
                track.nodes.Add(new ValuePengBool(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.GetTargetsByRange:
                track.nodes.Add(new GetTargetsByRange(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(4)), ""));
                break;
            case PengScript.PengScriptType.ForIterator:
                track.nodes.Add(new ForIterator(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(2)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(2)), ""));
                break;
            case PengScript.PengScriptType.ValuePengVector3:
                track.nodes.Add(new ValuePengVector3(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(3)), ""));
                break;
            case PengScript.PengScriptType.DebugLog:
                track.nodes.Add(new DebugLog(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.ValueFloatToString:
                track.nodes.Add(new ValueFloatToString(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.TransState:
                track.nodes.Add(new TransState(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.BreakPoint:
                track.nodes.Add(new BreakPoint(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.GlobalTimeScale:
                track.nodes.Add(new GlobalTimeScale(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(2)), ""));
                break;
            case PengScript.PengScriptType.MathCompare:
                track.nodes.Add(new MathCompare(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(3)), ""));
                break;
            case PengScript.PengScriptType.ValueIntToFloat:
                track.nodes.Add(new ValueIntToFloat(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.SetBlackBoardVariables:
                track.nodes.Add(new SetBlackBoardVariables(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(3)), ""));
                break;
            case PengScript.PengScriptType.OnEvent:
                if (editGlobal)
                {
                    track.nodes.Add(new OnEvent(mousePos, this, ref track, id,
                        PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                        PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(4)),
                        PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                }
                else
                {
                    EditorUtility.DisplayDialog("警告", "不允许在全局节点图以外的地方放置事件触发节点。", "确认");
                }
                break;
            case PengScript.PengScriptType.CustomEvent:
                track.nodes.Add(new CustomEvent(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(5)), ""));
                break;
            case PengScript.PengScriptType.GetVariables:
                track.nodes.Add(new GetVariables(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(4)), ""));
                break;
            case PengScript.PengScriptType.MathBool:
                track.nodes.Add(new MathBool(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(3)), ""));
                break;
            case PengScript.PengScriptType.ValueGetListCount:
                track.nodes.Add(new ValueGetListCount(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.PlayEffects:
                track.nodes.Add(new PlayEffects(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(6)), ""));
                break;
            case PengScript.PengScriptType.PlayAudio:
                track.nodes.Add(new PlayAudio(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(2)), ""));
                break;
            case PengScript.PengScriptType.GetInput:
                track.nodes.Add(new GetInput(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case PengScript.PengScriptType.MathStringEqual:
                track.nodes.Add(new MathStringEqual(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(2)), ""));
                break;
            case PengScript.PengScriptType.OnGround:
                if (editGlobal)
                {
                    track.nodes.Add(new OnGround(mousePos, this, ref track, id,
                        PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                        PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                        PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                }
                else
                {
                    EditorUtility.DisplayDialog("警告", "不允许在全局节点图以外的地方放置着地事件节点。", "确认");
                }
                break;
            case PengScript.PengScriptType.AllowChangeDirection:
                track.nodes.Add(new AllowChangeDirection(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.JumpForce:
                track.nodes.Add(new JumpForce(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
        }
    }
}
