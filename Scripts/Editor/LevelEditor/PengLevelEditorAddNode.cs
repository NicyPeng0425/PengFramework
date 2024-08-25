using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using PengLevelRuntimeFunction;

public partial class PengLevelEditor : EditorWindow
{
    public static PengLevelEditorNode ReadPengLevelEditorNode(XmlElement ele)
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
        Vector2 pos = PengLevelEditorNode.ParseStringToVector2(ele.GetAttribute("Position"));
        switch (type)
        {
            default:
                return null;
            case LevelFunctionType.Start:
                return new LevelStart(pos, null, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.GenerateActor:
                return new GenerateActor(pos, null, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.SetMainActor:
                return new SetMainActor(pos, null, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.TriggerWaitTime:
                return new TriggerWaitTime(pos, null, ID, outID, varOutID, varInID, specialInfo);
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
                nodes.Add(new LevelStart(mousePos, this, id, PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case LevelFunctionType.GenerateActor:
                nodes.Add(new GenerateActor(mousePos, this, id, PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(1)),
                    PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case LevelFunctionType.SetMainActor:
                nodes.Add(new SetMainActor(mousePos, this, id, PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case LevelFunctionType.TriggerWaitTime:
                nodes.Add(new TriggerWaitTime(mousePos, this, id, PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
        }
    }
}
