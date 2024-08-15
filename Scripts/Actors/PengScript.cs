using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Xml;
using System;
using Unity.VisualScripting;
using UnityEditor;
using static PengScript.GetVariables;

namespace PengScript
{
    /// <summary>
    /// 伟大的彭脚本！
    /// 
    /// 添加新脚本的关注点：
    /// 1. PengScriptType里添加新脚本的类型
    /// 2. 在PengNode.cs里添加新脚本的节点形式
    /// 3. 在PengActorStateEditorWindow.cs的ProcessAddNode()里写添加新脚本的节点的方法
    /// 4. 在PengActorStateEditorWindow.cs的ReadPengNode()里写读取新脚本的节点的方法
    /// 5. 在PengScript.cs里添加新脚本的运行时形式，包括构造函数及具体方法
    /// 6. 在PengActorState.ConstructRunTimePengScript()里添加运行时构建新脚本的方法
    /// </summary>

    public enum ConnectionPointType
    {
        In,
        Out,
        FlowIn,
        FlowOut,
    }

    public enum PengScriptType
    {
        [Description("1,轨道执行,事件,G,低封装")]
        OnTrackExecute,
        [Description("1,输出对象,调试,S,低封装")]
        DebugLog,
        [Description("1,播放动画,表现,B,高封装")]
        PlayAnimation,
        [Description("1,切换状态,功能,Q,高封装")]
        TransState,
        [Description("1,范围获取目标,功能,F,高封装")]
        GetTargetsByRange,
        [Description("1,全局时间变速,功能,Q,高封装")]
        GlobalTimeScale,
        [Description("0,播放音频,表现,B,高封装")]
        PlayAudio,
        [Description("0,播放特效,表现,B,高封装")]
        PlayEffects,
        [Description("0,清空目标,功能,Q,高封装")]
        ClearTargets,
        [Description("0,尝试索敌,功能,C,高封装")]
        TryGetEnemy,
        [Description("0,允许转向,功能,Y,高封装")]
        AllowChangeDirection,
        [Description("0,设置显隐,表现,S,高封装")]
        SetVisibility,
        [Description("0,伤害流程,功能,S,高封装")]
        AttackDamage,
        [Description("1,设置黑板变量,功能,S,低封装")]
        SetBlackBoardVariables,
        [Description("1,获取变量,值,H,低封装")]
        GetVariables,
        [Description("0,输入分歧,功能,S,高封装")]
        GetInput,
        [Description("1,条件分歧,分歧,T,低封装")]
        IfElse,
        [Description("0,枚举分歧,分歧,M,低封装")]
        SwitchEnum,
        [Description("1,For迭代,循环,F,低封装")]
        ForIterator,
        [Description("0,Foreach迭代,循环,F,低封装")]
        ForeachIterator,
        [Description("0,相机震动,表现,X,高封装")]
        CameraImpulse,
        [Description("0,后处理,表现,H,高封装")]
        PostProcess,
        [Description("0,相机偏移,表现,X,高封装")]
        CameraOffset,
        [Description("0,相机Fov,表现,X,高封装")]
        CameraFOV,
        [Description("0,加,运算,J,低封装")]
        MathPlus,
        [Description("0,减,运算,J,低封装")]
        MathMinus,
        [Description("0,乘,运算,C,低封装")]
        MathTime,
        [Description("0,除,运算,C,低封装")]
        MathDivide,
        [Description("0,平方,运算,P,低封装")]
        MathSquare,
        [Description("1,比较,运算,B,低封装")]
        MathCompare,
        [Description("1,布尔运算,运算,B,低封装")]
        MathBool,
        [Description("1,整型,值,Z,低封装")]
        ValuePengInt,
        [Description("1,浮点,值,F,低封装")]
        ValuePengFloat,
        [Description("1,布尔,值,B,低封装")]
        ValuePengBool,
        [Description("1,字符串,值,Z,低封装")]
        ValuePengString,
        [Description("1,Vector3,值,V,低封装")]
        ValuePengVector3,
        [Description("0,Vector2,值,V,低封装")]
        ValuePengVector2,
        [Description("0,取列表数量,值,Q,低封装")]
        ValueGetListCount,
        [Description("1,浮点转字符串,值,F,低封装")]
        ValueFloatToString,
        [Description("1,断点,调试,D,低封装")]
        BreakPoint,
        [Description("1,整型转浮点,值,Z,低封装")]
        ValueIntToFloat,
        [Description("0,着地分歧,分歧,Z,高封装")]
        OnGround,
        [Description("1,自定义事件,事件,Z,高封装")]
        CustomEvent,
        [Description("1,事件触发,事件,X,高封装")]
        OnEvent,
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
        public Dictionary<int, ScriptIDVarID> flowOutInfo = new Dictionary<int, ScriptIDVarID>();
        public Dictionary<int, ScriptIDVarID> varInID = new Dictionary<int, ScriptIDVarID>();

        //构造函数中调用，根据特殊信息对脚本进行初始化
        public virtual void Construct(string specialInfo)
        {

        }

