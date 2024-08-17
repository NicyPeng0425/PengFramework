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
    }
}
