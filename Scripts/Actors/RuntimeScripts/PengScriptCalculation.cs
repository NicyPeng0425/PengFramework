using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PengScript
{
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

