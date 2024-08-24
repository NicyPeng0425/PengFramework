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
        Vector2 pos = PengNode.ParseStringToVector2(ele.GetAttribute("Position"));
        switch (type)
        {
            default:
                return null;
            case PengScript.PengScriptType.OnTrackExecute:
                return new OnTrackExecute(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.PlayAnimation:
                return new PlayAnimation(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.IfElse:
                return new IfElse(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengInt:
                return new ValuePengInt(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengFloat:
                return new ValuePengFloat(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengString:
                return new ValuePengString(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengBool:
                return new ValuePengBool(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.GetTargetsByRange:
                return new GetTargetsByRange(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ForIterator:
                return new ForIterator(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValuePengVector3:
                return new ValuePengVector3(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValueFloatToString:
                return new ValueFloatToString(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.DebugLog:
                return new DebugLog(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.TransState:
                return new TransState(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.BreakPoint:
                return new BreakPoint(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.GlobalTimeScale:
                return new GlobalTimeScale(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.MathCompare:
                return new MathCompare(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValueIntToFloat:
                return new ValueIntToFloat(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.SetBlackBoardVariables:
                return new SetBlackBoardVariables(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.OnEvent:
                return new OnEvent(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.CustomEvent:
                return new CustomEvent(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.GetVariables:
                return new GetVariables(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.MathBool:
                return new MathBool(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.ValueGetListCount:
                return new ValueGetListCount(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.PlayEffects:
                return new PlayEffects(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.PlayAudio:
                return new PlayAudio(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.GetInput:
                return new GetInput(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.MathStringEqual:
                return new MathStringEqual(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.OnGround:
                return new OnGround(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.OnHit:
                return new OnHit(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.OnDie:
                return new OnDie(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.AllowChangeDirection:
                return new AllowChangeDirection(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.JumpForce:
                return new JumpForce(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.MoveByFrame:
                return new MoveByFrame(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.AddOrRemoveBuff:
                return new AddOrRemoveBuff(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.AttackDamage:
                return new AttackDamage(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.TryGetEnemy:
                return new TryGetEnemy(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
            case PengScript.PengScriptType.PerfectDodge:
                return new PerfectDodge(pos, null, ref track, ID, outID, varOutID, varInID, specialInfo);
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
            case PengScript.PengScriptType.OnHit:
                if (editGlobal)
                {
                    track.nodes.Add(new OnHit(mousePos, this, ref track, id,
                        PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                        PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                        PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                }
                else
                {
                    EditorUtility.DisplayDialog("警告", "不允许在全局节点图以外的地方放置受击事件节点。", "确认");
                }
                break;
            case PengScript.PengScriptType.OnDie:
                if (editGlobal)
                {
                    track.nodes.Add(new OnDie(mousePos, this, ref track, id,
                        PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                        PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                        PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                }
                else
                {
                    EditorUtility.DisplayDialog("警告", "不允许在全局节点图以外的地方放置死亡事件节点。", "确认");
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
            case PengScript.PengScriptType.MoveByFrame:
                track.nodes.Add(new MoveByFrame(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case PengScript.PengScriptType.AddOrRemoveBuff:
                track.nodes.Add(new AddOrRemoveBuff(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(3)), ""));
                break;
            case PengScript.PengScriptType.AttackDamage:
                track.nodes.Add(new AttackDamage(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(11)), ""));
                break;
            case PengScript.PengScriptType.TryGetEnemy:
                track.nodes.Add(new TryGetEnemy(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(2)), ""));
                break;
            case PengScript.PengScriptType.PerfectDodge:
                track.nodes.Add(new PerfectDodge(mousePos, this, ref track, id,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(2)), ""));
                break;
        }
    }
}
