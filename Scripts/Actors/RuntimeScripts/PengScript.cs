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
        [Description("1,播放音频,表现,B,高封装")]
        PlayAudio,
        [Description("1,播放特效,表现,B,高封装")]
        PlayEffects,
        [Description("0,尝试索敌,功能,C,高封装")]
        TryGetEnemy,
        [Description("1,允许转向,功能,Y,高封装")]
        AllowChangeDirection,
        [Description("0,设置显隐,表现,S,高封装")]
        SetVisibility,
        [Description("1,伤害流程,功能,S,高封装")]
        AttackDamage,
        [Description("1,设置黑板变量,功能,S,低封装")]
        SetBlackBoardVariables,
        [Description("1,获取变量,值,H,低封装")]
        GetVariables,
        [Description("1,输入分歧,分歧,S,高封装")]
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
        [Description("1,浮点比较,运算,B,低封装")]
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
        [Description("0,取列表数量,值,Q,低封装")]
        ValueGetListCount,
        [Description("1,浮点转字符串,值,F,低封装")]
        ValueFloatToString,
        [Description("1,断点,调试,D,低封装")]
        BreakPoint,
        [Description("1,整型转浮点,值,Z,低封装")]
        ValueIntToFloat,
        [Description("1,着地事件,事件,Z,高封装")]
        OnGround,
        [Description("1,自定义事件,事件,Z,高封装")]
        CustomEvent,
        [Description("1,事件触发,事件,X,高封装")]
        OnEvent,
        [Description("1,完美闪避判定,功能,W,高封装")]
        PerfectDodge,
        [Description("1,字符串相等,运算,Z,低封装")]
        MathStringEqual,
        [Description("1,瞬时跳跃力,功能,S,高封装")]
        JumpForce,
        [Description("1,每帧移动,功能,A,低封装")]
        MoveByFrame,
        [Description("1,增删Buff,功能,Z,高封装")]
        AddOrRemoveBuff,
        [Description("0,是否含有Buff,值,S,高封装")]
        HasBuff,
        [Description("0,Buff获取目标,功能,G,高封装")]
        GetTargetsByBuff,
        [Description("0,状态获取目标,功能,G,高封装")]
        GetTargetsByState,
        [Description("0,召唤Actor,功能,Z,高封装")]
        SummonActor,
        [Description("1,死亡事件,事件,S,高封装")]
        OnDie,
        [Description("1,受击事件,事件,S,高封装")]
        OnHit,
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
                    if (inVars[i] != null)
                    {
                        inVars[i].script = this;
                    }
                    
                }
            }
            if (outVars.Length > 0)
            {
                for (int i = 0; i < outVars.Length; i++)
                {
                    if (outVars[i] != null)
                    {
                        outVars[i].script = this;
                    }
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

}

