using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIBuildUp : MonoBehaviour
{
    public PlayerController playerScript;
    public Image fistBuildupImage;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        fistBuildupImage.fillAmount= playerScript.m_punchChargePercent;
    }
}
