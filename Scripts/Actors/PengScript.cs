using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using System.Reflection;
using static PengNode;
using System.Security.Cryptography.X509Certificates;
using static IfElse;
using System.Xml;
using System;

namespace PengScript
{
    /// <summary>
    /// ΰ�����ű���
    /// 
    /// ����½ű��Ĺ�ע�㣺
    /// 1. PengScriptType
    /// 2. ��PengNode.cs������½ű��Ľڵ���ʽ
    /// 3. ��PengActorStateEditorWindow.cs��RightMouseMenu()��д����½ű��Ľڵ�ķ���
    /// 4. ��PengActorStateEditorWindow.cs��ReadPengNode()��д��ȡ�½ű��Ľڵ�ķ���
    /// 5. ��PengScript.cs������½ű�������ʱ��ʽ���������캯�������巽��
    /// 6. ��PengActorState.ConstructRunTimePengScript()���������ʱ�����½ű��ķ���
    /// </summary>
    /// 


    public enum PengScriptType
    {
        [Description("���ִ��")]
        OnExecute,
        [Description("�������")]
        DebugLog,
        [Description("���Ŷ���")]
        PlayAnimation,
        [Description("�л�״̬")]
        TransState,
        [Description("��Χ��ȡĿ��")]
        GetTargetsByRange,
        [Description("ȫ��ʱ�����")]
        GlobalTimeScale,
        [Description("������Ƶ")]
        PlayAudio,
        [Description("������Ч")]
        PlayEffects,
        [Description("���Ŀ��")]
        ClearTargets,
        [Description("��������")]
        TryGetEnemy,
        [Description("����ת��")]
        AllowChangeDirection,
        [Description("��������")]
        SetVisibility,
        [Description("�˺�����")]
        AttackDamage,
        [Description("���úڰ����")]
        SetBlackBoardVariables,
        [Description("��ȡ�ڰ����")]
        GetBlackBoardVariables,
        [Description("�������")]
        GetInput,
        [Description("��������")]
        IfElse,
        [Description("ö�ٷ���")]
        SwitchEnum,
        [Description("���ͷ���")]
        SwitchInt,
        [Description("For����")]
        ForIterator,
        [Description("Foreach����")]
        ForeachIterator,
        [Description("��Ļ��")]
        CameraImpulse,
        [Description("����")]
        PostProcess,
        [Description("��ͷƫ��")]
        CameraOffset,
        [Description("��ͷFov")]
        CameraFOV,
        [Description("��")]
        MathPlus,
        [Description("��")]
        MathMinus,
        [Description("��")]
        MathTime,
        [Description("��")]
        MathDivide,
        [Description("ƽ��")]
        MathSquare,
        [Description("�Ƚ�")]
        MathCompare,
        [Description("����")]
        MathBool,
        [Description("����")]
        ValuePengInt,
    }

    public struct ScriptIDVarID
    {
        public int scriptID;
        public int varID;
    }


    public class BaseScript
    {

        //BaseScript�����ļ�
        //scriptName ����
        //scriptType �ű�����
        //scriptID �ű�ID
        //varInID ��������Ϣ
        //outID �ű�������Ϣ
        //������Ϣ

        //�ű����ƣ���ʾ�ڱ༭����
        public string scriptName = "Ĭ��";
        //�ű���������
        public PengActor actor;
        //�ű�����
        public PengScriptType type;
        //�������
        public PengTrack trackMaster;

        public PengVar[] inVars;
        public PengVar[] outVars;

        public int ID;
        public Dictionary<int, int> flowOutInfo = new Dictionary<int, int>();
        public Dictionary<int, ScriptIDVarID> varInID = new Dictionary<int, ScriptIDVarID>();


        public virtual void Construct(string specialInfo)
        {

        }

        //ִ��һ��
        public virtual void Execute()
        {
            Initial();
            Execute();
            ScriptFlowNext();
        }

        public virtual void Initial()
        {
            //ִ��ǰ�ĳ�ʼ��
            if (varInID.Count > 0 && inVars.Length > 0)
            {
                for (int i = 0; i < varInID.Count; i++)
                {
                    if (varInID.ElementAt(i).Value.scriptID > 0)
                    {
                        PengVar vari = trackMaster.GetOutPengVarByScriptIDPengVarID(varInID.ElementAt(i).Value.scriptID, varInID.ElementAt(i).Value.varID);
                        vari.script.GetValue();
                        PengVar.SetValue(ref inVars[varInID.ElementAt(i).Key], ref vari);
                    }
                }
            }
        }

        public virtual void Function()
        {
            //���幦��
        }

