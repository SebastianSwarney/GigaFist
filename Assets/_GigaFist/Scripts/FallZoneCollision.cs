using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallZoneCollision : MonoBehaviour
{
    public Transform resetZone;
    public bool fallen;
    public float timerLength;
    public float currentTime;


    //put this in ui script
    public GameObject fallText;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = timerLength;
    }

    // Update is called once per frame
    void Update()
    {

        if (fallen)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                transform.position = resetZone.position;
                fallText.SetActive(false);
                fallen = false;
                currentTime = timerLength;
            }
        }

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        print(hit);
        if (hit.gameObject.tag == "FallZone")
        {
            print("fall");
            fallen = true;
            fallText.SetActive(true);
        }


    }

}
