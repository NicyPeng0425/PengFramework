using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PengActorControl : MonoBehaviour
{
    public void ProcessInputAttack(InputAction.CallbackContext Obj)
    {
        if (!acceptInput)
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
        if (!acceptInput)
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
        if (!acceptInput)
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
        if (!acceptInput)
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
        if (!acceptInput)
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
        if (!acceptInput)
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
        if (!acceptInput)
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
}
