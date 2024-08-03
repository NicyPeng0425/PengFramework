using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PengScript
{
    /// <summary>
    /// ΰ�����ű���
    /// </summary>
    public enum PengScriptType
    { 
        DebugLogText,
    }

    public enum PengScriptTypeCN
    {
        ����ı�,
    }

    public enum ExecTime
    {
        Enter,
        Update,
        Exit,
    }


    public class BaseScript
    {
        //�ű����ƣ���ʾ�ڱ༭����
        public string scriptName = "Ĭ��";
        //�ű����壬��ʾ�ڱ༭����
        public string meaning = "�ű�����";
        //�ű���������
        public PengActor actor;
        //�ű���ִ��ʱ��
        public ExecTime execTime;
        //�ű�����
        public PengScriptType type;
        //�ű���ʼ֡
        public int time;
        //�ű�����֡
        public int end;
        //�ű�ID
        public int scriptID;
        //�ű�������ű�ID
        public int scriptFlowInID = -1;
        //�ű����ĳ��ű�ID
        public int scriptFlowOutID = -1;
        //�ű��˴�ִ�еĴ���
        public int thisRunFrame;
        //�ű��ܹ�ִ�еĴ���
        public int totalRunFrame;
        //�ű��Ƿ�����
        public bool enabled;

        //���ڿ�ʼִ��ʱִ��һ��
        public virtual void FirstExecute()
        {
            thisRunFrame = 1;
            totalRunFrame++;
        }

        //�ű������ڼ䣬���˿�ʼ֡�����֡��ÿ������֡��ִ��һ��
        public virtual void ContinueExecute()
        {
            thisRunFrame++;
            totalRunFrame++;
        }

        //�ű����������һִ֡��һ��
        public virtual void EndExecute()
        {
            thisRunFrame++;
            totalRunFrame++;
        }

        public void ScriptFlowNext()
        {
            //ִ����һ���ű�����ûд
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

