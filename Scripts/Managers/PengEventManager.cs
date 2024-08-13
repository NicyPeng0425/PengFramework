using PengScript;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PengEventManager
{
    public PengGameManager game;

    public void TriggerEvent(string eventName, int intMsg, float floatMsg, string stringMsg, bool boolMsg)
    {
        if (game.actors.Count > 0)
        {
            for (int i = 0; i < game.actors.Count; i++)
            {
                if (game.actors[i].globalTrack != null && game.actors[i].globalTrack.scripts.Count > 0)
                {
                    for (int j = 0; j < game.actors[i].globalTrack.scripts.Count; j++)
                    {
                        if (game.actors[i].globalTrack.scripts[j].type == PengScript.PengScriptType.OnEvent)
                        {
                            OnEvent func = game.actors[i].globalTrack.scripts[j] as OnEvent;
                            if (func.eventName.value == eventName)
                            {
                                func.EventTrigger(intMsg, floatMsg, stringMsg, boolMsg);
                            }
                        }
                    }
                }
            }
        }
    }
}
