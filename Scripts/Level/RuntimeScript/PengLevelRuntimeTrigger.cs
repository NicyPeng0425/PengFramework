using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PengLevelRuntimeFunction
{
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

    public class TriggerWaitTime : BaseScript
    {
        public PengLevelRuntimeLevelScriptVariables.PengInt waitTime = new PengLevelRuntimeLevelScriptVariables.PengInt("时间", 0);

        public float time = 0;
        public float wait = 0;
        public bool waitSet = false;
        public TriggerWaitTime(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[1];
            inVars[0] = waitTime;
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Enter()
        {
            time = 0;
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.TriggerWaitTime;
            if (info != "")
            {
                waitTime.value = int.Parse(info);
            }
        }

        public override void Function()
        {
            base.Function();
            if (!waitSet && level.master != null)
            {
                wait = ((float)waitTime.value) / level.master.game.globalFrameRate;
                waitSet = true;
            }
        }

        public override int CheckIfDone()
        {
            if (!waitSet)
            {
                return -1;
            }
            else
            {
                time += Time.deltaTime;
                if (time >= wait)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}
