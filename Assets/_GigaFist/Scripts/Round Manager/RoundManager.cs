using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GigaFist;

public class RoundManager : MonoBehaviour //Responsible for managing the beginning and end of a specific round, and report winner to MatchManager
{
    public static RoundManager Instance;

    public enum RoundState {Idle, In_Progress, Complete}
    public RoundState m_roundState;

    public int m_numberOfPlayers;

    public Vector3[] m_spawnPositions;
    public GameObject m_playerPrefab;

    [Header("Round Countdown Properties")]
    public int m_roundStartCountdownTime;
    public Text m_countdownText;
    public GameObject m_countdownObject;

    private List<PlayerController> m_players = new List<PlayerController>();
    private RoundData roundData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        ChangeRoundState(RoundState.Idle);
    }

    private void Update()
    {
        //! Testing
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            PlayerWon(0);
        }

        if (m_roundState == RoundState.Idle)
        {
            // ! Horrible temporary code lol
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == (int)SceneIndexes.CHAR_TEST)
            {
                if (MatchManager.Instance != null)
                {
                    if (MatchManager.Instance.m_cupState == MatchManager.CupState.RoundInProgress) //If the round should be in progress
                    {
                        m_numberOfPlayers = MatchManager.Instance.m_playersInCup;
                        StartRound();
                    }
                }
                else
                {
                    StartRound();
                }
            }
        }

        if (m_roundState == RoundState.Complete)
        {
            ChangeRoundState(RoundState.Idle);
            if (MatchManager.Instance != null)
            {
                DestroyAllPlayers();
                MatchManager.Instance.RoundComplete(roundData);
            }
        }
    }

    #region Round Control & Setup

    public void ChangeRoundState(RoundState newState)
    {
        if (newState != m_roundState)
        {
            m_roundState = newState;
        }
    }

    public void StartRound()
    {
        ChangeRoundState(RoundState.In_Progress);
        StartCoroutine(RunRoundStartUp());
    }

    private IEnumerator RunRoundStartUp()
    {

        SpawnPlayers();
        FreezePlayers();

        if (MatchManager.Instance != null)
        {
            roundData = new RoundData(MatchManager.Instance.m_playersInCup);
        }
        else
        {
            roundData = new RoundData(m_numberOfPlayers);
        }
        

        m_countdownObject.gameObject.SetActive(true);

        int t = m_roundStartCountdownTime + 1;

        while (t > 0)
        {
            t--;

            m_countdownText.text = t.ToString();
            yield return new WaitForSeconds(1f);
        }

        m_countdownObject.gameObject.SetActive(false);

        UnFreezePlayers();
    }

    private void SpawnPlayers()
    {
        Debug.Log("Spawn them bitches");
        m_players = new List<PlayerController>();
        for (int i = 0; i < m_numberOfPlayers; i++)
        {
            m_players.Add(Instantiate(m_playerPrefab, m_spawnPositions[i], Quaternion.identity).GetComponent<PlayerController>());
            m_players[i].RunRoundSetup(i, m_numberOfPlayers);
            Debug.Log(m_players[i].gameObject.name, m_players[i].gameObject);
        }
    }

    #endregion

    #region Player Control
    private void FreezePlayers()
    {
        foreach (PlayerController player in m_players)
        {
            player.FreezeSelf();
        }
    }

    private void UnFreezePlayers()
    {
        foreach (PlayerController player in m_players)
        {
            player.UnFreezeSelf();
        }
    }

    private void DestroyAllPlayers()
    {
        if (m_players != null)
        {
            foreach (PlayerController player in m_players)
            {
                Destroy(player.gameObject);
            }
        }
    }

    public void OnPlayerDeath()
    {
        CheckPlayers();
    }

    private void CheckPlayers()
    {
        int deadPlayers = 0;
        int alivePlayerIndex = 0;

        if (m_numberOfPlayers == 1)
        {
            Debug.Log("Shit no one won LMAO");

            return;
        }

        for (int i = 0; i < m_players.Count; i++)
        {
            if (m_players[i].m_states.m_aliveState == PlayerController.AliveState.IsDead)
            {
                deadPlayers++;
            }
            else
            {
                alivePlayerIndex = i;
            }
        }

        if (deadPlayers == m_numberOfPlayers - 1)
        {
            PlayerWon(alivePlayerIndex);
        }
    }

    private void PlayerWon(int p_winningPlayer)
    {
        Debug.Log("Player " + p_winningPlayer + " wins!!!!!!");
        ChangeRoundState(RoundState.Complete);
    }

    #endregion

    private void OnDrawGizmos()
    {
        foreach (Vector3 position in m_spawnPositions)
        {
            Gizmos.DrawWireSphere(position, 1f);
        }
    }
}
