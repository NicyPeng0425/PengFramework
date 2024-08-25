using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Cinemachine.DocumentationSortingAttribute;

public class PengLevelRuntimeManager : MonoBehaviour
{
    [HideInInspector]
    public List<PengLevel> levels = new List<PengLevel>();
    [HideInInspector]
    public PengGameManager game;

    private void Awake()
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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddNewLevel(PengLevel level)
    {
        level.master = this;
        level.LoadAllScripts(level.levelID);
        levels.Add(level);
        level.scripts.ElementAt(0).Value.Execute();
    }
}
