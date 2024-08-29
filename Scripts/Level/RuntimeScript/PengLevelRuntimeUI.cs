using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PengLevelRuntimeFunction
{
    public class GenerateBlack : BaseScript
    {
        public bool executed = false;
        public GenerateBlack(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
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
            type = LevelFunctionType.GenerateBlack;
        }
        public override void Enter()
        {
            executed = false;
        }

        public override void Function()
        {
            if (!executed)
            {
                if (level.master != null)
                {
                    if (level.master.game.blackImage == null)
                    {
                        GameObject blackGO = new GameObject();
                        blackGO.transform.SetParent(level.master.game.allBlackRoot);
                        RectTransform black = blackGO.AddComponent<RectTransform>();
                        black.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
                        black.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
                        black.anchorMin = new Vector2(0, 0);
                        black.anchorMax = new Vector2(1, 1);
                        black.name = "Black";
                        level.master.game.blackImage = blackGO.AddComponent<Image>();
                        level.master.game.blackImage.color = new Color(0.08f, 0.08f, 0.08f);
                    }
                    else
                    {
                        level.master.game.blackImage.gameObject.SetActive(true);
                    }
                    executed = true;
                }
            }
        }

        public override int CheckIfDone()
        {
            if (executed)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }

    public class EaseInBlack : BaseScript
    {
        public PengLevelRuntimeLevelScriptVariables.PengInt waitTime = new PengLevelRuntimeLevelScriptVariables.PengInt("时间", 0);

        public float wait = 0;
        public bool waitSet = false;
        public EaseInBlack(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
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
            waitSet = false; 
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.EaseInBlack;
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
                level.master.game.ControlBlackChangeFunc(true, wait);
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
                return 0;
            }
        }
    }

    public class EaseOutBlack : BaseScript
    {
        public PengLevelRuntimeLevelScriptVariables.PengInt waitTime = new PengLevelRuntimeLevelScriptVariables.PengInt("时间", 0);

        public float wait = 0;
        public bool waitSet = false;
        public EaseOutBlack(PengLevel level, int ID, string flowOutInfo, string varInInfo, string specialInfo)
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
            waitSet = false;
        }
        public override void Construct(string info)
        {
            base.Construct(info);
            type = LevelFunctionType.EaseOutBlack;
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
                level.master.game.ControlBlackChangeFunc(false, wait);
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
                return 0;
            }
        }
    }
}
