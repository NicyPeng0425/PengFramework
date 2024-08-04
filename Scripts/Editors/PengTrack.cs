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
    
    public PengTrack(ExecTime time, string name, int start, int end)
    {
        this.name = name;
        this.execTime = time;
        this.start = start;
        this.end = end;
    }
}
