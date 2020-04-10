using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GigaFist;

public class LevelSelector : MonoBehaviour //Just a simple script to load the selected level and call MatchManager
{
    public bool autoSelectLevel1 = false;
    public bool autoSelectLevel2 = false;

    private void Start()
    {
        if (autoSelectLevel1)
        {
            Invoke("SelectLevel1", 1f);
        }
        else if (autoSelectLevel2)
        {
            Invoke("SelectLevel2", 1f);
        }
    }

    private void SelectLevel1()
    {
        MatchManager.Instance.SetSelectLevel((int)SceneIndexes.CHAR_TEST);
    }

    private void SelectLevel2()
    {
        MatchManager.Instance.SetSelectLevel((int)SceneIndexes.LEVEL_ONE);
    }

    public void SelectLevel(int levelIndex)
    {
        GigaFist.SceneManager.instance.ChangeScene((GigaFist.SceneIndexes)levelIndex);
    }
}
