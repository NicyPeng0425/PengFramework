﻿using PengScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PengActor : MonoBehaviour
{
    [HideInInspector]
    public PengGameManager game;

    [ReadOnly,Label("角色ID")]
    public int actorID;
    [ReadOnly,Label("角色名称")]
    public string actorName;
    [ReadOnly,Label("角色阵营")]
    public int actorCamp;

    [HideInInspector]
    public PengTrack globalTrack;
    [HideInInspector]
    public static string initalName = "Idle";
    [HideInInspector]
    public List<SkinnedMeshRenderer> smrs = new List<SkinnedMeshRenderer>();
    [HideInInspector]
    public List<MeshRenderer> mrs = new List<MeshRenderer>();
    //运行时动态数据
    [HideInInspector]
    public string currentName;
    [HideInInspector]
    public string lastName;
    public Dictionary<string, IPengActorState> actorStates = new Dictionary<string, IPengActorState>();
    public IPengActorState current;
    public IPengActorState last;
    [HideInInspector]
    public float pauseTime;
    [HideInInspector]
    public bool alive = true;
    [HideInInspector]
    public Animator anim;
    [HideInInspector]
    public CharacterController ctrl;
    [HideInInspector]
    public int currentStateFrame = 0;
    [HideInInspector]
    public int currentStateLength = 0;
    [HideInInspector]
    public List<PengActor> targets = new List<PengActor>();
    [HideInInspector]
    public PengActorControl input;
    [HideInInspector]
    public PengBlackBoard<PengActor> bb;
    [HideInInspector]
    public PengBuffManager buff;
    [HideInInspector]
    float m_attackPower;
    [HideInInspector]
    public float attackPower
    {
        get 
        {
            if (buff.buffs.Count > 0)
            {
                float change = 0;
                foreach (PengBuff buff in buff.buffs)
                {
                    change += buff.attackPowerPercent * buff.stack * m_attackPower;
                    change += buff.attackPowerValue * buff.stack;
                }
                return m_attackPower + change;
            }
            else
            {
                return m_attackPower;
            }
        }
        set { m_attackPower = value; }
    }
    [HideInInspector]
    float m_defendPower;
    [HideInInspector]
    public float defendPower
    {
        get
        {
            if (buff.buffs.Count > 0)
            {
                float change = 0;
                foreach (PengBuff buff in buff.buffs)
                {
                    change += buff.defendPowerPercent * buff.stack * m_defendPower;
                    change += buff.defendPowerValue * buff.stack;
                }
                return m_defendPower + change;
            }
            else
            {
                return m_defendPower;
            }
        }
        set { m_defendPower = value; }
    }
    [HideInInspector]
    float m_criticalRate;
    [HideInInspector]
    public float criticalRate
    {
        get
        {
            if (buff.buffs.Count > 0)
            {
                float change = 0;
                foreach (PengBuff buff in buff.buffs)
                {
                    change += buff.criticalRateValue * buff.stack;
                }
                return m_criticalRate + change;
            }
            else
            {
                return m_criticalRate;
            }
        }
        set { m_criticalRate = value; }
    }
    float m_criticalDamageRatio;
    [HideInInspector]
    public float criticalDamageRatio
    {
        get
        {
            if (buff.buffs.Count > 0)
            {
                float change = 0;
                foreach (PengBuff buff in buff.buffs)
                {
                    change += buff.criticalDamageRatioValue * buff.stack;
                }
                return m_criticalDamageRatio + change;
            }
            else
            {
                return m_criticalDamageRatio;
            }
        }
        set { m_criticalDamageRatio = value; }
    }
    [HideInInspector]
    public float m_maxHP;
    [HideInInspector]
    float m_currentHP;
    [HideInInspector]
    public float currentHP
    {
        get { return m_currentHP; }
        set {
            bool yes = false;
            
            if (buff.buffs.Count > 0)
            {
                foreach (PengBuff buff in buff.buffs)
                {
                    if (buff.invincible) { yes = true; }
                }
            }
            if (yes && value < currentHP)
            {
                currentHP = value;
            }
            else
            {
                if (value >= m_maxHP) { m_currentHP = m_maxHP; UpdateHPBar(); }
                else if (value <= 0) { m_currentHP = 0; OnDie(); }
                else { m_currentHP = value; UpdateHPBar(); }
            }
        }
    }
    [HideInInspector]
    public bool underGravity
    {
        get {
            if (buff.buffs.Count > 0)
            {
                bool yes = true;
                foreach (PengBuff buff in buff.buffs)
                {
                    if (buff.notEffectedByGravity) { yes = false; }
                }
                return yes;
            }
            else
            {
                return true;
            }
        }
    }
    [HideInInspector]
    public bool unbreakable
    {
        get
        {
            if (buff.buffs.Count > 0)
            {
                bool yes = false;
                foreach (PengBuff buff in buff.buffs)
                {
                    if (buff.unBreakable) { yes = true; }
                }
                return yes;
            }
            else
            {
                return false;
            }
        }
    }
    [HideInInspector]
    public bool invincible
    {
        get
        {
            if (buff.buffs.Count > 0)
            {
                bool yes = false;
                foreach (PengBuff buff in buff.buffs)
                {
                    if (buff.invincible) { yes = true; }
                }
                return yes;
            }
            else
            {
                return false;
            }
        }
    }
    [HideInInspector]
    public bool isGrounded;
    [ReadOnly,Label("下落速度")]
    public float fallSpeed;
    [HideInInspector]
    public Vector3 inertia;
    [HideInInspector]
    bool inertiaUpdate = true;
    //抗打断
    [HideInInspector]
    float m_resist;
    [HideInInspector]
    public float resist
    {
        get 
        {
            if (buff.buffs.Count > 0)
            {
                float change = 0;
                foreach (PengBuff buff in buff.buffs)
                {
                    change += buff.resistValue * buff.stack;
                }
                return m_resist + change;
            }
            else
            {
                return m_resist;
            }
        }
    }
    [HideInInspector]
    public string stateBeforeGroundedName = "";
    [HideInInspector]
    public AudioSource speaker;
    [HideInInspector]
    public Transform hitVFXPivot;
    [HideInInspector]
    public PengActor lastHitActor;
    [HideInInspector]
    public Transform lookAt;
    [ReadOnly]
    public int runtimeID;
    //如果为空，说明不算敌人
    [HideInInspector]
    public PengLevel belongLevel;
    [HideInInspector]
    public PengHPBarUI hpBar;
    [HideInInspector]
    public PengActorState.StateType currentStateType;
    [HideInInspector]
    public NavMeshAgent agent;

    private void Awake()
    {
        anim = this.GetComponent<Animator>();
        ctrl = this.GetComponent<CharacterController>();
        input = this.AddComponent<PengActorControl>();
        input.actor = this;
        bb = new PengBlackBoard<PengActor>(this);
        buff = this.AddComponent<PengBuffManager>();
        buff.actorOwner = this;
        agent = this.GetComponent<NavMeshAgent>();
        agent.speed = 0;
        speaker = this.AddComponent<AudioSource>();
        hitVFXPivot = new GameObject().transform;
        hitVFXPivot.SetParent(this.transform);
        hitVFXPivot.localPosition = ctrl.center;
        lookAt = new GameObject().transform;
        lookAt.SetParent(this.transform);
        lookAt.localPosition = ctrl.height * 0.85f * Vector3.up;
        LoadActorState();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (actorStates.Count > 0)
        {
            current.OnUpdate();
        }

        ProcessGravity();
        UpdateHPBarPos();
    }

    public void TransState(string name, bool actively)
    {
        if (current != null)
        {
            lastName = currentName;
            last = current;
            last.OnExit();
            ProcessStateChange(actively);
        }
        currentName = name;
        current = actorStates[name];
        current.OnEnter();
    }

    public void ProcessStateChange(bool actively)
    {
        ProcessStateEnd();
        buff.ProcessStateChange(actively);
    }

    public void ProcessStateEnd()
    {
        buff.ProcessStateEnd();
    }

    public void ProcessGravity()
    {
        bool lastFrameGrounded = isGrounded;
        if (underGravity)
        {
            isGrounded = false;
            Vector3 posi = this.transform.position + Vector3.up * 0.35f;
            Collider[] cols = Physics.OverlapSphere(posi, 0.35f);
            if (cols.Length > 0)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    if (cols[i].transform.tag != "PengActor")
                    {
                        isGrounded = true;
                    }
                    else
                    {
                        Vector3 dir = transform.position - cols[i].transform.position;
                        dir = dir - dir.y * Vector3.up;
                        dir = dir.normalized;
                        ctrl.Move(dir * 12f * Time.deltaTime);
                    }
                }
            }
            if (isGrounded)
            {
                fallSpeed = -4f;
            }
            else
            {
                fallSpeed -= 15f * Time.deltaTime;
            }
        }
        else
        {
            fallSpeed = 0;
        }
        ctrl.Move(fallSpeed * Vector3.up * Time.deltaTime);
        if (inertiaUpdate)
        {
            inertia = (transform.forward - transform.forward.y * Vector3.up).normalized;
        }
        if (!lastFrameGrounded && isGrounded)
        {
            OnGrounded();
        }
        if (lastFrameGrounded && !isGrounded)
        {
            OnLaunch();
        }
    }

    public void OnGrounded()
    {
        if (globalTrack != null && globalTrack.scripts.Count > 0)
        {
            for (int i = 0; i < globalTrack.scripts.Count; i++)
            {
                if (globalTrack.scripts[i].type == PengScriptType.OnGround)
                {
                    globalTrack.scripts[i].Execute(0);
                }
            }
        }
        inertiaUpdate = true;
    }

    public void OnLaunch()
    {
        inertiaUpdate = false;
    }

    public void OnBreak()
    {
        
    }

    public void LoadActorState()
    {
        TextAsset textAsset = (TextAsset)Resources.Load("ActorData/" + actorID.ToString() + "/" + actorID.ToString());
        if(textAsset != null)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(textAsset.text);
            XmlElement root = xml.DocumentElement;
            foreach(XmlElement ele in root.ChildNodes)
            {
                if(ele.Name == "ActorInfo")
                {
                    foreach(XmlElement son in ele.ChildNodes)
                    {
                        if(son.Name == "ActorID")
                        {
                            if(int.Parse(son.GetAttribute("ActorID")) != actorID)
                            {
                                Debug.LogWarning("Actor" + actorID.ToString() + "的Actor数据中记载的ActorID与其本身的ID不符！");
                                this.gameObject.SetActive(false);
                                return;
                            }    
                        }
                        if (son.Name == "ActorName")
                        {
                            actorName = son.GetAttribute("ActorName");
                            continue;
                        }
                        if (son.Name == "Camp")
                        {
                            actorCamp = int.Parse(son.GetAttribute("Camp"));
                            continue;
                        }
                        if (son.Name == "Attribute")
                        {
                            m_maxHP = float.Parse(son.GetAttribute("MaxHP"));
                            currentHP = m_maxHP;
                            attackPower = float.Parse(son.GetAttribute("AttackPower"));
                            defendPower = float.Parse(son.GetAttribute("DefendPower"));
                            criticalRate = float.Parse(son.GetAttribute("CriticalRate"));
                            criticalDamageRatio = float.Parse(son.GetAttribute("CriticalDamageRatio"));
                            m_resist = float.Parse(son.GetAttribute("Resist"));
                            continue;
                        }
                        if (son.Name == "Track")
                        {
                            PengTrack track = new PengTrack((PengTrack.ExecTime)Enum.Parse(typeof(PengTrack.ExecTime), son.GetAttribute("ExecTime")), son.GetAttribute("Name"), int.Parse(son.GetAttribute("Start")),
                                int.Parse(son.GetAttribute("End")));

                            for (int j = 0; j < son.ChildNodes.Count; j++)
                            {
                                XmlElement scriptEle = son.ChildNodes[j] as XmlElement;
                                BaseScript script = PengActorState.ConstructRunTimePengScript(this, scriptEle, ref track, int.Parse(scriptEle.GetAttribute("ScriptID")), scriptEle.GetAttribute("OutID"), scriptEle.GetAttribute("VarInID"), scriptEle.GetAttribute("SpecialInfo"));
                                if (script != null)
                                {
                                    track.scripts.Add(script);
                                }
                            }
                            globalTrack = track;
                        }
                    }
                    continue;
                }
                if(ele.Name == "ActorState")
                {
                    foreach (XmlElement stateGroup in ele.ChildNodes)
                    {
                        foreach(XmlElement state in stateGroup.ChildNodes)
                        {
                            actorStates.Add(state.GetAttribute("Name"), new PengActorState(this, state));
                        }
                    }
                    continue;
                }
            }
        }
        else
        {
            Debug.Log("Actor" + actorID.ToString() + "的Actor数据读取失败！");
            this.gameObject.SetActive(false);
        }

        if (actorStates.ContainsKey("Intro"))
        {
            TransState("Intro", true);
        }
        else
        {
            TransState(initalName, true);
        }
    }

    public void OnDie()
    {
        if (globalTrack != null && globalTrack.scripts.Count > 0)
        {
            for (int i = 0; i < globalTrack.scripts.Count; i++)
            {
                if (globalTrack.scripts[i].type == PengScriptType.OnDie)
                {
                    globalTrack.scripts[i].Execute(0);
                }
            }
        }
        if (currentHP <= 0)
        {
            TransState("Dead", false);
            if (belongLevel != null)
            {
                belongLevel.currentEnemy.Remove(this);
            }

            if (hpBar != null)
            {
                GameObject.Destroy(hpBar.gameObject);
            }
        }
    }

    public void OnHit()
    {
        if (globalTrack != null && globalTrack.scripts.Count > 0)
        {
            for (int i = 0; i < globalTrack.scripts.Count; i++)
            {
                if (globalTrack.scripts[i].type == PengScriptType.OnHit)
                {
                    globalTrack.scripts[i].Execute(0);
                }
            }
        }

        if (input.aiCtrl && !input.active)
        {
            input.active = true;
            input.target = lastHitActor;
        }

        if (input.aiCtrl && !input.targetHistory.Contains(lastHitActor))
        {
            input.targetHistory.Add(lastHitActor);
        }
    }

    public void UpdateHPBarPos()
    {
        if (hpBar == null)
            return;
        if (((input.active && input.aiCtrl)) || !input.aiCtrl)
        {
            if (hpBar.type == PengHPBarUI.HPBarType.Enemy)
            {
                Vector3 posi = game.main.WorldToScreenPoint(transform.position + ctrl.height * Vector3.up + 0.2f * Vector3.up);
                posi.x -= Screen.width / 2;
                posi.y -= Screen.height / 2;
                hpBar.transform.localPosition = posi;
                hpBar.gameObject.SetActive(posi.z > 0);
            }
            else
            {
                if(!hpBar.gameObject.activeSelf) hpBar.gameObject.SetActive(true);
            }
            hpBar.hpBarBuffer.fillAmount = Mathf.Lerp(hpBar.hpBarBuffer.fillAmount, hpBar.hpBar.fillAmount, 2f * Time.deltaTime);
        }
    }

    public void UpdateHPBar()
    {
        if (hpBar == null)
            return;
        hpBar.hpBar.fillAmount = currentHP / m_maxHP;
    }

    private void OnDrawGizmos()
    {
        
    }
}

public class ReadOnlyAttribute : PropertyAttribute
{

}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif