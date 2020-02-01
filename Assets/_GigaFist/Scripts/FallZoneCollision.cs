using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallZoneCollision : MonoBehaviour
{
    public Transform resetZone;
    public int fallCount;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "FallZone")
        {
            fallCount++;
           transform.position = resetZone.position;
        }
    }

}
