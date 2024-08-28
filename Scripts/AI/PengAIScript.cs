using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace PengAIScript
{
    public enum AISensor
    {
        Target = 0,
        TargetDistance = 1,
        TargetRelativeDirection = 2,
        TargetCurrentState = 3,
        TargetCurrentStateFrame = 4,
        TargetCurrentHP = 5,
        SelfCurrentState = 6,
        SelfWanderTime = 7,
        SelfDecideGap = 8,
    }

    public enum AIScriptType
    {
        [Description("1,决策事件,Event")]
        EventDecide,
        [Description("0,条件分支,Condition")]
        Condition,
        [Description("0,随机分支,Condition")]
        Random,
        [Description("0,输入行为,Action")]
        InputAction,
        [Description("0,缩短决策间隔,Action")]
        ReduceDecideGap,
    }
}
