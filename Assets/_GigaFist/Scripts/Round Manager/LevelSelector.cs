using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GigaFist;

public class LevelSelector : MonoBehaviour //Just a simple script to load the selected level and call MatchManager
{
    public void SelectLevel(int levelIndex)
    {
        MatchManager.Instance.SetSelectLevel(levelIndex);
    }
}
