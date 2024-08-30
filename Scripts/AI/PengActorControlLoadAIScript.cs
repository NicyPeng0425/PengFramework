using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public partial class PengActorControl : MonoBehaviour
{
    public struct AIAttribute
    {
        public float chaseDistance;
        public float chaseStopDistance;
        public float decideCD;
        public float visibleDistance;
        public float visibleHeight;
        public float visibleAngle;
    }
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

        bool hasAttr = false;
        AIAttribute attr = new AIAttribute();
        foreach (XmlElement ele in infoChilds)
        {
            if (ele.Name == "ID")
            {
                //读取ID
                this.actor.actorID = int.Parse(ele.GetAttribute("ActorID"));
                continue;
            }
            if (ele.Name == "Attribute")
            {
                hasAttr = true;
                attr.chaseDistance = float.Parse(ele.GetAttribute("ChaseDistance"));
                attr.chaseStopDistance = float.Parse(ele.GetAttribute("ChaseStopDistance"));
                attr.decideCD = float.Parse(ele.GetAttribute("DecideCD"));
                attr.visibleDistance = float.Parse(ele.GetAttribute("VisibleDistance"));
                attr.visibleHeight = float.Parse(ele.GetAttribute("VisibleHeight"));
                attr.visibleAngle = float.Parse(ele.GetAttribute("VisibleAngle"));
            }
        }

        if (!hasAttr) 
        {
            attr.chaseDistance = 10f;
            attr.chaseStopDistance = 3f;
            attr.decideCD = 2f;
            attr.visibleDistance = 15f;
            attr.visibleHeight = 3f;
            attr.visibleAngle = 180f;
        }

        chaseDistance = attr.chaseDistance;
        chaseStopDistance = attr.chaseStopDistance;
        decideCD = attr.decideCD;
        visibleDistance = attr.visibleDistance;
        visibleAngle = attr.visibleAngle;
        visibleHeight = attr.visibleHeight;

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
            case PengAIScript.AIScriptType.Condition:
                return new PengAIScript.Condition(this, ID, flowOutInfo, specialInfo);
            case PengAIScript.AIScriptType.Empty:
                return new PengAIScript.Empty(this, ID, flowOutInfo, specialInfo);
            case PengAIScript.AIScriptType.InputAction:
                return new PengAIScript.InputAction(this, ID, flowOutInfo, specialInfo);
            case PengAIScript.AIScriptType.ReduceDecideGap:
                return new PengAIScript.ReduceDecideGap(this, ID, flowOutInfo, specialInfo);
            case PengAIScript.AIScriptType.Sequence:
                return new PengAIScript.Sequence(this, ID, flowOutInfo, specialInfo);
            case PengAIScript.AIScriptType.Random:
                return new PengAIScript.Random(this, ID, flowOutInfo, specialInfo);
        }
    }
}
