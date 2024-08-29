﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public partial class PengActorControl : MonoBehaviour
{
    public void LoadActorAI()
    {
        TextAsset textAsset = (TextAsset)Resources.Load("AIs/" + actor.actorID.ToString() + "/" + actor.actorID.ToString());
        if (textAsset == null)
        {
            Debug.LogWarning("Actor" + actor.actorID.ToString() + "的AI逻辑获取失败！");
            return;
        }

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(textAsset.text);
        XmlElement root = doc.DocumentElement;
        XmlNodeList childs = root.ChildNodes;

        XmlElement aiInfo = null;
        XmlElement aiScript = null;
        foreach (XmlElement ele in childs)
        {
            if (ele.Name == "ActorAIInfo")
            {
                aiInfo = ele;
                continue;
            }
            if (ele.Name == "ActorAIScript")
            {
                aiScript = ele;
                continue;
            }
        }

        if (aiInfo == null || aiInfo.ChildNodes.Count == 0)
        {
            Debug.LogError(actor.actorID.ToString() + "的AI数据里没有AI信息！怎么回事呢？");
            return;
        }

        if (aiScript == null || aiScript.ChildNodes.Count == 0)
        {
            Debug.LogError(actor.actorID.ToString() + "的AI数据里没有AI脚本！怎么回事呢？");
            return;
        }

        XmlNodeList infoChilds = aiInfo.ChildNodes;
        /*
        foreach (XmlElement ele in infoChilds)
        {
            if (ele.Name == "ID")
            {
                //读取ID
                this.actor.actorID = int.Parse(ele.GetAttribute("ActorID"));
                continue;
            }
        }*/

        XmlNodeList scriptChild = aiScript.ChildNodes;

        foreach (XmlElement ele in scriptChild)
        {
            PengAIScript.AIScriptType type = (PengAIScript.AIScriptType)Enum.Parse(typeof(PengAIScript.AIScriptType), ele.GetAttribute("ScriptType"));
            if (type == PengAIScript.AIScriptType.EventDecide)
            {
                int id = int.Parse(ele.GetAttribute("ScriptID"));
                string flowInfo = ele.GetAttribute("OutID");
                string info = ele.GetAttribute("SpecialInfo");
                scripts.Add(id, ConstructFunctions(type, id, flowInfo, info));
            }
        }

        foreach (XmlElement ele in scriptChild)
        {
            PengAIScript.AIScriptType type = (PengAIScript.AIScriptType)Enum.Parse(typeof(PengAIScript.AIScriptType), ele.GetAttribute("ScriptType"));
            if (type != PengAIScript.AIScriptType.EventDecide)
            {
                int id = int.Parse(ele.GetAttribute("ScriptID"));
                string flowInfo = ele.GetAttribute("OutID");
                string info = ele.GetAttribute("SpecialInfo");
                scripts.Add(id, ConstructFunctions(type, id, flowInfo, info));
            }
        }
    }

    public PengAIScript.PengAIBaseScript ConstructFunctions(PengAIScript.AIScriptType type, int ID, string flowOutInfo, string specialInfo)
    {
        switch (type)
        {
            default:
                return null;
            case PengAIScript.AIScriptType.EventDecide:
                return new PengAIScript.DecideEvent(this, ID, flowOutInfo, specialInfo);
        }
    }
}
