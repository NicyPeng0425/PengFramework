using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengBlackBoard<T>
{
    public T owner;

    public Dictionary<string, int> intBB;
    public Dictionary<string, float> floatBB;
    public Dictionary<string, bool> boolBB;
    public Dictionary<string, string> stringBB;
    public Dictionary<string, PengActor> pengActorBB;
    public Dictionary<string, List<PengActor>> pengActorListBB;
    public PengBlackBoard(T owner)
    {
        this.owner = owner;
        intBB = new Dictionary<string, int>();
        floatBB = new Dictionary<string, float>();
        boolBB = new Dictionary<string, bool>();
        stringBB = new Dictionary<string, string>();
        pengActorBB = new Dictionary<string, PengActor>();
        pengActorListBB = new Dictionary<string, List<PengActor>>();
    }

    public void SetBBVar(string name, int value)
    {
        if (intBB.ContainsKey(name)){ intBB[name] = value; }
        else { intBB.Add(name, value); }
        
    }

    public void SetBBVar(string name, float value)
    {
        if (floatBB.ContainsKey(name)) { floatBB[name] = value; }
        else { floatBB.Add(name, value); }
    }

    public void SetBBVar(string name, bool value)
    {
        if (boolBB.ContainsKey(name)) { boolBB[name] = value; }
        else { boolBB.Add(name, value); }
    }

    public void SetBBVar(string name, string value)
    {
        if (stringBB.ContainsKey(name)) { stringBB[name] = value; }
        else { stringBB.Add(name, value); }
    }

    public void SetBBVar(string name, PengActor value)
    {
        if (pengActorBB.ContainsKey(name)) { pengActorBB[name] = value; }
        else { pengActorBB.Add(name, value); }
    }

    public void SetBBVar(string name, List<PengActor> value)
    {
        if (pengActorListBB.ContainsKey(name)) { pengActorListBB[name] = value; }
        else { pengActorListBB.Add(name, value); }
    }
    /// <summary>
    /// ���ûȡ�����᷵��-999999��6��9��
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetBBInt(string name)
    {
        if (intBB.ContainsKey(name)) { return intBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "�ĺڰ���ûȡ����Ϊ" + name +"�ı������������Ͳ���Ϊ�գ��ʷ���-999999"); return -999999; }
    }

    /// <summary>
    /// ���ûȡ�����᷵��-999999��6��9��
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public float GetBBFloat(string name)
    {
        if (floatBB.ContainsKey(name)) { return floatBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "�ĺڰ���ûȡ����Ϊ" + name + "�ı��������ڸ��㲻��Ϊ�գ��ʷ���-999999"); return -999999; }
    }

    /// <summary>
    /// ���ûȡ�����᷵��false
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool GetBBBool(string name)
    {
        if (boolBB.ContainsKey(name)) { return boolBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "�ĺڰ���ûȡ����Ϊ" + name + "�ı��������ڲ�������Ϊ�գ��ʷ���false"); return false; }
    }

    /// <summary>
    /// ���ûȡ�����᷵��null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetBBString(string name)
    {
        if (stringBB.ContainsKey(name)) { return stringBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "�ĺڰ���ûȡ����Ϊ" + name + "�ı������ʷ���null"); return null; }
    }

    /// <summary>
    /// ���ûȡ�����᷵��null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public PengActor GetBBPengActor(string name)
    {
        if (pengActorBB.ContainsKey(name)) { return pengActorBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "�ĺڰ���ûȡ����Ϊ" + name + "�ı������ʷ���null"); return null; }
    }

    /// <summary>
    /// ���ûȡ�����᷵��null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public List<PengActor> GetBBPengActorList(string name)
    {
        if (pengActorListBB.ContainsKey(name)) { return pengActorListBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "�ĺڰ���ûȡ����Ϊ" + name + "�ı������ʷ���null"); return null; }
    }
}
