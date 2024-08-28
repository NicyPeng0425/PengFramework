using PengLevelRuntimeFunction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public partial class PengAIEditor : EditorWindow
{

    public PengAIEditorNode.PengAIEditorNode ReadPengAIEditorNode(XmlElement ele)
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
                return new PengAIEditorNode.EventDecide(pos, this, ID, outID, specialInfo);
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
                nodes.Add(new PengAIEditorNode.EventDecide(mousePos, this, id, PengAIEditorNode.PengAIEditorNode.ParseDictionaryIntIntToString(PengAIEditorNode.PengAIEditorNode.DefaultDictionaryIntInt(1)), "")); break;
        }
    }
}
