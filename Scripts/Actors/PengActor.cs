using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class PengActor : MonoBehaviour
{
    [HideInInspector]
    public PengGameManager game;

    public int actorID;
    public string actorName;
    public int actorCamp;

    [HideInInspector]
    public static string initalName = "Idle";

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
    public PengBuffManager<PengActor> buff;

    private void Awake()
    {
        anim = this.GetComponent<Animator>();
        ctrl = this.GetComponent<CharacterController>();
        this.AddComponent<PengActorControl>();
        bb = new PengBlackBoard<PengActor>(this);
        buff = this.AddComponent<PengBuffManager<PengActor>>();
        buff.owner = this;
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
    }


    public void ProcessStateEnd()
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
}
