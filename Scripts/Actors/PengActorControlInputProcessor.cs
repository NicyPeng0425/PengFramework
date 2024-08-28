using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PengActorControl : MonoBehaviour
{
    public void ProcessInputAttack(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
            { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Attack);
            actions.Add(actor.game.currentFrame, at); 
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Attack);
        }
    }

    public void ProcessInputDodge(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Dodge);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Dodge);
        }
    }

    public void ProcessInputJump(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Jump);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Jump);
        }
    }

    public void ProcessInputSkill_A(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Skill_A);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Skill_A);
        }
    }

    public void ProcessInputSkill_B(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Skill_B);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Skill_B);
        }
    }

    public void ProcessInputSkill_C(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Skill_C);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Skill_C);
        }
    }

    public void ProcessInputSkill_D(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Skill_D);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Skill_D);
        }
    }

    public void ProcessInputAttack_Up(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Attack_Up);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Attack_Up);
        }
    }

    public void ProcessInputDodge_Up(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Dodge_Up);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Dodge_Up);
        }
    }

    public void ProcessInputJump_Up(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Jump_Up);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Jump_Up);
        }
    }

    public void ProcessInputSkill_A_Up(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Skill_A_Up);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Skill_A_Up);
        }
    }

    public void ProcessInputSkill_B_Up(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Skill_B_Up);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Skill_B_Up);
        }
    }

    public void ProcessInputSkill_C_Up(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Skill_C_Up);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Skill_C_Up);
        }
    }

    public void ProcessInputSkill_D_Up(InputAction.CallbackContext Obj)
    {
        if (!acceptInput || aiCtrl)
        { return; }

        if (!actions.ContainsKey(actor.game.currentFrame))
        {
            List<ActionType> at = new List<ActionType>();
            at.Add(ActionType.Skill_D_Up);
            actions.Add(actor.game.currentFrame, at);
        }
        else
        {
            actions[actor.game.currentFrame].Add(ActionType.Skill_D_Up);
        }
    }
}
