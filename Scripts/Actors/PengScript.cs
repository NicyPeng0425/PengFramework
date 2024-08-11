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
    /// 伟大的彭脚本！
    /// 
    /// 添加新脚本的关注点：
    /// 1. PengScriptType
    /// 2. 在PengNode.cs里添加新脚本的节点形式
    /// 3. 在PengActorStateEditorWindow.cs的RightMouseMenu()里写添加新脚本的节点的方法
    /// 4. 在PengActorStateEditorWindow.cs的ReadPengNode()里写读取新脚本的节点的方法
    /// 5. 在PengScript.cs里添加新脚本的运行时形式，包括构造函数及具体方法
    /// 6. 在PengActorState.ConstructRunTimePengScript()里添加运行时构建新脚本的方法
    /// </summary>
    /// 


    public enum PengScriptType
    {
        [Description("轨道执行")]
        OnTrackExecute,
        [Description("输出对象")]
        DebugLog,
        [Description("播放动画")]
        PlayAnimation,
        [Description("切换状态")]
        TransState,
        [Description("范围获取目标")]
        GetTargetsByRange,
        [Description("全局时间变速")]
        GlobalTimeScale,
        [Description("播放音频")]
        PlayAudio,
        [Description("播放特效")]
        PlayEffects,
        [Description("清空目标")]
        ClearTargets,
        [Description("尝试索敌")]
        TryGetEnemy,
        [Description("允许转向")]
        AllowChangeDirection,
        [Description("设置显隐")]
        SetVisibility,
        [Description("伤害流程")]
        AttackDamage,
        [Description("设置黑板变量")]
        SetBlackBoardVariables,
        [Description("获取黑板变量")]
        GetBlackBoardVariables,
        [Description("输入分歧")]
        GetInput,
        [Description("条件分歧")]
        IfElse,
        [Description("枚举分歧")]
        SwitchEnum,
        [Description("整型分歧")]
        SwitchInt,
        [Description("For迭代")]
        ForIterator,
        [Description("Foreach迭代")]
        ForeachIterator,
        [Description("屏幕震动")]
        CameraImpulse,
        [Description("后处理")]
        PostProcess,
        [Description("镜头偏移")]
        CameraOffset,
        [Description("镜头Fov")]
        CameraFOV,
        [Description("加")]
        MathPlus,
        [Description("减")]
        MathMinus,
        [Description("乘")]
        MathTime,
        [Description("除")]
        MathDivide,
        [Description("平方")]
        MathSquare,
        [Description("比较")]
        MathCompare,
        [Description("布尔")]
        MathBool,
        [Description("整型")]
        ValuePengInt,
        [Description("浮点")]
        ValuePengFloat,
        [Description("布尔")]
        ValuePengBool,
        [Description("字符串")]
        ValuePengString,
    }

    public struct ScriptIDVarID
    {
        public int scriptID;
        public int varID;
    }

    public class BaseScript
    {

        //BaseScript描述文件
        //scriptName 名字
        //scriptType 脚本类型
        //scriptID 脚本ID
        //varInID 参数入信息
        //outID 脚本流出信息
        //参数信息

        //脚本名称，显示在编辑器上
        public string scriptName = "默认";
        //脚本的所有者
        public PengActor actor;
        //脚本类型
        public PengScriptType type;
        //所属轨道
        public PengTrack trackMaster;

        public PengVar[] inVars;
        public PengVar[] outVars;

        public int ID;
        public Dictionary<int, int> flowOutInfo = new Dictionary<int, int>();
        public Dictionary<int, ScriptIDVarID> varInID = new Dictionary<int, ScriptIDVarID>();


        public virtual void Construct(string specialInfo)
        {

        }

        //执行一次
        public virtual void Execute()
        {
            Initial();
            Function();
            ScriptFlowNext();
        }

        public virtual void Initial()
        {
            //执行前的初始化
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
            //具体功能
        }

        public virtual void ScriptFlowNext()
        {
            //脚本流
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
        public PengInt pengTrackExecuteFrame = new PengInt(null, "轨道执行帧", 0, ConnectionPointType.Out);
        public PengInt pengStateExecuteFrame = new PengInt(null, "状态执行帧", 0, ConnectionPointType.Out);
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
        public PengString pengAnimationName = new PengString(null, "动画名称", 0, ConnectionPointType.In);
        public PengBool pengHardCut = new PengBool(null, "是否硬切", 1, ConnectionPointType.In);
        public PengFloat pengTransitionNormalizedTime = new PengFloat(null, "过度时间", 2, ConnectionPointType.In);
        public PengFloat pengStartAtNormalizedTime = new PengFloat(null, "开始时间", 3, ConnectionPointType.In);
        public PengInt pengAnimationLayer = new PengInt(null, "动画层", 4, ConnectionPointType.Out);
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
                Debug.LogWarning("不存在该动画状态：" + pengAnimationName.value + "，来自状态"+actor.currentName + "的" + trackMaster.name + "轨道的" + ID.ToString() + "号脚本。");
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
                PengBool condition = new PengBool(null, "条件", i, ConnectionPointType.In);
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
        public PengInt pengInt = new PengInt(null, "值", 0, ConnectionPointType.Out);
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
        public PengFloat pengFloat = new PengFloat(null, "值", 0, ConnectionPointType.Out);
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
        public PengString pengString = new PengString(null, "值", 0, ConnectionPointType.Out);
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
        public PengBool pengBool = new PengBool(null, "值", 0, ConnectionPointType.Out);
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
        public PengListPengActor result = new PengListPengActor(null, "获取到的目标", 0, ConnectionPointType.Out);

        public PengInt typeNum = new PengInt(null, "范围类型", 0, ConnectionPointType.In);
        public PengInt pengCamp = new PengInt(null, "阵营", 1, ConnectionPointType.In);
        public PengVector3 pengPara = new PengVector3(null, "参数", 2, ConnectionPointType.In);
        public PengVector3 pengOffset = new PengVector3(null, "偏移", 3, ConnectionPointType.In);
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
        public PengInt firstIndex = new PengInt(null, "首个指数", 0, ConnectionPointType.In);
        public PengInt lastIndex = new PengInt(null, "末个指数", 1, ConnectionPointType.In);

        public PengInt pengIndex = new PengInt(null, "指数", 0, ConnectionPointType.Out);
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

