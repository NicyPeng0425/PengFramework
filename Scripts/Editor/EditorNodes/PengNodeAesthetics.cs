using PengScript;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using System;
using System.Reflection;

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
        meaning = "播放一个动画。\n" +
            "动画名称需填入Animator内的动画状态的名称，而不是动画Clip的名称。\n" +
            "过渡时间、开始时间需填入一个范围从0到1的归一化浮点值。\n" +
            "默认的动画层为0。";

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
        meaning = "播放一个特效。\n" +
            "特效路径填入其在Resources/Effects文件夹下的相对地址即可。例如，我想播放一个地址在Assets/Resources/Effects/100425/Slash的特效，那么我只需要填入100425/Slash即可。";

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

public class PlayAudio : PengNode
{
    public PengEditorVariables.PengString soundPath;
    public PengEditorVariables.PengFloat soundVol;

    public string oldPath;

    public List<AudioClip> clipList;
    public List<string> pathList;
    public AudioSource source;
    public PlayAudio(Vector2 pos, PengActorStateEditorWindow master, ref PengEditorTrack trackMaster, int nodeID, string outID, string varOutID, string varInID, string specialInfo)
    {
        InitialDraw(pos, master);
        this.trackMaster = trackMaster;
        this.nodeID = nodeID;
        this.outID = ParseStringToDictionaryIntNodeIDConnectionID(outID);
        this.varOutID = ParseStringToDictionaryIntListNodeIDConnectionID(varOutID);
        this.varInID = ParseStringToDictionaryIntNodeIDConnectionID(varInID);
        meaning = "播放一个音效。\n" +
            "音效路径填入其在Resources/Sounds文件夹下的相对地址即可。\n" +
            "需要注意的是，此处需填入文件夹的相对地址，然后会从该文件夹下随机选择一个音效播放。\n" +
            "例如，我想播放一个地址在Assets/Resources/Sounds/100425/HitSound文件夹下的音效，那么我只需要填入100425/HitSound即可。";

        inPoints = new PengNodeConnection[1];
        inPoints[0] = new PengNodeConnection(ConnectionPointType.FlowIn, 0, this, null);
        outPoints = new PengNodeConnection[1];
        outPoints[0] = new PengNodeConnection(ConnectionPointType.FlowOut, 0, this, null);

        inVars = new PengEditorVariables.PengVar[2];
        outVars = new PengEditorVariables.PengVar[0];

        soundPath = new PengEditorVariables.PengString(this, "音效文件夹", 0, ConnectionPointType.In);
        soundVol = new PengEditorVariables.PengFloat(this, "音轨声量", 1, ConnectionPointType.In);

        soundPath.value = "Sounds/";
        soundVol.value = 1;
        soundPath.point = null;
        soundVol.point = null;
        oldPath = "";
        clipList = new List<AudioClip>();
        pathList = new List<string>();

        ReadSpecialParaDescription(specialInfo);

        inVars[0] = soundPath;
        inVars[1] = soundVol;
        LoadSound();

        type = NodeType.Action;
        scriptType = PengScript.PengScriptType.PlayAudio;
        nodeName = GetDescription(scriptType);

        paraNum = 2;
    }

    public override void Draw()
    {
        base.Draw();
        for (int i = 0; i < paraNum; i++)
        {
            Rect field = new Rect(inVars[i].varRect.x + 45, inVars[i].varRect.y, 40, 18);
            Rect longField = new Rect(field.x, field.y, field.width + 140, field.height);
            switch (i)
            {
                case 0:
                    oldPath = soundPath.value;
                    soundPath.value = EditorGUI.TextField(longField, soundPath.value);
                    break;
                case 1:
                    soundVol.value = EditorGUI.Slider(field, soundVol.value, 0, 2);
                    break;
            }
        }
        if (soundPath.value != oldPath)
        {
            LoadSound();
        }
        if (!EditorApplication.isPlaying)
        {
            Rect play = new Rect(soundPath.varRect.x + 180, soundPath.varRect.y - 27, 40, 20);
            if (GUI.Button(play, "试听"))
            {
                PlayInEdtior();
            }
        }
    }

    public void PlayInEdtior()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }

        if (clipList != null && clipList.Count == 0)
        {

            LoadSound();
        }

        if (source == null)
        {
            source = GameObject.FindWithTag("PengGameManager").GetComponent<AudioSource>();
        }

        if (clipList != null && clipList.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, clipList.Count);

            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] 
                {
                    typeof(AudioClip), typeof(int), typeof(bool)
                },
                null
                );
            method.Invoke(
                null,
                new object[] 
                {
                    clipList[index], 0, false
                }
            );
        }
    }

    public void LoadSound()
    {
        if (source == null)
        {
            source = GameObject.FindWithTag("PengGameManager").GetComponent<AudioSource>();
        }

        if (!Directory.Exists(Application.dataPath + "/Resources/Sounds/" + soundPath.value))
            return;
        DirectoryInfo direct = new DirectoryInfo(Application.dataPath + "/Resources/Sounds/" + soundPath.value);
        FileInfo[] files = direct.GetFiles();
        if (files != null && files.Length > 0)
        {
            clipList.Clear();
            pathList.Clear();
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".meta"))
                {
                    continue;
                }
                if (files[i].Name.EndsWith(".mp3") || files[i].Name.EndsWith(".wav"))
                {
                    string name = files[i].Name;
                    string[] noPost = name.Split(".");
                    clipList.Add(Resources.Load<AudioClip>("Sounds/" + soundPath.value + "/" + noPost[0]));
                    pathList.Add("Sounds/" + soundPath.value + "/" + noPost[0]);
                }
            }
        }
    }

    public override string SpecialParaDescription()
    {
        string paths = "";
        if (pathList.Count > 0)
        {
            
            for (int i = 0; i < pathList.Count; i++)
            {
                paths += pathList[i];
                if (i != pathList.Count - 1)
                {
                    paths += ",";
                }
            }
        }
        else
        {
            paths = "null";
        }
        string result = soundPath.value + ";" + soundVol.value + ";" + paths;
        return result;
    }

    public override void ReadSpecialParaDescription(string info)
    {
        if (info != "")
        {
            string[] str = info.Split(';');
            soundPath.value = str[0];
            soundVol.value = float.Parse(str[1]);
            oldPath = soundPath.value;
        }
    }
}