using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengLevelRuntimeManager : MonoBehaviour
{
    [HideInInspector]
    public List<PengLevel> levels;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] lvlGOs = GameObject.FindGameObjectsWithTag("PengLevel");
        for (int i = 0; i < lvlGOs.Length; i++)
        {
            AddNewLevel(lvlGOs[i].GetComponent<PengLevel>());
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
