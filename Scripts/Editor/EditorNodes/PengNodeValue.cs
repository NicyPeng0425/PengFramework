using PengScript;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static PengScript.GetVariables;

public class ValuePengInt : PengNode
{
    public PengEditorVariables.PengInt pengInt;

    public ValuePengInt(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[0];
        outVars = new PengEditorVariables.PengVar[1];
        pengInt = new PengEditorVariables.PengInt(this, "值", 0, ConnectionPointType.Out);
        outVars[0] = pengInt;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValuePengInt;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        Rect intField = new Rect(pengInt.varRect.x + 45, pengInt.varRect.y, 65, 18);
        pengInt.value = EditorGUI.IntField(intField, pengInt.value);
    }
    public override string SpecialParaDescription()
    {
        return pengInt.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            pengInt.value = int.Parse(info);
        }
        else
        {
            pengInt.value = 0;
        }
    }
}

public class ValuePengFloat : PengNode
{
    public PengEditorVariables.PengFloat pengFloat;

    public ValuePengFloat(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[0];
        outVars = new PengEditorVariables.PengVar[1];
        pengFloat = new PengEditorVariables.PengFloat(this, "值", 0, ConnectionPointType.Out);
        outVars[0] = pengFloat;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValuePengFloat;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        Rect floatField = new Rect(pengFloat.varRect.x + 45, pengFloat.varRect.y, 65, 18);
        pengFloat.value = EditorGUI.FloatField(floatField, pengFloat.value);
    }
    public override string SpecialParaDescription()
    {
        return pengFloat.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            pengFloat.value = float.Parse(info);
        }
        else
        {
            pengFloat.value = 0f;
        }
    }
}

public class ValuePengString : PengNode
{
    public PengEditorVariables.PengString pengString;

    public ValuePengString(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[0];
        outVars = new PengEditorVariables.PengVar[1];
        pengString = new PengEditorVariables.PengString(this, "值", 0, ConnectionPointType.Out);
        outVars[0] = pengString;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValuePengString;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        Rect field = new Rect(pengString.varRect.x + 45, pengString.varRect.y, 65, 18);
        pengString.value = GUI.TextField(field, pengString.value);

    }
    public override string SpecialParaDescription()
    {
        return pengString.value;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        pengString.value = info;
    }
}

public class ValuePengBool : PengNode
{
    public PengEditorVariables.PengBool pengBool;

    public ValuePengBool(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[0];
        outVars = new PengEditorVariables.PengVar[1];
        pengBool = new PengEditorVariables.PengBool(this, "值", 0, ConnectionPointType.Out);
        outVars[0] = pengBool;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValuePengBool;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        Rect field = new Rect(pengBool.varRect.x + 45, pengBool.varRect.y, 65, 18);
        pengBool.value = GUI.Toggle(field, pengBool.value, "");

    }
    public override string SpecialParaDescription()
    {
        return pengBool.value ? "1" : "0";
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            pengBool.value = int.Parse(info) > 0;
        }
        else
        {
            pengBool.value = false;
        }
    }
}

public class ValuePengVector3 : PengNode
{
    public PengEditorVariables.PengFloat pengX;
    public PengEditorVariables.PengFloat pengY;
    public PengEditorVariables.PengFloat pengZ;

    public PengEditorVariables.PengVector3 pengVec3;

