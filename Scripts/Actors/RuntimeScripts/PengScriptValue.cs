using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PengScript
{
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
            ActorCurrentStateName = 10,
            ActorLastStateName = 11,
            ActorOnGround = 12,
            ActorDirectionInput = 13,
            ActorDirectionInputMagnitude = 14,
            ActorForward = 15,
            ActorDirectionProcessed = 16,
            ActorInertia = 17,
            ActorFallSpeed = 18,
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
            Actor当前状态名 = 10,
            Actor上个状态名 = 11,
            Actor是否着地 = 12,
            Actor方向输入 = 13,
            Actor方向输入模长 = 14,
            Actor正前方 = 15,
            Actor方向输入世界坐标 = 16,
            Actor惯性 = 17,
            下落速度 = 18,
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
                        else if (varType == VariableType.ActorName || varType == VariableType.ActorCurrentStateName || varType == VariableType.ActorLastStateName)
                        {
                            outVars[0] = stringOut;
                        }
                        else if (varType == VariableType.ActorAttackPower || varType == VariableType.ActorDefendPower ||
                            varType == VariableType.ActorCriticalRate || varType == VariableType.ActorCriticalDamageRatio ||
                            varType == VariableType.ActorCurrentHP || varType == VariableType.ActorMaxHP ||
                            varType == VariableType.ActorDirectionInputMagnitude || varType == VariableType.ActorFallSpeed)
                        {
                            outVars[0] = floatOut;
                        }
                        else if (varType == VariableType.ActorPosition || varType == VariableType.ActorDirectionInput ||
                            varType == VariableType.ActorForward || varType == VariableType.ActorDirectionProcessed ||
                            varType == VariableType.ActorInertia)
                        {
                            outVars[0] = vec3Out;
                        }
                        else if(varType == VariableType.ActorOnGround)
                        {
                            outVars[0] = boolOut;
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
                        case VariableType.ActorCurrentStateName:
                            stringOut.value = ppa.value.currentName;
                            break;
                        case VariableType.ActorLastStateName:
                            stringOut.value = ppa.value.lastName;
                            break;
                        case VariableType.ActorOnGround:
                            boolOut.value = ppa.value.isGrounded;
                            break;
                        case VariableType.ActorDirectionInput:
                            vec3Out.value = ppa.value.input.originalInputDir;
                            break;
                        case VariableType.ActorDirectionInputMagnitude:
                            floatOut.value = ppa.value.input.originalInputDir.magnitude;
                            break;
                        case VariableType.ActorForward:
                            vec3Out.value = ppa.value.transform.forward;
                            break;
                        case VariableType.ActorDirectionProcessed:
                            vec3Out.value = ppa.value.input.processedInputDir;
                            break;
                        case VariableType.ActorInertia:
                            vec3Out.value = ppa.value.inertia;
                            break;
                        case VariableType.ActorFallSpeed:
                            floatOut.value = ppa.value.fallSpeed;
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

    public class ValueGetListCount : BaseScript
    {
        public PengList<PengActor> pengList = new PengList<PengActor>("Actor列表", 0, ConnectionPointType.In);

        public PengInt pengInt = new PengInt("元素个数", 0, ConnectionPointType.Out);
        public ValueGetListCount(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
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
            type = PengScriptType.ValueGetListCount;
            scriptName = GetDescription(type);
            inVars[0] = pengList;
            outVars[0] = pengInt;
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
            pengInt.value = pengList.value.Count;
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            base.SetValue(inVarID, varSource);
            switch (inVarID)
            {
                case 0:
                    PengList<PengActor> pl = varSource as PengList<PengActor>;
                    pengList.value = pl.value;
                    break;
            }
        }
    }
}
