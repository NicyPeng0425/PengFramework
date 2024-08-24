using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class PengLevelRuntimeFunction
{
    public enum LevelFunctionType
    {
        [Description("1,起点,触发器")]
        Start,
        [Description("0,屏蔽所有UI,UI")]
        SetAllUIDisabled,
        [Description("0,显示所有UI,UI")]
        SetAllUIEnabled,
        [Description("0,生成Boss血条,UI")]
        GenerateBossHPBar,
        [Description("0,黑屏渐入,UI")]
        EaseInBlack,
        [Description("0,黑屏渐出,UI")]
        EaseOutBlack,
        [Description("0,生成Actor,功能")]
        GenerateActor,
        [Description("0,跳转场景,功能")]
        JumpToScene,
        [Description("0,设置主控,功能")]
        SetMainActor,
        [Description("0,增加Buff,功能")]
        AddBuff,
        [Description("0,开始操控,功能")]
        StartControl,
        [Description("0,屏蔽操控,功能")]
        EndControl,
        [Description("0,切状态,功能")]
        TransAction,
        [Description("0,设置空气墙,功能")]
        SetAirWall,
        [Description("0,关闭空气墙,功能")]
        CloseAirWall,
        [Description("0,生成对话,剧情")]
        GenerateDialog,
        [Description("0,获取当前主控,值")]
        GetCurrentMainActor,
        [Description("0,Actor组合成List,值")]
        ParseActorsToList,
        [Description("0,等待时间,触发器")]
        TriggerWaitTime,
        [Description("0,等待到达区域,触发器")]
        TriggerWaitArrival,
        [Description("0,等待主控输入,触发器")]
        TriggerWaitInput,
        [Description("0,等待分支选择,触发器")]
        TriggerWaitSelection,
        [Description("0,等待所有敌人死亡,触发器")]
        TriggerWaitEnemyDie,
    }
}
