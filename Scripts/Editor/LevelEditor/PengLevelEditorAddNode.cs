using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using static PengLevelRuntimeFunction;

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
        }
    }
}
