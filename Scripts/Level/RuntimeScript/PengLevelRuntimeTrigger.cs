using PengVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PengScript.GetTargetsByRange;

namespace PengLevelRuntimeFunction
{
    public class LevelStart : BaseScript
    {
        public LevelStart(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarIDLevel(varInInfo);
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
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarIDLevel(varInInfo);
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

    public class TriggerWaitArrival : BaseScript
    {
        public PengScript.GetTargetsByRange.RangeType range;
        public PengLevelRuntimeLevelScriptVariables.PengVector3 posV = new PengLevelRuntimeLevelScriptVariables.PengVector3("位置", 0);
        public PengLevelRuntimeLevelScriptVariables.PengVector3 para = new PengLevelRuntimeLevelScriptVariables.PengVector3("参数", 1);

        float timeCnt = 0;
        float timeCheck = 0.5f;
        public TriggerWaitArrival(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarIDLevel(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[2];
            inVars[0] = posV;
            inVars[1] = para;
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Enter()
        {
            timeCnt = 0;
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.TriggerWaitArrival;
            if (info != "")
            {
                string[] str = info.Split(";");
                range = (PengScript.GetTargetsByRange.RangeType)int.Parse(str[0]);
                posV.value = PengScript.BaseScript.ParseStringToVector3(str[1]);
                para.value = PengScript.BaseScript.ParseStringToVector3(str[2]);
            }
        }

        public override void Function()
        {
            timeCnt += Time.deltaTime;
            base.Function();
        }

        public override int CheckIfDone()
        {
            if (timeCnt >= timeCheck && level.master.game.mainActor != null)
            {
                timeCnt -= timeCheck;
                return Check();
            }
            else
            {
                return -1;
            }
        }

        public int Check()
        {
            Vector3 pos = level.transform.position + posV.value.x * level.transform.right + posV.value.y * level.transform.up + posV.value.z * level.transform.forward;
            switch (range)
            {
                case RangeType.Cylinder:
                    Vector3 selfDir1 = new Vector3(level.transform.forward.x, 0, level.transform.forward.z);
                    Vector3 tarDir1 = new Vector3(level.master.game.mainActor.transform.position.x - pos.x, 0, level.master.game.mainActor.transform.position.z - pos.z);
                    if ((level.master.game.mainActor.transform.position.y >= pos.y && level.master.game.mainActor.transform.position.y <= pos.y + para.value.y)&& tarDir1.magnitude <= para.value.x)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                case RangeType.Sphere:
                    if ((level.master.game.mainActor.transform.position - pos).magnitude <= para.value.x)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                case RangeType.Box:
                    if (level.master.game.mainActor.transform.position.x >= pos.x - para.value.x / 2 && level.master.game.mainActor.transform.position.x <= pos.x + para.value.x / 2 &&
                        level.master.game.mainActor.transform.position.y >= pos.y - para.value.y / 2 && level.master.game.mainActor.transform.position.y <= pos.y + para.value.y / 2 &&
                        level.master.game.mainActor.transform.position.z >= pos.x - para.value.z / 2 && level.master.game.mainActor.transform.position.z <= pos.z + para.value.z / 2)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
            }
            return -1;
        }
    }

    public class TriggerWaitEnemyDie : BaseScript
    {
        public TriggerWaitEnemyDie(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.level = level;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarIDLevel(varInInfo);
            inVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            outVars = new PengLevelRuntimeLevelScriptVariables.PengLevelVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.TriggerWaitEnemyDie;
        }

        public override int CheckIfDone()
        {
            if (level.currentEnemy.Count == 0)
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
