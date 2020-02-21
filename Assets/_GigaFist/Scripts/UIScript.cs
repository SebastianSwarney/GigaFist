using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScript : MonoBehaviour
{
    public TextMeshProUGUI playerText;
    public TextMeshProUGUI opponentText;
    //amount of falls for player 1
    public FallZoneCollision playerDeaths;
    public int playerFallCount;
    public int opponentFallCount;

    // Update is called once per frame
    void Update()
    {

        playerText.text = ""+opponentFallCount;
       opponentText.text = "" + playerDeaths.fallCount;
    }
}
