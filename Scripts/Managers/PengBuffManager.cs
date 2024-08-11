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