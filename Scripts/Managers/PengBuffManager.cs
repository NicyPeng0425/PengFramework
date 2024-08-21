using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class PengBuffManager: MonoBehaviour
{
    [HideInInspector]
    public PengGameManager gameOwner;
    [HideInInspector]
    public PengActor actorOwner;
    [HideInInspector]
    public List<PengBuff> buffs = new List<PengBuff>();

    public Dictionary<int, XmlElement> buffDic = new Dictionary<int, XmlElement>(2000);

    private void Awake()
    {
        
    }

    void Start()
    {
        if (gameOwner != null)
        {
            TextAsset ta = Resources.Load("BuffData/Universal/BuffTable") as TextAsset;
            XmlDocument result = new XmlDocument();
            if (ta == null)
                return;
            result.LoadXml(ta.text);
            XmlElement root = result.DocumentElement;
            if (root.ChildNodes.Count > 0)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    XmlElement ele = root.ChildNodes[i] as XmlElement;
                    buffDic.Add(int.Parse(ele.GetAttribute("ID")), ele);
                }
                Debug.Log("共加载" + buffDic.Count.ToString() + "个Buff。");
            }
        }
    }

    void Update()
    {
        if (buffs.Count > 0)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                buffs[i].OnUpdate();
            }
        }
    }

    public void AddBuff(int ID)
    {
        bool hasSame = false;
        if (buffs.Count > 0)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i].ID == ID)
                {
                    hasSame = true;
                    buffs[i].stack++;
                    buffs[i].OnAdd();
                    break;
                }
            }
        }
        if (!hasSame)
        {
            PengBuff buff = new PengBuff(ID, this);
            buffs.Add(buff);
            buff.OnAdd();
        }
    }

    public void RemoveBuff(int ID)
    {
        List<PengBuff> toRemove = new List<PengBuff>();
        foreach (PengBuff buff in buffs)
        {
            if (buff.ID == ID)
            {
                toRemove.Add(buff);
                if (!buff.removeOnceForAll)
                {
                    break;
                }
            }
        }
        if (toRemove.Count > 0)
        {
            foreach (PengBuff buff in toRemove)
            {
                buff.stack--;
            }
            foreach (PengBuff buff in toRemove)
            {
                buff.OnRemove();
            }
        }
        else
        {
            Debug.LogWarning("试图移除ID为" + ID.ToString() + "的Buff，但并未找到。");
        }
        
    }

    public void RemoveCertainBuff(PengBuff buff)
    {
        buffs.Remove(buff);
        buff.OnRemove();
    }

    public void RemoveAllBuff()
    {
        List<PengBuff> toRemove = new List<PengBuff>();
        foreach (PengBuff buff in buffs)
        {
            if (buff.removeConditionType != PengBuff.RemoveConditionType.Permanent)
            {
                toRemove.Add(buff);
            }
        }
        buffs.Clear();
        foreach (PengBuff buff in toRemove)
        {
            buff.OnRemove();
        }
    }

    public void RemoveAllBadDeBuff()
    {
        List<PengBuff> toRemove = new List<PengBuff>();
        foreach (PengBuff buff in buffs)
        {
            if (buff.isDebuff && buff.removeConditionType != PengBuff.RemoveConditionType.Permanent)
            {
                toRemove.Add(buff);
            }
        }
        foreach (PengBuff buff in toRemove)
        {
            buffs.Remove(buff);
        }
        foreach (PengBuff buff in toRemove)
        {
            buff.OnRemove();
        }
    }

    public void RemoveAllGoodBuff()
    {
        List<PengBuff> toRemove = new List<PengBuff>();
        foreach (PengBuff buff in buffs)
        {
            if (!buff.isDebuff && buff.removeConditionType != PengBuff.RemoveConditionType.Permanent)
            {
                toRemove.Add(buff);
            }
        }
        foreach (PengBuff buff in toRemove)
        {
            buffs.Remove(buff);
        }
        foreach (PengBuff buff in toRemove)
        {
            buff.OnRemove();
        }
    }

    public void ProcessStateEnd()
    {
        if (buffs.Count > 0)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                buffs[i].OnStateEnd();
            }
        }
    }

    public void ProcessStateChange(bool actively)
    {
        if (buffs.Count > 0)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                buffs[i].OnStateChange(actively);
            }
        }
    }
}

