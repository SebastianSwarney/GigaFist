using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionScript : MonoBehaviour
{
    public GameObject selectedUI;
    public float offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        selectedUI = EventSystem.current.currentSelectedGameObject;
        transform.position = new Vector3(transform.position.x, selectedUI.transform.position.y+offset, transform.position.z);


    }
}
