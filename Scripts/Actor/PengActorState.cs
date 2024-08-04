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
    public List<PengTrack> tracks = new List<PengTrack>();

    public int currentFrameNum;

    public PengActorState(PengActor actor, XmlDocument stateInfo)
    {
        this.actor = actor;
    }
    public void OnEnter()
    {
        currentFrameNum = 0;
        /*
        foreach (BaseScript info in scripts)
        {
            if (info.execTime == PengScript.ExecTime.Enter && info.enabled)
            {
                info.FirstExecute();
                info.ScriptFlowNext();
            }
        }*/
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {
    }
}
