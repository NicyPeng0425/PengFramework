using PengScript;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class PlayAnimation : PengNode
{
    public PengEditorVariables.PengString pengAnimationName;
    public PengEditorVariables.PengBool pengHardCut;
    public PengEditorVariables.PengFloat pengTransitionNormalizedTime;
    public PengEditorVariables.PengFloat pengStartAtNormalizedTime;
    public PengEditorVariables.PengInt pengAnimationLayer;

    public PlayAnimation(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
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
        pengAnimationName = new PengEditorVariables.PengString(this, "动画名称", 0, ConnectionPointType.In);
        pengHardCut = new PengEditorVariables.PengBool(this, "是否硬切", 1, ConnectionPointType.In);
        pengTransitionNormalizedTime = new PengEditorVariables.PengFloat(this, "过渡时间", 2, ConnectionPointType.In);
        pengStartAtNormalizedTime = new PengEditorVariables.PengFloat(this, "开始时间", 3, ConnectionPointType.In);
        pengAnimationLayer = new PengEditorVariables.PengInt(this, "动画层", 4, ConnectionPointType.In);
        inVars[0] = pengAnimationName;
        inVars[1] = pengHardCut;
        inVars[2] = pengTransitionNormalizedTime;
        inVars[3] = pengStartAtNormalizedTime;
        inVars[4] = pengAnimationLayer;

        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.PlayAnimation;
        nodeName = GetDescription(scriptType);

        paraNum = 5;
    }

    public override void Draw()
    {
        base.Draw();
    }
}

public class PlayEffects : PengNode
{
    public PengEditorVariables.PengString effectPath;
    public PengEditorVariables.PengBool follow;
    public PengEditorVariables.PengVector3 posOffset;
    public PengEditorVariables.PengVector3 rotOffset;
    public PengEditorVariables.PengVector3 scaleOffset;
    public PengEditorVariables.PengFloat deleteTime;

    public ParticleSystem ps;
    public string oldPath;
    public PlayEffects(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);

        inPoints = new PengNodeConnection[2]; 
        inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        inPoints[1] = new PengNodeConnection(ConnectionPointType.FlowIn, 1, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);

        inVars = new PengEditorVariables.PengVar[6];
        outVars = new PengEditorVariables.PengVar[0];

        effectPath = new PengEditorVariables.PengString(this, "特效路径", 0, ConnectionPointType.In);
        follow = new PengEditorVariables.PengBool(this, "跟随？", 1, ConnectionPointType.In);
        posOffset = new PengEditorVariables.PengVector3(this, "位置偏移", 2, ConnectionPointType.In);
        rotOffset = new PengEditorVariables.PengVector3(this, "旋转偏移", 3, ConnectionPointType.In);
        scaleOffset = new PengEditorVariables.PengVector3(this, "缩放偏移", 4, ConnectionPointType.In);
        deleteTime = new PengEditorVariables.PengFloat(this, "删除时间", 5, ConnectionPointType.In);

        effectPath.value = "Effects/";
        scaleOffset.value = Vector3.one;
        effectPath.point = null;
        follow.point = null;
        posOffset.point = null;
        rotOffset.point = null;
        scaleOffset.point = null;
        deleteTime.point = null;
        oldPath = "";
        ps = null;

        ReadSpecialParaDescription(specialInfo);

        inVars[0] = effectPath;
        inVars[1] = follow;
        inVars[2] = posOffset;
        inVars[3] = rotOffset;
        inVars[4] = scaleOffset;
        inVars[5] = deleteTime;

        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.PlayEffects;
        nodeName = GetDescription(scriptType);

        paraNum = 6;
    }

    public override void Draw()
    {
        base.Draw();
        GUIStyle style2 = new GUIStyle("CN EntryInfo");
        style2.fontSize = 12;
        style2.alignment = TextAnchor.UpperLeft;
        style2.fontStyle = FontStyle.Bold;
        style2.normal.textColor = Color.white;
        Rect enter = new Rect(inPoints[0].rect.x - 10, inPoints[0].rect.y, 70, 20);
        Rect breakin = new Rect(inPoints[1].rect.x - 10, inPoints[1].rect.y, 70, 20);
        GUI.Box(enter, "播放", style2);
        GUI.Box(breakin, "停止", style2);
        for (int i = 0; i < paraNum; i++)
        {
            Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 40, 18);
            Rect longField = new Rect(field.x, field.y, field.width + 140, field.height);
            switch (i)
            {
                case 0:
                    effectPath.value = EditorGUI.TextField(longField, effectPath.value);
                    break;
                case 1:
                    follow.value = EditorGUI.Toggle(field, follow.value);
                    break;
                case 2:
                    posOffset.value = EditorGUI.Vector3Field(longField, "", posOffset.value);
                    break;
                case 3:
                    rotOffset.value = EditorGUI.Vector3Field(longField, "", rotOffset.value);
                    break;
                case 4:
                    scaleOffset.value = EditorGUI.Vector3Field(longField, "", scaleOffset.value);
                    break;
                case 5:
                    deleteTime.value = EditorGUI.FloatField(field, deleteTime.value);
                    break;
            }
        }
        UpdateParticle();
    }

    public void UpdateParticle()
    {
        if (File.Exists(Application.dataPath + "Resources/Effects/" + effectPath.value))
        {
            if (ps == null)
            {
                GameObject psGO = Resources.Load("Effects/" + effectPath.value) as GameObject;
                if (psGO.GetComponent<ParticleSystem>() != null)
                {
                    ps = GameObject.Instantiate(psGO).GetComponent<ParticleSystem>();
                    oldPath = Application.dataPath + "Resources/Effects/" + effectPath.value;
                }
            }
            else
            {
                if (oldPath != Application.dataPath + "Resources/Effects/" + effectPath.value)
                {
                    GameObject psGO = Resources.Load("Effects/" + effectPath.value) as GameObject;
                    if (psGO.GetComponent<ParticleSystem>() != null)
                    {
                        GameObject.DestroyImmediate(ps.gameObject);
                        ps = GameObject.Instantiate(psGO).GetComponent<ParticleSystem>();
                        oldPath = Application.dataPath + "Resources/Effects/" + effectPath.value;
                    }
                }
            }
        }
    }

    public override string SpecialParaDescription()
    {
        string result = effectPath.value + ";" + (follow.value ? "1" : "0") + ";" + BaseScript.ParseVector3ToString(posOffset.value) + ";" + BaseScript.ParseVector3ToString(rotOffset.value) + ";" + BaseScript.ParseVector3ToString(scaleOffset.value) + ";" + deleteTime.value.ToString();
        return result;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(';');
            effectPath.value = str[0];
            follow.value = int.Parse(str[1]) > 0;
            posOffset.value = BaseScript.ParseStringToVector3(str[2]);
            rotOffset.value = BaseScript.ParseStringToVector3(str[3]);
            scaleOffset.value = BaseScript.ParseStringToVector3(str[4]);
            deleteTime.value = float.Parse(str[5]);
            UpdateParticle();
        }
    }
}