        //执行一次
        public virtual void Execute(int functionIndex)
        {
            Initial(functionIndex);
            Function(functionIndex);
            ScriptFlowNext();
        }

        public virtual void Initial(int functionIndex)
        {
            //单次执行前的初始化
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

        //不执行也想获得该节点的计算值，则可以重写并调用此方法
        public virtual void GetValue()
        {

        }

        //有值流入的都需要写
        public virtual void SetValue(int inVarID, PengVar varSource)
        {

        }

        public virtual void Function(int functionIndex)
        {
            //具体功能
        }

        public virtual void ScriptFlowNext()
        {
            //脚本流，除了分支和迭代，基本不需要重写
            if (trackMaster != null && trackMaster.scripts.Count > 0)
            {
                if (flowOutInfo.Count > 0)
                {
                    for(int i = 0; i < flowOutInfo.Count; i++)
                    {
                        if(flowOutInfo.ElementAt(i).Value.scriptID >= 0)
                        {
                            if(trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(i).Value.scriptID) == null)
                            {
                                continue;
                            }
                            else
                            {
                                trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(i).Value.scriptID).Execute(flowOutInfo.ElementAt(i).Value.varID);
                            }
                        }
                    }
                }
            }
        }
        //不需要动
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
        public static string GetDescription(Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description.Split(",")[1];
                    }
                }
            }
            return null;
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
        public static Dictionary<int, int> ParseStringToDictionaryIntInt(string str)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            if (str == "")
                return result;
            string[] strings = str.Split(",");
            if (strings.Length > 0)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    string[] s = strings[i].Split(":");
                    result.Add(int.Parse(s[0]), int.Parse(s[1]));
                }
            }
            return result;
        }
    }

    public class OnTrackExecute: BaseScript
    {
        public PengInt pengTrackExecuteFrame = new PengInt("轨道执行帧", 0, ConnectionPointType.Out);
        public PengInt pengStateExecuteFrame = new PengInt("状态执行帧", 0, ConnectionPointType.Out);
        public OnTrackExecute(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
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

        public override void Initial(int functionIndex)
        {
            pengTrackExecuteFrame.value = actor.currentStateFrame - trackMaster.start;
            pengStateExecuteFrame.value = actor.currentStateFrame;
        }
    }

    public class PlayAnimation : BaseScript
    {
        public PengString pengAnimationName = new PengString("动画名称", 0, ConnectionPointType.In);
        public PengBool pengHardCut = new PengBool("是否硬切", 1, ConnectionPointType.In);
        public PengFloat pengTransitionNormalizedTime = new PengFloat("过度时间", 2, ConnectionPointType.In);
        public PengFloat pengStartAtNormalizedTime = new PengFloat("开始时间", 3, ConnectionPointType.In);
        public PengInt pengAnimationLayer = new PengInt("动画层", 4, ConnectionPointType.Out);
        public PlayAnimation(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
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

        public override void Initial(int functionIndex)
        {
            base.Initial(functionIndex);
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

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
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

    public class ValuePengInt : BaseScript
    {
        public PengInt pengInt = new PengInt("值", 0, ConnectionPointType.Out);
        public ValuePengInt(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
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
        public PengFloat pengFloat = new PengFloat("值", 0, ConnectionPointType.Out);
        public ValuePengFloat(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
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
        public PengString pengString = new PengString("值", 0, ConnectionPointType.Out);
        public ValuePengString(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
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
        public PengBool pengBool = new PengBool("值", 0, ConnectionPointType.Out);
        public ValuePengBool(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
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
        public enum RangeType
        {
            [Description("圆柱体")]
            Cylinder,
            [Description("球体")]
            Sphere,
            [Description("盒形")]
            Box,
        }

        public RangeType rangeType = RangeType.Cylinder;
        public PengList<PengActor> result = new PengList<PengActor>("获取到的目标", 0, ConnectionPointType.Out);

        public PengInt typeNum = new PengInt("范围类型", 0, ConnectionPointType.In);
        public PengInt pengCamp = new PengInt("阵营", 1, ConnectionPointType.In);
        public PengVector3 pengPara = new PengVector3("参数", 2, ConnectionPointType.In);
        public PengVector3 pengOffset = new PengVector3("偏移", 3, ConnectionPointType.In);
        public GetTargetsByRange(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
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
                string[] strFirst = specialInfo.Split(";");
                switch (int.Parse(strFirst[0]))
                {
                    case 1:
                        rangeType = RangeType.Cylinder;
                        break;
                    case 2:
                        rangeType = RangeType.Sphere;
                        break;
                    case 3:
                        rangeType = RangeType.Box;
                        break;
                }
                typeNum.value = (int) rangeType;
                string[] strSecond = strFirst[1].Split(",");
                pengPara.value = new Vector3(float.Parse(strSecond[0]), float.Parse(strSecond[1]), float.Parse(strSecond[2]));
                string[] strThird = strFirst[2].Split(",");
                pengOffset.value = new Vector3(float.Parse(strThird[0]), float.Parse(strThird[1]), float.Parse(strThird[2]));
                pengCamp.value = int.Parse(strFirst[3]);
            }

            outVars[0] = result;

            inVars[0] = typeNum;
            inVars[1] = pengCamp;
            inVars[2] = pengPara;
            inVars[3] = pengOffset;
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

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
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
        public PengInt firstIndex = new PengInt("首个指数", 0, ConnectionPointType.In);
        public PengInt lastIndex = new PengInt("末个指数", 1, ConnectionPointType.In);

        public PengInt pengIndex = new PengInt("指数", 0, ConnectionPointType.Out);
        public bool breakOrNot = false;
        public ForIterator(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
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

        public override void Initial(int functionIndex)
        {
            base.Initial(functionIndex);
            if (functionIndex == 0)
            {
                breakOrNot = false;
            }
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

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            if (functionIndex == 1)
            {
                breakOrNot = true;
            }
            if (!breakOrNot)
            {
                for (int i = firstIndex.value; i <= lastIndex.value; i++)
                {
                    pengIndex.value = i;
                    if (flowOutInfo.ElementAt(0).Value.scriptID > 0 && trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(0).Value.scriptID) != null)
                    {
                        trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(0).Value.scriptID).Execute(flowOutInfo.ElementAt(0).Value.varID);
                        if (breakOrNot)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public override void ScriptFlowNext()
        {
            if (flowOutInfo.ElementAt(1).Value.scriptID > 0 && trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(1).Value.scriptID) != null)
            {
                trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(1).Value.scriptID).Execute(flowOutInfo.ElementAt(1).Value.varID);
            }
        }
    }

    public class ValuePengVector3 : BaseScript
    {
        public PengFloat pengX = new PengFloat("值", 0, ConnectionPointType.In);
        public PengFloat pengY = new PengFloat("值", 1, ConnectionPointType.In);
        public PengFloat pengZ = new PengFloat("值", 2, ConnectionPointType.In);

        public PengVector3 pengVector3 = new PengVector3("值", 0, ConnectionPointType.Out);
        public ValuePengVector3(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.ValuePengVector3;
            scriptName = GetDescription(type);
            inVars[0] = pengX;
            inVars[1] = pengY;
            inVars[2] = pengZ;
            outVars[0] = pengVector3;
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                pengX.value = float.Parse(str[0]);
                pengY.value = float.Parse(str[1]);
                pengZ.value = float.Parse(str[2]);
            }
        }

        public override void GetValue()
        {
            base.GetValue();
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
            pengVector3.value = new Vector3(pengX.value, pengY.value, pengZ.value);
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            base.SetValue(inVarID, varSource);
            switch (inVarID)
            {
                case 0:
                    PengFloat pf1 = varSource as PengFloat;
                    pengX.value = pf1.value;
                    break;
                case 1:
                    PengFloat pf2 = varSource as PengFloat;
                    pengY.value = pf2.value;
                    break;
                case 2:
                    PengFloat pf3 = varSource as PengFloat;
                    pengZ.value = pf3.value;
                    break;
            }
        }
    }

    public class DebugLog : BaseScript
    {
        public PengT pengT = new PengT("对象", 0, ConnectionPointType.In);
        public string str = "";
        public DebugLog(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.DebugLog;
            scriptName = GetDescription(type);
            inVars[0] = pengT;
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            str = "";
            switch (inVarID)
            {
                case 0:
                    switch(varSource.type)
                    {
                        case PengVarType.Float:
                            PengFloat pf = varSource as PengFloat;
                            str ="(float)" + pf.value.ToString();
                            break;
                        case PengVarType.Int:
                            PengInt pi = varSource as PengInt;
                            str = "(int)" + pi.value.ToString();
                            break;
                        case PengVarType.String:
                            PengString ps = varSource as PengString;
                            str = "(string)" + ps.value;
                            break;
                        case PengVarType.Bool:
                            PengBool pb = varSource as PengBool;
                            str = "(bool)" + (pb.value ? "true" : "false");
                            break;
                        case PengVarType.PengActor:
                            PengPengActor ppa = varSource as PengPengActor;
                            str = "(PengActor)" + "Actor" + ppa.value.actorID.ToString();
                            break;
                        case PengVarType.PengList:
                            PengList<PengActor> plpa = varSource as PengList<PengActor>;
                            str = "(List<PengActor>)";
                            if (plpa.value.Count > 0)
                            {
                                for (int i = 0; i < plpa.value.Count; i ++)
                                {
                                    str += "Actor" + plpa.value[i].actorID.ToString();
                                    if (i != plpa.value.Count - 1)
                                    {
                                        str += ",";
                                    }
                                }
                            }
                            else
                            {
                                str = str + "Null";
                            }    
                            break;
                        case PengVarType.Vector3:
                            PengVector3 pv3 = varSource as PengVector3;
                            str = pv3.value.ToString();
                            break;
                        case PengVarType.Vector2:
                            PengVector2 pv2 = varSource as PengVector2;
                            str = pv2.value.ToString();
                            break;
                        case PengVarType.T:
                            str = "";
                            break;
                    }
                    break;
            }
            if (str != "")
            {
                str = "Actor" + actor.actorID.ToString() + "在" + actor.currentName + "状态的" + trackMaster.name + "轨道中调用了DebugLog，内容为：" + str;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            if (str != "")
            {
                Debug.Log(str);
            }
            else
            {
                Debug.Log("Actor" + actor.actorID.ToString() + "在" + actor.currentName + "状态的" + trackMaster.name + "轨道中调用了DebugLog，但没有取得对象。");
            }
        }
    }

    public class ValueFloatToString : BaseScript
    {
        public PengFloat pengFloat = new PengFloat("浮点", 0, ConnectionPointType.In);

        public PengString pengString = new PengString("文本", 0, ConnectionPointType.Out);
        public ValueFloatToString(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.ValueFloatToString;
            scriptName = GetDescription(type);
            inVars[0] = pengFloat;
            outVars[0] = pengString;
            if (specialInfo != "")
            {
                pengFloat.value = float.Parse(specialInfo);
            }
        }

        public override void GetValue()
        {
            base.GetValue();
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
            pengString.value = pengFloat.value.ToString();
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            base.SetValue(inVarID, varSource);
            switch (inVarID)
            {
                case 0:
                    PengFloat pf1 = varSource as PengFloat;
                    pengFloat.value = pf1.value;
                    break;
            }
        }
    }

    public class TransState : BaseScript
    {
        public PengString stateName = new PengString("状态名称", 0, ConnectionPointType.In);
        public TransState(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                stateName.value = specialInfo;
            }
            type = PengScriptType.TransState;
            scriptName = GetDescription(type);
            inVars[0] = stateName;
        }

        public override void GetValue()
        {
            base.GetValue();
            if (varInID[0].scriptID > 0)
            {
                PengVar vari = trackMaster.GetOutPengVarByScriptIDPengVarID(varInID[0].scriptID, varInID[0].varID);
                vari.script.GetValue();
                SetValue(0, vari);
            }
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengString ps = varSource as PengString;
                    stateName.value = ps.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            if (actor.actorStates.ContainsKey(stateName.value))
            {
                actor.TransState(stateName.value, true);
            }
            else
            {
                Debug.Log("Actor" + actor.actorID.ToString() + "在" + actor.currentName + "状态的" + trackMaster.name + "轨道中调用了切换状态，但没有给定名称的状态。");
            }
        }
    }

    public class BreakPoint : BaseScript
    {
        public BreakPoint(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.BreakPoint;
            scriptName = GetDescription(type);
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
#if UNITY_EDITOR
            if (!EditorApplication.isPaused)
            {
                EditorApplication.isPaused = true;
            }
#endif
        }
    }

    public class GlobalTimeScale : BaseScript
    {
        public PengFloat timeScale = new PengFloat("时间速度", 0, ConnectionPointType.In);
        public PengFloat duration = new PengFloat("持续时间", 1, ConnectionPointType.In);

        public GlobalTimeScale(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                timeScale.value = float.Parse(str[0]);
                duration.value = float.Parse(str[1]);
            }
            inVars[0] = timeScale;
            inVars[1] = duration;
            type = PengScriptType.GlobalTimeScale;
            scriptName = GetDescription(type);
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengFloat pf = varSource as PengFloat;
                    timeScale.value = pf.value;
                    break;
                case 1:
                    PengFloat pf1 = varSource as PengFloat;
                    duration.value = pf1.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            actor.game.GloablTimeScaleFunc(timeScale.value, duration.value);
        }
    }

    public class MathCompare : BaseScript
    {
        public enum CompareType
        {
            Less = 0,
            NoLess = 1,
            Larger = 2,
            NoLarger = 3,
            Equal = 4,
            NotEqual = 5,
        }

        public enum CompareTypeCN
        {
            小于 = 0,
            不小于 = 1,
            大于 = 2,
            不大于 = 3,
            等于 = 4,
            不等于 = 5,
        }

        public PengFloat compare1 = new PengFloat("比较数一", 0, ConnectionPointType.In);
        public PengInt compareType = new PengInt("比较方式", 1, ConnectionPointType.In);
        public PengFloat compare2 = new PengFloat("比较数二", 2, ConnectionPointType.In);

        public CompareType compare;

        public PengBool result = new PengBool("结果", 0, ConnectionPointType.Out);
        public MathCompare(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.MathCompare;
            scriptName = GetDescription(type);
            inVars[0] = compare1;
            inVars[1] = compareType;
            inVars[2] = compare2;
            outVars[0] = result;
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                compare1.value = float.Parse(str[0]);
                compareType.value = int.Parse(str[1]);
                compare = (PengScript.MathCompare.CompareType)compareType.value;
                compare2.value = float.Parse(str[2]);
            }
        }

        public override void GetValue()
        {
            base.GetValue();
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
            switch (compare)
            {
                case CompareType.Less:
                    result.value = compare1.value < compare2.value;
                    break;
                case CompareType.NoLess:
                    result.value = compare1.value >= compare2.value;
                    break;
                case CompareType.Larger:
                    result.value = compare1.value > compare2.value;
                    break;
                case CompareType.NoLarger:
                    result.value = compare1.value <= compare2.value;
                    break;
                case CompareType.Equal:
                    result.value = compare1.value == compare2.value;
                    break;
                case CompareType.NotEqual:
                    result.value = compare1.value != compare2.value;
                    break;
            }
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            base.SetValue(inVarID, varSource);
            switch (inVarID)
            {
                case 0:
                    PengFloat pf1 = varSource as PengFloat;
                    compare1.value = pf1.value;
                    break;
                case 1:
                    PengInt pf2 = varSource as PengInt;
                    compareType.value = pf2.value;
                    break;
                case 2:
                    PengFloat pf3 = varSource as PengFloat;
                    compare2.value = pf3.value;
                    break;
            }
        }
    }

    public class ValueIntToFloat : BaseScript
    {
        public PengFloat pengFloat = new PengFloat("浮点", 0, ConnectionPointType.Out);

        public PengInt pengInt = new PengInt("整型", 0, ConnectionPointType.In);
        public ValueIntToFloat(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.ValueIntToFloat;
            scriptName = GetDescription(type);
            inVars[0] = pengInt;
            outVars[0] = pengFloat;
            if (specialInfo != "")
            {
                pengInt.value = int.Parse(specialInfo);
            }
        }

        public override void GetValue()
        {
            base.GetValue();
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
            pengFloat.value = (float)pengInt.value;
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            base.SetValue(inVarID, varSource);
            switch (inVarID)
            {
                case 0:
                    PengInt pf1 = varSource as PengInt;
                    pengInt.value = pf1.value;
                    break;
            }
        }
    }

    public enum BBTarget
    {
        Self = 0,
        Global = 1,
        Targets = 2,
    }

    public enum BBTargetCN
    {
        自身 = 0,
        全局 = 1,
        目标 = 2,
    }

    public class SetBlackBoardVariables : BaseScript
    {
        public PengString varName = new PengString("变量名", 0, ConnectionPointType.In);
        public PengT value = new PengT("值", 1, ConnectionPointType.In);
        public PengInt targetType = new PengInt("目标类型", 2, ConnectionPointType.In);

        public BBTarget target;

        public SetBlackBoardVariables(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.SetBlackBoardVariables;
            scriptName = GetDescription(type);
            inVars[0] = varName;
            inVars[1] = value;
            inVars[2] = targetType;
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                varName.value = str[0];
                targetType.value = int.Parse(str[1]);
                target = (BBTarget)targetType.value;
            }
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            base.SetValue(inVarID, varSource);
            switch (inVarID)
            {
                case 0:
                    PengString ps = varSource as PengString;
                    varName.value = ps.value;
                    break;
                case 1:
                    value.value = varSource;
                    break;
                case 2:
                    PengInt pi = varSource as PengInt;
                    targetType.value = pi.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            switch (target)
            {
                case BBTarget.Self:
                    SetBBVar(varName.value, value, actor.bb);
                    break;
                case BBTarget.Global:
                    SetBBVar(varName.value, value, actor.game.bb);
                    break;
                case BBTarget.Targets:
                    if (actor.targets.Count > 0)
                    {
                        for (int i = 0; i < actor.targets.Count; i++)
                        {
                            SetBBVar(varName.value, value, actor.targets[i].bb);
                        }
                    }
                    break;
            }
        }

        public void SetBBVar(string name, PengT var, PengBlackBoard<PengGameManager> bb)
        {
            switch (var.value.GetType().FullName)
            {
                case "PengVariables.PengInt":
                    PengInt pi = var.value as PengInt;
                    bb.SetBBVar(name, pi.value);
                    break;
                case "PengVariables.PengFloat":
                    PengFloat pf = var.value as PengFloat;
                    bb.SetBBVar(name, pf.value);
                    break;
                case "PengVariables.PengString":
                    PengString ps = var.value as PengString;
                    bb.SetBBVar(name, ps.value);
                    break;
                case "PengVariables.PengBool":
                    PengBool pb = var.value as PengBool;
                    bb.SetBBVar(name, pb.value);
                    break;
                case "PengVariables.PengActor":
                    PengPengActor ppa = var.value as PengPengActor;
                    bb.SetBBVar(name, ppa.value);
                    break;
                default:
                    Debug.LogWarning("不支持的黑板变量类型。");
                    break;
            }
        }

        public void SetBBVar(string name, PengT var, PengBlackBoard<PengActor> bb)
        {
            switch (var.value.GetType().FullName)
            {
                case "PengVariables.PengInt":
                    PengInt pi = var.value as PengInt;
                    bb.SetBBVar(name, pi.value);
                    break;
                case "PengVariables.PengFloat":
                    PengFloat pf = var.value as PengFloat;
                    bb.SetBBVar(name, pf.value);
                    break;
                case "PengVariables.PengString":
                    PengString ps = var.value as PengString;
                    bb.SetBBVar(name, ps.value);
                    break;
                case "PengVariables.PengBool":
                    PengBool pb = var.value as PengBool;
                    bb.SetBBVar(name, pb.value);
                    break;
                case "PengVariables.PengActor":
                    PengPengActor ppa = var.value as PengPengActor;
                    bb.SetBBVar(name, ppa.value);
                    break;
                default:
                    Debug.LogWarning("不支持的黑板变量类型。");
                    break;
            }
        }
    }

    public class OnEvent : BaseScript
    {
        public PengString eventName = new PengString("事件名称", 0, ConnectionPointType.In);

        public PengInt intMessage = new PengInt("整型参数", 0, ConnectionPointType.Out);
        public PengFloat floatMessage = new PengFloat("浮点参数", 1, ConnectionPointType.Out);
        public PengString stringMessage = new PengString("字符串参数", 2, ConnectionPointType.Out);
        public PengBool boolMessage = new PengBool("布尔参数", 3 , ConnectionPointType.Out);
        public OnEvent(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[4];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.OnEvent;
            scriptName = GetDescription(type);
            eventName.value = specialInfo;
            if (eventName.value == "")
            {
                Debug.LogWarning("存在事件触发脚本，其事件名称为空。");
            }
            inVars[0] = eventName;
            outVars[0] = intMessage;
            outVars[1] = floatMessage;
            outVars[2] = stringMessage;
            outVars[3] = boolMessage;
        }

        public void EventTrigger(int intMsg, float floatMsg, string stringMsg, bool boolMsg)
        {
            intMessage.value = intMsg;
            floatMessage.value = floatMsg;
            stringMessage.value = stringMsg;
            boolMessage.value = boolMsg;
            Execute(0);
        }
    }

    public class CustomEvent : BaseScript
    {
        public PengString eventName = new PengString("事件名称", 0, ConnectionPointType.In);

        public PengInt intMessage = new PengInt("整型参数", 1, ConnectionPointType.In);
        public PengFloat floatMessage = new PengFloat("浮点参数", 2, ConnectionPointType.In);
        public PengString stringMessage = new PengString("字符串参数", 3, ConnectionPointType.In);
        public PengBool boolMessage = new PengBool("布尔参数", 4, ConnectionPointType.In);
        public CustomEvent(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.CustomEvent;
            scriptName = GetDescription(type);
            inVars[0] = eventName;
            inVars[1] = intMessage;
            inVars[2] = floatMessage;
            inVars[3] = stringMessage;
            inVars[4] = boolMessage;

            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                eventName.value = str[0];
                intMessage.value = int.Parse(str[1]);
                floatMessage.value = float.Parse(str[2]);
                stringMessage.value = str[3];
                boolMessage.value = int.Parse(str[4]) > 0;
            }
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengString ps = varSource as PengString;
                    eventName.value = ps.value;
                    break;
                case 1:
                    PengInt pb = varSource as PengInt;
                    intMessage.value = pb.value;
                    break;
                case 2:
                    PengFloat pf1 = varSource as PengFloat;
                    floatMessage.value = pf1.value;
                    break;
                case 3:
                    PengString pf2 = varSource as PengString;
                    stringMessage.value = pf2.value;
                    break;
                case 4:
                    PengBool pi = varSource as PengBool;
                    boolMessage.value = pi.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            actor.game.eventManager.TriggerEvent(eventName.value, intMessage.value, floatMessage.value, stringMessage.value, boolMessage.value);
        }
    }

    public class GetVariables : BaseScript
    {
        public enum VariableSource
        {
            ActorBlackBoard = 0,
            GlobalBlackBoard = 1,
            ActorAttribute = 2,
        }

        public enum VariableSourceCN
        {
            角色黑板 = 0,
            全局黑板 = 1,
            角色属性 = 2,
        }

        public enum BlackBoardType
        {
            Int = 0,
            Float = 1,
            String = 2,
            Bool = 3,
            PengActor = 4
        }

        public enum BlackBoardTypeCN
        {
            整型黑板 = 0,
            浮点黑板 = 1,
            字符串黑板 = 2,
            布尔黑板 = 3,
            Actor黑板 = 4,
        }

        public enum VariableType
        {
            ActorID = 0,
            ActorName = 1,
            ActorCamp = 2,
            ActorAttackPower = 3,
            ActorDefendPower = 4,
            ActorCriticalRate = 5,
            ActorCriticalDamageRatio = 6,
            ActorCurrentHP = 7,
            ActorMaxHP = 8,
            ActorPosition = 9,
        }

        public enum VariableTypeCN
        {
            ActorID = 0,
            Actor名字 = 1,
            Actor阵营 = 2,
            攻击力 = 3,
            防御力 = 4,
            暴击率 = 5,
            暴伤倍率 = 6,
            当前生命值 = 7,
            最大生命值 = 8,
            位置 = 9,
        }

        public PengInt int1 = new PengInt("占位", 0, ConnectionPointType.In);
        public PengInt int2 = new PengInt("占位", 1, ConnectionPointType.In);

        public PengInt intOut = new PengInt("整型输出", 0, ConnectionPointType.Out);
        public PengFloat floatOut = new PengFloat("浮点输出", 0, ConnectionPointType.Out);
        public PengString stringOut = new PengString("字符串输出", 0, ConnectionPointType.Out);
        public PengBool boolOut = new PengBool("布尔输出", 0, ConnectionPointType.Out);
        public PengPengActor actorOut = new PengPengActor("角色输出", 0, ConnectionPointType.Out);
        public PengVector3 vec3Out = new PengVector3("向量输出", 0, ConnectionPointType.Out);

        public VariableSource variableSource;
        public BlackBoardType bbType;
        public VariableType varType;
        public PengPengActor ppa;
        public PengString varName;
        public GetVariables(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.GetVariables;
            scriptName = GetDescription(type);
            inVars[0] = int1;
            inVars[1] = int2;
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                variableSource = (VariableSource)int.Parse(str[0]);
                bbType = (BlackBoardType)int.Parse(str[1]);
                varType = (VariableType)int.Parse(str[2]);
                int1.value = (int)variableSource;
                switch (variableSource)
                {
                    case VariableSource.ActorBlackBoard:
                        int2.value = (int)bbType;
                        switch (bbType)
                        {
                            case BlackBoardType.Int:
                                outVars[0] = intOut;
                                break;
                            case BlackBoardType.Float:
                                outVars[0] = floatOut;
                                break;
                            case BlackBoardType.String:
                                outVars[0] = stringOut;
                                break;
                            case BlackBoardType.Bool:
                                outVars[0] = boolOut;
                                break;
                            case BlackBoardType.PengActor:
                                outVars[0] = actorOut;
                                break;
                        }
                        ppa = new PengPengActor("角色", 2, ConnectionPointType.In);
                        inVars[2] = ppa;
                        varName = new PengString("变量名", 3, ConnectionPointType.In);
                        if (str.Length > 3)
                        {
                            varName.value = str[3];
                        }
                        inVars[3] = varName;
                        break;
                    case VariableSource.GlobalBlackBoard:
                        int2.value = (int)bbType;
                        switch (bbType)
                        {
                            case BlackBoardType.Int:
                                outVars[0] = intOut;
                                break;
                            case BlackBoardType.Float:
                                outVars[0] = floatOut;
                                break;
                            case BlackBoardType.String:
                                outVars[0] = stringOut;
                                break;
                            case BlackBoardType.Bool:
                                outVars[0] = boolOut;
                                break;
                            case BlackBoardType.PengActor:
                                outVars[0] = actorOut;
                                break;
                        }
                        varName = new PengString("变量名", 2, ConnectionPointType.In);
                        if (str.Length > 3)
                        {
                            varName.value = str[3];
                        }
                        inVars[2] = varName;
                        break;
                    case VariableSource.ActorAttribute:
                        int2.value = (int)varType;
                        if (varType == VariableType.ActorID || varType == VariableType.ActorCamp)
                        {
                            outVars[0] = intOut;
                        }
                        else if (varType == VariableType.ActorName)
                        {
                            outVars[0] = stringOut;
                        }
                        else if (varType == VariableType.ActorAttackPower || varType == VariableType.ActorDefendPower ||
                            varType == VariableType.ActorCriticalRate || varType == VariableType.ActorCriticalDamageRatio ||
                            varType == VariableType.ActorCurrentHP || varType == VariableType.ActorMaxHP)
                        {
                            outVars[0] = floatOut;
                        }
                        else if (varType == VariableType.ActorPosition)
                        {
                            outVars[0] = vec3Out;
                        }
                        ppa = new PengPengActor("角色", 2, ConnectionPointType.In);
                        inVars[2] = ppa;
                        break;
                }
            }
        }

        public override void GetValue()
        {
            base.GetValue();
            if (varInID.Count > 0 && inVars.Length > 0)
            {
                for (int i = 2; i < varInID.Count; i++)
                {
                    if (varInID.ElementAt(i).Value.scriptID > 0)
                    {
                        PengVar vari = trackMaster.GetOutPengVarByScriptIDPengVarID(varInID.ElementAt(i).Value.scriptID, varInID.ElementAt(i).Value.varID);
                        vari.script.GetValue();
                        SetValue(i, vari);
                    }
                }
            }
            if ((variableSource == VariableSource.ActorBlackBoard && (ppa.value == null || varName.value == "")) ||
                (variableSource == VariableSource.GlobalBlackBoard && varName.value == "") ||
                (variableSource == VariableSource.ActorAttribute && ppa.value == null))
            {
                Debug.Log("可能取不到变量！PengActor未引用时，默认设为自身。");
            }
            if (ppa.value == null)
            {
                ppa.value = actor;
            }
            switch (variableSource)
            {
                case VariableSource.ActorBlackBoard:
                    switch (bbType)
                    {
                        case BlackBoardType.Int:
                            intOut.value = ppa.value.bb.GetBBInt(varName.value);
                            break;
                        case BlackBoardType.Float:
                            floatOut.value = ppa.value.bb.GetBBFloat(varName.value);
                            break;
                        case BlackBoardType.String:
                            stringOut.value = ppa.value.bb.GetBBString(varName.value);
                            break;
                        case BlackBoardType.Bool:
                            boolOut.value = ppa.value.bb.GetBBBool(varName.value);
                            break;
                        case BlackBoardType.PengActor:
                            actorOut.value = ppa.value.bb.GetBBPengActor(varName.value);
                            break;
                    }
                    break;
                case VariableSource.GlobalBlackBoard:
                    switch (bbType)
                    {
                        case BlackBoardType.Int:
                            intOut.value = actor.game.bb.GetBBInt(varName.value);
                            break;
                        case BlackBoardType.Float:
                            floatOut.value = actor.game.bb.GetBBFloat(varName.value);
                            break;
                        case BlackBoardType.String:
                            stringOut.value = actor.game.bb.GetBBString(varName.value);
                            break;
                        case BlackBoardType.Bool:
                            boolOut.value = actor.game.bb.GetBBBool(varName.value);
                            break;
                        case BlackBoardType.PengActor:
                            actorOut.value = actor.game.bb.GetBBPengActor(varName.value);
                            break;
                    }
                    break;
                case VariableSource.ActorAttribute:
                    switch (varType)
                    {
                        case VariableType.ActorID:
                            intOut.value = ppa.value.actorID;
                            break;
                        case VariableType.ActorName:
                            stringOut.value = ppa.value.actorName;
                            break;
                        case VariableType.ActorCamp:
                            intOut.value = ppa.value.actorCamp;
                            break;
                        case VariableType.ActorAttackPower:
                            floatOut.value = ppa.value.attackPower;
                            break;
                        case VariableType.ActorDefendPower:
                            floatOut.value = ppa.value.defendPower;
                            break;
                        case VariableType.ActorCriticalRate:
                            floatOut.value = ppa.value.criticalRate;
                            break;
                        case VariableType.ActorCriticalDamageRatio:
                            floatOut.value = ppa.value.criticalDamageRatio;
                            break;
                        case VariableType.ActorCurrentHP:
                            floatOut.value = ppa.value.currentHP;
                            break;
                        case VariableType.ActorMaxHP:
                            floatOut.value = ppa.value.m_maxHP;
                            break;
                        case VariableType.ActorPosition:
                            vec3Out.value = ppa.value.transform.position;
                            break;
                    }
                    break;
            }
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            base.SetValue(inVarID, varSource);
            switch (inVarID)
            {
                case 2:
                    switch (varSource.GetType().FullName)
                    {
                        case "PengVariables.PengPengActor":
                            PengPengActor ppa = varSource as PengPengActor;
                            this.ppa.value = ppa.value;
                            break;
                        case "PengVariables.PengString":
                            PengString ps = varSource as PengString;
                            varName.value = ps.value;
                            break;
                    }
                    break;
                case 3:
                    PengString ps2 = varSource as PengString;
                    varName.value = ps2.value;
                    break;
            }
        }
    }

    public class MathBool : BaseScript
    {
        public enum BoolType
        {
            AND = 0,
            OR = 1,
            NOT = 2,
        }

        public PengBool bool1 = new PengBool("布尔值一", 0, ConnectionPointType.In);
        public PengInt boolInt = new PengInt("运算方式", 1, ConnectionPointType.In);
        public PengBool bool2 = new PengBool("布尔值二", 2, ConnectionPointType.In);

        public BoolType boolType;

        public PengBool result = new PengBool("结果", 0, ConnectionPointType.Out);
        public MathBool(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.MathBool;
            scriptName = GetDescription(type);
            inVars[0] = bool1;
            inVars[1] = boolInt;
            inVars[2] = bool2;
            outVars[0] = result;
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                bool1.value = int.Parse(str[0]) > 0;
                boolInt.value = int.Parse(str[1]);
                boolType = (BoolType)boolInt.value;
                bool2.value = int.Parse(str[2]) > 0;
            }
        }

        public override void GetValue()
        {
            base.GetValue();
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
            switch (boolType)
            {
                case BoolType.AND:
                    result.value = bool1.value & bool2.value;
                    break;
                case BoolType.OR:
                    result.value = bool1.value | bool2.value;
                    break;
                case BoolType.NOT:
                    result.value = !bool1.value;
                    break;
            }
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            base.SetValue(inVarID, varSource);
            switch (inVarID)
            {
                case 0:
                    PengBool pb1 = varSource as PengBool;
                    bool1.value = pb1.value;
                    break;
                case 1:
                    PengInt pi = varSource as PengInt;
                    boolInt.value = pi.value;
                    boolType = (BoolType)boolInt.value;
                    break;
                case 2:
                    PengBool pb2 = varSource as PengBool;
                    bool2.value = pb2.value;
                    break;
            }
        }
    }
}

