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

    public int currentFrameNum;
    public float frameCnt = 0f;
    public List<bool> executedTags = new List<bool>();

    public PengActorState(PengActor actor, XmlDocument stateInfo)
    {
        this.actor = actor;
        ConstructActorState();
    }
    public void OnEnter()
    {
        currentFrameNum = 0;
        frameCnt = 0f;
        executedTags = InitialBoolList(length, false);

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
            if (frameCnt > length / actor.game.globalFrameRate)
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
                        actor.TransState(PengActor.initalName);
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

    public void ConstructActorState()
    {
        
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
}
