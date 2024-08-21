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

    public string otherInfo = "";
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

    public PengEditorTrack(PengTrack.ExecTime time, string name, int start, int end, PengActorStateEditorWindow master, bool isNew)
    {
        this.trackName = name;
        this.execTime = time;
        this.start = start;
        this.end = end;
        this.master = master;
        PengEditorTrack track = this;
        if (isNew && time != PengTrack.ExecTime.Global)
        {
            nodes.Add(new OnTrackExecute(Vector2.zero, master, ref track, 1,
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(1)),
                    PengNode.ParseDictionaryIntListNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntListNodeIDConnectionID(2)),
                    PengNode.ParseDictionaryIntNodeIDConnectionIDToString(PengNode.DefaultDictionaryIntNodeIDConnectionID(0)), ""));

            nodes[0].ProcessDrag(new Vector2(300f, 415f));
        }
    }

}
