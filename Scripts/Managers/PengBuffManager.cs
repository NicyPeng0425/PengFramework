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
            //����Buff�����Dic��
            //�ǵ���д����ѯbuff��Ϣ�Ĺ��ܵ�Buff�Ĺ��캯����
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
/// Buff�ܴﵽ�Ĺ���
/// 1. ���ô���
/// 2. ���Ƴ�
/// 3. ������
/// 4. �ܶ����ڣ�����������
/// 5. �ܳ���һ��ʱ�����ɢ
/// 6. ������ʱ���ܳ���һ��ʱ���ȫ����ɢ��Ҳ��һ��һ�����ɢ
/// 7. ���ڶ����α�����������ɢ
/// 8. ���ڶ����α�����������ɢ
/// 9. ��ʶΪ��������
/// 10. ��ִ����Ϊ������CD���ָ�Ѫ��������Ѫ����ˢ��CD����ɱ���ı����ԣ��������룬�ı�UI���ı似��...
/// 11. ��ͻ��ϵ
/// 12. �ܱ�ʶ������������
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

    //���Լӳ�
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
