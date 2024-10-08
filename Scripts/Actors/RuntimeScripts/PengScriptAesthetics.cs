﻿using PengVariables;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PengScript
{
    public class PlayAnimation : BaseScript
    {
        public PengString pengAnimationName = new PengString("动画名称", 0, ConnectionPointType.In);
        public PengBool pengHardCut = new PengBool("是否硬切", 1, ConnectionPointType.In);
        public PengFloat pengTransitionNormalizedTime = new PengFloat("过度时间", 2, ConnectionPointType.In);
        public PengFloat pengStartAtNormalizedTime = new PengFloat("开始时间", 3, ConnectionPointType.In);
        public PengInt pengAnimationLayer = new PengInt("动画层", 4, ConnectionPointType.Out);
        public PlayAnimation(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.PlayAnimation;
            scriptName = GetDescription(type);
            inVars[0] = pengAnimationName;
            inVars[1] = pengHardCut;
            inVars[2] = pengTransitionNormalizedTime;
            inVars[3] = pengStartAtNormalizedTime;
            inVars[4] = pengAnimationLayer;
        }

        public override void Initial(int functionIndex)
        {
            base.Initial(functionIndex);
        }

        public override void SetValue(int inVarID, PengVar varSource)
        {
            switch (inVarID)
            {
                case 0:
                    PengString ps = varSource as PengString;
                    pengAnimationName.value = ps.value;
                    break;
                case 1:
                    PengBool pb = varSource as PengBool;
                    pengHardCut.value = pb.value;
                    break;
                case 2:
                    PengFloat pf1 = varSource as PengFloat;
                    pengTransitionNormalizedTime.value = pf1.value;
                    break;
                case 3:
                    PengFloat pf2 = varSource as PengFloat;
                    pengStartAtNormalizedTime.value = pf2.value;
                    break;
                case 4:
                    PengInt pi = varSource as PengInt;
                    pengAnimationLayer.value = pi.value;
                    break;
            }
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            int stateId = Animator.StringToHash(pengAnimationName.value);
            if (!actor.anim.HasState(pengAnimationLayer.value, stateId))
            {
                Debug.LogWarning("不存在该动画状态：" + pengAnimationName.value + "，来自状态" + actor.currentName + "的" + trackMaster.name + "轨道的" + ID.ToString() + "号脚本。");
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

    public class PlayEffects : BaseScript
    {
        public PengString effectPath = new PengString("特效路径", 0, ConnectionPointType.In);
        public PengBool follow = new PengBool("跟随", 1, ConnectionPointType.In);
        public PengVector3 posOffset = new PengVector3("位置偏移", 2, ConnectionPointType.In);
        public PengVector3 rotOffset = new PengVector3("旋转偏移", 3, ConnectionPointType.In);
        public PengVector3 scaleOffset = new PengVector3("缩放偏移", 4, ConnectionPointType.In);
        public PengFloat deleteTime = new PengFloat("停止时间", 5, ConnectionPointType.In);

        public ParticleSystem ps;
        public PlayEffects(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.PlayEffects;
            scriptName = GetDescription(type);
            inVars[0] = effectPath;
            inVars[1] = follow;
            inVars[2] = posOffset;
            inVars[3] = rotOffset;
            inVars[4] = scaleOffset;
            inVars[5] = deleteTime;

            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(';');
                effectPath.value = str[0];
                follow.value = int.Parse(str[1]) > 0;
                posOffset.value = BaseScript.ParseStringToVector3(str[2]);
                rotOffset.value = BaseScript.ParseStringToVector3(str[3]);
                scaleOffset.value = BaseScript.ParseStringToVector3(str[4]);
                deleteTime.value = float.Parse(str[5]);
            }
        }

        public override void Initial(int functionIndex)
        {
            base.Initial(functionIndex);
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            if (functionIndex == 0)
            {
                ps = actor.game.GenerateVFX(effectPath.value, posOffset.value, rotOffset.value, scaleOffset.value, actor, follow.value, deleteTime.value);
            }
            else if (functionIndex == 1)
            {
                ps.Stop();
            }
        }
    }

    public class PlayAudio : BaseScript
    {
        public PengString audioPath = new PengString("音频路径", 0, ConnectionPointType.In);
        public PengFloat audioVol = new PengFloat("音量", 1, ConnectionPointType.In);

        public List<string> paths = new List<string>();
        public List<AudioClip> clips = new List<AudioClip>();
        public PlayAudio(PengActor actor, PengTrack track, int ID, string flowOutInfo, string varInInfo, string specialInfo)
        {
            this.actor = actor;
            this.trackMaster = track;
            this.ID = ID;
            this.flowOutInfo = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(flowOutInfo);
            this.varInID = PengGameManager.ParseStringToDictionaryIntScriptIDVarID(varInInfo);
            inVars = new PengVar[varInID.Count];
            outVars = new PengVar[0];
            Construct(specialInfo);
            InitialPengVars();
        }

        public override void Construct(string specialInfo)
        {
            base.Construct(specialInfo);

            type = PengScriptType.PlayAudio;
            scriptName = GetDescription(type);
            inVars[0] = audioPath;
            inVars[1] = audioVol;

            if (specialInfo != "")
            {
                string[] str = specialInfo.Split(';');
                audioPath.value = str[0];
                audioVol.value = float.Parse(str[1]);

                if (str[2] != "null")
                {
                    string[] paths = str[2].Split(",");
                    if (paths.Length > 0)
                    {
                        for (int i = 0; i < paths.Length; i++)
                        {
                            clips.Add(Resources.Load<AudioClip>(paths[i]));
                        }
                    }
                }
            }
        }

        public override void Initial(int functionIndex)
        {
            base.Initial(functionIndex);
        }

        public override void Function(int functionIndex)
        {
            base.Function(functionIndex);
            if (clips.Count > 0)
            {
                int index = Random.Range(0, clips.Count);
                actor.speaker.PlayOneShot(clips[index]);
            }
        }
    }

}
