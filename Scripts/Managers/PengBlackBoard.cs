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
    /// 如果没取到，会返回-999999（6个9）
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetBBInt(string name)
    {
        if (intBB.ContainsKey(name)) { return intBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "的黑板上没取到名为" + name +"的变量，由于整型不可为空，故返回-999999"); return -999999; }
    }

    /// <summary>
    /// 如果没取到，会返回-999999（6个9）
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public float GetBBFloat(string name)
    {
        if (floatBB.ContainsKey(name)) { return floatBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "的黑板上没取到名为" + name + "的变量，由于浮点不可为空，故返回-999999"); return -999999; }
    }

    /// <summary>
    /// 如果没取到，会返回false
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool GetBBBool(string name)
    {
        if (boolBB.ContainsKey(name)) { return boolBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "的黑板上没取到名为" + name + "的变量，由于布尔不可为空，故返回false"); return false; }
    }

    /// <summary>
    /// 如果没取到，会返回null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetBBString(string name)
    {
        if (stringBB.ContainsKey(name)) { return stringBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "的黑板上没取到名为" + name + "的变量，故返回null"); return null; }
    }

    /// <summary>
    /// 如果没取到，会返回null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public PengActor GetBBPengActor(string name)
    {
        if (pengActorBB.ContainsKey(name)) { return pengActorBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "的黑板上没取到名为" + name + "的变量，故返回null"); return null; }
    }

    /// <summary>
    /// 如果没取到，会返回null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public List<PengActor> GetBBPengActorList(string name)
    {
        if (pengActorListBB.ContainsKey(name)) { return pengActorListBB[name]; }
        else { Debug.LogError(owner.GetType().ToString() + "的黑板上没取到名为" + name + "的变量，故返回null"); return null; }
    }
}
