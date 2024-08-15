using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PengScript
{
    public class ValuePengInt : BaseScript
    {
        public PengInt pengInt = new PengInt("ֵ", 0, ConnectionPointType.Out);
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
        public PengFloat pengFloat = new PengFloat("ֵ", 0, ConnectionPointType.Out);
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
        public PengString pengString = new PengString("ֵ", 0, ConnectionPointType.Out);
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
        public PengBool pengBool = new PengBool("ֵ", 0, ConnectionPointType.Out);
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
        public PengFloat pengX = new PengFloat("ֵ", 0, ConnectionPointType.In);
        public PengFloat pengY = new PengFloat("ֵ", 1, ConnectionPointType.In);
        public PengFloat pengZ = new PengFloat("ֵ", 2, ConnectionPointType.In);

        public PengVector3 pengVector3 = new PengVector3("ֵ", 0, ConnectionPointType.Out);
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
        public PengFloat pengFloat = new PengFloat("����", 0, ConnectionPointType.In);

        public PengString pengString = new PengString("�ı�", 0, ConnectionPointType.Out);
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
        public PengFloat pengFloat = new PengFloat("����", 0, ConnectionPointType.Out);

        public PengInt pengInt = new PengInt("����", 0, ConnectionPointType.In);
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
            ��ɫ�ڰ� = 0,
            ȫ�ֺڰ� = 1,
            ��ɫ���� = 2,
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
            ���ͺڰ� = 0,
            ����ڰ� = 1,
            �ַ����ڰ� = 2,
            �����ڰ� = 3,
            Actor�ڰ� = 4,
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
        }

        public enum VariableTypeCN
        {
            ActorID = 0,
            Actor���� = 1,
            Actor��Ӫ = 2,
            ������ = 3,
            ������ = 4,
            ������ = 5,
            ���˱��� = 6,
            ��ǰ����ֵ = 7,
            �������ֵ = 8,
            λ�� = 9,
        }

        public PengInt int1 = new PengInt("ռλ", 0, ConnectionPointType.In);
        public PengInt int2 = new PengInt("ռλ", 1, ConnectionPointType.In);

        public PengInt intOut = new PengInt("�������", 0, ConnectionPointType.Out);
        public PengFloat floatOut = new PengFloat("�������", 0, ConnectionPointType.Out);
        public PengString stringOut = new PengString("�ַ������", 0, ConnectionPointType.Out);
        public PengBool boolOut = new PengBool("�������", 0, ConnectionPointType.Out);
        public PengPengActor actorOut = new PengPengActor("��ɫ���", 0, ConnectionPointType.Out);
        public PengVector3 vec3Out = new PengVector3("�������", 0, ConnectionPointType.Out);

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
                        ppa = new PengPengActor("��ɫ", 2, ConnectionPointType.In);
                        inVars[2] = ppa;
                        varName = new PengString("������", 3, ConnectionPointType.In);
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
                        varName = new PengString("������", 2, ConnectionPointType.In);
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
                        else if (varType == VariableType.ActorName)
                        {
                            outVars[0] = stringOut;
                        }
                        else if (varType == VariableType.ActorAttackPower || varType == VariableType.ActorDefendPower ||
                            varType == VariableType.ActorCriticalRate || varType == VariableType.ActorCriticalDamageRatio ||
                            varType == VariableType.ActorCurrentHP || varType == VariableType.ActorMaxHP)
                        {
                            outVars[0] = floatOut;
                        }
                        else if (varType == VariableType.ActorPosition)
                        {
                            outVars[0] = vec3Out;
                        }
                        ppa = new PengPengActor("��ɫ", 2, ConnectionPointType.In);
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
                Debug.Log("����ȡ����������PengActorδ����ʱ��Ĭ����Ϊ����");
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
        public PengList<PengActor> pengList = new PengList<PengActor>("Actor�б�", 0, ConnectionPointType.In);

        public PengInt pengInt = new PengInt("Ԫ�ظ���", 0, ConnectionPointType.Out);
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
