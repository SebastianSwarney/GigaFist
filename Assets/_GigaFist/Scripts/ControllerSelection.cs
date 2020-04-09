using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using GigaFist;
using UnityEngine.EventSystems;

public class ControllerSelection : MonoBehaviour
{
    public enum MenuState { PlayerSelect, RoundSelect }
    public MenuState state;
    public Rewired.Player controllerOneInput;
    public EventSystem eventSystem;

    public GameObject playerSelectFirstObject;
    public GameObject roundSelectFirstObject;

    // Start is called before the first frame update
    void Start()
    {
        if (controllerOneInput == null)
        {
            controllerOneInput = ReInput.players.GetPlayer(0);
        }

        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(playerSelectFirstObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (controllerOneInput != null)
        {
            switch (state)
            {
                case MenuState.PlayerSelect:
                    if (controllerOneInput.GetButtonDown("Punch"))
                    {
                        state = MenuState.RoundSelect;
                        if (roundSelectFirstObject != null && eventSystem != null)
                        {
                            eventSystem.firstSelectedGameObject = roundSelectFirstObject;
                            eventSystem.SetSelectedGameObject(roundSelectFirstObject);
                        }
                    }
                    break;

                case MenuState.RoundSelect:
                    if (controllerOneInput.GetButtonDown("Uppercut"))
                    {
                        state = MenuState.PlayerSelect;
                        if (playerSelectFirstObject != null && eventSystem != null)
                        {
                            eventSystem.firstSelectedGameObject = playerSelectFirstObject;
                            eventSystem.SetSelectedGameObject(playerSelectFirstObject);
                        }
                    }
                    break;
            }
        }
    }

    public void SelectPlayerCount(int count)
    {
        MatchManager.Instance.m_playersInCup = count;
    }

    public void SelectRoundCount(int count)
    {
        MatchManager.Instance.m_numberOfRounds = count;
        MatchManager.Instance.StartCup();
    }

}