    public ValuePengVector3(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[3];
        outVars = new PengEditorVariables.PengVar[1];

        pengX = new PengEditorVariables.PengFloat(this, "X", 0, ConnectionPointType.In);
        pengY = new PengEditorVariables.PengFloat(this, "Y", 1, ConnectionPointType.In);
        pengZ = new PengEditorVariables.PengFloat(this, "Z", 2, ConnectionPointType.In);
        pengVec3 = new PengEditorVariables.PengVector3(this, "Vector3", 0, ConnectionPointType.Out);

        inVars[0] = pengX;
        inVars[1] = pengY;
        inVars[2] = pengZ;
        outVars[0] = pengVec3;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValuePengVector3;
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
                        pengX.value = EditorGUI.FloatField(field, pengX.value);
                        break;
                    case 1:
                        pengY.value = EditorGUI.FloatField(field, pengY.value);
                        break;
                    case 2:
                        pengZ.value = EditorGUI.FloatField(field, pengZ.value);
                        break;
                }
            }
        }
    }
    public override string SpecialParaDescription()
    {
        return pengX.value.ToString() + "," + pengY.value.ToString() + "," + pengZ.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(",");
            pengX.value = float.Parse(str[0]);
            pengY.value = float.Parse(str[1]);
            pengZ.value = float.Parse(str[2]);
        }
    }
}

public class ValueFloatToString : PengNode
{
    public PengEditorVariables.PengFloat pengFloat;

    public PengEditorVariables.PengString pengString;

    public ValueFloatToString(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[1];
        outVars = new PengEditorVariables.PengVar[1];

        pengFloat = new PengEditorVariables.PengFloat(this, "浮点", 0, ConnectionPointType.In);
        pengString = new PengEditorVariables.PengString(this, "字符串", 0, ConnectionPointType.Out);

        inVars[0] = pengFloat;
        outVars[0] = pengString;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValueFloatToString;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        if (varInID[0].nodeID < 0)
        {
            Rect field = new Rect(inVars[0].varRect.x + 45, inVars[0].varRect.y, 65, 18);
            pengFloat.value = EditorGUI.FloatField(field, pengFloat.value);
        }
    }
    public override string SpecialParaDescription()
    {
        return pengFloat.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            pengFloat.value = float.Parse(info);
        }
    }
}

public class ValueIntToFloat : PengNode
{
    public PengEditorVariables.PengFloat pengFloat;

    public PengEditorVariables.PengInt pengInt;

    public ValueIntToFloat(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[1];
        outVars = new PengEditorVariables.PengVar[1];

        pengFloat = new PengEditorVariables.PengFloat(this, "浮点", 0, ConnectionPointType.Out);
        pengInt = new PengEditorVariables.PengInt(this, "整型", 0, ConnectionPointType.In);

        inVars[0] = pengInt;
        outVars[0] = pengFloat;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValueIntToFloat;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        if (varInID[0].nodeID < 0)
        {
            Rect field = new Rect(inVars[0].varRect.x + 45, inVars[0].varRect.y, 65, 18);
            pengInt.value = EditorGUI.IntField(field, pengInt.value);
        }
    }
    public override string SpecialParaDescription()
    {
        return pengInt.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            pengInt.value = int.Parse(info);
        }
    }
}

public class GetVariables : PengNode
{
    public PengEditorVariables.PengInt varSourceInt;
    public PengScript.GetVariables.VariableSource variableSource;
    public PengScript.GetVariables.VariableSourceCN variableSourceCN;

    public PengEditorVariables.PengInt bbTypeInt;
    public PengScript.GetVariables.BlackBoardType bbType;
    public PengScript.GetVariables.BlackBoardTypeCN bbTypeCN;

    public PengEditorVariables.PengInt variableTypeInt;
    public PengScript.GetVariables.VariableType varType;
    public PengScript.GetVariables.VariableTypeCN varTypeCN;

    public PengEditorVariables.PengString varName;
    public PengEditorVariables.PengPengActor targetActor;

    public PengEditorVariables.PengInt intOut;
    public PengEditorVariables.PengFloat floatOut;
    public PengEditorVariables.PengString stringOut;
    public PengEditorVariables.PengBool boolOut;
    public PengEditorVariables.PengPengActor actorOut;
    public PengEditorVariables.PengVector3 vec3Out;


