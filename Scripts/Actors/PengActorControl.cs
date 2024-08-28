using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public partial class PengActorControl : MonoBehaviour
{
    [HideInInspector]
    public bool aiCtrl = true;
    [HideInInspector]
    public bool active = false;
    [HideInInspector]
    public PengActor actor;
    //玩家的原始输入，即WASD所指示的世界坐标系指向
    [HideInInspector]
    public Vector3 originalInputDir;
    //处理后的输入，一般直接用来指示AI的前进方向，或者表示玩家的原始输入在经过相机的变换后的前进方向
    [HideInInspector]
    public Vector3 processedInputDir;
    [HideInInspector]
    public bool acceptInput = false;
    public Dictionary<int, List<ActionType>> actions = new Dictionary<int, List<ActionType>>();

    [HideInInspector]
    public PengActor target = null;
    [HideInInspector]
    public float targetDistance { get { if (target != null) { return ((target.transform.position - this.transform.position) - (target.transform.position - this.transform.position).y * Vector3.up).magnitude; }else { return 100f; } } }
    [HideInInspector]
    public Vector3 targetDirection { get { if (target != null) { return ((target.transform.position - this.transform.position) - (target.transform.position - this.transform.position).y * Vector3.up).normalized; } else { return Vector3.zero; } } }
    [HideInInspector]
    public string targetCurrentState{ get { if (target != null) { return target.currentName;  } else { return ""; } }}
    [HideInInspector]
    public int targetCurrentStateFrame {  get { if (target != null) { return target.currentStateFrame; } else { return -1; } } }
    [HideInInspector]
    public float targetCurrentHP { get { if (target != null) { return target.currentHP; } else { return -1; } } }
    [HideInInspector]
    public bool chasing = false;
    //追逐距离，目标距离超过这个值将触发追逐
    [HideInInspector]
    public float chaseDistance = 999f;
    //追逐停止距离，目标距离小于这个值将停止追逐。一般来说，追逐停止距离要小于追逐距离，且至少小于20%左右，避免刚停止追逐后目标又超出追逐距离进而开始继续追逐。
    [HideInInspector]
    public float chaseStopDistance = 5f;

    void Start()
    {
    }

    void Update()
    {
        if ((actions.Count > 0))
        {
            for (int i = actions.Count - 1; i >= 0; i--)
            {
                if (actor.game.currentFrame - actions.ElementAt(i).Key >= 2)
                {
                    actions.Remove(actions.ElementAt(i).Key);
                }
            }
        }

        if (acceptInput)
        {
            if (aiCtrl)
            {
                if (active)
                {
                    AIControlLogic();
                }
            }
            else
            {
                InputSystemLogic();
            }
        }
        else
        {
            originalInputDir = Vector2.zero;
            processedInputDir = Vector2.zero;
            actions.Clear();
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

    public bool TryGetTarget()
    {
        return false;
    }

    public void AIControlLogic()
    {
        //距离过远时：追逐（正常Move状态）方向读取Agent
        //距离较近时：决策一次
        //决策完毕：缩短或不缩短下次决策的间隔
        //立回
        //立回完毕继续决策


        if (target == null)
        {
            active = TryGetTarget();
        }

        if (target == null)
        {
            return;
        }

        if (actor.currentStateType == PengActorState.StateType.待机 || actor.currentStateType == PengActorState.StateType.移动 || actor.currentStateType == PengActorState.StateType.空中待机 || actor.currentStateType == PengActorState.StateType.空中移动)
        {
            if (targetDistance >= chaseDistance && !chasing)
            {
                chasing = true;
            }
            if (chasing && targetDistance <= chaseStopDistance)
            {
                chasing = false;
            }

            if (chasing)
            {
                //追逐
            }
            else
            {
                //决策
            }
        }
    }
}
