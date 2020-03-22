using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GigaFist;

public class LevelSelector : MonoBehaviour //Just a simple script to load the selected level and call MatchManager
{
    public bool autoSelectLevel1 = false;

    private void Start()
    {
        if (autoSelectLevel1)
        {
            Invoke("SelectLevel1", 1.5f);
        }
    }

    private void SelectLevel1()
    {
        MatchManager.Instance.SetSelectLevel((int)SceneIndexes.CHAR_TEST);
    }

    public void SelectLevel(int levelIndex)
    {
        MatchManager.Instance.SetSelectLevel(levelIndex);
    }
}
