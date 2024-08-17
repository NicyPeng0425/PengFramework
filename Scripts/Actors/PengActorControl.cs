using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class PengActorControl : MonoBehaviour
{
    [HideInInspector]
    public bool aiCtrl = true;
    [HideInInspector]
    public PengActor actor;
    //玩家的原始输入，即WASD所指示的世界坐标系指向
    [HideInInspector]
    public Vector3 originalInputDir;
    //处理后的输入，一般直接用来指示AI的前进方向，或者表示玩家的原始输入在经过相机的变换后的前进方向
    [HideInInspector]
    public Vector3 processedInputDir;
    [HideInInspector]
    public bool acceptInput = true;
    public Dictionary<int, List<ActionType>> actions = new Dictionary<int, List<ActionType>>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((actions.Count > 0))
        {
            for (int i = actions.Count - 1; i >= 0; i++)
            {
                if (actor.game.currentFrame - actions.ElementAt(i).Key >= 2)
                {
                    actions.Remove(actions.ElementAt(i).Key);
                }
            }
        }

        if (aiCtrl)
        {
            AIControlLogic();
        }
        else
        {
            InputSystemLogic();
        }
    }

    public void InputSystemLogic()
    {
        Vector2 input = actor.game.input.Basic.Move.ReadValue<Vector2>();
        originalInputDir = new Vector3(input.x, 0, input.y);
        if (originalInputDir.magnitude > 0.05f)
        {
            Vector3 move;
            Quaternion rot = Quaternion.Euler(0f, actor.game.main.gameObject.transform.rotation.eulerAngles.y, 0f);
            move = rot * Vector3.forward * originalInputDir.z + rot * Vector3.right * originalInputDir.x;
            move = new Vector3(move.x, 0f, move.z);
            move = move.normalized;
            //inertia = move * _para.dashSpeed;
            processedInputDir = move;
        }
        else
        {
            processedInputDir = Vector3.zero;
        }
    }

    public void AIControlLogic()
    {

    }
}
