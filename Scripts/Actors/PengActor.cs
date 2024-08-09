using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengActor : MonoBehaviour
{
    [HideInInspector]
    public PengGameManager game;

    public int actorID;

    [HideInInspector]
    public static string initalName = "Idle";

    //运行时动态数据
    [HideInInspector]
    public string currentName;
    [HideInInspector]
    public string lastName;
    public Dictionary<string, IPengActorState> actorStates;
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


    private void Awake()
    {
        

        anim = this.GetComponent<Animator>();
        ctrl = this.GetComponent<CharacterController>();
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
        current.OnUpdate();
    }

    public void TransState(string name)
    {
        if (current != null)
        {
            lastName = currentName;
            last = current;
            last.OnExit();
        }
        currentName = name;
        current = actorStates[name];
        current.OnEnter();
    }
}
