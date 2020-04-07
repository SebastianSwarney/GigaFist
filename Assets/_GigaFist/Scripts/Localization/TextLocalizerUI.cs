using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class TextLocalizerUI : MonoBehaviour
{
    TextMeshProUGUI textField;

    public LocalizedString localizedString;

    // Start is called before the first frame update
    void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (textField == null)
        {
            textField = GetComponent<TextMeshProUGUI>();
        }

        if (textField != null)
        {
            textField.SetText(localizedString.value);
        }
    }
}