    public GetVariables(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[4];
        outVars = new PengEditorVariables.PengVar[1];

        varSourceInt = new PengEditorVariables.PengInt(this, "变量来源", 0, ConnectionPointType.In);
        bbTypeInt = new PengEditorVariables.PengInt(this, "黑板类型", 1, ConnectionPointType.In);
        variableTypeInt = new PengEditorVariables.PengInt(this, "变量类型", 1, ConnectionPointType.In);

        targetActor = new PengEditorVariables.PengPengActor(this, "角色来源", 2, ConnectionPointType.In);

        varName = new PengEditorVariables.PengString(this, "变量名称", 3, ConnectionPointType.In);

        varSourceInt.point = null;
        bbTypeInt.point = null;
        variableTypeInt.point = null;
        inVars[0] = varSourceInt;
        intOut = new PengEditorVariables.PengInt(this, "整型输出", 0, ConnectionPointType.Out);
        floatOut = new PengEditorVariables.PengFloat(this, "浮点输出", 0, ConnectionPointType.Out);
        stringOut = new PengEditorVariables.PengString(this, "字符串输出", 0, ConnectionPointType.Out);
        boolOut = new PengEditorVariables.PengBool(this, "布尔输出", 0, ConnectionPointType.Out);
        actorOut = new PengEditorVariables.PengPengActor(this, "角色输出", 0, ConnectionPointType.Out);
        vec3Out = new PengEditorVariables.PengVector3(this, "向量输出", 0, ConnectionPointType.Out);

        varSourceInt.value = 0;
        variableSource = (PengScript.GetVariables.VariableSource)varSourceInt.value;
        variableSourceCN = (PengScript.GetVariables.VariableSourceCN)varSourceInt.value;
        bbTypeInt.value = 0;
        bbType = (PengScript.GetVariables.BlackBoardType)bbTypeInt.value;
        bbTypeCN = (PengScript.GetVariables.BlackBoardTypeCN)bbTypeInt.value;
        variableTypeInt.value = 0;
        varType = (PengScript.GetVariables.VariableType)variableTypeInt.value;
        varTypeCN = (PengScript.GetVariables.VariableTypeCN)variableTypeInt.value;

        inVars[1] = bbTypeInt;
        inVars[2] = targetActor;
        inVars[3] = varName;
        paraNum = 4;
        outVars[0] = intOut;

        ReadSpecialParaDescription(specialInfo);

        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.GetVariables;
        nodeName = GetDescription(scriptType);

    }

