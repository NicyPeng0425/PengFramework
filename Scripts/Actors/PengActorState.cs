using PengScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class PengActorState : IPengActorState
{
    public string stateName;
    public PengActor actor;
    public int length;
    public bool isLoop;
    public List<PengTrack> tracks;

    public int m_currentFrameNum;
    public int currentFrameNum
    { 
        get {return m_currentFrameNum;}
        set
        {
            m_currentFrameNum = value;
            if (actor != null)
            { actor.currentStateFrame = value; }
        }
    }
    public float frameCnt = 0f;
    public List<bool> executedTags = new List<bool>();

    public PengActorState(PengActor actor, XmlElement stateInfo)
    {
        this.actor = actor;
        this.length = int.Parse(stateInfo.GetAttribute("Length"));
        this.isLoop = int.Parse(stateInfo.GetAttribute("IsLoop")) > 0;
        this.tracks = ReadActorTracks(stateInfo);
    }
    public void OnEnter()
    {
        currentFrameNum = 0;
        frameCnt = 0f;
        executedTags = InitialBoolList(length, false);
        actor.currentStateLength = length;

        if (tracks.Count > 0)
        {
            for (int j = 0; j < tracks.Count; j++)
            {
                if (tracks[j].execTime == PengTrack.ExecTime.Enter)
                {
                    tracks[j].ExecuteOnce();
                }
            }
        }
        
    }

    public void OnUpdate()
    {
        if (actor.pauseTime > 0)
        {
            actor.pauseTime -= Time.deltaTime;
        }
        else
        {
            frameCnt += Time.deltaTime;
            if (frameCnt > ((float)length) / actor.game.globalFrameRate)
            {
                if(isLoop)
                {
                    currentFrameNum = 0;
                    frameCnt = 0f;
                    executedTags = InitialBoolList(length, false);
                }
                else
                {
                    if(actor.alive)
                    {
                        Debug.Log("Trans");
                        actor.TransState(PengActor.initalName, true);
                    }
                    return;
                }
            }

            currentFrameNum = Mathf.FloorToInt(frameCnt * actor.game.globalFrameRate);
            for (int i = 0; i <= currentFrameNum; i++)
            {
                if (!executedTags[i])
                {
                    executedTags[i] = true;
                    if(tracks.Count > 0)
                    {
                        for(int j = 0; j < tracks.Count; j++)
                        {
                            if (tracks[j].execTime == PengTrack.ExecTime.Update && i >= tracks[j].start && i <= tracks[j].end)
                            {
                                tracks[j].ExecuteOnce();
                            }
                        }
                    }
                }
            }
        }
    }

    public void OnExit()
    {
        if (tracks.Count > 0)
        {
            for (int j = 0; j < tracks.Count; j++)
            {
                if (tracks[j].execTime == PengTrack.ExecTime.Exit)
                {
                    tracks[j].ExecuteOnce();
                }
            }
        }
    }

    public static List<bool> InitialBoolList(int num, bool value)
    {
        List<bool> result = new List<bool>();
        for (int i = 0; i < num; i++)
        {
            result.Add(value);
        }
        return result;
    }

    public List<PengTrack> ReadActorTracks(XmlElement stateEle)
    {
        List<PengTrack> result = new List<PengTrack>();
        for (int i = 0; i < stateEle.ChildNodes.Count; i++)
        {
            XmlElement trackEle = stateEle.ChildNodes[i] as XmlElement;
            PengTrack track = new PengTrack((PengTrack.ExecTime)Enum.Parse(typeof(PengTrack.ExecTime), trackEle.GetAttribute("ExecTime")), trackEle.GetAttribute("Name"), int.Parse(trackEle.GetAttribute("Start")),
                int.Parse(trackEle.GetAttribute("End")));
            
            for(int j = 0; j < trackEle.ChildNodes.Count; j++)
            {
                XmlElement scriptEle = trackEle.ChildNodes[j] as XmlElement;
                BaseScript script = ConstructRunTimePengScript(actor, scriptEle, ref track, int.Parse(scriptEle.GetAttribute("ScriptID")), scriptEle.GetAttribute("OutID"), scriptEle.GetAttribute("VarInID"), scriptEle.GetAttribute("SpecialInfo"));
                if(script != null)
                {
                    track.scripts.Add(script);
                }
            }
            result.Add(track);
        }
        return result;
    }

    public static BaseScript ConstructRunTimePengScript(PengActor actor, XmlElement scriptEle, ref PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
    {
        PengScriptType scriptType = (PengScriptType)Enum.Parse(typeof(PengScriptType), scriptEle.GetAttribute("ScriptType"));
        switch (scriptType)
        {
            default:
                return null;
            case PengScriptType.OnTrackExecute:
                return new OnTrackExecute(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.DebugLog:
                return new DebugLog(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.PlayAnimation:
                return new PlayAnimation(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.IfElse:
                return new IfElse(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.ValuePengInt:
                return new ValuePengInt(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.ValuePengFloat:
                return new ValuePengFloat(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.ValuePengString:
                return new ValuePengString(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.ValuePengBool:
                return new ValuePengBool(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.GetTargetsByRange:
                return new GetTargetsByRange(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.ForIterator:
                return new ForIterator(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.ValuePengVector3:
                return new ValuePengVector3(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.ValueFloatToString:
                return new ValueFloatToString(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.TransState:
                return new TransState(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.BreakPoint:
                return new BreakPoint(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.GlobalTimeScale:
                return new GlobalTimeScale(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.MathCompare:
                return new MathCompare(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.ValueIntToFloat:
                return new ValueIntToFloat(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.SetBlackBoardVariables:
                return new SetBlackBoardVariables(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.OnEvent:
                return new OnEvent(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.CustomEvent:
                return new CustomEvent(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.GetVariables:
                return new GetVariables(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.MathBool:
                return new MathBool(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.ValueGetListCount:
                return new ValueGetListCount(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
            case PengScriptType.PlayEffects:
                return new PlayEffects(actor, track, ID, flowOutInfo, varInInfo, specialInfo);
        }
    }
}
