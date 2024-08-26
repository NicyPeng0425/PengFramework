using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using PengLevelRuntimeFunction;
using System.Reflection;
using System.Linq;

public partial class PengLevelEditor : EditorWindow
{
    public static PengLevelEditorNodes.PengLevelEditorNode ReadPengLevelEditorNode(XmlElement ele)
    {
        LevelFunctionType type;
        if (ele.GetAttribute("ScriptType") == "Start")
        {
            type = LevelFunctionType.Start;
        }
        else
        {
            type = (LevelFunctionType)Enum.Parse(typeof(LevelFunctionType), ele.GetAttribute("ScriptType"));
        }
        int ID = int.Parse(ele.GetAttribute("ScriptID"));
        string outID = ele.GetAttribute("OutID");
        string varOutID = ele.GetAttribute("VarOutID");
        string varInID = ele.GetAttribute("VarInID");
        string specialInfo = ele.GetAttribute("SpecialInfo");
        Vector2 pos = PengLevelEditorNodes.PengLevelEditorNode.ParseStringToVector2(ele.GetAttribute("Position"));
        switch (type)
        {
            default:
                return null;
            case LevelFunctionType.Start:
                return new PengLevelEditorNodes.LevelStart(pos, null, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.GenerateActor:
                return new PengLevelEditorNodes.GenerateActor(pos, null, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.SetMainActor:
                return new PengLevelEditorNodes.SetMainActor(pos, null, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.TriggerWaitTime:
                return new PengLevelEditorNodes.TriggerWaitTime(pos, null, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.StartControl:
                return new PengLevelEditorNodes.StartControl(pos, null, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.EndControl:
                return new PengLevelEditorNodes.EndControl(pos, null, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.TriggerWaitArrival:
                return new PengLevelEditorNodes.TriggerWaitArrival(pos, null, ID, outID, varOutID, varInID, specialInfo);
        }
    }

    private void ProcessAddNode(Vector2 mousePos, LevelFunctionType type)
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
            case LevelFunctionType.Start:
                nodes.Add(new PengLevelEditorNodes.LevelStart(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case LevelFunctionType.GenerateActor:
                nodes.Add(new PengLevelEditorNodes.GenerateActor(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case LevelFunctionType.SetMainActor:
                nodes.Add(new PengLevelEditorNodes.SetMainActor(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case LevelFunctionType.TriggerWaitTime:
                nodes.Add(new PengLevelEditorNodes.TriggerWaitTime(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case LevelFunctionType.StartControl:
                nodes.Add(new PengLevelEditorNodes.StartControl(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case LevelFunctionType.EndControl:
                nodes.Add(new PengLevelEditorNodes.EndControl(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case LevelFunctionType.TriggerWaitArrival:
                nodes.Add(new PengLevelEditorNodes.TriggerWaitArrival(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(3)), ""));
                break;
        }
    }
}
