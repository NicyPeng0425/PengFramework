using PengLevelRuntimeFunction;
using PengScript;
using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace PengAIScript
{
    public enum AISensor
    {
        Target = 0,
        TargetDistance = 1,
        TargetRelativeDirection = 2,
        TargetCurrentState = 3,
        TargetCurrentStateFrame = 4,
        TargetCurrentHP = 5,
        SelfCurrentState = 6,
        SelfWanderTime = 7,
        SelfDecideGap = 8,
    }

    public enum AIScriptType
    {
        [Description("1,决策事件,Event")]
        EventDecide,
        [Description("0,条件分支,Condition")]
        Condition,
        [Description("0,随机分支,Condition")]
        Random,
        [Description("0,输入行为,Action")]
        InputAction,
        [Description("0,缩短决策间隔,Action")]
        ReduceDecideGap,
    }

    public class PengAIBaseScript
    {
        public string scriptName = "默认";
        public PengActorControl ai;
        public AIScriptType type;
        public int ID;
        //每个节点只允许一个Enter
        public Dictionary<int, int> flowOutInfo = new Dictionary<int, int>();

        public virtual void Construct(string info)
        {

        }

        public virtual void Enter()
        {
            //做点什么
        }

        public virtual void Execute()
        {
            Initial();
            Function();
            ScriptFlowNext();
        }

        public virtual void Initial()
        {
        }

        public virtual void Function()
        {
        }

        public virtual void ScriptFlowNext()
        {
            if (flowOutInfo.Count > 0)
            {
                for (int i = 0; i < flowOutInfo.Count; i++)
                {
                    if (flowOutInfo.ElementAt(i).Value >= 0)
                    {
                        ai.scripts[flowOutInfo.ElementAt(i).Value].Execute();
                    }
                }
            }
        }
    }

    public class DecideEvent : PengAIBaseScript
    {
        public DecideEvent(PengActorControl ai, int ID, string flowOutInfo, string specialInfo)
        {
            this.ai = ai;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntInt(flowOutInfo);
            Construct(specialInfo);
        }

        public override void Construct(string info)
        {
            base.Construct(info);

            if (info != "")
            {

            }

            type = PengAIScript.AIScriptType.EventDecide;
        }

        public override void Function()
        {
            base.Function();
        }
    }
}
