using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using System.Reflection;
using static PengNode;
using System.Security.Cryptography.X509Certificates;

namespace PengScript
{
    /// <summary>
    /// 伟大的彭脚本！
    /// 
    /// 添加新脚本的关注点：
    /// 1. PengScriptType
    /// 2. 构造函数及具体方法
    /// 3. PengActorState.ConstructRunTimePengScript()
    /// 4. PengNode.cs
    /// 5. PengActorStateEditorWindow.RightMouseMenu()
    /// </summary>
    /// 


    public enum PengScriptType
    {
        [Description("轨道执行")]
        OnExecute,
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

        //执行一次
        public virtual void Execute()
        {
            Initial();
            Execute();
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
                        PengVar.SetValue(ref inVars[varInID.ElementAt(i).Key], ref vari);
                    }
                }
            }
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
        public PengInt pengTrackExecuteFrame;
        public PengInt pengStateExecuteFrame;
        public OnTrackExecute(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.scriptName = "轨道执行";
            this.type = PengScriptType.OnExecute;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[0];
            outVars = new PengVar[2];
            outVars[0] = pengTrackExecuteFrame;
            outVars[1] = pengStateExecuteFrame;
            InitialPengVars();
        }

        public override void Initial()
        {
            pengTrackExecuteFrame.value = actor.currentStateFrame - trackMaster.start;
            pengStateExecuteFrame.value = actor.currentStateFrame;
        }
    }

    public class PlayAnimation : BaseScript
    {
        public PengString pengAnimationName;
        public PengBool pengHardCut;
        public PengFloat pengTransitionNormalizedTime;
        public PengFloat pengStartAtNormalizedTime;
        public PengInt pengAnimationLayer;
        public PlayAnimation(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.scriptName = "播放动画";
            this.type = PengScriptType.OnExecute;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[5];
            outVars = new PengVar[0];
            inVars[0] = pengAnimationName;
            inVars[1] = pengHardCut;
            inVars[2] = pengTransitionNormalizedTime;
            inVars[3] = pengStartAtNormalizedTime;
            inVars[4] = pengAnimationLayer;
            InitialPengVars();
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
                Debug.LogWarning("不存在该动画状态");
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
        PengBool[] conditions;
        public IfElse(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo)
        {
            type = PengScriptType.IfElse;

            this.actor = actor;
            trackMaster = track;
            scriptName = GetDescription(type);
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);

            inVars = new PengVar[5];
            outVars = new PengVar[0];


            InitialPengVars();
        }

        public override void Initial()
        {
            base.Initial();
        }

        public override void Function()
        {
            base.Function();
            if (conditions.Length > 0)
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (conditions[i].value)
                    {
                        //?
                    }
                }
            }
        }
    }
}

