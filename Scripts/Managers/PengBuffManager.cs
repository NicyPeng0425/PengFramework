using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengBuffManager<T>: MonoBehaviour
{

    public T owner;

    public List<PengBuff<T>> buffs;
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
        buffs.Add(new PengBuff<T>(ID, this, owner));
    }

    public void RemoveBuff(int ID)
    {
        List<PengBuff<T>> toRemove = new List<PengBuff<T>>();
        foreach (PengBuff<T> buff in buffs)
        {
            if (buff.ID == ID)
            {
                toRemove.Add(buff);
            }
        }
        foreach (PengBuff<T> buff in toRemove)
        {
            buffs.Remove(buff);
        }
    }

    public void RemoveBuff(int ID, int num)
    {
        int nu = 0;
        List<PengBuff<T>> toRemove = new List<PengBuff<T>>();
        foreach (PengBuff<T> buff in buffs)
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
        foreach (PengBuff<T> buff in toRemove)
        {
            buffs.Remove(buff);
        }
    }

    public void RemoveAllBuff()
    {
        buffs.Clear();
    }

    public void RemoveAllBadDeBuff()
    {
        List<PengBuff<T>> toRemove = new List<PengBuff<T>>();
        foreach (PengBuff<T> buff in buffs)
        {
            if (buff.isDebuff)
            {
                toRemove.Add(buff);
            }
        }
        foreach (PengBuff<T> buff in toRemove)
        {
            buffs.Remove(buff);
        }
    }

    public void RemoveAllGoodBuff()
    {
        List<PengBuff<T>> toRemove = new List<PengBuff<T>>();
        foreach (PengBuff<T> buff in buffs)
        {
            if (!buff.isDebuff)
            {
                toRemove.Add(buff);
            }
        }
        foreach (PengBuff<T> buff in toRemove)
        {
            buffs.Remove(buff);
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

public class PengBuff<T>
{
    public int ID;
    public string Name;
    public string Description;
    public bool isDebuff;
    public PengBuffManager<T> master;
    public T owner;

    public float existTime;

    public PengBuff(int ID, PengBuffManager<T> master, T owner)
    {
        this.ID = ID;
        this.master = master;
        this.owner = owner;
    }

    public virtual void OnAdd()
    {

    }

    public virtual void OnUpdate()
    {
        existTime += Time.deltaTime;
    }

    public virtual void OnRemove()
    {

    }
}