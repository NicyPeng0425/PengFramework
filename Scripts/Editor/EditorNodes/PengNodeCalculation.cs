using PengScript;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static PengScript.MathCompare;

public class MathCompare : PengNode
{
    public PengScript.MathCompare.CompareType compare;
    public PengScript.MathCompare.CompareTypeCN compareCN;

    public PengEditorVariables.PengFloat compare1;
    public PengEditorVariables.PengInt compareType;
    public PengEditorVariables.PengFloat compare2;

    public PengEditorVariables.PengBool result;

    public MathCompare(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[3];
        outVars = new PengEditorVariables.PengVar[1];

        compare1 = new PengEditorVariables.PengFloat(this, "比较数一", 0, ConnectionPointType.In);
        compareType = new PengEditorVariables.PengInt(this, "比较方式", 1, ConnectionPointType.In);
        compare2 = new PengEditorVariables.PengFloat(this, "比较数二", 2, ConnectionPointType.In);
        result = new PengEditorVariables.PengBool(this, "结果", 0, ConnectionPointType.Out);
        compare = PengScript.MathCompare.CompareType.Less;

        inVars[0] = compare1;
        inVars[1] = compareType;
        inVars[2] = compare2;
        outVars[0] = result;

        compareType.point = null;
        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.MathCompare;
        nodeName = GetDescription(scriptType);

        paraNum = 3;
    }

    public override void Draw()
    {
        base.Draw();
        for (int i = 0; i < 3; i++)
        {
            if (varInID[i].nodeID < 0)
            {
                Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 65, 18);
                switch (i)
                {
                    case 0:
                        compare1.value = EditorGUI.FloatField(field, compare1.value);
                        break;
                    case 1:
                        compare = (PengScript.MathCompare.CompareType)((int)(PengScript.MathCompare.CompareTypeCN)EditorGUI.EnumPopup(field, compareCN));
                        compareCN = (CompareTypeCN)(int)compare;
                        compareType.value = (int)compare;
                        break;
                    case 2:
                        compare2.value = EditorGUI.FloatField(field, compare2.value);
                        break;
                }
            }
        }
    }
    public override string SpecialParaDescription()
    {
        string compareInt = ((int)compare).ToString();
        return compare1.value.ToString() + "," + compareInt + "," + compare2.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(",");
            compare1.value = float.Parse(str[0]);
            compareType.value = int.Parse(str[1]);
            compareCN = (PengScript.MathCompare.CompareTypeCN)compareType.value;
            compare = (PengScript.MathCompare.CompareType)compareType.value;
            compare2.value = float.Parse(str[2]);
        }
    }
}

public class MathBool : PengNode
{
    public PengScript.MathBool.BoolType boolType;

    public PengEditorVariables.PengBool bool1;
    public PengEditorVariables.PengInt boolInt;
    public PengEditorVariables.PengBool bool2;

    public PengEditorVariables.PengBool result;

    public MathBool(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[3];
        outVars = new PengEditorVariables.PengVar[1];

        bool1 = new PengEditorVariables.PengBool(this, "布尔值一", 0, ConnectionPointType.In);
        boolInt = new PengEditorVariables.PengInt(this, "运算方式", 1, ConnectionPointType.In);
        boolInt.value = 0;
        bool2 = new PengEditorVariables.PengBool(this, "布尔值二", 2, ConnectionPointType.In);
        result = new PengEditorVariables.PengBool(this, "结果", 0, ConnectionPointType.Out);
        boolType = PengScript.MathBool.BoolType.AND;

        inVars[0] = bool1;
        inVars[1] = boolInt;
        inVars[2] = bool2;
        outVars[0] = result;

        boolInt.point = null;
        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.MathBool;
        nodeName = GetDescription(scriptType);

        paraNum = 3;
    }

    public override void Draw()
    {
        base.Draw();
        ChangeNode();
        for (int i = 0; i < paraNum; i++)
        {
            if (varInID[i].nodeID < 0)
            {
                Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 65, 18);
                switch (i)
                {
                    case 0:
                        bool1.value = EditorGUI.Toggle(field, bool1.value);
                        break;
                    case 1:
                        boolType = (PengScript.MathBool.BoolType)EditorGUI.EnumPopup(field, boolType);
                        boolInt.value = (int)boolType;
                        break;
                    case 2:
                        bool2.value = EditorGUI.Toggle(field, bool2.value);
                        break;
                }
            }
        }
    }
    public override string SpecialParaDescription()
    {
        return (bool1.value ? "1" : "0") + "," + boolInt.value.ToString() + "," + (bool2.value ? "1" : "0");
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(",");
            bool1.value = int.Parse(str[0]) > 0;
            boolInt.value = int.Parse(str[1]);
            boolType = (PengScript.MathBool.BoolType)boolInt.value;
            bool2.value = int.Parse(str[2]) > 0;
            ChangeNode();
        }
    }

    public void ChangeNode()
    {
        if (boolType == PengScript.MathBool.BoolType.NOT)
        {
            inVars = new PengEditorVariables.PengVar[2];
            inVars[0] = bool1;
            inVars[1] = boolInt;
            paraNum = 2;
        }
        else
        {
            inVars = new PengEditorVariables.PengVar[3];
            inVars[0] = bool1;
            inVars[1] = boolInt;
            inVars[2] = bool2;
            paraNum = 3;
        }
    }
}
