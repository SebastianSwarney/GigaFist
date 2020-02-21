using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacerSelect : MonoBehaviour
{

    //change colours
    public Material[] playerColours;
    public Material currentColour;
    public int currentlySelected;
    MeshRenderer characterMesh;


    // Start is called before the first frame update
    void Start()
    {
        characterMesh = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //update material to currently selected colour (from an array of materials)
        currentColour = playerColours[currentlySelected];
        characterMesh.material = currentColour;

        if (Input.GetKeyDown(KeyCode.W))
        {
            //reset if going up after reached end of colour options
            if (currentlySelected == playerColours.Length-1)
            {
                currentlySelected = 0;
            }
            else { currentlySelected++; }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentlySelected == 0)
            {
                currentlySelected = playerColours.Length-1;
            }
            else { currentlySelected--; }

        }
    }


}
