using PengScript;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GetTargetsByRange : PengNode
{
    public PengEditorVariables.PengList<PengActor> result;
    public PengScript.GetTargetsByRange.RangeType rangeType = PengScript.GetTargetsByRange.RangeType.Cylinder;
    public PengEditorVariables.PengInt typeNum;
    public PengEditorVariables.PengInt pengCamp;
    public PengEditorVariables.PengVector3 pengPara;
    public PengEditorVariables.PengVector3 pengOffset;

    public GetTargetsByRange(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoints = new PengNodeConnection[1];
        inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);

        inVars = new PengEditorVariables.PengVar[4];
        outVars = new PengEditorVariables.PengVar[1];

        typeNum = new PengEditorVariables.PengInt(this, "范围类型", 0, ConnectionPointType.In);
        typeNum.point = null;
        result = new PengEditorVariables.PengList<PengActor>(this, "获取到的目标", 0, ConnectionPointType.Out);
        pengCamp = new PengEditorVariables.PengInt(this, "阵营", 1, ConnectionPointType.In);
        pengPara = new PengEditorVariables.PengVector3(this, "参数", 2, ConnectionPointType.In);
        pengOffset = new PengEditorVariables.PengVector3(this, "偏移", 3, ConnectionPointType.In);

        outVars[0] = result;
        inVars[0] = typeNum;
        inVars[1] = pengCamp;
        inVars[2] = pengPara;
        inVars[3] = pengOffset;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.GetTargetsByRange;
        nodeName = GetDescription(scriptType);

        paraNum = 4;
    }

    public override void Draw()
    {
        base.Draw();


        for (int i = 0; i < 4; i++)
        {
            if (varInID[i].nodeID < 0)
            {
                Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 65, 18);
                Rect vec3Field = new Rect(field.x, field.y, field.width + 115, field.height);
                switch (i)
                {
                    case 0:
                        rangeType = (PengScript.GetTargetsByRange.RangeType)EditorGUI.EnumPopup(field, rangeType);
                        typeNum.value = (int)rangeType;
                        break;
                    case 1:
                        pengCamp.value = EditorGUI.IntField(field, pengCamp.value);
                        break;
                    case 2:
                        pengPara.value = EditorGUI.Vector3Field(vec3Field, "", pengPara.value);
                        if (rangeType == PengScript.GetTargetsByRange.RangeType.Cylinder)
                        {
                            if (pengPara.value.z >= 180)
                            {
                                pengPara.value.z = 180;
                            }
                        }
                        break;
                    case 3:
                        pengOffset.value = EditorGUI.Vector3Field(vec3Field, "", pengOffset.value);
                        break;
                }

            }
        }
        if (pengPara.value.x <= 0)
        {
            pengPara.value.x = 0;
        }
        if (pengPara.value.y <= 0)
        {
            pengPara.value.y = 0;
        }
        if (pengPara.value.z <= 0)
        {
            pengPara.value.z = 0;
        }
    }
    public override string SpecialParaDescription()
    {
        string info = "";
        switch (rangeType)
        {
            default:
                info = "";
                break;
            case PengScript.GetTargetsByRange.RangeType.Cylinder:
                info = "1";
                break;
            case PengScript.GetTargetsByRange.RangeType.Sphere:
                info = "2";
                break;
            case PengScript.GetTargetsByRange.RangeType.Box:
                info = "3";
                break;
        }
        info += ";";
        info += pengPara.value.x.ToString() + "," + pengPara.value.y.ToString() + "," + pengPara.value.z.ToString() + ";";
        info += pengOffset.value.x.ToString() + "," + pengOffset.value.y.ToString() + "," + pengOffset.value.z.ToString() + ";";
        info += pengCamp.value.ToString();
        return info;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] strFirst = info.Split(";");
            switch (int.Parse(strFirst[0]))
            {
                case 1:
                    rangeType = PengScript.GetTargetsByRange.RangeType.Cylinder;
                    break;
                case 2:
                    rangeType = PengScript.GetTargetsByRange.RangeType.Sphere;
                    break;
                case 3:
                    rangeType = PengScript.GetTargetsByRange.RangeType.Box;
                    break;
            }
            typeNum.value = (int)rangeType;
            string[] strSecond = strFirst[1].Split(",");
            pengPara.value = new Vector3(float.Parse(strSecond[0]), float.Parse(strSecond[1]), float.Parse(strSecond[2]));
            string[] strThird = strFirst[2].Split(",");
            pengOffset.value = new Vector3(float.Parse(strThird[0]), float.Parse(strThird[1]), float.Parse(strThird[2]));
            pengCamp.value = int.Parse(strFirst[3]);
        }
    }
}

