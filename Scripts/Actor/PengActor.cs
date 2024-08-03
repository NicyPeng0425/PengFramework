using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengActor : MonoBehaviour
{
    public PengGameManager game;

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
        
    }
}
