using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallZoneCollision : MonoBehaviour
{
    public Transform resetZone;

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
        print(hit);
        if (hit.gameObject.tag == "FallZone")
        {
            print("fall");
           transform.position = resetZone.position;
        }
    }

}