public class TransState : PengNode
{
    public PengEditorVariables.PengString stateName;

    public TransState(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoints = new PengNodeConnection[1]; inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[1];
        outVars = new PengEditorVariables.PengVar[0];
        stateName = new PengEditorVariables.PengString(this, "状态名称", 0, ConnectionPointType.In);
        inVars[0] = stateName;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.TransState;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        if (varInID[0].nodeID < 0)
        {
            Rect field = new Rect(inVars[0].varRect.x + 45, inVars[0].varRect.y, 65, 18);
            stateName.value = EditorGUI.TextField(field, stateName.value);
        }
    }

    public override string SpecialParaDescription()
    {
        return stateName.value;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            stateName.value = info;
        }
    }
}

public class GlobalTimeScale : PengNode
{
    public PengEditorVariables.PengFloat timeScale;
    public PengEditorVariables.PengFloat duration;
    public GlobalTimeScale(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoints = new PengNodeConnection[1]; inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[2];
        outVars = new PengEditorVariables.PengVar[0];

        timeScale = new PengEditorVariables.PengFloat(this, "时间速度", 0, ConnectionPointType.In);
        duration = new PengEditorVariables.PengFloat(this, "持续时间", 1, ConnectionPointType.In);

        inVars[0] = timeScale;
        inVars[1] = duration;
        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.GlobalTimeScale;
        nodeName = GetDescription(scriptType);

        paraNum = 2;
    }

    public override void Draw()
    {
        base.Draw();
        for (int i = 0; i < 2; i++)
        {
            if (varInID[i].nodeID < 0)
            {
                Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 65, 18);
                switch (i)
                {
                    case 0:
                        timeScale.value = EditorGUI.FloatField(field, timeScale.value);
                        break;
                    case 1:
                        duration.value = EditorGUI.FloatField(field, duration.value);
                        break;
                }
            }
        }
    }

    public override string SpecialParaDescription()
    {
        return timeScale.value.ToString() + "," + duration.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(",");
            timeScale.value = float.Parse(str[0]);
            duration.value = float.Parse(str[1]);
        }
    }
}
public class SetBlackBoardVariables : PengNode
{
    public PengEditorVariables.PengString varName;
    public PengEditorVariables.PengT value;
    public PengEditorVariables.PengInt targetType;

    public PengScript.BBTarget BBTarget;
    public BBTargetCN BBTargetCN;

    public SetBlackBoardVariables(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoints = new PengNodeConnection[1]; inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[3];
        outVars = new PengEditorVariables.PengVar[0];
        varName = new PengEditorVariables.PengString(this, "变量名", 0, ConnectionPointType.In);
        value = new PengEditorVariables.PengT(this, "值", 1, ConnectionPointType.In);
        targetType = new PengEditorVariables.PengInt(this, "目标类型", 2, ConnectionPointType.In);
        inVars[0] = varName;
        inVars[1] = value;
        inVars[2] = targetType;
        targetType.point = null;

        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.SetBlackBoardVariables;
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
                        varName.value = EditorGUI.TextField(field, varName.value);
                        break;
                    case 1:

                        break;
                    case 2:
                        BBTarget = (BBTarget)((int)(BBTargetCN)EditorGUI.EnumPopup(field, BBTargetCN));
                        BBTargetCN = (BBTargetCN)(int)BBTarget;
                        targetType.value = (int)BBTarget;
                        break;
                }
            }
        }
    }

    public override string SpecialParaDescription()
    {
        return varName.value + "," + targetType.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(",");
            varName.value = str[0];
            targetType.value = int.Parse(str[1]);
            BBTarget = (PengScript.BBTarget)targetType.value;
            BBTargetCN = (BBTargetCN)targetType.value;
        }
    }
}

public class CustomEvent : PengNode
{
    public PengEditorVariables.PengString eventName;

    public PengEditorVariables.PengInt intMessage;
    public PengEditorVariables.PengFloat floatMessage;
    public PengEditorVariables.PengString stringMessage;
    public PengEditorVariables.PengBool boolMessage;

