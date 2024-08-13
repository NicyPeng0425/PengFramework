using PengScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PengActor : MonoBehaviour
{
    [HideInInspector]
    public PengGameManager game;

    [ReadOnly]
    public int actorID;
    [ReadOnly]
    public string actorName;
    [ReadOnly]
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
                    change += buff.attackPowerPercent * m_attackPower;
                    change += buff.attackPowerValue;
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
                    change += buff.defendPowerPercent * m_defendPower;
                    change += buff.defendPowerValue;
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
                    change += buff.criticalRateValue;
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
                    change += buff.criticalDamageRatioValue;
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
    float m_maxHP;
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
                if (value >= m_maxHP) { m_currentHP = m_maxHP; }
                else if (value <= 0) { m_currentHP = 0; OnDie(); }
                else { m_currentHP = value; }
            }
        }
    }
    [HideInInspector]
    public bool underGravity
    {
        get {
            if (buff.buffs.Count > 0)
            {
                bool yes = false;
                foreach (PengBuff buff in buff.buffs)
                {
                    if (buff.notEffectedByGravity) { yes = true; }
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
    public bool isGrounded;

    private void Awake()
    {
        anim = this.GetComponent<Animator>();
        ctrl = this.GetComponent<CharacterController>();
        input = this.AddComponent<PengActorControl>();
        input.actor = this;
        bb = new PengBlackBoard<PengActor>(this);
        buff = this.AddComponent<PengBuffManager>();
        buff.actorOwner = this;
        LoadActorState();
    }
    // Start is called before the first frame update
    void Start()
    {
        game = GameObject.FindWithTag("PengGameManager").GetComponent<PengGameManager>();
        game.actors.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (actorStates.Count > 0)
        {
            current.OnUpdate();
        }
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