    public override void Draw()
    {
        base.Draw();
        bool clearReference = false;
        for (int i = 0; i < paraNum; i++)
        {
            if (varInID[i].nodeID < 0)
            {
                Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 65, 18);
                switch (i)
                {
                    case 0:
                        variableSource = (PengScript.GetVariables.VariableSource)EditorGUI.EnumPopup(field, (PengScript.GetVariables.VariableSourceCN)varSourceInt.value);
                        if ((int)variableSource != varSourceInt.value)
                        {
                            clearReference = true;
                        }
                        varSourceInt.value = (int)variableSource;
                        variableSourceCN = (PengScript.GetVariables.VariableSourceCN)varSourceInt.value;
                        break;
                    case 1:
                        switch (variableSource)
                        {
                            case PengScript.GetVariables.VariableSource.ActorBlackBoard:
                                bbType = (PengScript.GetVariables.BlackBoardType)EditorGUI.EnumPopup(field, (PengScript.GetVariables.BlackBoardTypeCN)bbTypeInt.value);
                                if ((int)bbType != bbTypeInt.value)
                                {
                                    clearReference = true;
                                }
                                bbTypeInt.value = (int)bbType;
                                bbTypeCN = (PengScript.GetVariables.BlackBoardTypeCN)bbTypeInt.value;
                                break;
                            case PengScript.GetVariables.VariableSource.GlobalBlackBoard:
                                bbType = (PengScript.GetVariables.BlackBoardType)EditorGUI.EnumPopup(field, (PengScript.GetVariables.BlackBoardTypeCN)bbTypeInt.value);
                                if ((int)bbType != bbTypeInt.value)
                                {
                                    clearReference = true;
                                }
                                bbTypeInt.value = (int)bbType;
                                bbTypeCN = (PengScript.GetVariables.BlackBoardTypeCN)bbTypeInt.value;
                                break;
                            case PengScript.GetVariables.VariableSource.ActorAttribute:
                                varType = (PengScript.GetVariables.VariableType)EditorGUI.EnumPopup(field, (PengScript.GetVariables.VariableTypeCN)variableTypeInt.value);
                                if ((int)varType != variableTypeInt.value)
                                {
                                    clearReference = true;
                                }
                                variableTypeInt.value = (int)varType;
                                varTypeCN = (PengScript.GetVariables.VariableTypeCN)variableTypeInt.value;
                                break;
                        }
                        break;
                    case 2:
                        switch (variableSource)
                        {
                            case PengScript.GetVariables.VariableSource.GlobalBlackBoard:
                                varName.value = EditorGUI.TextField(field, varName.value);
                                break;
                        }
                        break;
                    case 3:
                        varName.value = EditorGUI.TextField(field, varName.value);
                        break;
                }
            }
        }
        ChangeTheNode();
        if (clearReference)
        {
            ClearReference();
        }
    }
    public override string SpecialParaDescription()
    {
        return varSourceInt.value.ToString() + "," + bbTypeInt.value.ToString() + "," + variableTypeInt.value.ToString() + "," + varName.value;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(",");
            varSourceInt.value = int.Parse(str[0]);
            variableSource = (PengScript.GetVariables.VariableSource)varSourceInt.value;
            variableSourceCN = (PengScript.GetVariables.VariableSourceCN)varSourceInt.value;
            bbTypeInt.value = int.Parse(str[1]);
            bbType = (PengScript.GetVariables.BlackBoardType)bbTypeInt.value;
            bbTypeCN = (PengScript.GetVariables.BlackBoardTypeCN)bbTypeInt.value;
            variableTypeInt.value = int.Parse(str[2]);
            varType = (PengScript.GetVariables.VariableType)variableTypeInt.value;
            varTypeCN = (PengScript.GetVariables.VariableTypeCN)variableTypeInt.value;
            if (str.Length > 3)
            {
                varName.value = str[3];
            }
            ChangeTheNode();
        }
    }

    public void ClearReference()
    {
        switch (variableSource)
        {
            case PengScript.GetVariables.VariableSource.ActorBlackBoard:
                varInID = DefaultDictionaryIntNodeIDConnectionID(4);
                break;
            case PengScript.GetVariables.VariableSource.GlobalBlackBoard:
                varInID = DefaultDictionaryIntNodeIDConnectionID(3);
                break;
            case PengScript.GetVariables.VariableSource.ActorAttribute:
                varInID = DefaultDictionaryIntNodeIDConnectionID(3);
                break;
        }
        if (trackMaster.nodes.Count > 1)
        {
            for (int i = 0; i < trackMaster.nodes.Count; i++)
            {
                if (trackMaster.nodes[i].varInID.Count > 0)
                {
                    for (int j = 0; j < trackMaster.nodes[i].varInID.Count; j++)
                    {
                        if (trackMaster.nodes[i].varInID.ElementAt(j).Value.nodeID == nodeID)
                        {
                            trackMaster.nodes[i].varInID[j] = DefaultNodeIDConnectionID();
                        }
                    }
                }
            }
        }
    }

    public void ChangeTheNode()
    {
        switch (variableSource)
        {
            case PengScript.GetVariables.VariableSource.ActorBlackBoard:
                inVars = new PengEditorVariables.PengVar[4];
                inVars[0] = varSourceInt;
                inVars[1] = bbTypeInt;
                inVars[2] = targetActor;
                targetActor.index = 2;
                inVars[3] = varName;
                varName.index = 3;
                paraNum = 4;
                switch (bbType)
                {
                    case PengScript.GetVariables.BlackBoardType.Int:
                        outVars[0] = intOut;
                        break;
                    case PengScript.GetVariables.BlackBoardType.Float:
                        outVars[0] = floatOut;
                        break;
                    case PengScript.GetVariables.BlackBoardType.String:
                        outVars[0] = stringOut;
                        break;
                    case PengScript.GetVariables.BlackBoardType.Bool:
                        outVars[0] = boolOut;
                        break;
                    case PengScript.GetVariables.BlackBoardType.PengActor:
                        outVars[0] = actorOut;
                        break;
                }
                break;
            case PengScript.GetVariables.VariableSource.GlobalBlackBoard:
                inVars = new PengEditorVariables.PengVar[3];
                inVars[0] = varSourceInt;
                inVars[1] = bbTypeInt;
                inVars[2] = varName;
                varName.index = 2;
                paraNum = 3;
                switch (bbType)
                {
                    case PengScript.GetVariables.BlackBoardType.Int:
                        outVars[0] = intOut;
                        break;
                    case PengScript.GetVariables.BlackBoardType.Float:
                        outVars[0] = floatOut;
                        break;
                    case PengScript.GetVariables.BlackBoardType.String:
                        outVars[0] = stringOut;
                        break;
                    case PengScript.GetVariables.BlackBoardType.Bool:
                        outVars[0] = boolOut;
                        break;
                    case PengScript.GetVariables.BlackBoardType.PengActor:
                        outVars[0] = actorOut;
                        break;
                }
                break;
            case PengScript.GetVariables.VariableSource.ActorAttribute:
                inVars = new PengEditorVariables.PengVar[3];
                inVars[0] = varSourceInt;
                variableTypeInt.index = 1;
                inVars[1] = variableTypeInt;
                inVars[2] = targetActor;
                targetActor.index = 2;
                paraNum = 3;
                if (varType == PengScript.GetVariables.VariableType.ActorID || varType == PengScript.GetVariables.VariableType.ActorCamp)
                {
                    outVars[0] = intOut;
                }
                else if (varType == PengScript.GetVariables.VariableType.ActorName || varType == VariableType.ActorCurrentStateName || varType == VariableType.ActorLastStateName)
                {
                    outVars[0] = stringOut;
                }
                else if (varType == PengScript.GetVariables.VariableType.ActorAttackPower ||
                    varType == PengScript.GetVariables.VariableType.ActorDefendPower ||
                    varType == PengScript.GetVariables.VariableType.ActorCriticalRate ||
                    varType == PengScript.GetVariables.VariableType.ActorCriticalDamageRatio ||
                    varType == PengScript.GetVariables.VariableType.ActorCurrentHP ||
                    varType == PengScript.GetVariables.VariableType.ActorMaxHP ||
                    varType == VariableType.ActorDirectionInputMagnitude)
                {
                    outVars[0] = floatOut;
                }
                else if (varType == PengScript.GetVariables.VariableType.ActorPosition || varType== VariableType.ActorDirectionInput || varType == VariableType.ActorDirectionProcessed || varType == VariableType.ActorForward)
                {
                    outVars[0] = vec3Out;
                }
                else if (varType == PengScript.GetVariables.VariableType.ActorOnGround)
                {
                    outVars[0] = boolOut;
                }
                break;
        }
    }
}

public class ValueGetListCount : PengNode
{
    public PengEditorVariables.PengList<PengActor> list;

    public PengEditorVariables.PengInt count;

    public ValueGetListCount(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inVars = new PengEditorVariables.PengVar[1];
        outVars = new PengEditorVariables.PengVar[1];

        list = new PengEditorVariables.PengList<PengActor>(this, "Actor列表", 0, ConnectionPointType.In);
        count = new PengEditorVariables.PengInt(this, "元素个数", 0, ConnectionPointType.Out);

        inVars[0] = count;
        outVars[0] = list;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Value;
        scriptType = PengScript.PengScriptType.ValueGetListCount;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }
}
