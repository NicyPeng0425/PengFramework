using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengEffectManager : MonoBehaviour
{
    public PengActor actor;
    public PengGameManager game;
    public ParticleSystem ps;
    public Transform followTarget;
    public Vector3 posOffset;
    public Vector3 rotOffset;
    public Vector3 scaleOffset;
    public float time = 0;
    public float deleteTime = 0;
    public string path;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (followTarget != null)
        {
            this.transform.SetParent(followTarget, false);
            this.transform.localPosition = posOffset;
            this.transform.localRotation = Quaternion.Euler(rotOffset);
            this.transform.localScale = scaleOffset;
            this.transform.SetParent(game.vfxRoot, true);
        }
        time += Time.deltaTime;
        if (time > deleteTime)
        {
            ps.Stop();
            this.gameObject.SetActive(false);
        }
    }
}
