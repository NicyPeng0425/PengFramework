using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PengLevel : MonoBehaviour
{
    public PengLevelRuntimeManager master;
    public int levelID;
    public Dictionary<int, PengLevelRuntimeFunction.BaseScript> scripts;
    public PengLevelRuntimeFunction.BaseScript current;
    private void Awake()
    {
        this.gameObject.tag = "PengLevel";
        LoadAllScripts(levelID);
        scripts[0].Execute();
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
