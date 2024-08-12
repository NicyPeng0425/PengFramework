using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengBuffManager: MonoBehaviour
{
    [HideInInspector]
    public PengGameManager gameOwner;
    [HideInInspector]
    public PengActor actorOwner;
    [HideInInspector]
    public List<PengBuff> buffs = new List<PengBuff>();

    public Dictionary<int, string> buffDic1 = new Dictionary<int, string>(2000);
    public Dictionary<int, string> buffDic2 = new Dictionary<int, string>(2000);
    public Dictionary<int, string> buffDic3 = new Dictionary<int, string>(2000);
    public Dictionary<int, string> buffDic4 = new Dictionary<int, string>(2000);
    public Dictionary<int, string> buffDic5 = new Dictionary<int, string>(2000);

    private void Awake()
    {
        if (gameOwner != null)
        {
            //加载Buff到五个Dic中
            //记得再写个查询buff信息的功能到Buff的构造函数里
        }
    }

    void Start()
    {
        
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
        PengBuff buff = new PengBuff(ID, this);
        buffs.Add(buff);
        buff.OnAdd();
    }

    public void RemoveBuff(int ID)
    {
        List<PengBuff> toRemove = new List<PengBuff>();
        foreach (PengBuff buff in buffs)
        {
            if (buff.ID == ID)
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

    public void RemoveBuff(int ID, int num)
    {
        int nu = 0;
        List<PengBuff> toRemove = new List<PengBuff>();
        foreach (PengBuff buff in buffs)
        {
            if (buff.ID == ID)
            {
                toRemove.Add(buff);
                nu++;
                if (nu >= num)
                {
                    break;
                }
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
            toRemove.Add(buff);
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
            if (buff.isDebuff)
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
            if (!buff.isDebuff)
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

    public enum StackType
    {
        Stackable,
        NonStackable,
    }

    public enum RemoveType
    {
        AllStack,
        OneByOne,
        AllSameTypeBuff,
    }

    public float existTime;
    public RemoveConditionType removeConditionType;
    public StackType stackType;
    public RemoveType removeType;

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

    }

    public virtual void OnUpdate()
    {
        if (removeConditionType == RemoveConditionType.Time)
        {
            if (existTime > 0)
            {
                existTime -= Time.deltaTime;
                if (existTime <= 0)
                {
                    master.RemoveCertainBuff(this);
                }
            }
        }
    }

    public virtual void OnRemove()
    {

    }

    public virtual void OnStateEnd()
    {

    }

    public virtual void OnStateChange(bool actively)
    {

    }

    public void ConstructBuffByID(int id)
    {

    }
}

public class BuffBehavior
{
    public PengBuff owner;
}
