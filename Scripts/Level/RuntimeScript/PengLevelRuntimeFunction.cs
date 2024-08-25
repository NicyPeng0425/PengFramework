using PengScript;
using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace PengLevelRuntimeFunction
{
    public enum LevelFunctionType
    {
        [Description("1,起点,触发器")]
        Start,
        [Description("0,屏蔽所有UI,UI")]
        SetAllUIDisabled,
        [Description("0,显示所有UI,UI")]
        SetAllUIEnabled,
        [Description("0,生成Boss血条,UI")]
        GenerateBossHPBar,
        [Description("0,黑屏渐入,UI")]
        EaseInBlack,
        [Description("0,黑屏渐出,UI")]
        EaseOutBlack,
        [Description("0,生成Actor,功能")]
        GenerateActor,
        [Description("0,跳转场景,功能")]
        JumpToScene,
        [Description("0,设置主控,功能")]
        SetMainActor,
        [Description("0,增加Buff,功能")]
        AddBuff,
        [Description("0,开始操控,功能")]
        StartControl,
        [Description("0,屏蔽操控,功能")]
        EndControl,
        [Description("0,切状态,功能")]
        TransAction,
        [Description("0,设置空气墙,功能")]
        SetAirWall,
        [Description("0,关闭空气墙,功能")]
        CloseAirWall,
        [Description("0,生成对话,剧情")]
        GenerateDialog,
        [Description("0,获取当前主控,值")]
        GetCurrentMainActor,
        [Description("0,Actor组合成List,值")]
        ParseActorsToList,
        [Description("0,等待时间,触发器")]
        TriggerWaitTime,
        [Description("0,等待到达区域,触发器")]
        TriggerWaitArrival,
        [Description("0,等待主控输入,触发器")]
        TriggerWaitInput,
        [Description("0,等待分支选择,触发器")]
        TriggerWaitSelection,
        [Description("0,等待所有敌人死亡,触发器")]
        TriggerWaitEnemyDie,
    }

    public struct ScriptIDVarID
    {
        public int scriptID;
        public int varID;
    }

    public class BaseScript
    {
        public string scriptName = "默认";
        public PengLevel level;
        public LevelFunctionType type;
        public PengLevelRuntimeLevelScriptVariables.PengLevelVar[] inVars;
        public PengLevelRuntimeLevelScriptVariables.PengLevelVar[] outVars;
        public int ID;
        public int flowOutIDCache = -1;
        //每个节点只允许一个Enter
        public Dictionary<int, int> flowOutInfo = new Dictionary<int, int>();
        public Dictionary<int, ScriptIDVarID> varInID = new Dictionary<int, ScriptIDVarID>();

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
            flowOutIDCache = CheckIfDone();
            if (flowOutIDCache >= 0)
            {
                Final();
                level.ChangeScript(flowOutInfo[flowOutIDCache]);
            }
        }

        public virtual void Initial()
        {
            if (varInID.Count > 0 && inVars.Length > 0)
            {
                for (int i = 0; i < varInID.Count; i++)
                {
                    if (varInID.ElementAt(i).Value.scriptID > 0)
                    {
                        PengLevelRuntimeLevelScriptVariables.PengLevelVar vari = level.scripts[varInID.ElementAt(i).Value.scriptID].outVars[varInID.ElementAt(i).Value.varID];
                        vari.script.GetValue();
                        SetValue(i, vari);
                    }
                }
            }
        }

        //值节点需要重写
        public virtual void GetValue()
        {

        }

        //有值流入的都需要写
        public virtual void SetValue(int inVarID, PengLevelRuntimeLevelScriptVariables.PengLevelVar varSource)
        {

        }

        public virtual void Function()
        {

        }

        public virtual int CheckIfDone()
        {
            return -1;
        }

        public virtual void Final()
        {

        }

        public void InitialPengVars()
        {
            if (inVars.Length > 0)
            {
                for (int i = 0; i < inVars.Length; i++)
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
            string[] strings = str.Split(";");
            if (strings.Length > 0)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    string[] s1 = strings[i].Split("|");
                    string[] s2 = s1[1].Split(":");
                    result.Add(int.Parse(s1[0]), int.Parse(s2[0]));
                }
            }
            return result;
        }
    }

    public class LevelStart : BaseScript
    {
        public LevelStart(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[varInID.Count];
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.Start;
        }

        public override int CheckIfDone()
        {
            return 0;
        }
    }
}


