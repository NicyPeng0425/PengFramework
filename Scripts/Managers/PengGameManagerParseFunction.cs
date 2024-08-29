using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PengScript;
using System.Linq;

public partial class PengGameManager : MonoBehaviour
{
    public static Dictionary<int, PengScript.ScriptIDVarID> ParseStringToDictionaryIntScriptIDVarID(string str)
    {
        Dictionary<int, ScriptIDVarID> result = new Dictionary<int, ScriptIDVarID>();
        if (str == "")
            return result;
        string[] strings = str.Split(";");
        if (strings.Length > 0)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                string[] s1 = strings[i].Split("|");
                string[] s2 = s1[1].Split(":");
                ScriptIDVarID sivi = new ScriptIDVarID();
                sivi.scriptID = int.Parse(s2[0]);
                sivi.varID = int.Parse(s2[1]);
                result.Add(int.Parse(s1[0]), sivi);
            }
        }
        return result;
    }

    public static Dictionary<int, PengLevelRuntimeFunction.ScriptIDVarID> ParseStringToDictionaryIntScriptIDVarIDLevel(string str)
    {
        Dictionary<int, PengLevelRuntimeFunction.ScriptIDVarID> result = new Dictionary<int, PengLevelRuntimeFunction.ScriptIDVarID>();
        if (str == "")
            return result;
        string[] strings = str.Split(";");
        if (strings.Length > 0)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                string[] s1 = strings[i].Split("|");
                string[] s2 = s1[1].Split(":");
                PengLevelRuntimeFunction.ScriptIDVarID sivi = new PengLevelRuntimeFunction.ScriptIDVarID();
                sivi.scriptID = int.Parse(s2[0]);
                sivi.varID = int.Parse(s2[1]);
                result.Add(int.Parse(s1[0]), sivi);
            }
        }
        return result;
    }

    public static string ParseDictionaryIntIntToString(Dictionary<int, int> dic)
    {
        string result = "";
        if (dic.Count > 0)
        {
            for (int i = 0; i < dic.Count; i++)
            {
                result += dic.ElementAt(i).Key.ToString() + ":" + dic.ElementAt(i).Value.ToString();
                if (i != dic.Count - 1)
                {
                    result += ";";
                }
            }
        }
        return result;
    }

    public static Dictionary<int, int> ParseStringToDictionaryIntInt(string str)
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        if (str != "")
        {
            string[] strings = str.Split(";");
            if (strings.Length > 0)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    string[] s = strings[i].Split(":");
                    result.Add(int.Parse(s[0]), int.Parse(s[1]));
                }
            }
        }
        return result;
    }




    public static string ParseVector2ToString(Vector2 vec)
    {
        string result = "";
        result += vec.x.ToString() + ",";
        result += vec.y.ToString();
        return result;
    }

    public static Vector2 ParseStringToVector2(string str)
    {
        string[] s = str.Split(",");
        if (s.Length == 2)
        {
            return new Vector2(float.Parse(s[0]), float.Parse(s[1]));
        }
        else
        {
            Debug.LogError("字符串格式不正确，无法转成Vector2！");
            return Vector2.zero;
        }
    }

    public static Dictionary<int, int> DefaultDictionaryIntInt(int num)
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                result.Add(i, -1);
            }
        }
        return result;
    }
}
