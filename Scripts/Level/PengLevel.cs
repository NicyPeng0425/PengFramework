using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class PengLevel : MonoBehaviour
{
    [HideInInspector]
    public PengLevelRuntimeManager master;
    [ReadOnly]
    public int levelID;
    public string levelName;
    public string info;
    public Dictionary<int, PengLevelRuntimeFunction.BaseScript> scripts = new Dictionary<int, PengLevelRuntimeFunction.BaseScript>();
    public PengLevelRuntimeFunction.BaseScript current;

    [Label("空气墙子物体")]
    public List<GameObject> airWalls = new List<GameObject>();

    [HideInInspector]
    public List<PengActor> currentEnemy = new List<PengActor>();
    private void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.tag != "PengLevel")
        {
            gameObject.tag = "PengLevel";
            master = GameObject.FindWithTag("PengGameManager").GetComponent<PengLevelRuntimeManager>();
            master.AddNewLevel(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (current != null)
        {
            current.Execute();
        }
    }

    public void ChangeScript(int ID)
    {
        current = scripts[ID];
        current.Enter();
    }
}
