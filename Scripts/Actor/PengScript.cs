using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace PengScript
{
    /// <summary>
    /// ΰ�����ű���
    /// </summary>
    /// 

    public enum PengScriptType
    {
        [Description("���ִ��")]
        OnExecute,
        [Description("����ı�")]
        DebugLogText,
        [Description("���Ŷ���")]
        PlayAnimation,
    }

    public class BaseScript
    {
        //�ű����ƣ���ʾ�ڱ༭����
        public string scriptName = "Ĭ��";
        //�ű����壬��ʾ�ڱ༭����
        public string meaning = "�ű�����";
        //�ű���������
        public PengActor actor;
        //�ű�����
        public PengScriptType type;
        //�ű�ID
        public int scriptID;
        //�ű�������ű�ID
        public int scriptFlowInID = -1;
        //�ű����ĳ��ű�ID
        public int scriptFlowOutID = -1;
        //�ű��Ƿ�����
        public bool enabled;
        public PengTrack track;

        //ִ��һ��
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