        public virtual void ScriptFlowNext()
        {
            //�ű���
            if (trackMaster != null && trackMaster.scripts.Count > 0)
            {
                if (flowOutInfo.Count > 0)
                {
                    for(int i = 0; i < flowOutInfo.Count; i++)
                    {
                        if(flowOutInfo.ElementAt(i).Value >= 0)
                        {
                            if(trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(i).Value) == null)
                            {
                                continue;
                            }
                            else
                            {
                                trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(i).Value).Execute();
                            }
                        }
                    }
                }
            }
        }

        public virtual void GetValue()
        {

        }

        public void InitialPengVars()
        {
            if (inVars.Length > 0)
            {
                for (int i = 0; i < inVars.Length ; i++)
                {
                    inVars[i].script = this;
                }
            }
            if (outVars.Length > 0)
            {
                for (int i = 0; i < outVars.Length; i++)
                {
                    outVars[i].script = this;
                }
            }
        }

        public static Vector3 ParseStringToVector3(string s)
        {
            s = s.Replace("(", "").Replace(")", "").Replace(" ", "");
            string[] str = s.Split(",");
            if (str.Length == 3)
            {
                return new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2]));
            }
            else
            {
                return Vector3.zero;
            }
        }

        public static string ParseVector3ToString(Vector3 v)
        {
            string s = "(" + v.x.ToString() + "," + v.y.ToString() + "," + v.z.ToString() + ")";
            return s;
        }

        public static string ParseStringListToString(List<string> list)
        {
            string str = "";
            if (list.Count > 0)
            {
                for (global::System.Int32 i = 0; i < list.Count; i++)
                {
                    str += list[i];
                    if (i < list.Count - 1)
                    {
                        str += ",";
                    }
                }
            }
            return str;
        }

        public static List<string> ParseStringToStringList(string str)
        {
            List<string> s = new List<string>();
            string[] strings = str.Split(",");
            if (strings.Length > 0)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    s.Add(strings[i]);
                }
            }
            return s;
        }

        public static ScriptIDVarID InitialScriptIDVarID()
        {
            ScriptIDVarID sivi = new ScriptIDVarID();
            sivi.scriptID = -1;
            sivi.varID = -1;
            return sivi;
        }

        public static Dictionary<int, ScriptIDVarID> ParseStringToDictionaryIntScriptIDVarID(string str)
        {
            Dictionary<int, ScriptIDVarID> result = new Dictionary<int, ScriptIDVarID>();
            if (str == "")
                return result;
            string[] strings = str.Split(";");
            if (strings.Length > 0)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    string[] s1 = strings[i].Split("|");
                    string[] s2 = s1[1].Split(":");
                    ScriptIDVarID sivi = new ScriptIDVarID();
                    sivi.scriptID = int.Parse(s2[0]);
                    sivi.varID = int.Parse(s2[1]);
                    result.Add(int.Parse(s1[0]), sivi);
                }
            }
            return result;
        }

    }

    public class OnTrackExecute: BaseScript
    {
        public PengInt pengTrackExecuteFrame = new PengInt(null, "���ִ��֡", 0, ConnectionPointType.Out);
        public PengInt pengStateExecuteFrame = new PengInt(null, "״ִ̬��֡", 0, ConnectionPointType.Out);
        public OnTrackExecute(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[this.flowOutInfo.Count];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.OnExecute;
            scriptName = GetDescription(type);

            outVars[0] = pengTrackExecuteFrame;
            outVars[1] = pengStateExecuteFrame;
        }

        public override void Initial()
        {
            pengTrackExecuteFrame.value = actor.currentStateFrame - trackMaster.start;
            pengStateExecuteFrame.value = actor.currentStateFrame;
        }
    }

    public class PlayAnimation : BaseScript
    {
        public PengString pengAnimationName = new PengString(null, "��������", 0, ConnectionPointType.In);
        public PengBool pengHardCut = new PengBool(null, "�Ƿ�Ӳ��", 1, ConnectionPointType.In);
        public PengFloat pengTransitionNormalizedTime = new PengFloat(null, "����ʱ��", 2, ConnectionPointType.In);
        public PengFloat pengStartAtNormalizedTime = new PengFloat(null, "��ʼʱ��", 3, ConnectionPointType.In);
        public PengInt pengAnimationLayer = new PengInt(null, "������", 4, ConnectionPointType.Out);
        public PlayAnimation(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[this.flowOutInfo.Count];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.OnExecute;
            scriptName = GetDescription(type);
            inVars[0] = pengAnimationName;
            inVars[1] = pengHardCut;
            inVars[2] = pengTransitionNormalizedTime;
            inVars[3] = pengStartAtNormalizedTime;
            inVars[4] = pengAnimationLayer;
        }

        public override void Initial()
        {
            base.Initial();
        }

        public override void Function()
        {
            base.Function();
            int stateId = Animator.StringToHash(pengAnimationName.value);
            if (!actor.anim.HasState(pengAnimationLayer.value, stateId)) 
            {
                Debug.LogWarning("�����ڸö���״̬");
                return;
            }
            if (pengHardCut.value)
            {
                actor.anim.Play(pengAnimationName.value, pengAnimationLayer.value, pengStartAtNormalizedTime.value);
            }
            else
            {
                actor.anim.CrossFade(pengAnimationName.value, pengTransitionNormalizedTime.value, pengAnimationLayer.value, pengStartAtNormalizedTime.value);
            }
        }
    }

    public class IfElse : BaseScript
    {
        public List<IfElseIfElse> conditionTypes = new List<IfElseIfElse>();
        public int executeNum = -1;
        public IfElse(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[this.flowOutInfo.Count];
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
                inVars[i] = new PengBool(null, "����", i, ConnectionPointType.In);
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

        public override void Initial()
        {
            base.Initial();
            executeNum = -1;
        }

        public override void Function()
        {
            base.Function();
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
                    if (flowOutInfo[executeNum] >= 0 && trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(executeNum).Value) != null)
                    {
                        trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(executeNum).Value).Execute();
                    }
                }
            }
        }
    }

    public class ValuePengInt : BaseScript
    {
        public PengInt pengInt = new PengInt(null, "ֵ", 0, ConnectionPointType.Out);
        public ValuePengInt(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[this.flowOutInfo.Count];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.ValuePengInt;
            scriptName = GetDescription(type);

            outVars[0] = pengInt;
            pengInt.value = int.Parse(specialInfo);
        }

    }
}