/// <summary>
/// Buff能达到的功能
/// 1. 永久存在
/// 2. 被移除
/// 3. 被检索
/// 4. 能多层存在，可以设上限
/// 5. 能持续一段时间后消散
/// 6. 多层存在时，能持续一段时间后全部消散，也能一层一层地消散
/// 7. 能在动作段被动结束后消散
/// 8. 能在动作段被动结束后消散
/// 9. 标识为增益或减益
/// 10. 能执行行为：减少CD，恢复血量，减少血量，刷新CD，必杀，改变属性，禁用输入，改变UI，改变技能...
/// 11. 冲突关系
/// 12. 能标识自身的特殊存在
/// </summary>

public class PengBuff
{
    public int ID;
    public string Name;
    public string Description;
    public bool isDebuff;
    public PengBuffManager master;
    public PengGameManager gameOwner;
    public PengActor actorOwner;

    public enum RemoveConditionType
    {
        Permanent,
        StateEnd,
        Time,
        Contrast,
    }


    public float existTime;
    public float existTimeCount;

    public RemoveConditionType removeConditionType;
    public int stackLimit;
    public bool removeOnceForAll;

    //属性加成
    public float attackPowerValue;
    public float attackPowerPercent;
    public float defendPowerValue;
    public float defendPowerPercent;
    public float criticalRateValue;
    public float criticalDamageRatioValue;
    public bool notEffectedByGravity;
    public bool unBreakable;
    public bool invincible;

    int m_stack;
    public int stack
    {
        get { return m_stack; }
        set 
        { 
            if (value <= stackLimit)
            {
                m_stack = value;
            }
            if (m_stack <= 0)
            {
                if (removeConditionType != RemoveConditionType.Permanent)
                {
                    master.RemoveCertainBuff(this);
                }
                else
                {
                    m_stack = 1;
                }
            }
        }
    }

    public PengBuff(int ID, PengBuffManager master)
    {
        this.ID = ID;
        this.master = master;
        if(master.gameOwner != null) { this.gameOwner =  master.gameOwner; }
        else if(master.actorOwner != null) { this.actorOwner = master.actorOwner; }
        ConstructBuffByID(this.ID);
    }

    public virtual void OnAdd()
    {
        if (removeConditionType == RemoveConditionType.Time)
        {
            existTimeCount = existTime;
        }
    }

    public virtual void OnUpdate()
    {
        if (removeConditionType == RemoveConditionType.Time)
        {
            if (existTimeCount > 0)
            {
                existTimeCount -= Time.deltaTime;
                if (existTimeCount <= 0)
                {
                    if (removeOnceForAll)
                    {
                        stack = 0;
                    }
                    else
                    {
                        stack--;
                    }
                }
            }
        }
    }

    public virtual void OnRemove()
    {

    }

    public virtual void OnStateEnd()
    {
        if (removeConditionType == RemoveConditionType.StateEnd)
        {
            stack--;
        }
    }

    public virtual void OnStateChange(bool actively)
    {

    }

    public void ConstructBuffByID(int id)
    {
        if (gameOwner.buff.buffDic.ContainsKey(id))
        {
            XmlElement ele = gameOwner.buff.buffDic[id];
            ID = int.Parse(ele.GetAttribute("ID"));
            Name = ele.GetAttribute("Name");
            Description = ele.GetAttribute("Description");
            isDebuff = int.Parse(ele.GetAttribute("IsDebuff")) > 0;
            existTime = float.Parse(ele.GetAttribute("ExistTime"));
            removeConditionType = (RemoveConditionType)Enum.Parse(typeof(RemoveConditionType), ele.GetAttribute("RemoveCondition"));
            stackLimit = int.Parse(ele.GetAttribute("StackLimit"));
            removeOnceForAll = int.Parse(ele.GetAttribute("RemoveOnceForAll")) > 0;
            attackPowerValue = float.Parse(ele.GetAttribute("AttackPowerValue"));
            attackPowerPercent = float.Parse(ele.GetAttribute("AttackPowerPercent"));
            defendPowerValue = float.Parse(ele.GetAttribute("DefendPowerValue"));
            defendPowerPercent = float.Parse(ele.GetAttribute("DefendPowerPercent"));
            criticalRateValue = float.Parse(ele.GetAttribute("CriticalRateValue"));
            criticalDamageRatioValue = float.Parse(ele.GetAttribute("CriticalDamageRatioValue"));
            notEffectedByGravity = int.Parse(ele.GetAttribute("NotEffectedByGravity")) > 0;
            unBreakable = int.Parse(ele.GetAttribute("Unbreakable")) > 0;
            invincible = int.Parse(ele.GetAttribute("Invicible")) > 0;
        }
        else
        {
            Debug.LogWarning("不存在ID为" + id.ToString() + "的Buff，添加Buff失败。");
        }

    }
}

public class BuffBehavior
{
    public PengBuff owner;
}
