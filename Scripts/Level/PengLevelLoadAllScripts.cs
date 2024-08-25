using PengScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public partial class PengLevel : MonoBehaviour
{
    public void LoadAllScripts(int levelID)
    {
        string path = "Plot/" + levelID.ToString() + "/" + levelID.ToString();
        TextAsset textAsset = (TextAsset)Resources.Load(path);
        if (textAsset == null)
        {
            Debug.LogError(levelID.ToString() + "的数据读取失败！怎么回事呢？");
            return;
        }
        XmlDocument doc = new XmlDocument();
        XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", "");
        doc.LoadXml(textAsset.text);

        XmlElement root = doc.DocumentElement;
        XmlNodeList childs = root.ChildNodes;

        XmlElement levelInfo = null;
        XmlElement levelScript = null;
        foreach (XmlElement ele in childs)
        {
            if (ele.Name == "LevelInfo")
            {
                levelInfo = ele;
                continue;
            }
            if (ele.Name == "LevelScript")
            {
                levelScript = ele;
                continue;
            }
        }

        if (levelInfo == null || levelInfo.ChildNodes.Count == 0)
        {
            Debug.LogError(levelID.ToString() + "的关卡数据里没有关卡信息！怎么回事呢？");
            return;
        }

        if (levelScript == null || levelScript.ChildNodes.Count == 0)
        {
            Debug.LogError(levelID.ToString() + "的关卡数据里没有关卡脚本！怎么回事呢？");
            return;
        }

        XmlNodeList infoChilds = levelInfo.ChildNodes;
        foreach (XmlElement ele in infoChilds)
        {
            if (ele.Name == "ID")
            {
                //读取ID
                this.levelID = int.Parse(ele.GetAttribute("LevelID"));
                continue;
            }
        }

        XmlNodeList scriptChild = levelScript.ChildNodes;

        foreach (XmlElement ele in scriptChild)
        {
            PengLevelRuntimeFunction.LevelFunctionType type = (PengLevelRuntimeFunction.LevelFunctionType)Enum.Parse(typeof(PengLevelRuntimeFunction.LevelFunctionType), ele.GetAttribute("ScriptType"));

            if (type == PengLevelRuntimeFunction.LevelFunctionType.Start)
            {
                int id = int.Parse(ele.GetAttribute("ScriptID"));
                string flowInfo = ele.GetAttribute("OutID");
                string varInInfo = ele.GetAttribute("VarInID");
                string info = ele.GetAttribute("SpecialInfo");
                scripts.Add(id, ConstructFunctions(type, id, flowInfo, varInInfo, info));
            }
        }

        foreach (XmlElement ele in scriptChild)
        {
            PengLevelRuntimeFunction.LevelFunctionType type = (PengLevelRuntimeFunction.LevelFunctionType)Enum.Parse(typeof(PengLevelRuntimeFunction.LevelFunctionType), ele.GetAttribute("ScriptType"));
            if (type != PengLevelRuntimeFunction.LevelFunctionType.Start)
            {
                int id = int.Parse(ele.GetAttribute("ScriptID"));
                string flowInfo = ele.GetAttribute("OutID");
                string varInInfo = ele.GetAttribute("VarInID");
                string info = ele.GetAttribute("SpecialInfo");
                scripts.Add(id, ConstructFunctions(type, id, flowInfo, varInInfo, info));
            }
        }
    }

    public PengLevelRuntimeFunction.BaseScript ConstructFunctions(PengLevelRuntimeFunction.LevelFunctionType type, int ID, string flowOutInfo, string varInInfo, string specialInfo)
    {
        switch (type)
        {
            default:
                return null;
            case PengLevelRuntimeFunction.LevelFunctionType.Start:
                return new PengLevelRuntimeFunction.LevelStart(this, ID, flowOutInfo, varInInfo, specialInfo);
            case PengLevelRuntimeFunction.LevelFunctionType.GenerateActor:
                return new PengLevelRuntimeFunction.GenerateActor(this, ID, flowOutInfo, varInInfo, specialInfo);
            case PengLevelRuntimeFunction.LevelFunctionType.SetMainActor:
                return new PengLevelRuntimeFunction.SetMainActor(this, ID, flowOutInfo, varInInfo, specialInfo);
            case PengLevelRuntimeFunction.LevelFunctionType.TriggerWaitTime:
                return new PengLevelRuntimeFunction.TriggerWaitTime(this, ID, flowOutInfo, varInInfo, specialInfo);
        }
    }
}
