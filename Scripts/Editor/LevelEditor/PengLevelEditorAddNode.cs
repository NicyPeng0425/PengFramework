using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using PengLevelRuntimeFunction;
using System.Reflection;
using System.Linq;
using Unity.VisualScripting;

public partial class PengLevelEditor : EditorWindow
{
    public PengLevelEditorNodes.PengLevelEditorNode ReadPengLevelEditorNode(XmlElement ele)
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
                return new PengLevelEditorNodes.LevelStart(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.GenerateActor:
                return new PengLevelEditorNodes.GenerateActor(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.SetMainActor:
                return new PengLevelEditorNodes.SetMainActor(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.TriggerWaitTime:
                return new PengLevelEditorNodes.TriggerWaitTime(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.StartControl:
                return new PengLevelEditorNodes.StartControl(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.EndControl:
                return new PengLevelEditorNodes.EndControl(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.TriggerWaitArrival:
                return new PengLevelEditorNodes.TriggerWaitArrival(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.GenerateBlack:
                return new PengLevelEditorNodes.GenerateBlack(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.EaseInBlack:
                return new PengLevelEditorNodes.EaseInBlack(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.EaseOutBlack:
                return new PengLevelEditorNodes.EaseOutBlack(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.GenerateEnemy:
                return new PengLevelEditorNodes.GenerateEnemy(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.TriggerWaitEnemyDie:
                return new PengLevelEditorNodes.TriggerWaitEnemyDie(pos, this, ID, outID, varOutID, varInID, specialInfo);
            case LevelFunctionType.ActiveActor:
                return new PengLevelEditorNodes.ActiveActor(pos, this, ID, outID, varOutID, varInID, specialInfo);
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
            case LevelFunctionType.GenerateBlack:
                nodes.Add(new PengLevelEditorNodes.GenerateBlack(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case LevelFunctionType.EaseInBlack:
                nodes.Add(new PengLevelEditorNodes.EaseInBlack(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case LevelFunctionType.EaseOutBlack:
                nodes.Add(new PengLevelEditorNodes.EaseOutBlack(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)), ""));
                break;
            case LevelFunctionType.GenerateEnemy:
                nodes.Add(new PengLevelEditorNodes.GenerateEnemy(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case LevelFunctionType.TriggerWaitEnemyDie:
                nodes.Add(new PengLevelEditorNodes.TriggerWaitEnemyDie(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
            case LevelFunctionType.ActiveActor:
                nodes.Add(new PengLevelEditorNodes.ActiveActor(mousePos, this, id, PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntListNodeIDConnectionID(0)),
                    PengLevelEditorNodes.PengLevelEditorNode.ParseDictionaryIntNodeIDConnectionIDToString(PengLevelEditorNodes.PengLevelEditorNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));
                break;
        }
    }
}
