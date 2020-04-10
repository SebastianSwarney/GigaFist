using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionScript : MonoBehaviour
{
    public GameObject selectedUI;
    public float offset;

    // Update is called once per frame
    void Update()
    {
        //set selection UI to align with the currently selected UI element
        selectedUI = EventSystem.current.currentSelectedGameObject;
        transform.position = new Vector3(transform.position.x, selectedUI.transform.position.y+offset, transform.position.z);


    }
}
