using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengActor : MonoBehaviour
{

    public PengGameManager game;


    public static string initalName = "Idle";

    //运行时动态数据
    public string currentName;
    public string lastName;
    public Dictionary<string, IPengActorState> actorStates;
    public IPengActorState current;
    public IPengActorState last;
    public float pauseTime;
    public bool alive = true;


    private void Awake()
    {
        game = GameObject.FindWithTag("PengGameManager").GetComponent<PengGameManager>();
        game.actors.Add(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        
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
