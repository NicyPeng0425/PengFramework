using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace PengScript
{
    public class GetTargetsByRange : BaseScript
    {
        public enum RangeType
        {
            [Description("圆柱体")]
            Cylinder,
            [Description("球体")]
            Sphere,
            [Description("盒形")]
            Box,
        }

        public RangeType rangeType = RangeType.Cylinder;
        public PengList<PengActor> result = new PengList<PengActor>("获取到的目标", 0, ConnectionPointType.Out);

        public PengInt typeNum = new PengInt("范围类型", 0, ConnectionPointType.In);
        public PengInt pengCamp = new PengInt("阵营", 1, ConnectionPointType.In);
        public PengVector3 pengPara = new PengVector3("参数", 2, ConnectionPointType.In);
        public PengVector3 pengOffset = new PengVector3("偏移", 3, ConnectionPointType.In);
        public GetTargetsByRange(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
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

            type = PengScriptType.GetTargetsByRange;
            scriptName = GetDescription(type);

            if (specialInfo != "")
            {
                string[] strFirst = specialInfo.Split(";");
                switch (int.Parse(strFirst[0]))
                {
                    case 1:
                        rangeType = RangeType.Cylinder;
                        break;
                    case 2:
                        rangeType = RangeType.Sphere;
                        break;
                    case 3:
                        rangeType = RangeType.Box;
                        break;
                }
                typeNum.value = (int)rangeType;
                string[] strSecond = strFirst[1].Split(",");
                pengPara.value = new Vector3(float.Parse(strSecond[0]), float.Parse(strSecond[1]), float.Parse(strSecond[2]));
                string[] strThird = strFirst[2].Split(",");
                pengOffset.value = new Vector3(float.Parse(strThird[0]), float.Parse(strThird[1]), float.Parse(strThird[2]));
                pengCamp.value = int.Parse(strFirst[3]);
            }

            outVars[0] = result;

            inVars[0] = typeNum;
            inVars[1] = pengCamp;
            inVars[2] = pengPara;
            inVars[3] = pengOffset;
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengInt pi = varSource as PengInt;
                    typeNum.value = pi.value;
                    break;
                case 1:
                    PengInt pi1 = varSource as PengInt;
                    pengCamp.value = pi1.value;
                    break;
                case 2:
                    PengVector3 pv = varSource as PengVector3;
                    pengPara.value = pv.value;
                    break;
                case 3:
                    PengVector3 pv2 = varSource as PengVector3;
                    pengOffset.value = pv2.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            result.value = new List<PengActor>();
            actor.targets.Clear();
            Vector3 pos = actor.transform.position + pengOffset.value.x * actor.transform.right + pengOffset.value.y * actor.transform.up + pengOffset.value.z * actor.transform.forward;
            Collider[] returns = new Collider[0];
            if (actor.game.actors.Count > 0)
            {
                switch (rangeType)
                {
                    case RangeType.Cylinder:
                        List<CharacterController> c = new List<CharacterController>();
                        foreach (PengActor a in actor.game.actors)
                        {
                            Vector3 selfDir1 = new Vector3(actor.transform.forward.x, 0, actor.transform.forward.z);
                            Vector3 tarDir1 = new Vector3(a.transform.position.x - pos.x, 0, a.transform.position.z - pos.z);
                            float angle1 = Vector3.Angle(selfDir1, tarDir1);
                            if ((a.transform.position.y >= pos.y && a.transform.position.y <= pos.y + pengPara.value.y) && angle1 <= pengPara.value.z && tarDir1.magnitude <= pengPara.value.x)
                            {
                                c.Add(a.ctrl);
                            }
                        }
                        returns = new Collider[c.Count];
                        if (c.Count > 0)
                        {
                            for (int i = 0; i < c.Count; i++)
                            {
                                returns[i] = c[i];
                            }
                        }
                        break;
                    case RangeType.Sphere:
                        returns = Physics.OverlapSphere(pos, pengPara.value.x);
                        break;
                    case RangeType.Box:
                        returns = Physics.OverlapBox(pos, pengPara.value / 2, actor.transform.rotation);
                        break;
                }
            }
            if (returns.Length > 0)
            {
                foreach (Collider re in returns)
                {
                    PengActor pa = re.GetComponent<PengActor>();
                    if (pa != null && pa.actorCamp == pengCamp.value && pa.alive)
                    {
                        result.value.Add(pa);
                        actor.targets.Add(pa);
                    }
                }
            }
        }
    }

    public class TransState : BaseScript
    {
        public PengString stateName = new PengString("状态名称", 0, ConnectionPointType.In);
        public TransState(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                stateName.value = specialInfo;
            }
            type = PengScriptType.TransState;
            scriptName = GetDescription(type);
            inVars[0] = stateName;
        }

        public override void GetValue()
        {
            base.GetValue();
            if (varInID[0].scriptID > 0)
            {
                PengVar vari = trackMaster.GetOutPengVarByScriptIDPengVarID(varInID[0].scriptID, varInID[0].varID);
                vari.script.GetValue();
                SetValue(0, vari);
            }
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengString ps = varSource as PengString;
                    stateName.value = ps.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            if (actor.actorStates.ContainsKey(stateName.value))
            {
                actor.TransState(stateName.value, true);
            }
            else
            {
                Debug.Log("Actor" + actor.actorID.ToString() + "在" + actor.currentName + "状态的" + trackMaster.name + "轨道中调用了切换状态，但没有给定名称的状态。");
            }
        }
    }

    public class GlobalTimeScale : BaseScript
    {
        public PengFloat timeScale = new PengFloat("时间速度", 0, ConnectionPointType.In);
        public PengFloat duration = new PengFloat("持续时间", 1, ConnectionPointType.In);

        public GlobalTimeScale(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                timeScale.value = float.Parse(str[0]);
                duration.value = float.Parse(str[1]);
            }
            inVars[0] = timeScale;
            inVars[1] = duration;
            type = PengScriptType.GlobalTimeScale;
            scriptName = GetDescription(type);
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengFloat pf = varSource as PengFloat;
                    timeScale.value = pf.value;
                    break;
                case 1:
                    PengFloat pf1 = varSource as PengFloat;
                    duration.value = pf1.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            actor.game.GloablTimeScaleFunc(timeScale.value, duration.value);
        }
    }

    public enum BBTarget
    {
        Self = 0,
        Global = 1,
        Targets = 2,
    }

    public enum BBTargetCN
    {
        自身 = 0,
        全局 = 1,
        目标 = 2,
    }

    public class SetBlackBoardVariables : BaseScript
    {
        public PengString varName = new PengString("变量名", 0, ConnectionPointType.In);
        public PengT value = new PengT("值", 1, ConnectionPointType.In);
        public PengInt targetType = new PengInt("目标类型", 2, ConnectionPointType.In);

        public BBTarget target;

        public SetBlackBoardVariables(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.SetBlackBoardVariables;
            scriptName = GetDescription(type);
            inVars[0] = varName;
            inVars[1] = value;
            inVars[2] = targetType;
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                varName.value = str[0];
                targetType.value = int.Parse(str[1]);
                target = (BBTarget)targetType.value;
            }
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            base.SetValue(inVarID, varSource);
            switch (inVarID)
            {
                case 0:
                    PengString ps = varSource as PengString;
                    varName.value = ps.value;
                    break;
                case 1:
                    value.value = varSource;
                    break;
                case 2:
                    PengInt pi = varSource as PengInt;
                    targetType.value = pi.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            switch (target)
            {
                case BBTarget.Self:
                    SetBBVar(varName.value, value, actor.bb);
                    break;
                case BBTarget.Global:
                    SetBBVar(varName.value, value, actor.game.bb);
                    break;
                case BBTarget.Targets:
                    if (actor.targets.Count > 0)
                    {
                        for (int i = 0; i < actor.targets.Count; i++)
                        {
                            SetBBVar(varName.value, value, actor.targets[i].bb);
                        }
                    }
                    break;
            }
        }

        public void SetBBVar(string name, PengT var, PengBlackBoard<PengGameManager> bb)
        {
            switch (var.value.GetType().FullName)
            {
                case "PengVariables.PengInt":
                    PengInt pi = var.value as PengInt;
                    bb.SetBBVar(name, pi.value);
                    break;
                case "PengVariables.PengFloat":
                    PengFloat pf = var.value as PengFloat;
                    bb.SetBBVar(name, pf.value);
                    break;
                case "PengVariables.PengString":
                    PengString ps = var.value as PengString;
                    bb.SetBBVar(name, ps.value);
                    break;
                case "PengVariables.PengBool":
                    PengBool pb = var.value as PengBool;
                    bb.SetBBVar(name, pb.value);
                    break;
                case "PengVariables.PengActor":
                    PengPengActor ppa = var.value as PengPengActor;
                    bb.SetBBVar(name, ppa.value);
                    break;
                default:
                    Debug.LogWarning("不支持的黑板变量类型。");
                    break;
            }
        }

        public void SetBBVar(string name, PengT var, PengBlackBoard<PengActor> bb)
        {
            switch (var.value.GetType().FullName)
            {
                case "PengVariables.PengInt":
                    PengInt pi = var.value as PengInt;
                    bb.SetBBVar(name, pi.value);
                    break;
                case "PengVariables.PengFloat":
                    PengFloat pf = var.value as PengFloat;
                    bb.SetBBVar(name, pf.value);
                    break;
                case "PengVariables.PengString":
                    PengString ps = var.value as PengString;
                    bb.SetBBVar(name, ps.value);
                    break;
                case "PengVariables.PengBool":
                    PengBool pb = var.value as PengBool;
                    bb.SetBBVar(name, pb.value);
                    break;
                case "PengVariables.PengActor":
                    PengPengActor ppa = var.value as PengPengActor;
                    bb.SetBBVar(name, ppa.value);
                    break;
                default:
                    Debug.LogWarning("不支持的黑板变量类型。");
                    break;
            }
        }
    }

    public class CustomEvent : BaseScript
    {
        public PengString eventName = new PengString("事件名称", 0, ConnectionPointType.In);

        public PengInt intMessage = new PengInt("整型参数", 1, ConnectionPointType.In);
        public PengFloat floatMessage = new PengFloat("浮点参数", 2, ConnectionPointType.In);
        public PengString stringMessage = new PengString("字符串参数", 3, ConnectionPointType.In);
        public PengBool boolMessage = new PengBool("布尔参数", 4, ConnectionPointType.In);
        public CustomEvent(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.CustomEvent;
            scriptName = GetDescription(type);
            inVars[0] = eventName;
            inVars[1] = intMessage;
            inVars[2] = floatMessage;
            inVars[3] = stringMessage;
            inVars[4] = boolMessage;

            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                eventName.value = str[0];
                intMessage.value = int.Parse(str[1]);
                floatMessage.value = float.Parse(str[2]);
                stringMessage.value = str[3];
                boolMessage.value = int.Parse(str[4]) > 0;
            }
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengString ps = varSource as PengString;
                    eventName.value = ps.value;
                    break;
                case 1:
                    PengInt pb = varSource as PengInt;
                    intMessage.value = pb.value;
                    break;
                case 2:
                    PengFloat pf1 = varSource as PengFloat;
                    floatMessage.value = pf1.value;
                    break;
                case 3:
                    PengString pf2 = varSource as PengString;
                    stringMessage.value = pf2.value;
                    break;
                case 4:
                    PengBool pi = varSource as PengBool;
                    boolMessage.value = pi.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            actor.game.eventManager.TriggerEvent(eventName.value, intMessage.value, floatMessage.value, stringMessage.value, boolMessage.value);
        }
    }

    public class AllowChangeDirection : BaseScript
    {
        public PengFloat lerp = new PengFloat("时间速度", 0, ConnectionPointType.In);

        public AllowChangeDirection(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                lerp.value = float.Parse(specialInfo);
            }
            inVars[0] = lerp;
            type = PengScriptType.AllowChangeDirection;
            scriptName = GetDescription(type);
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengFloat pf = varSource as PengFloat;
                    lerp.value = pf.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            Vector3 currentDir = new Vector3(actor.transform.forward.x, 0, actor.transform.forward.z);
            currentDir = currentDir.normalized;
            Vector3 finalDir = Vector3.zero;
            if (actor.input.processedInputDir.magnitude > 0.125f)
            {
                if (lerp.value >= 0.99f)
                {
                    finalDir = actor.input.processedInputDir;
                }
                else
                {
                    if ((currentDir + actor.input.processedInputDir).magnitude <= 0.01f)
                    {
                        currentDir = Quaternion.Euler(0,5,0) * currentDir;
                        currentDir = new Vector3(currentDir.x, 0, currentDir.z).normalized;
                    }
                    finalDir = Vector3.Lerp(currentDir, actor.input.processedInputDir, lerp.value);
                }
                actor.transform.LookAt(actor.transform.position + finalDir);
            }
        }
    }

    public class JumpForce : BaseScript
    {
        public PengFloat force = new PengFloat("跳跃力", 0, ConnectionPointType.In);

        public JumpForce(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                force.value = float.Parse(specialInfo);
            }
            inVars[0] = force;
            type = PengScriptType.JumpForce;
            scriptName = GetDescription(type);
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengFloat pf = varSource as PengFloat;
                    force.value = pf.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            actor.fallSpeed = force.value;
        }
    }

    public class MoveByFrame : BaseScript
    {
        public PengVector3 force = new PengVector3("速度", 0, ConnectionPointType.In);

        public MoveByFrame(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                force.value = ParseStringToVector3(specialInfo);
            }
            inVars[0] = force;
            type = PengScriptType.MoveByFrame;
            scriptName = GetDescription(type);
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengVector3 pf = varSource as PengVector3;
                    force.value = pf.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            Vector3 dir = actor.transform.forward * force.value.z + actor.transform.up * force.value.y + actor.transform.right * force.value.x;
            actor.ctrl.Move(dir * (1 / actor.game.globalFrameRate));
        }
    }

    public enum AddOrRemove
    {
        Add,
        Remove,
    }

    public enum CertainOrAll
    {
        Certain,
        AllBuff,
        AllGoodBuff,
        AllBadDebuff,
    }

    public class AddOrRemoveBuff : BaseScript
    {
        public PengPengActor ppa = new PengPengActor("目标", 0, ConnectionPointType.In);
        public AddOrRemove aor = AddOrRemove.Add;
        public CertainOrAll coa = CertainOrAll.Certain;
        public PengInt id = new PengInt("ID", 2, ConnectionPointType.In);

        public AddOrRemoveBuff(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(',');
                int aorInt = int.Parse(str[0]);
                aor = (AddOrRemove)aorInt;
                int coaInt = int.Parse(str[1]);
                coa = (CertainOrAll)coaInt;
                id.value = int.Parse(str[2]);
                switch (aor)
                {
                    case AddOrRemove.Add:
                        inVars = new PengVar[3];
                        inVars[0] = ppa;
                        inVars[2] = id;
                        id.index = 2;
                        break;
                    case AddOrRemove.Remove:
                        switch (coa)
                        {
                            case CertainOrAll.Certain:
                                inVars = new PengVar[4];
                                inVars[0] = ppa;
                                inVars[3] = id;
                                id.index = 3;
                                break;
                            default:
                                inVars = new PengVar[3];
                                inVars[0] = ppa;
                                break;
                        }
                        break;
                }
            }
            type = PengScriptType.AddOrRemoveBuff;
            scriptName = GetDescription(type);
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            PengInt pi = varSource as PengInt;
            id.value = pi.value;
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            if (varInID[0].scriptID < 0)
            {
                ppa.value = actor;
            }
            switch (aor)
            {
                case AddOrRemove.Add:
                    ppa.value.buff.AddBuff(id.value);
                    break;
                case AddOrRemove.Remove:
                    switch (coa)
                    {
                        case CertainOrAll.Certain:
                            ppa.value.buff.RemoveBuff(id.value);
                            break;
                        case CertainOrAll.AllBuff:
                            ppa.value.buff.RemoveAllBuff();
                            break;
                        case CertainOrAll.AllGoodBuff:
                            ppa.value.buff.RemoveAllGoodBuff();
                            break;
                        case CertainOrAll.AllBadDebuff:
                            ppa.value.buff.RemoveAllBadDeBuff();
                            break;
                    }
                    break;
            }
        }
    }

    public enum AttackPowerType
    {
        轻 = 0,
        中 = 1,
        重 = 2,
    }

    public enum BreakType
    {
        不打断 = 0,
        直接打断 = 1,
        打断力 = 2,
    }

    public class AttackDamage : BaseScript
    {
        public AttackPowerType attackPowerType = AttackPowerType.轻;
        public BreakType breakType = BreakType.不打断;
        public PengFloat breakResist = new PengFloat("打断力", 2, ConnectionPointType.In);
        public PengString hitAudioPath = new PengString("受击音效", 3, ConnectionPointType.In);
        public PengFloat hitAudioVol = new PengFloat("受击音量", 4, ConnectionPointType.In);
        public PengString hitVFXPath = new PengString("受击特效", 5, ConnectionPointType.In);
        public PengFloat damageRatio = new PengFloat("伤害倍率", 6, ConnectionPointType.In);
        public PengVector3 cameraImpulseSpeed = new PengVector3("震屏速度", 7, ConnectionPointType.In);
        public PengFloat cameraImpulseTime = new PengFloat("震屏时间", 8, ConnectionPointType.In);
        public PengFloat hitPause = new PengFloat("受击顿帧", 9, ConnectionPointType.In);
        public PengFloat attackerPause = new PengFloat("攻击顿帧", 10, ConnectionPointType.In);

        public List<AudioClip> clips = new List<AudioClip>();
        public AttackDamage(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(";");
                attackPowerType = (AttackPowerType)int.Parse(str[0]);
                breakType = (BreakType)int.Parse(str[1]);
                breakResist.value = float.Parse(str[2]);
                string[] hitAudioPaths = str[3].Split("|");
                if (hitAudioPaths.Length > 0)
                {
                    if (hitAudioPaths[0] != "")
                    {
                        hitAudioPath.value = hitAudioPaths[0];
                    }
                    for (int i = 1; i < hitAudioPaths.Length; i++)
                    {
                        clips.Add(Resources.Load<AudioClip>(hitAudioPaths[i]));
                    }
                }
                hitAudioVol.value = float.Parse(str[4]);
                if (str[5] == "-1")
                {
                    hitVFXPath.value = "";
                }
                else
                {
                    hitVFXPath.value = str[5];
                }
                damageRatio.value = float.Parse(str[6]);
                cameraImpulseSpeed.value = BaseScript.ParseStringToVector3(str[7]);
                cameraImpulseTime.value = float.Parse(str[8]);
                hitPause.value = float.Parse(str[9]);
                attackerPause.value = float.Parse(str[10]);
            }
            inVars[2] = breakResist;
            inVars[3] = hitAudioPath;
            inVars[4] = hitAudioVol;
            inVars[5] = hitVFXPath;
            inVars[6] = damageRatio;
            inVars[7] = cameraImpulseSpeed;
            inVars[8] = cameraImpulseTime;
            inVars[9] = hitPause;
            inVars[10] = attackerPause;
            type = PengScriptType.AttackDamage;
            scriptName = GetDescription(type);
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            if (actor.targets.Count == 0)
                return;

            actor.pauseTime += attackerPause.value;
            for (int i = 0; i < actor.targets.Count; i++)
            {
                actor.targets[i].lastHitActor = actor;
                actor.targets[i].OnHit();
                actor.targets[i].pauseTime += hitPause.value;
                if (clips.Count > 0)
                {
                    int rand = Random.Range(0, clips.Count);
                    actor.targets[i].speaker.PlayOneShot(clips[rand], hitAudioVol.value);
                }

                if (!actor.targets[i].invincible)
                {
                    float dmg = actor.attackPower * damageRatio.value;
                    float critical = Random.Range(0f, 1f);
                    if (critical <= actor.criticalRate)
                    {
                        dmg *= actor.criticalDamageRatio;
                    }
                    dmg -= actor.targets[i].defendPower;
                    actor.targets[i].currentHP -= dmg;
                    //UI跳字
                }

                actor.game.GenerateVFX(hitVFXPath.value, actor.targets[i].hitVFXPivot.transform.position, Vector3.zero, Vector3.one, actor.targets[i], true, 1);


                if ((breakType == BreakType.打断力 && breakResist.value >= actor.targets[i].resist && !actor.targets[i].unbreakable) ||
                    (breakType == BreakType.直接打断 && !actor.targets[i].unbreakable))
                {
                    switch (attackPowerType)
                    {
                        case AttackPowerType.轻:
                            if (actor.targets[i].actorStates.ContainsKey("LightHit"))
                            {
                                actor.targets[i].TransState("LightHit", false);
                            }
                            else
                            {
                                Debug.LogWarning("Actor" + actor.targets[i].actorID.ToString() + "受到轻攻击，但并没有轻受击硬直的状态可供切换，请考虑添加名为LightHit的状态。");
                            }
                            break;
                        case AttackPowerType.中:
                            if (actor.targets[i].actorStates.ContainsKey("MiddleHit"))
                            {
                                actor.targets[i].TransState("MiddleHit", false);
                            }
                            else
                            {
                                Debug.LogWarning("Actor" + actor.targets[i].actorID.ToString() + "受到中攻击，但并没有中受击硬直的状态可供切换，请考虑添加名为MiddleHit的状态。");
                            }
                            break;
                        case AttackPowerType.重:
                            if (actor.targets[i].actorStates.ContainsKey("HeavyHit"))
                            {
                                actor.targets[i].TransState("HeavyHit", false);
                            }
                            else
                            {
                                Debug.LogWarning("Actor" + actor.targets[i].actorID.ToString() + "受到重攻击，但并没有重受击硬直的状态可供切换，请考虑添加名为HeavyHit的状态。");
                            }
                            break;
                    }
                }
                else
                {
                    switch (attackPowerType)
                    {
                        //程序化骨骼抖动
                        case AttackPowerType.轻:
                            break;
                        case AttackPowerType.中:
                            break;
                        case AttackPowerType.重:
                            break;
                    }
                }
            }
            //震屏
            actor.targets.Clear();
        }
    }

    public class TryGetEnemy : BaseScript
    {
        public PengFloat range = new PengFloat("范围", 0, ConnectionPointType.In);
        public PengFloat angle = new PengFloat("角度", 1, ConnectionPointType.In);

        public TryGetEnemy(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(",");
                range.value = float.Parse(str[0]);
                angle.value = float.Parse(str[1]);
            }
            inVars[0] = range;
            inVars[1] = angle;
            type = PengScriptType.TryGetEnemy;
            scriptName = GetDescription(type);
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            float radius = range.value + 1;
            Vector3 dir = Vector3.zero;
            PengActor tar = null;
            foreach (PengActor a in actor.game.actors)
            {
                if (a.alive && a.actorCamp != actor.actorCamp && a.gameObject.activeSelf)
                {
                    Vector3 d = a.transform.position - actor.transform.position;
                    d = new Vector3(d.x, 0, d.z);
                    Vector3 forward = new Vector3(actor.transform.forward.x, 0, actor.transform.forward.z);
                    if (Vector3.Angle(forward, d) <= angle.value)
                    {
                        float dis = d.magnitude;
                        if (dis <= radius)
                        {
                            radius = dis;
                            dir = d;
                            tar = a;
                        }
                    }
                }
            }
            if (tar != null)
            {
                actor.transform.LookAt(actor.transform.position + dir.normalized);
            }
            else
            {
                if (actor.input.processedInputDir.magnitude >= 0.05f)
                {
                    Vector3 finalDir = actor.input.processedInputDir;
                    actor.transform.LookAt(actor.transform.position + finalDir);
                }
            }
        }
    }

    public class PerfectDodge : BaseScript
    {
        public PerfectDodge(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Execute(int functionIndex)
        {
            Initial(functionIndex);
            Function(functionIndex);
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);
            type = PengScriptType.PerfectDodge;
            scriptName = GetDescription(type);
        }

        public override void Function(int functionIndex)
        {
            bool dodge = false;
            for (int i = 0; i < actor.game.actors.Count; i++)
            {
                if (actor.game.actors[i].actorCamp != actor.actorCamp)
                {
                    if (actor.game.actors[i].targets.Contains(actor))
                    {
                        dodge = true;
                        break;
                    }
                }
            }
            if (dodge)
            {
                ScriptFlowNext();
            }
        }
    }
}

