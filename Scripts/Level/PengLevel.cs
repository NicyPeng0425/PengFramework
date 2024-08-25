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
    [ReadOnly]
    public string levelName;
    [ReadOnly]
    public string info;
    public Dictionary<int, PengLevelRuntimeFunction.BaseScript> scripts = new Dictionary<int, PengLevelRuntimeFunction.BaseScript>();
    public PengLevelRuntimeFunction.BaseScript current;

    [Label("空气墙子物体")]
    public List<GameObject> airWalls = new List<GameObject>();
    private void Awake()
    {
        this.gameObject.tag = "PengLevel";
        LoadAllScripts(levelID);
        scripts.ElementAt(0).Value.Execute();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        current.Execute();
    }

    public void ChangeScript(int ID)
    {
        current = scripts[ID];
        current.Enter();
    }
}
