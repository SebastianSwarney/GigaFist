using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIBuildUp : MonoBehaviour
{
    public PlayerController playerScript;
    public Image fistBuildupImage;




    // Update is called once per frame
    void Update()
    {
        //Charge up effect for punch
        //fill amount of chargeup based on current punchcharge percent
        fistBuildupImage.fillAmount= playerScript.m_punchChargePercent;
    }
}
