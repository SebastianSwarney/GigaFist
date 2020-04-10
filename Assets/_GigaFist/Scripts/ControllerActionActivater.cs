using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class ControllerActionActivater : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LocalizationSystem.SetLanguage(LocalizationSystem.Language.English, true);
            Debug.Log("Set Language to English");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LocalizationSystem.SetLanguage(LocalizationSystem.Language.French, true);
            Debug.Log("Set Language to French");
        }
    }
}
