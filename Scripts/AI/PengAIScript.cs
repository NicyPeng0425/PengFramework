using PengLevelRuntimeFunction;
using PengScript;
using PengVariables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static PengActorControl;

namespace PengAIScript
{
    public enum AISensor
    {
        TargetDistance = 1,
        TargetRelativeDirection = 2,
        TargetCurrentState = 3,
        TargetCurrentStateFrame = 4,
        TargetCurrentHP = 5,
        SelfCurrentState = 6,
        SelfWanderTime = 7,
        SelfDecideGap = 8,
        Value = 9,
    }

    public enum AISingleSensor
    {
        HasTarget,
    }

    public enum AIScriptType
    {
        [Description("1,决策事件,事件")]
        EventDecide,
        [Description("1,条件分支,分支")]
        Condition,
        [Description("1,随机分支,分支")]
        Random,
        [Description("1,输入行为,行为")]
        InputAction,
        [Description("1,缩短决策间隔,行为")]
        ReduceDecideGap,
        [Description("1,空逻辑,行为")]
        Empty,
        [Description("1,序列分支,分支")]
        Sequence,
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

        public virtual bool Execute()
        {
            Initial();
            Function();
            return ScriptFlowNext();
        }

        public virtual void Initial()
        {
        }

        public virtual void Function()
        {
        }

