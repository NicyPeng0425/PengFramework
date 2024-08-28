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
        [Description("0,根节点,Action")]
        Root,
        [Description("0,序列,Composite")]
        Sequence,
        [Description("0,距离条件,Conditional")]
        Distance,
        [Description("0,重复,Decorator")]
        Repeat,
    }
}
