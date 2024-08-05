using PengScript;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengTrack
{
    public enum ExecTime
    {
        Enter,
        Update,
        Exit,
    }
    public string name;
    public ExecTime execTime;

    public int start;
    public int end;
    PengActorStateEditorWindow master;
    public List<PengNode> nodes = new List<PengNode>();
    public List<PengNodeConnectionLine> lines = new List<PengNodeConnectionLine>();

    public List<BaseScript> scripts = new List<BaseScript>();
    
    public PengTrack(ExecTime time, string name, int start, int end, PengActorStateEditorWindow master)
    {
        this.name = name;
        this.execTime = time;
        this.start = start;
        this.end = end;
        this.master = master;
        nodes.Add(new OnExecute(Vector2.zero, master, this));

        nodes[0].ProcessDrag(new Vector2(300f, 415f));
    }

    public void ExecuteOnce()
    {
        if (scripts.Count > 0)
        {
            scripts[0].Execute();
            scripts[0].ScriptFlowNext();
        }
    }
/*
#if UNITY_EDITOR
    public void ProcessEvent(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                {

                }
                break;
        }
    }

    public void RightButtonMenu()
    {

    }

#endif*/
}
