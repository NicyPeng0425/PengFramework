using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using System.Reflection;
using static PengNode;
using System.Security.Cryptography.X509Certificates;

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

    public struct ScriptIDVarID
    {
        public int scriptID;
        public int varID;
    }


    public class BaseScript
    {

        //BaseScript�����ļ�
        //scriptName ����
        //scriptType �ű�����
        //scriptID �ű�ID
        //varInID ��������Ϣ
        //outID �ű�������Ϣ
        //������Ϣ

        //�ű����ƣ���ʾ�ڱ༭����
        public string scriptName = "Ĭ��";
        //�ű���������
        public PengActor actor;
        //�ű�����
        public PengScriptType type;
        //�������
        public PengTrack trackMaster;

        public PengVar[] inVars;
        public PengVar[] outVars;

        public int ID;
        public Dictionary<int, int> flowOutInfo = new Dictionary<int, int>();
        public Dictionary<int, ScriptIDVarID> varInID = new Dictionary<int, ScriptIDVarID>();

        //ִ��һ��
        public virtual void Execute()
        {
            Initial();
            Execute();
            ScriptFlowNext();
        }

        public virtual void Initial()
        {
            //ִ��ǰ�ĳ�ʼ��
            if (varInID.Count > 0 && inVars.Length > 0)
            {
                for (int i = 0; i < varInID.Count; i++)
                {
                    if (varInID.ElementAt(i).Value.scriptID > 0)
                    {
                        PengVar vari = trackMaster.GetOutPengVarByScriptIDPengVarID(varInID.ElementAt(i).Value.scriptID, varInID.ElementAt(i).Value.varID);
                        PengVar.SetValue(ref inVars[varInID.ElementAt(i).Key], ref vari);
                    }
                }
            }
        }

        public virtual void Function()
        {
            //���幦��
        }

        public virtual void ScriptFlowNext()
        {
            //�ű���
            if (trackMaster != null && trackMaster.scripts.Count > 0)
            {
                if (flowOutInfo.Count > 0)
                {
                    for(int i = 0; i < flowOutInfo.Count; i++)
                    {
                        if(flowOutInfo.ElementAt(i).Value >= 0)
                        {
                            if(trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(i).Value) == null)
                            {
                                continue;
                            }
                            else
                            {
                                trackMaster.GetScriptByScriptID(flowOutInfo.ElementAt(i).Value).Execute();
                            }
                        }
                    }
                }
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

        public static ScriptIDVarID InitialScriptIDVarID()
        {
            ScriptIDVarID sivi = new ScriptIDVarID();
            sivi.scriptID = -1;
            sivi.varID = -1;
            return sivi;
        }

        public static Dictionary<int, ScriptIDVarID> ParseStringToDictionaryIntScriptIDVarID(string str)
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

        public static void SetPengFloat(ref PengFloat toBeSet, ref PengFloat toBeGet)
        {

        }
    }

    public class OnTrackExecute: BaseScript
    {
        public PengInt pengTrackExecuteFrame;
        public PengInt pengStateExecuteFrame;
        public OnTrackExecute(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.scriptName = "���ִ��";
            this.type = PengScriptType.OnExecute;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[0];
            outVars = new PengVar[2];
            outVars[0] = pengTrackExecuteFrame;
            outVars[1] = pengStateExecuteFrame;
        }

        public override void Initial()
        {
            pengTrackExecuteFrame.value = actor.currentStateFrame - trackMaster.start;
            pengStateExecuteFrame.value = actor.currentStateFrame;
        }
    }

    public class PlayAnimation : BaseScript
    {
        public PengString pengAnimationName;
        public PengBool pengHardCut;
        public PengFloat pengTransitionNormalizedTime;
        public PengFloat pengStartAtNormalizedTime;
        public PengInt pengAnimationLayer;
        public PlayAnimation(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.scriptName = "���Ŷ���";
            this.type = PengScriptType.OnExecute;
            this.ID = ID;
            this.flowOutInfo = ParseStringToDictionaryIntInt(flowOutInfo);
            this.varInID = ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[5];
            outVars = new PengVar[0];
            inVars[0] = pengAnimationName;
            inVars[1] = pengHardCut;
            inVars[2] = pengTransitionNormalizedTime;
            inVars[3] = pengStartAtNormalizedTime;
            inVars[4] = pengAnimationLayer;
        }

        public override void Initial()
        {
            base.Initial();
        }

        public override void Function()
        {
            base.Function();
            int stateId = Animator.StringToHash(pengAnimationName.value);
            if (!actor.anim.HasState(pengAnimationLayer.value, stateId)) 
            {
                Debug.LogWarning("�����ڸö���״̬");
                return;
            }
            if (pengHardCut.value)
            {
                actor.anim.Play(pengAnimationName.value, pengAnimationLayer.value, pengStartAtNormalizedTime.value);
            }
            else
            {
                actor.anim.CrossFade(pengAnimationName.value, pengTransitionNormalizedTime.value, pengAnimationLayer.value, pengStartAtNormalizedTime.value);
            }
        }
    }
}

