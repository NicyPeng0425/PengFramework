using PengScript;
using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
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
        [Description("1,黑屏渐入,UI")]
        EaseInBlack,
        [Description("1,黑屏渐出,UI")]
        EaseOutBlack,
        [Description("1,生成黑屏,UI")]
        GenerateBlack,
        [Description("1,生成Actor,功能")]
        GenerateActor,
        [Description("1,生成敌人,功能")]
        GenerateEnemy,
        [Description("0,跳转场景,功能")]
        JumpToScene,
        [Description("1,设置主控,功能")]
        SetMainActor,
        [Description("0,增加Buff,功能")]
        AddBuff,
        [Description("1,激活ActorAI,功能")]
        ActiveActor,
        [Description("1,开始操控,功能")]
        StartControl,
        [Description("1,屏蔽操控,功能")]
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
        [Description("1,等待时间,触发器")]
        TriggerWaitTime,
        [Description("1,等待到达区域,触发器")]
        TriggerWaitArrival,
        [Description("0,等待主控输入,触发器")]
        TriggerWaitInput,
        [Description("0,等待分支选择,触发器")]
        TriggerWaitSelection,
        [Description("1,等待所有敌人死亡,触发器")]
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
        //执行哪个出流？
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
                if (flowOutInfo[flowOutIDCache] >= 0)
                {
                    level.ChangeScript(flowOutInfo[flowOutIDCache]);
                }
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

    public class GenerateActor : BaseScript
    {
        public PengLevelRuntimeLevelScriptVariables.PengInt actorID = new PengLevelRuntimeLevelScriptVariables.PengInt("角色ID", 0);
        public PengLevelRuntimeLevelScriptVariables.PengPengActor actor = new PengLevelRuntimeLevelScriptVariables.PengPengActor("角色", 0);

        public bool done = false;
        public GenerateActor(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[1];
            inVars[0] = actorID;
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[1];
            outVars[0] = actor;
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Enter()
        {
            done = false;
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.GenerateActor;
            if (info != "")
            {
                actorID.value = int.Parse(info);
            }
        }

        public override void Function()
        {
            if (!done)
            {
                actor.value = level.master.game.AddNewActor(actorID.value, level.transform.position, level.transform.position + level.transform.forward);
                done = true;
            }
        }

        public override int CheckIfDone()
        {
            return 0;
        }
    }

    public class SetMainActor : BaseScript
    {
        public PengLevelRuntimeLevelScriptVariables.PengPengActor actor = new PengLevelRuntimeLevelScriptVariables.PengPengActor("角色", 0);

        public bool done = false;
        public SetMainActor(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[1];
            inVars[0] = actor;
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Enter()
        {
            done = false;
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.SetMainActor;
        }

        public override void Function()
        {
            if (!done)
            {
                level.master.game.SetMainActor(actor.value, true);
                done = true;
            }
        }

        public override void SetValue(int inVarID, PengLevelRuntimeLevelScriptVariables.PengLevelVar varSource)
        {
            PengLevelRuntimeLevelScriptVariables.PengPengActor val = varSource as PengLevelRuntimeLevelScriptVariables.PengPengActor;
            actor.value = val.value;
        }

        public override int CheckIfDone()
        {
            return 0;
        }
    }

    public class StartControl : BaseScript
    {
        public bool done = false;
        public StartControl(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Enter()
        {
            done = false;
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.StartControl;
        }

        public override void Function()
        {
            if (!done)
            {
                level.master.game.EnableActorInput();
                done = true;
            }
        }

        public override int CheckIfDone()
        {
            return 0;
        }
    }

    public class EndControl : BaseScript
    {
        public bool done = false;
        public EndControl(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Enter()
        {
            done = false;
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.EndControl;
        }

        public override void Function()
        {
            if (!done)
            {
                level.master.game.DisableActorInput();
                done = true;
            }
        }

        public override int CheckIfDone()
        {
            return 0;
        }
    }

    public class GenerateEnemy : BaseScript
    {
        public bool done = false;
        public List<Vector3> pos = new List<Vector3>();
        public List<Vector3> rot = new List<Vector3>();
        public List<int> ids = new List<int>();
        public GenerateEnemy(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Enter()
        {
            done = false;
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.GenerateEnemy;
            if (info != "")
            {
                string[] strings = info.Split(";");
                if (strings.Length > 0)
                {
                    for (int i = 0; i < strings.Length; i++)
                    {
                        string[] str = strings[i].Split("|");

                        pos.Add(PengScript.BaseScript.ParseStringToVector3(str[0]));
                        rot.Add(PengScript.BaseScript.ParseStringToVector3(str[1]));
                        ids.Add(int.Parse(str[2]));
                    }
                }
            }
        }

        public override void Function()
        {
            if (!done)
            {
                if (level.master != null)
                {
                    if (pos.Count > 0)
                    {
                        for (int i = 0; i < pos.Count; i++)
                        {
                            PengActor enemy = level.master.game.AddNewActor(ids[i], pos[i], rot[i]);
                            level.currentEnemy.Add(enemy);
                        }
                    }
                    done = true;
                }
            }
        }

        public override int CheckIfDone()
        {
            if (done)
                return 0;
            else
                return -1;
        }
    }

    public class ActiveActor : BaseScript
    {
        public bool done = false;
        public ActiveActor(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Enter()
        {
            done = false;
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.ActiveActor;
            if (info != "")
            {
            }
        }

        public override void Function()
        {
            if (!done)
            {
                if (level.currentEnemy.Count > 0)
                {
                    for (int i = 0; i < level.currentEnemy.Count; i++)
                    {
                        level.currentEnemy[i].input.active = true;
                    }
                }
                done = true;
            }
        }

        public override int CheckIfDone()
        {
            if (done)
                return 0;
            else
                return -1;
        }
    }
}


