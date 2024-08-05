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
    public List<PengNode> nodes = new List<PengNode>();
    public List<PengNodeConnectionLine> lines = new List<PengNodeConnectionLine>();

    public List<BaseScript> scripts = new List<BaseScript>();
    
    public PengTrack(ExecTime time, string name, int start, int end, PengActorStateEditorWindow master)
    {
        this.name = name;
        this.execTime = time;
        this.start = start;
        this.end = end;
        nodes.Add(new OnExecute(Vector2.zero, master, this));
    }

    public void ExecuteOnce()
    {
        if (scripts.Count > 0)
        {
            scripts[0].Execute();
            scripts[0].ScriptFlowNext();
        }
    }
}
