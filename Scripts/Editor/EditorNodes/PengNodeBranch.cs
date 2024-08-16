using PengScript;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfElse : PengNode
{

    public List<PengScript.IfElse.IfElseIfElse> conditionTypes = new List<PengScript.IfElse.IfElseIfElse>();

    public IfElse(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);
        this.ReadSpecialParaDescription(specialInfo);

        inPoints = new PengNodeConnection[1];
        inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[this.outID.Count];
        inVars = new PengEditorVariables.PengVar[this.varInID.Count];
        paraNum = this.varInID.Count;
        for (int i = 0; i < this.outID.Count; i++)
        {
            outPoints[i] = new PengNodeConnection(ConnectionPointType.FlowOut, i, this, null);
            PengEditorVariables.PengBool ifInVar = new PengEditorVariables.PengBool(this, "条件" + (i + 1).ToString(), i, ConnectionPointType.In);
            inVars[i] = ifInVar;
        }

        outVars = new PengEditorVariables.PengVar[0];
        type = NodeType.Branch;
        scriptType = PengScript.PengScriptType.IfElse;
        nodeName = GetDescription(scriptType);

    }

    public override void Draw()
    {
        base.Draw();
        if (inVars.Length > 0)
        {
            for (int i = 0; i < inVars.Length; i++)
            {
                Rect re = new Rect(inVars[i].varRect.x + 100, inVars[i].varRect.y, 60, 18);
                if (i == 0)
                {
                    GUI.Box(re, "If");
                }
                else
                {
                    if (i == inVars.Length - 1)
                    {
                        GUI.Box(re, conditionTypes[conditionTypes.Count - 1].ToString());
                    }
                    else
                    {
                        GUI.Box(re, "ElseIf");
                    }
                }
                GUIStyle style = new GUIStyle("CN EntryInfo");
                style.fontSize = 12;
                style.alignment = TextAnchor.UpperRight;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.white;
                Rect flowOut = new Rect(outPoints[i].rect.x - 80, outPoints[i].rect.y, 70, 20);
                GUI.Box(flowOut, "条件" + (i + 1).ToString(), style);
            }
            Rect add = new Rect(inVars[inVars.Length - 1].varRect.x, inVars[inVars.Length - 1].varRect.y + 20, 60, 20);
            if (GUI.Button(add, "添加"))
            {
                this.AddConditions();
            }
            if (inVars.Length > 1)
            {
                Rect remove = new Rect(add.x + 70, add.y, 60, 20);
                if (GUI.Button(remove, "移除"))
                {
                    this.RemoveConditions();
                }
                Rect change = new Rect(add.x + 140, add.y, 60, 20);
                if (GUI.Button(change, "更改"))
                {
                    this.ChangeCondition(inVars.Length - 1);
                }
            }

        }
    }

    public override string SpecialParaDescription()
    {
        string result = "";
        if (conditionTypes.Count > 0)
        {
            for (int i = 0; i < conditionTypes.Count; i++)
            {
                result += conditionTypes[i].ToString();
                if (i != conditionTypes.Count - 1)
                {
                    result += ",";
                }
            }
        }
        return result;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(",");
            for (int i = 0; i < str.Length; i++)
            {
                conditionTypes.Add((PengScript.IfElse.IfElseIfElse)Enum.Parse(typeof(PengScript.IfElse.IfElseIfElse), str[i]));
            }
        }
        else
        {
            conditionTypes.Add(PengScript.IfElse.IfElseIfElse.If);
        }
    }

    public void AddConditions()
    {
        PengEditorVariables.PengBool newVar = new PengEditorVariables.PengBool(this, "条件" + (inVars.Length + 1).ToString(), inVars.Length, ConnectionPointType.In);
        PengEditorVariables.PengVar[] newInVars = new PengEditorVariables.PengVar[inVars.Length + 1];
        for (int i = 0; i < inVars.Length; i++)
        {
            newInVars[i] = inVars[i];
        }
        newInVars[newInVars.Length - 1] = newVar;
        inVars = newInVars;

        conditionTypes.Add(PengScript.IfElse.IfElseIfElse.ElseIf);

        PengNodeConnection[] newOutPoints = new PengNodeConnection[inVars.Length];
        for (int i = 0; i < outPoints.Length; i++)
        {
            newOutPoints[i] = outPoints[i];
        }
        newOutPoints[newOutPoints.Length - 1] = new PengNodeConnection(ConnectionPointType.FlowOut, inVars.Length - 1, this, null);
        outPoints = newOutPoints;
        NodeIDConnectionID nici = new NodeIDConnectionID();
        nici.nodeID = -1;
        outID.Add(newInVars.Length - 1, nici);
        varInID.Add(newInVars.Length - 1, DefaultNodeIDConnectionID());
        paraNum++;

    }

    public void RemoveConditions()
    {
        if (inVars.Length > 1)
        {
            if (GetNodeByNodeID(varInID[varInID.Count - 1].nodeID).varOutID[varInID[varInID.Count - 1].connectionID].Count > 0)
            {
                for (int j = GetNodeByNodeID(varInID[varInID.Count - 1].nodeID).varOutID[varInID[varInID.Count - 1].connectionID].Count - 1; j >= 0; j--)
                {
                    if (GetNodeByNodeID(varInID[varInID.Count - 1].nodeID).varOutID[varInID[varInID.Count - 1].connectionID][j].nodeID == nodeID)
                    {
                        GetNodeByNodeID(varInID[varInID.Count - 1].nodeID).varOutID[varInID[varInID.Count - 1].connectionID].RemoveAt(j);
                        break;
                    }
                }
            }
            PengEditorVariables.PengVar[] newInVars = new PengEditorVariables.PengVar[inVars.Length - 1];
            PengNodeConnection[] newOutPoints = new PengNodeConnection[inVars.Length - 1];
            for (int i = 0; i < newInVars.Length; i++)
            {
                newInVars[i] = inVars[i];
                newOutPoints[i] = outPoints[i];
            }
            outPoints = newOutPoints;
            inVars = newInVars;

            paraNum--;
            conditionTypes.RemoveAt(inVars.Length);
            outID.Remove(inVars.Length);
            varInID.Remove(inVars.Length);
        }
    }

    public void ChangeCondition(int index)
    {
        if (conditionTypes[index] == PengScript.IfElse.IfElseIfElse.ElseIf)
        {
            conditionTypes[index] = PengScript.IfElse.IfElseIfElse.Else;
        }
        else if (conditionTypes[index] == PengScript.IfElse.IfElseIfElse.Else)
        {
            conditionTypes[index] = PengScript.IfElse.IfElseIfElse.ElseIf;
        }
    }
}
