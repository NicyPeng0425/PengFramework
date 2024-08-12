using PengScript;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengEditorTrack
{
    public string trackName;
    public PengTrack.ExecTime execTime;

    public int start;
    public int end;
    PengActorStateEditorWindow m_master;
    public PengActorStateEditorWindow master
    {
        get { return m_master; }
        set
        {
            m_master = value;
            if (nodes.Count > 0)
            {
                foreach (PengNode node in nodes)
                {
                    if (node != null)
                    {
                        node.master = m_master;
                    }
                }
            }
        }
    }
    public List<PengNode> nodes = new List<PengNode>();
    public List<BaseScript> scripts = new List<BaseScript>();

    public PengEditorTrack(PengTrack.ExecTime time, string name, int start, int end, PengActorStateEditorWindow master, bool isNew)
    {
        this.trackName = name;
        this.execTime = time;
        this.start = start;
        this.end = end;
        this.master = master;
        PengEditorTrack track = this;
        if (isNew)
        {
            nodes.Add(new OnTrackExecute(Vector2.zero, master, ref track, 1,
                    PengNode.ParseDictionaryIntIntToString(PengNode.DefaultDictionaryIntInt(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(2)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));

            nodes[0].ProcessDrag(new Vector2(300f, 415f));
        }
    }

    public void ExecuteOnce()
    {
        if (scripts.Count > 0)
        {
            scripts[0].Execute();
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
