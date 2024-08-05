using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace PengScript
{
    /// <summary>
    /// 伟大的彭脚本！
    /// </summary>
    /// 

    public enum PengScriptType
    {
        [Description("轨道执行")]
        OnExecute,
        [Description("输出文本")]
        DebugLogText,
        [Description("播放动画")]
        PlayAnimation,
    }

    public class BaseScript
    {
        //脚本名称，显示在编辑器上
        public string scriptName = "默认";
        //脚本含义，显示在编辑器上
        public string meaning = "脚本基类";
        //脚本的所有者
        public PengActor actor;
        //脚本类型
        public PengScriptType type;
        //脚本ID
        public int scriptID;
        //脚本流的入脚本ID
        public int scriptFlowInID = -1;
        //脚本流的出脚本ID
        public int scriptFlowOutID = -1;
        //脚本是否启用
        public bool enabled;
        public PengTrack track;

        //执行一次
        public virtual void Execute()
        {
            
        }

        public bool ScriptFlowNext()
        {
            if (scriptFlowOutID > 0)
            {
                for (int i = 0; i < track.scripts.Count; i++)
                {
                    if (track.scripts[i].scriptID == scriptFlowOutID)
                    {
                        track.scripts[i].Execute();
                        track.scripts[i].ScriptFlowNext();
                        break;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Vector3 ParseStringToVector3(string s)
        {
            s = s.Replace("(", "").Replace(")", "").Replace(" ", "");
            string[] str = s.Split(",");
            if (str.Length == 3)
            {
                return new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2]));
            }
            else
            {
                return Vector3.zero;
            }
        }

        public static string ParseVector3ToString(Vector3 v)
        {
            string s = "(" + v.x.ToString() + "," + v.y.ToString() + "," + v.z.ToString() + ")";
            return s;
        }

        public static string ParseStringListToString(List<string> list)
        {
            string str = "";
            if (list.Count > 0)
            {
                for (global::System.Int32 i = 0; i < list.Count; i++)
                {
                    str += list[i];
                    if (i < list.Count - 1)
                    {
                        str += ",";
                    }
                }
            }
            return str;
        }

        public static List<string> ParseStringToStringList(string str)
        {
            List<string> s = new List<string>();
            string[] strings = str.Split(",");
            if (strings.Length > 0)
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    s.Add(strings[i]);
                }
            }
            return s;
        }
    }

}

