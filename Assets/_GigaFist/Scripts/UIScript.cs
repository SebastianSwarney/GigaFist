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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        playerText.text = ""+opponentFallCount;
       opponentText.text = "" + playerDeaths.fallCount;
    }
}
