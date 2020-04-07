using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationChanger : MonoBehaviour
{
    public LocalizationSystem.Language language;

    public void ChangeLanguage(int language) //Public method to change the language to a specified one (e.g On Button Clicks)
    {
        if (System.Enum.IsDefined(typeof(LocalizationSystem.Language), language))
        {
            LocalizationSystem.SetLanguage((LocalizationSystem.Language)language, true);
        }
    }
}