    public CustomEvent(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoints = new PengNodeConnection[1]; inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[5];
        outVars = new PengEditorVariables.PengVar[0];
        intMessage = new PengEditorVariables.PengInt(this, "整型参数", 1, ConnectionPointType.In);
        floatMessage = new PengEditorVariables.PengFloat(this, "浮点参数", 2, ConnectionPointType.In);
        stringMessage = new PengEditorVariables.PengString(this, "字符串参数", 3, ConnectionPointType.In);
        boolMessage = new PengEditorVariables.PengBool(this, "布尔参数", 4, ConnectionPointType.In);
        eventName = new PengEditorVariables.PengString(this, "事件名称", 0, ConnectionPointType.In);
        inVars[0] = eventName;
        inVars[1] = intMessage;
        inVars[2] = floatMessage;
        inVars[3] = stringMessage;
        inVars[4] = boolMessage;
        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.CustomEvent;
        nodeName = GetDescription(scriptType);

        paraNum = 5;
    }

    public override void Draw()
    {
        base.Draw();
        for (int i = 0; i < 5; i++)
        {
            if (varInID[i].nodeID < 0)
            {
                Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 65, 18);
                switch (i)
                {
                    case 0:
                        eventName.value = EditorGUI.TextField(field, eventName.value);
                        break;
                    case 1:
                        intMessage.value = EditorGUI.IntField(field, intMessage.value);
                        break;
                    case 2:
                        floatMessage.value = EditorGUI.FloatField(field, floatMessage.value);
                        break;
                    case 3:
                        stringMessage.value = EditorGUI.TextField(field, stringMessage.value);
                        break;
                    case 4:
                        boolMessage.value = EditorGUI.Toggle(field, boolMessage.value);
                        break;
                }
            }
        }
    }

    public override string SpecialParaDescription()
    {
        return eventName.value + "," + intMessage.value.ToString() + "," + floatMessage.value.ToString() + "," + stringMessage.value + "," + (boolMessage.value ? "1" : "0");
    }
    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(",");
            eventName.value = str[0];
            intMessage.value = int.Parse(str[1]);
            floatMessage.value = float.Parse(str[2]);
            stringMessage.value = str[3];
            boolMessage.value = int.Parse(str[4]) > 0;
        }
    }
}

public class AllowChangeDirection : PengNode
{
    public PengEditorVariables.PengFloat lerp;
    public AllowChangeDirection(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);
        this.meaning = "执行时允许读取方向控制，并进行转向。转向插值的数值越高，转向越快、平滑越少。";

        inPoints = new PengNodeConnection[1]; 
        inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[1];
        outVars = new PengEditorVariables.PengVar[0];

        lerp = new PengEditorVariables.PengFloat(this, "转向插值", 0, ConnectionPointType.In);

        inVars[0] = lerp;
        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.AllowChangeDirection;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        for (int i = 0; i < 1; i++)
        {
            if (varInID[i].nodeID < 0)
            {
                Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 65, 18);
                switch (i)
                {
                    case 0:
                        lerp.value = EditorGUI.Slider(field, lerp.value, 0f, 1f);
                        break;
                }
            }
        }
    }

    public override string SpecialParaDescription()
    {
        return lerp.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            lerp.value = float.Parse(info);
        }
    }
}

public class JumpForce : PengNode
{
    public PengEditorVariables.PengFloat force;
    public JumpForce(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);
        this.meaning = "修改瞬时跳跃力。一般放在跳跃状态的Enter帧，执行一次即可，建议值为10以上。";

        inPoints = new PengNodeConnection[1];
        inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);
        inVars = new PengEditorVariables.PengVar[1];
        outVars = new PengEditorVariables.PengVar[0];

        force = new PengEditorVariables.PengFloat(this, "跳跃力", 0, ConnectionPointType.In);

        inVars[0] = force;
        ReadSpecialParaDescription(specialInfo);
        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.JumpForce;
        nodeName = GetDescription(scriptType);

        paraNum = 1;
    }

    public override void Draw()
    {
        base.Draw();
        for (int i = 0; i < 1; i++)
        {
            if (varInID[i].nodeID < 0)
            {
                Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 65, 18);
                switch (i)
                {
                    case 0:
                        force.value = EditorGUI.FloatField(field, force.value);
                        break;
                }
            }
        }
    }

    public override string SpecialParaDescription()
    {
        return force.value.ToString();
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            force.value = float.Parse(info);
        }
    }
}