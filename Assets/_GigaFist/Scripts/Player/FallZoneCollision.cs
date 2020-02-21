using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallZoneCollision : MonoBehaviour
{
    public Transform resetZone;
    public int fallCount;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "FallZone")
        {
            fallCount++;
           transform.position = resetZone.position;
        }
    }

}
