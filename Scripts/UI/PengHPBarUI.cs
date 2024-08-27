using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PengHPBarUI : MonoBehaviour
{
    public enum HPBarType
    {
        Player,
        Enemy,
        Boss,
    }

    [HideInInspector]
    public PengActor master;
    [HideInInspector]
    public HPBarType type;

    public Image hpBar;
    public Image hpBarBuffer;
    public Text bossName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
