using PengVariables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PengScript
{
    public class OnTrackExecute : BaseScript
    {
        public PengInt pengTrackExecuteFrame = new PengInt("轨道执行帧", 0, ConnectionPointType.Out);
        public PengInt pengStateExecuteFrame = new PengInt("状态执行帧", 0, ConnectionPointType.Out);
        public OnTrackExecute(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[2];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.OnTrackExecute;
            scriptName = GetDescription(type);

            outVars[0] = pengTrackExecuteFrame;
            outVars[1] = pengStateExecuteFrame;
        }

        public override void Initial(int functionIndex)
        {
            pengTrackExecuteFrame.value = actor.currentStateFrame - trackMaster.start;
            pengStateExecuteFrame.value = actor.currentStateFrame;
        }
    }

    public class OnEvent : BaseScript
    {
        public PengString eventName = new PengString("事件名称", 0, ConnectionPointType.In);

        public PengInt intMessage = new PengInt("整型参数", 0, ConnectionPointType.Out);
        public PengFloat floatMessage = new PengFloat("浮点参数", 1, ConnectionPointType.Out);
        public PengString stringMessage = new PengString("字符串参数", 2, ConnectionPointType.Out);
        public PengBool boolMessage = new PengBool("布尔参数", 3, ConnectionPointType.Out);
        public OnEvent(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[4];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.OnEvent;
            scriptName = GetDescription(type);
            eventName.value = specialInfo;
            if (eventName.value == "")
            {
                Debug.LogWarning("存在事件触发脚本，其事件名称为空。");
            }
            inVars[0] = eventName;
            outVars[0] = intMessage;
            outVars[1] = floatMessage;
            outVars[2] = stringMessage;
            outVars[3] = boolMessage;
        }

        public void EventTrigger(int intMsg, float floatMsg, string stringMsg, bool boolMsg)
        {
            intMessage.value = intMsg;
            floatMessage.value = floatMsg;
            stringMessage.value = stringMsg;
            boolMessage.value = boolMsg;
            Execute(0);
        }
    }

    public class OnGround : BaseScript
    {
        public OnGround(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[4];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.OnGround;
            scriptName = GetDescription(type);
        }
    }

    public class OnHit : BaseScript
    {
        public OnHit(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.OnHit;
            scriptName = GetDescription(type);
        }
    }

    public class OnDie : BaseScript
    {
        public OnDie(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[1];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            type = PengScriptType.OnDie;
            scriptName = GetDescription(type);
        }
    }
}

