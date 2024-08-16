using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PengScript
{
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
}