        public virtual bool ScriptFlowNext()
        {
            bool hasSon = false;
            if (flowOutInfo.Count > 0)
            {
                for (int i = 0; i < flowOutInfo.Count; i++)
                {
                    if (flowOutInfo.ElementAt(i).Value >= 0)
                    {
                        hasSon = true;
                    }
                }
            }

            if (hasSon)
            {
                bool toReturn = false;
                if (flowOutInfo.Count > 0)
                {
                    for (int i = 0; i < flowOutInfo.Count; i++)
                    {
                        if (flowOutInfo.ElementAt(i).Value >= 0)
                        {
                            if (ai.scripts[flowOutInfo.ElementAt(i).Value].Execute())
                            {
                                toReturn = true;
                                break;
                            }
                        }
                    }
                }
                return toReturn;
            }
            else
            {
                return true;
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

    public class ConditionVar
    {
        public enum JudgmentType
        {
            If,
            ElseIf,
        }

        public enum JudgmentEntryType
        {
            单目运算式,
            双目运算式,
        }

        public JudgmentType type = JudgmentType.ElseIf;
        public Judgement judge;
    }

    public struct Judgement
    {
        public PengAIScript.ConditionVar.JudgmentEntryType type;
        public PengScript.MathCompare.CompareTypeCN compareType;
        public PengAIScript.AISensor sensor1;
        public PengAIScript.AISensor sensor2;
        public float floatVal1;
        public float floatVal2;
        public int intVal1;
        public int intVal2;
        public string strVal1;
        public string strVal2;
        public AISingleSensor singleSensor;
        public bool singleSensorTrue;
    }

    public class Condition : PengAIBaseScript
    {
        public List<PengAIScript.ConditionVar> conditions = new List<PengAIScript.ConditionVar>();
        public Condition(PengActorControl ai, int ID, string flowOutInfo, string specialInfo)
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
                string[] strings = info.Split(";");
                if (strings.Length > 0)
                {
                    for (int i = 0; i < strings.Length; i++)
                    {
                        if (strings[i] != "")
                        {
                            string[] str = strings[i].Split("|");
                            if (str.Length > 0)
                            {
                                PengAIScript.ConditionVar cond = new PengAIScript.ConditionVar();
                                cond.type = (PengAIScript.ConditionVar.JudgmentType)Enum.Parse(typeof(PengAIScript.ConditionVar.JudgmentType), str[0]);
                                cond.judge.type = (PengAIScript.ConditionVar.JudgmentEntryType)Enum.Parse(typeof(PengAIScript.ConditionVar.JudgmentEntryType), str[1]);
                                cond.judge.compareType = (PengScript.MathCompare.CompareTypeCN)Enum.Parse(typeof(PengScript.MathCompare.CompareTypeCN), str[2]);
                                cond.judge.sensor1 = (PengAIScript.AISensor)Enum.Parse(typeof(PengAIScript.AISensor), str[3]);
                                cond.judge.sensor2 = (PengAIScript.AISensor)Enum.Parse(typeof(PengAIScript.AISensor), str[4]);
                                cond.judge.floatVal1 = float.Parse(str[5]);
                                cond.judge.floatVal2 = float.Parse(str[6]);
                                cond.judge.intVal1 = int.Parse(str[7]);
                                cond.judge.intVal2 = int.Parse(str[8]);
                                cond.judge.strVal1 = str[9] == "null" ? "" : str[9];
                                cond.judge.strVal2 = str[10] == "null" ? "" : str[10];
                                cond.judge.singleSensor = (PengAIScript.AISingleSensor)Enum.Parse(typeof(PengAIScript.AISingleSensor), str[11]);
                                cond.judge.singleSensorTrue = int.Parse(str[12]) > 0;
                                conditions.Add(cond);
                            }
                        }
                    }
                }
            }
            type = PengAIScript.AIScriptType.Condition;
        }

        public override bool ScriptFlowNext()
        {
            bool toReturn = false;
            for (int i = 0; i < conditions.Count; i++)
            {
                switch (conditions[i].judge.type)
                {
                    case ConditionVar.JudgmentEntryType.单目运算式:
                        switch (conditions[i].judge.singleSensor)
                        {
                            case AISingleSensor.HasTarget:
                                bool hasTarget = ai.target != null;
                                if((conditions[i].judge.singleSensorTrue && hasTarget) || (!conditions[i].judge.singleSensorTrue && !hasTarget))
                                {
                                    if (ai.scripts[flowOutInfo.ElementAt(i).Value].Execute())
                                    {
                                        toReturn = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                                break;
                        }
                        break;
                    case ConditionVar.JudgmentEntryType.双目运算式:
                        if (conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetDistance || conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetRelativeDirection || conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetCurrentHP ||
                            conditions[i].judge.sensor1 == PengAIScript.AISensor.SelfWanderTime || conditions[i].judge.sensor1 == PengAIScript.AISensor.SelfDecideGap || conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetCurrentStateFrame)
                        {
                            float value1 = GetValue(conditions[i].judge.sensor1);
                            float value2 = conditions[i].judge.sensor1 == AISensor.Value? conditions[i].judge.floatVal2 : GetValue(conditions[i].judge.sensor2);
                            if (Compare(value1, conditions[i].judge.compareType, value2))
                            {
                                if (ai.scripts[flowOutInfo.ElementAt(i).Value].Execute())
                                {
                                    toReturn = true;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        else if (conditions[i].judge.sensor1 == PengAIScript.AISensor.TargetCurrentState || conditions[i].judge.sensor1 == PengAIScript.AISensor.SelfCurrentState)
                        {
                            string value1 = GetStringValue(conditions[i].judge.sensor1);
                            string value2 = conditions[i].judge.sensor1 == AISensor.Value ? conditions[i].judge.strVal2 : GetStringValue(conditions[i].judge.sensor2);
                            if (CompareString(value1, conditions[i].judge.compareType, value2))
                            {
                                if (ai.scripts[flowOutInfo.ElementAt(i).Value].Execute())
                                {
                                    toReturn = true;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        break;
                }
                if (toReturn)
                {
                    break;
                }
            }
            return toReturn;
        }

        public bool Compare(float val1, MathCompare.CompareTypeCN compare, float val2)
        {
            bool result = false;
            switch (compare)
            {
                case MathCompare.CompareTypeCN.小于:
                    if (val1 < val2)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.不小于:
                    if (val1 >= val2)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.大于:
                    if (val1 > val2)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.不大于:
                    if (val1 <= val2)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.等于:
                    if (val1 == val2)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.不等于:
                    if (val1 != val2)
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }

        public bool CompareString(string val1, MathCompare.CompareTypeCN compare, string val2)
        {
            bool result = false;
            switch (compare)
            {
                case MathCompare.CompareTypeCN.小于:
                    if (val1.Length < val2.Length)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.不小于:
                    if (val1.Length >= val2.Length)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.大于:
                    if (val1.Length > val2.Length)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.不大于:
                    if (val1.Length <= val2.Length)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.等于:
                    if (val1 == val2)
                    {
                        result = true;
                    }
                    break;
                case MathCompare.CompareTypeCN.不等于:
                    if (val1 != val2)
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }

        public float GetValue(AISensor sensor)
        {
            float value = 0;
            switch (sensor)
            {
                case AISensor.TargetDistance:
                    value = ai.targetDistance;
                    break;
                case AISensor.TargetRelativeDirection:
                    value = Vector3.Angle(ai.transform.forward, ai.targetDirection);
                    break;
                case AISensor.TargetCurrentStateFrame:
                    value = (float) ai.targetCurrentStateFrame;
                    break;
                case AISensor.TargetCurrentHP:
                    value = ai.targetCurrentHP;
                    break;
                case AISensor.SelfWanderTime:
                    value = ai.wanderTime;
                    break;
                case AISensor.SelfDecideGap:
                    value = ai.decideCDTimeCount;
                    break;
            }
            return value;
        }

        public string GetStringValue(AISensor sensor)
        {
            string result = "";
            switch (sensor)
            {
                case AISensor.TargetCurrentState:
                    result = ai.target.currentName;
                    break;
                case AISensor.SelfCurrentState:
                    result = ai.actor.currentName;
                    break;
            }
            return result;
        }

        public override void Function()
        {
            base.Function();
        }
    }

    public class Empty : PengAIBaseScript
    {
        bool returns;
        public Empty(PengActorControl ai, int ID, string flowOutInfo, string specialInfo)
        {
            this.ai = ai;
            this.ID = ID;
            this.flowOutInfo = new Dictionary<int, int>();
            Construct(specialInfo);
        }

        public override void Construct(string info)
        {
            base.Construct(info);

            if (info != "")
            {
                returns = int.Parse(info) > 0;
            }

            type = AIScriptType.Empty;
        }

        public override bool ScriptFlowNext()
        {
            return returns;
        }
    }

    public class InputAction : PengAIBaseScript
    {
        public ActionType action;
        public InputAction(PengActorControl ai, int ID, string flowOutInfo, string specialInfo)
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
                action = (ActionType)Enum.Parse(typeof(ActionType), info);
            }

            type = AIScriptType.InputAction;
        }

        public override void Function()
        {
            if (!ai.actions.ContainsKey(ai.actor.game.currentFrame))
            {
                List<ActionType> at = new List<ActionType>();
                at.Add(action);
                ai.actions.Add(ai.actor.game.currentFrame, at);
            }
            else
            {
                ai.actions[ai.actor.game.currentFrame].Add(action);
            }
        }
    }

    public class ReduceDecideGap : PengAIBaseScript
    {
        public float time1;
        public float time2;
        public ReduceDecideGap(PengActorControl ai, int ID, string flowOutInfo, string specialInfo)
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
                string[] str = info.Split(",");
                time1 = float.Parse(str[0]);
                time2 = float.Parse(str[1]);
            }

            type = AIScriptType.ReduceDecideGap;
        }

        public override void Function()
        {
            if (time1 == time2){ ai.decideCDTimeCount += time1;}
            else { ai.decideCDTimeCount += UnityEngine.Random.Range(time1, time2); }
        }
    }

    public class Sequence : PengAIBaseScript
    {
        public Sequence(PengActorControl ai, int ID, string flowOutInfo, string specialInfo)
        {
            this.ai = ai;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntInt(flowOutInfo);
            Construct(specialInfo);
        }

        public override void Construct(string info)
        {
            base.Construct(info);
            type = PengAIScript.AIScriptType.Sequence;
        }
    }

    public class Random : PengAIBaseScript
    {
        public List<float> ratios = new List<float>();
        public Random(PengActorControl ai, int ID, string flowOutInfo, string specialInfo)
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
                ratios = new List<float>();
                string[] str = info.Split(",");
                for (int i = 0; i < str.Length; i++)
                {
                    ratios.Add(float.Parse(str[i]));
                }
            }
            type = PengAIScript.AIScriptType.Random;
        }

        public override bool ScriptFlowNext()
        {
            float rand = UnityEngine.Random.Range(0f, 1f);
            float cal = 0;
            bool toReturn = false;
            for (int i = 0; i < ratios.Count; i++)
            {
                if (rand >= cal && rand <= ratios[i])
                {
                    if (ai.scripts[flowOutInfo.ElementAt(i).Value].Execute())
                    {
                        toReturn = true;
                    }
                    else
                    {
                        break;
                    }
                }
                cal += ratios[i];
            }
            return toReturn;
        }
    }
}
