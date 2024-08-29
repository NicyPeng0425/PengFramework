using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PengActorControl : MonoBehaviour
{
    public enum ActionType
    {
        Attack,
        Dodge,
        Jump,
        Skill_A,
        Skill_B,
        Skill_C,
        Skill_D,
        Attack_Up,
        Dodge_Up,
        Jump_Up,
        Skill_A_Up,
        Skill_B_Up,
        Skill_C_Up,
        Skill_D_Up,
        AI_Forward,
        AI_Backward,
        AI_Right,
        AI_Left,
        AI_Forward_Right,
        AI_Forward_Left,
        AI_Backward_Right,
        AI_Backward_Left,
    }

    public void InputListener()
    {
        actor.game.input.Basic.Attack.started += ProcessInputAttack;
        actor.game.input.Basic.Dodge.started += ProcessInputDodge;
        actor.game.input.Basic.Jump.started += ProcessInputJump;
        actor.game.input.Basic.Skill_A.started += ProcessInputSkill_A;
        actor.game.input.Basic.Skill_B.started += ProcessInputSkill_B;
        actor.game.input.Basic.Skill_C.started += ProcessInputSkill_C;
        actor.game.input.Basic.Skill_D.started += ProcessInputSkill_D;

        actor.game.input.Basic.Attack.canceled += ProcessInputAttack_Up;
        actor.game.input.Basic.Dodge.canceled += ProcessInputDodge_Up;
        actor.game.input.Basic.Jump.canceled += ProcessInputJump_Up;
        actor.game.input.Basic.Skill_A.canceled += ProcessInputSkill_A_Up;
        actor.game.input.Basic.Skill_B.canceled += ProcessInputSkill_B_Up;
        actor.game.input.Basic.Skill_C.canceled += ProcessInputSkill_C_Up;
        actor.game.input.Basic.Skill_D.canceled += ProcessInputSkill_D_Up;
    }
}
