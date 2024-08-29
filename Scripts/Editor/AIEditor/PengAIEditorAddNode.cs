using PengLevelRuntimeFunction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public partial class PengAIEditor : EditorWindow
{

    public static PengAIEditorNode.PengAIEditorNode ReadPengAIEditorNode(XmlElement ele, PengAIEditor editor)
    {
        PengAIScript.AIScriptType type;
        if (ele.GetAttribute("ScriptType") == "EventDecide")
        {
            type = PengAIScript.AIScriptType.EventDecide;
        }
        else
        {
            type = (PengAIScript.AIScriptType)Enum.Parse(typeof(PengAIScript.AIScriptType), ele.GetAttribute("ScriptType"));
        }
        int ID = int.Parse(ele.GetAttribute("ScriptID"));
        string outID = ele.GetAttribute("OutID");
        string specialInfo = ele.GetAttribute("SpecialInfo");
        Vector2 pos = PengLevelEditorNodes.PengLevelEditorNode.ParseStringToVector2(ele.GetAttribute("Position"));
        switch (type)
        {
            default:
                return null;
            case PengAIScript.AIScriptType.EventDecide:
                return new PengAIEditorNode.EventDecide(pos, editor, ID, outID, specialInfo);
            case PengAIScript.AIScriptType.Condition:
                return new PengAIEditorNode.Condition(pos, editor, ID, outID, specialInfo);
        }
    }
    private void ProcessAddNode(Vector2 mousePos, PengAIScript.AIScriptType type)
    {
        int id = 1;
        bool idSame = true;

        while (idSame)
        {
            idSame = false;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (id == nodes[i].nodeID)
                {
                    idSame = true;
                    id++;
                }
            }
        }
        switch (type)
        {
            case PengAIScript.AIScriptType.EventDecide:
                nodes.Add(new PengAIEditorNode.EventDecide(mousePos, this, id, PengGameManager.ParseDictionaryIntIntToString(PengGameManager.DefaultDictionaryIntInt(1)), "")); break;
            case PengAIScript.AIScriptType.Condition:
                PengAIEditorNode.Condition result = new PengAIEditorNode.Condition(mousePos, this, id, PengGameManager.ParseDictionaryIntIntToString(PengGameManager.DefaultDictionaryIntInt(1)), "");
                PengAIScript.ConditionVar cond = new PengAIScript.ConditionVar();
                cond.type = PengAIScript.ConditionVar.JudgmentType.If;
                PengAIScript.Judgement judge = new PengAIScript.Judgement();
                judge.type = PengAIScript.ConditionVar.JudgmentEntryType.双目运算式;
                judge.compareType = PengScript.MathCompare.CompareTypeCN.不小于;
                judge.sensor1 = PengAIScript.AISensor.TargetDistance;
                judge.sensor2 = PengAIScript.AISensor.Value;
                cond.judge = judge;
                result.conditions.Add(cond);
                nodes.Add(result); break;
        }
    }
}
