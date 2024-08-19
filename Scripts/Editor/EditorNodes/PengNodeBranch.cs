using PengScript;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

public class GetInput : PengNode
{
    public List<PengScript.GetInput.InputMap> maps = new List<PengScript.GetInput.InputMap>();

    public GetInput(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);
        this.meaning = "通过接受不同的输入，切换至不同的状态。切换的优先级取决于输入映射在列表中的靠前程度。\n" +
            "需要将包含该节点的轨道拉长为需要接收输入的时间段。\n";
        
        inPoints = new PengNodeConnection[1];
        inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        
        paraNum = 1;

        inVars = new PengEditorVariables.PengVar[0];
        outVars = new PengEditorVariables.PengVar[0];

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Branch;
        scriptType = PengScript.PengScriptType.GetInput;
        nodeName = GetDescription(scriptType);

        rect = new Rect(rect.x, rect.y, rect.width * 2 + 80, rect.height);
        rectSmall = new Rect(rectSmall.x, rectSmall.y, rect.width, rectSmall.height + 80);
    }

    public override void Draw()
    {
        base.Draw();
        Rect add = new Rect(rect.x + 510, rect.y + 5, 30,18);
        if (GUI.Button(add, EditorGUIUtility.IconContent("d_winbtn_mac_max_h")))
        {
            PengScript.GetInput.InputMap map = new PengScript.GetInput.InputMap();
            map.stateName = "Attack1";
            map.frameOffset = 3;
            map.input = PengActorControl.ActionType.Attack;
            maps.Add(map);
        }
        paraNum = maps.Count;
        int change1 = -1;
        int change2 = -1;
        int remove = -1;
        if (maps.Count > 0)
        {
            for (int i = 0; i < maps.Count; i++)
            {
                int tem1 = -1;
                int tem2 = -1;
                Rect varRect = new Rect(rectSmall.x + 5f, rect.y + rect.height + 5 + 23 * i, 110, 18);
                DrawEntry(varRect, i, out tem1, out tem2, out remove);
                if (tem1 >= 0 && tem2 >= 0)
                {
                    change1 = tem1;
                    change2 = tem2;
                }
            }
        }
        if (change1 >= 0 && change2 >= 0)
        {
            PengScript.GetInput.InputMap map1 = new PengScript.GetInput.InputMap();
            map1.input = maps[change1].input;
            map1.frameOffset = maps[change1].frameOffset;
            map1.stateName = maps[change1].stateName;
            PengScript.GetInput.InputMap map2 = new PengScript.GetInput.InputMap();
            map2.input = maps[change2].input;
            map2.frameOffset = maps[change2].frameOffset;
            map2.stateName = maps[change2].stateName;
            maps[change1] = map2;
            maps[change2] = map1;
        }
        if (remove > 0)
        {
            maps.RemoveAt(remove);
        }
    }

    public void DrawEntry(Rect rect, int index, out int change1, out int change2, out int remove)
    {
        change1 = -1;
        change2 = -1;
        remove = -1;
        int tem1 = -1;
        int tem2 = -1;
        PengScript.GetInput.InputMap map = maps[index];
        Rect lbl = new Rect(rect.x, rect.y, 50, 18);
        GUI.Box(lbl, "状态名");
        Rect stn = new Rect(lbl.x + lbl.width + 10, rect.y, 80, rect.height);
        map.stateName = GUI.TextField(stn, map.stateName);
        Rect lbl2 = new Rect(stn.x + stn.width + 10, rect.y, 50, rect.height);
        GUI.Box(lbl2, "跳转帧");
        Rect fmo = new Rect(lbl2.x + lbl.width + 10, rect.y, 50, rect.height);
        map.frameOffset = EditorGUI.IntField(fmo, map.frameOffset);
        Rect lbl3 = new Rect(fmo.x + fmo.width + 10, rect.y, 40, rect.height);
        GUI.Box(lbl3, "输入");
        Rect ipt = new Rect(lbl3.x + lbl3.width + 10, rect.y, 120, rect.height);
        map.input = (PengActorControl.ActionType)EditorGUI.EnumPopup(ipt, map.input);
        Rect up = new Rect(ipt.x + ipt.width + 10, rect.y, 25, rect.height);
        if (index != 0)
        {
            if (GUI.Button(up, EditorGUIUtility.IconContent("CollabPush")))
            {
                tem1 = index;
                tem2 = index - 1;
            }
        }
        Rect down = new Rect(up.x + up.width + 2, rect.y, up.width, rect.height);
        if (index != maps.Count - 1)
        {
            if (GUI.Button(down, EditorGUIUtility.IconContent("CollabPull")))
            {
                tem1 = index;
                tem2 = index + 1;
            }
        }
        Rect del = new Rect(down.x + down.width + 8, rect.y, down.width, rect.height);
        if (maps.Count > 1)
        {
            if (GUI.Button(del, EditorGUIUtility.IconContent("d_winbtn_mac_close_h")))
            {
                remove = index;
            }
        }
        change1 = tem1;
        change2 = tem2;
        maps[index] = map;
    }

    public override string SpecialParaDescription()
    {
        string result = "";
        if (maps.Count > 0)
        {
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].stateName != "")
                {
                    result += maps[i].stateName + "," + maps[i].frameOffset.ToString() + "," + maps[i].input.ToString();
                    if (i != maps.Count - 1)
                    {
                        result += ";";
                    }
                }
            }
        }
        return result;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(";");
            if (str.Length > 0)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    string[] s1 = str[i].Split(",");
                    PengScript.GetInput.InputMap map = new PengScript.GetInput.InputMap();
                    map.stateName = s1[0];
                    map.frameOffset = int.Parse(s1[1]);
                    map.input = (PengActorControl.ActionType)Enum.Parse(typeof(PengActorControl.ActionType), s1[2]);
                    maps.Add(map);
                }
            }
        }
        else
        {
            PengScript.GetInput.InputMap map = new PengScript.GetInput.InputMap();
            map.stateName = "Attack1";
            map.frameOffset = 3;
            map.input = PengActorControl.ActionType.Attack;
            maps.Add(map);
        }
    }

}
