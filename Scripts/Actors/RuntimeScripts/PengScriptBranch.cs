using PengVariables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PengScript
{
    public class IfElse : BaseScript
    {
        public enum IfElseIfElse
        {
            If,
            ElseIf,
            Else
        }
        public List<IfElseIfElse> conditionTypes = new List<IfElseIfElse>();
        public int executeNum = -1;
        public PengBool[] bools;
        public IfElse(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengBool[varInID.Count];
            bools = new PengBool[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            type = PengScriptType.IfElse;
            scriptName = GetDescription(type);
            for (int i = 0; i < inVars.Length; i++)
            {
                PengBool condition = new PengBool("条件", i, ConnectionPointType.In);
                inVars[i] = condition;
                bools[i] = condition;
            }

            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                for (int i = 0; i < str.Length; i++)
                {
                    conditionTypes.Add((IfElseIfElse)Enum.Parse(typeof(IfElseIfElse), str[i]));
                }
            }
            else
            {
                conditionTypes.Add(IfElseIfElse.If);
            }
        }

        public override void Initial(int functionIndex)
        {
            base.Initial(functionIndex);
            executeNum = -1;
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            PengBool pb = varSource as PengBool;
            bools[inVarID].value = pb.value;
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            if (inVars.Length > 0)
            {
                if (conditionTypes[conditionTypes.Count - 1] != IfElseIfElse.Else)
                {
                    for (int i = 0; i < inVars.Length; i++)
                    {
                        PengBool boolV = inVars[i] as PengBool;
                        if (boolV.value)
                        {
                            executeNum = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < inVars.Length; i++)
                    {
                        PengBool boolV = inVars[i] as PengBool;
                        if (i != inVars.Length - 1)
                        {
                            if (boolV.value)
                            {
                                executeNum = i;
                                break;
                            }
                        }
                        else
                        {
                            if (executeNum < 0)
                            {
                                executeNum = i;
                            }
                        }
                    }
                }
            }
        }

        public override void ScriptFlowNext()
        {
            if (executeNum >= 0)
            {
                if (flowOutInfo.Count > 0)
                {
                    if (flowOutInfo[executeNum].scriptID >= 0 && trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(executeNum).Value.scriptID) != null)
                    {
                        trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(executeNum).Value.scriptID).Execute(flowOutInfo.ElementAt(executeNum).Value.varID);
                    }
                }
            }
        }
    }

    public class GetInput : BaseScript
    {
        public struct InputMap
        {
            public string stateName;
            public int frameOffset;
            public PengActorControl.ActionType input;
        }

        public List<InputMap> maps = new List<InputMap>();
        public int toTrans = -1;
        public GetInput(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[0];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(";");
                if (str.Length > 0)
                {
                    for (int i = 0; i < str.Length; i++)
                    {
                        string[] s1 = str[i].Split(",");
                        InputMap map = new InputMap();
                        map.stateName = s1[0];
                        map.frameOffset = int.Parse(s1[1]);
                        map.input = (PengActorControl.ActionType)Enum.Parse(typeof(PengActorControl.ActionType), s1[2]);
                        maps.Add(map);
                    }
                }
            }
            type = PengScriptType.GetInput;
            scriptName = GetDescription(type);
        }

        public override void Initial(int functionIndex)
        {
            base.Initial(functionIndex);
            if (actor.currentStateFrame - trackMaster.start == 0)
            {
                toTrans = -1;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            
            if (actor.input.actions.Count > 0)
            {
                for (int i = 0; i < actor.input.actions.Count; i++)
                {
                    for (int f = maps.Count - 1; f >= 0; f--)
                    {
                        if (actor.input.actions.ElementAt(i).Value.Contains(maps[f].input))
                        {
                            if (toTrans < 0)
                            {
                                toTrans = f;
                            }
                            else if(f <= toTrans)
                            {
                                toTrans = f;
                            }   
                        }
                    }
                }
            }

            if (toTrans >= 0)
            {
                if (actor.currentStateFrame - trackMaster.start >= maps[toTrans].frameOffset)
                {
                    if (actor.actorStates.ContainsKey(maps[toTrans].stateName))
                    {
                        actor.TransState(maps[toTrans].stateName, true);
                    }
                    else
                    {
                        Debug.Log("Actor" + actor.actorID.ToString() + "在" + actor.currentName + "状态的" + trackMaster.name + "轨道中调用了输入分歧-切换状态，但没有给定名称的状态。");
                    }
                }
            }
        }
    }
}
