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
        Global,
    }
    public string name;
    public ExecTime execTime;

    public int start;
    public int end;

    public List<BaseScript> scripts = new List<BaseScript>();
    
    public PengTrack(ExecTime time, string name, int start, int end)
    {
        this.name = name;
        this.execTime = time;
        this.start = start;
        this.end = end;
        PengTrack track = this; 
    }

    public void ExecuteOnce()
    {
        if (scripts.Count > 0)
        {
            scripts[0].Execute(0);
        }
    }

    public BaseScript GetScriptByScriptID(int id)
    {
        if (scripts.Count > 0)
        {
            for (int i = 0; i < scripts.Count; i++)
            {
                if (scripts[i].ID == id)
                {
                    return scripts[i];
                }
            }
            return null;
        }
        return null;
    }

    public PengVariables.PengVar GetOutPengVarByScriptIDPengVarID(int scriptID, int varOutID)
    {
        if (GetScriptByScriptID(scriptID) == null)
        { return null; }
        else
        { return GetScriptByScriptID(scriptID).outVars[varOutID]; }
    }
}
