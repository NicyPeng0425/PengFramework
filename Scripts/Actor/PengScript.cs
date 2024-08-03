using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PengScript
{
    /// <summary>
    /// 伟大的彭脚本！
    /// </summary>
    public enum PengScriptType
    { 
        DebugLogText,
    }

    public enum PengScriptTypeCN
    {
        输出文本,
    }

    public enum ExecTime
    {
        Enter,
        Update,
        Exit,
    }


    public class BaseScript
    {
        //脚本名称，显示在编辑器上
        public string scriptName = "默认";
        //脚本含义，显示在编辑器上
        public string meaning = "脚本基类";
        //脚本的所有者
        public PengActor actor;
        //脚本的执行时间
        public ExecTime execTime;
        //脚本类型
        public PengScriptType type;
        //脚本开始帧
        public int time;
        //脚本结束帧
        public int end;
        //脚本ID
        public int scriptID;
        //脚本流的入脚本ID
        public int scriptFlowInID = -1;
        //脚本流的出脚本ID
        public int scriptFlowOutID = -1;
        //脚本此次执行的次数
        public int thisRunFrame;
        //脚本总共执行的次数
        public int totalRunFrame;
        //脚本是否启用
        public bool enabled;

        //仅在开始执行时执行一次
        public virtual void FirstExecute()
        {
            thisRunFrame = 1;
            totalRunFrame++;
        }

        //脚本持续期间，除了开始帧与结束帧，每个动作帧都执行一次
        public virtual void ContinueExecute()
        {
            thisRunFrame++;
            totalRunFrame++;
        }

        //脚本持续的最后一帧执行一次
        public virtual void EndExecute()
        {
            thisRunFrame++;
            totalRunFrame++;
        }

        public void ScriptFlowNext()
        {
            //执行下一个脚本，还没写
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

