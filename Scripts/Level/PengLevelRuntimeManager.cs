using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PengLevelRuntimeManager : MonoBehaviour
{
    [HideInInspector]
    public List<PengLevel> levels = new List<PengLevel>();
    [HideInInspector]
    public PengGameManager game;
    [HideInInspector]
    PengLevel defaultLevel;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] lvlGOs = GameObject.FindGameObjectsWithTag("PengLevel");
        for (int i = 0; i < lvlGOs.Length; i++)
        {
            PengLevel lvl = lvlGOs[i].GetComponent<PengLevel>();
            if (lvl != null)
            {
                AddNewLevel(lvl);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddNewLevel(PengLevel level)
    {
        level.master = this;
        levels.Add(level);
    }
}
