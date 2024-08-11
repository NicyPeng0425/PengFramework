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
using static GetTargetsByRange;
using static UnityEditor.PlayerSettings;

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
        OnTrackExecute,
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
        [Description("����")]
        ValuePengFloat,
        [Description("����")]
        ValuePengBool,
        [Description("�ַ���")]
        ValuePengString,
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
            Function();
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
                        SetValue(i, vari);
                    }
                }
            }
        }

        public virtual void SetValue(int inVarID, PengVar varSource)
        {

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
            outVars = new PengVar[2];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.OnTrackExecute;
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
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.OnTrackExecute;
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

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengString ps = varSource as PengString;
                    pengAnimationName.value = ps.value;
                    break;
                case 1:
                    PengBool pb = varSource as PengBool;
                    pengHardCut.value = pb.value;
                    break;
                case 2:
                    PengFloat pf1 = varSource as PengFloat;
                    pengTransitionNormalizedTime.value = pf1.value;
                    break;
                case 3:
                    PengFloat pf2 = varSource as PengFloat;
                    pengStartAtNormalizedTime.value = pf2.value;
                    break;
                case 4:
                    PengInt pi = varSource as PengInt;
                    pengAnimationLayer.value = pi.value;
                    break;
            }
        }

        public override void Function()
        {
            base.Function();
            int stateId = Animator.StringToHash(pengAnimationName.value);
            if (!actor.anim.HasState(pengAnimationLayer.value, stateId)) 
            {
                Debug.LogWarning("�����ڸö���״̬��" + pengAnimationName.value + "������״̬"+actor.currentName + "��" + trackMaster.name + "�����" + ID.ToString() + "�Žű���");
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
        public PengBool[] bools;
        public IfElse(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
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
                PengBool condition = new PengBool(null, "����", i, ConnectionPointType.In);
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

        public override void Initial()
        {
            base.Initial();
            executeNum = -1;
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            PengBool pb = varSource as PengBool;
            bools[inVarID].value = pb.value;
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
            outVars = new PengVar[1];
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

    public class ValuePengFloat : BaseScript
    {
        public PengFloat pengFloat = new PengFloat(null, "ֵ", 0, ConnectionPointType.Out);
        public ValuePengFloat(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.ValuePengFloat;
            scriptName = GetDescription(type);

            outVars[0] = pengFloat;
            pengFloat.value = float.Parse(specialInfo);
        }
    }

    public class ValuePengString : BaseScript
    {
        public PengString pengString = new PengString(null, "ֵ", 0, ConnectionPointType.Out);
        public ValuePengString(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.ValuePengString;
            scriptName = GetDescription(type);

            outVars[0] = pengString;
            pengString.value = specialInfo;
        }
    }

    public class ValuePengBool : BaseScript
    {
        public PengBool pengBool = new PengBool(null, "ֵ", 0, ConnectionPointType.Out);
        public ValuePengBool(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.ValuePengFloat;
            scriptName = GetDescription(type);

            outVars[0] = pengBool;
            pengBool.value = int.Parse(specialInfo) > 0;
        }
    }

    public class GetTargetsByRange : BaseScript
    {
        public RangeType rangeType = RangeType.Cylinder;
        public PengListPengActor result = new PengListPengActor(null, "��ȡ����Ŀ��", 0, ConnectionPointType.Out);

        public PengInt typeNum = new PengInt(null, "��Χ����", 0, ConnectionPointType.In);
        public PengInt pengCamp = new PengInt(null, "��Ӫ", 1, ConnectionPointType.In);
        public PengVector3 pengPara = new PengVector3(null, "����", 2, ConnectionPointType.In);
        public PengVector3 pengOffset = new PengVector3(null, "ƫ��", 3, ConnectionPointType.In);
        public GetTargetsByRange(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.GetTargetsByRange;
            scriptName = GetDescription(type);

            if (specialInfo != "")
            {
                switch (int.Parse(specialInfo))
                {
                    case 1:
                        rangeType = RangeType.Cylinder;
                        typeNum.value = 1;
                        break;
                    case 2:
                        rangeType = RangeType.Sphere;
                        typeNum.value = 2;
                        break;
                    case 3:
                        rangeType = RangeType.Box;
                        typeNum.value = 3;
                        break;
                }
            }

            outVars[0] = result;

            inVars[0] = typeNum;
            inVars[1] = pengCamp;
            inVars[2] = pengPara;
            inVars[3] = pengOffset;
        }

        public override void Initial()
        {
            base.Initial();
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengInt pi = varSource as PengInt;
                    typeNum.value = pi.value;
                    break;
                case 1:
                    PengInt pi1 = varSource as PengInt;
                    pengCamp.value = pi1.value;
                    break;
                case 2:
                    PengVector3 pv = varSource as PengVector3;
                    pengPara.value = pv.value;
                    break;
                case 3:
                    PengVector3 pv2 = varSource as PengVector3;
                    pengOffset.value = pv2.value;
                    break;
            }
        }

        public override void Function()
        {
            base.Function();
            result.value = new List<PengActor>();
            Vector3 pos = actor.transform.position + pengOffset.value.x * actor.transform.right + pengOffset.value.y * actor.transform.up + pengOffset.value.z * actor.transform.forward;
            Collider[] returns = new Collider[0];
            if (actor.game.actors.Count > 0)
            {
                switch (rangeType)
                {
                    case RangeType.Cylinder:
                        List<CharacterController> c = new List<CharacterController>();
                        foreach (PengActor a in actor.game.actors)
                        {
                            Vector3 selfDir1 = new Vector3(actor.transform.forward.x, 0, actor.transform.forward.z);
                            Vector3 tarDir1 = new Vector3(a.transform.position.x - actor.transform.position.x, 0, a.transform.position.z - actor.transform.position.z);
                            float angle1 = Vector3.Angle(selfDir1, tarDir1);
                            if ((a.transform.position.y >= pos.y && a.transform.position.y <= pos.y + pengPara.value.y) && angle1 <= pengPara.value.z && tarDir1.magnitude <= pengPara.value.x)
                            {
                                c.Add(a.ctrl);
                            }
                        }
                        returns = new Collider[c.Count];
                        if (c.Count > 0)
                        {
                            for (int i = 0; i < c.Count; i++)
                            {
                                returns[i] = c[i];
                            }
                        }
                        break;
                    case RangeType.Sphere:
                        returns = Physics.OverlapSphere(pos, pengPara.value.x);
                        break;
                    case RangeType.Box:
                        returns = Physics.OverlapBox(pos, pengPara.value / 2, actor.transform.rotation);
                        break;
                }
            }
            if (returns.Length > 0)
            {
                foreach (Collider re in returns)
                {
                    PengActor pa = re.GetComponent<PengActor>();
                    if (pa != null && pa.actorCamp == pengCamp.value && pa.alive)
                    {
                        result.value.Add(pa);
                    }
                }
            }
        }
    }

    public class ForIterator : BaseScript
    {
        public PengInt firstIndex = new PengInt(null, "�׸�ָ��", 0, ConnectionPointType.In);
        public PengInt lastIndex = new PengInt(null, "ĩ��ָ��", 1, ConnectionPointType.In);

        public PengInt pengIndex = new PengInt(null, "ָ��", 0, ConnectionPointType.Out);
        public ForIterator(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.ForIterator;
            scriptName = GetDescription(type);


            outVars[0] = pengIndex;

            inVars[0] = firstIndex;
            inVars[1] = lastIndex;
        }

        public override void Initial()
        {
            base.Initial();
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengInt pi = varSource as PengInt;
                    firstIndex.value = pi.value;
                    break;
                case 1:
                    PengInt pi1 = varSource as PengInt;
                    lastIndex.value = pi1.value;
                    break;
            }
        }

        public override void Function()
        {
            base.Function();
            for (int i = firstIndex.value; i <= lastIndex.value; i++)
            {
                pengIndex.value = i;
                if (flowOutInfo.ElementAt(0).Value > 0 && trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(0).Value) != null)
                {
                    trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(0).Value).Execute();
                }
            }
        }

        public override void ScriptFlowNext()
        {
            if (flowOutInfo.ElementAt(1).Value > 0 && trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(1).Value) != null)
            {
                trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(1).Value).Execute();
            }
        }
    }

}

