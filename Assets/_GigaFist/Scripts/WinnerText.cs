using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinnerText : MonoBehaviour
{
    public TMPro.TextMeshProUGUI text;

    private void Start()
    {
        if (GigaFist.MatchManager.Instance != null)
        {
            if (text != null)
            {
                text.SetText(GigaFist.MatchManager.Instance.GetWinningPlayerNumber().ToString());
            }
        }
    }
}
