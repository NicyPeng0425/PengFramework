using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Xml;

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
    public List<PengActor> targetHistory = new List<PengActor>();
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
    [HideInInspector]
    public float chasingTimeCount = 0;
    //追逐距离，目标距离超过这个值将触发追逐
    [HideInInspector]
    public float chaseDistance = 999f;
    //追逐停止距离，目标距离小于这个值将停止追逐。一般来说，追逐停止距离要小于追逐距离，且至少小于20%左右，避免刚停止追逐后目标又超出追逐距离进而开始继续追逐。
    [HideInInspector]
    public float chaseStopDistance = 5f;
    [HideInInspector]
    public NavMeshPath chasePath;
    [HideInInspector]
    public float calculatePathGap = 0;
    [HideInInspector]
    public Vector3 chaseDir
    {
        get { if (chasePath != null && chasePath.corners.Length >= 2) { return chasePath.corners[1]; } else { return this.transform.position + this.transform.forward; } }
    }
    //可视距离
    [HideInInspector]
    public float visibleDistance = 10f;
    //可视角度。从自身正前方出发，向两侧各自延伸该参数一半的角度
    [HideInInspector]
    public float visibleAngle = 120f;
    //可视高度。从自身角色控制器的中心出发，向上下各自延伸该参数一半的高度
    [HideInInspector]
    public float visibleHeight = 2.5f;
    [HideInInspector]
    public float decideCDTimeCount = 0f;
    //决策间隔
    [HideInInspector]
    public float decideCD = 2f;
    [HideInInspector]
    public float wanderTime = 0f;
    [HideInInspector]
    public Dictionary<int, PengAIScript.PengAIBaseScript> scripts = new Dictionary<int, PengAIScript.PengAIBaseScript>();

    private void Awake()
    {
        
    }

    void Start()
    {
        LoadActorAI();
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
                AIControlLogic();
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
        if (!active)
        {
            if (actor.game.actors.Count > 0)
            {
                for (int i = 0; i < actor.game.actors.Count; i++)
                {
                    if (actor.game.actors[i].actorCamp != actor.actorCamp && actor.game.actors[i].alive &&
                        Mathf.Abs(actor.game.actors[i].transform.position.y - (actor.ctrl.center.y + transform.position.y)) <= visibleAngle * 0.5f &&
                        Vector3.Angle(transform.forward, ((actor.game.actors[i].transform.position - this.transform.position) - (actor.game.actors[i].transform.position - this.transform.position).y * Vector3.up)) <= visibleAngle * 0.5f &&
                        ((actor.game.actors[i].transform.position - this.transform.position) - (actor.game.actors[i].transform.position - this.transform.position).y * Vector3.up).magnitude <= visibleDistance)
                    {
                        target = actor.game.actors[i];
                        return true;
                    }
                }
            }
            return false;
        }
        else
        {
            if (targetHistory.Count > 0)
            {
                for (int i = 0; i < targetHistory.Count; i++)
                {
                    if (targetHistory[i].actorCamp != actor.actorCamp && targetHistory[i].alive)
                    {
                        target = targetHistory[i];
                        return true;
                    }
                }
            }
            return false;
        }
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

        if (actor.lastHitActor != target)
        {
            target = actor.lastHitActor;
        }

        if (!target.alive)
        {
            target = null;
            TryGetTarget();
        }

        if (actor.currentStateType == PengActorState.StateType.待机 || actor.currentStateType == PengActorState.StateType.移动 || actor.currentStateType == PengActorState.StateType.空中待机 || actor.currentStateType == PengActorState.StateType.空中移动)
        {
            if(actor.currentStateType == PengActorState.StateType.移动 || actor.currentStateType == PengActorState.StateType.空中移动)
            {
                wanderTime += Time.deltaTime;
            }
            if (targetDistance >= chaseDistance && !chasing)
            {
                chasing = true;
                calculatePathGap = 0.6f;
            }
            if (chasing && targetDistance <= chaseStopDistance)
            {
                chasing = false;
            }

            if (chasing)
            {
                Chase();
            }
            else
            {
                decideCDTimeCount += Time.deltaTime;
                if (decideCDTimeCount >= decideCD)
                {
                    Decide();
                    decideCDTimeCount = 0f;
                    wanderTime = 0f;
                }
            }
        }
    }

    public void Chase()
    {
        calculatePathGap += Time.deltaTime;
        if (calculatePathGap >= 0.5f || chaseDir.magnitude <= 1f)
        {
            actor.agent.CalculatePath(target.transform.position, chasePath);
            processedInputDir = ((chaseDir - this.transform.position) - (chaseDir - this.transform.position).y * Vector3.up).normalized;
            calculatePathGap = 0;
        }
    }

    public void Decide()
    {
        if (!scripts.ElementAt(0).Value.Execute())
        {
            Debug.LogWarning("决策失败！");
        }
    }

    public void OnDrawGizmos()
    {
        if (chasePath != null && chasePath.corners.Length > 0)
        {
            for (int i = 0; i < chasePath.corners.Length; i++)
            {
                if (i != chasePath.corners.Length - 1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(chasePath.corners[i], chasePath.corners[i + 1]);
                }
            }
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(this.transform.position, chaseDir);
        }
    }
}
