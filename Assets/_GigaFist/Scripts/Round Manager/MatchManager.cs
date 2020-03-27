using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace GigaFist
{
    public class MatchManager : MonoBehaviour //Responsible for keeping track of the current Cup and the scoring, as well as beginning 
    {
        public static MatchManager Instance;
        public enum CupState { Idle, CupStart, StartRound, RoundInProgress, Intermission, CupEnd }
        // * CupStart: Create score tracking and go to level select
        // * StartRound: Launch game into a round
        // * RoundInProgress: wait back for a report from RoundManager for final scores from input devices
        // * Intermission: intermission scene or moment of pause where players can choose to continue or not (all press A to continue or something)
        // * CupEnd: all rounds in a Cup have been played, determine victor and go to win screen. Save stats.

        [Header("Cup Settings")]
        public CupState m_cupState;
        [SerializeField]
        private string m_cupSavePath;
        public string m_cupSaveFileName = "Cup ";
        [Space]

        [Range(2, 4)]
        public int m_playersInCup;
        [Range(1, 15)]
        public int m_numberOfRounds;
        public bool m_passIntermissionAutomatically = false;

        [Space]
        [Header("Configuration")]
        public float stateTickRate = 0.25f;
        public SceneIndexes scn_LevelSelect;
        public SceneIndexes scn_CupEnd;

        //Tracking
        private CupData m_cupData;
        private string m_cupID;
        private string m_cupFilePath;

        private int m_currentRound;
        private bool m_roundComplete = false;

        private bool intermissionComplete = false;

        private bool m_levelSelected = false;
        [HideInInspector]
        public SceneIndexes m_selectedLevelIndex;

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this);
            }
            StartCoroutine(CupStateController());
        }

        void Update()
        {
            //! Testing
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Unloaded Cup " + m_cupID);
                UnloadCupData();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadCup();
                Debug.Log("Loaded Cup " + m_cupID);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("Saved Cup " + m_cupID);
                SaveCup();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                StartCup();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                RoundData fake = new RoundData(m_playersInCup);
                fake.roundWinner = new PlayerData(10, 420);
                RoundComplete(fake);
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                CompleteIntermission();
            }
        }

        #region Cup State

        private IEnumerator CupStateController()
        {
            for (; ; )
            {
                switch (m_cupState)
                {
                    case CupState.Idle:
                        break;
                    case CupState.CupStart:
                        CupStart();
                        break;
                    case CupState.StartRound:
                        StartRound();
                        break;
                    case CupState.RoundInProgress:
                        RoundInProgress();
                        break;
                    case CupState.Intermission:
                        Intermission();
                        break;
                    case CupState.CupEnd:
                        CupEnd();
                        break;
                }
                yield return new WaitForSeconds(stateTickRate);
            }
        }

        private void ChangeCupState(CupState newState)
        {
            m_cupState = newState;
        }

        private void CupStart() // * CupStart: Create score tracking and go to level select
        {
            //Create the save data for the Cup
            CreateSave();

            //Reset Variables
            m_currentRound = 0;
            m_levelSelected = false;
            m_roundComplete = false;
            intermissionComplete = false;

            // Load scene selection level
            ChangeCupState(CupState.StartRound);
            SceneManager.instance.ChangeScene(scn_LevelSelect);
        }

        public void StartCup()
        {
            ChangeCupState(CupState.CupStart);
            Debug.Log("Cup Started!");
        }

        private void StartRound() // * StartRound: Launch game into a round
        {
            m_roundComplete = false;

            // Listen for level selection, and load appropriate level
            if (m_levelSelected)
            {
                SceneManager.instance.ChangeScene(m_selectedLevelIndex);
                m_currentRound++;
                ChangeCupState(CupState.RoundInProgress);
            }
            else //Go back to level select
            {
                //if (SceneManager.instance.currentScene != SceneIndexes.LEVEL_SELECT)
                //{
                //    SceneManager.instance.ChangeScene(SceneIndexes.LEVEL_SELECT);
                //}
            }
        }

        public void SetSelectLevel(int index)
        {
            //If the index given was actually valid
            if (Enum.IsDefined(typeof(SceneIndexes), index))
            {
                m_selectedLevelIndex = (SceneIndexes)index;
            }
            else //Otherwise, select a default level
            {
                m_selectedLevelIndex = SceneIndexes.LEVEL_ONE;
            }
            m_levelSelected = true;
        }

        private void RoundInProgress() // * RoundInProgress: wait back for a report from RoundManager for final scores from input devices
        {
            //Start round if it hasn't started
            if (m_roundComplete)
            {
                //Proceed onto the intermission step
                ChangeCupState(CupState.Intermission);
            }
            else
            {
                if (RoundManager.Instance != null)
                {
                    if (RoundManager.Instance.m_roundState != RoundManager.RoundState.In_Progress)
                    {
                        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == (int)m_selectedLevelIndex)
                        {
                            RoundManager.Instance.m_numberOfPlayers = m_playersInCup;
                            RoundManager.Instance.StartRound();
                        }
                    }
                }
            }
        }

        public void RoundComplete(RoundData roundData)
        {
            //Save the RoundData to the Cup data
            SaveRound(roundData, m_currentRound - 1);
            m_roundComplete = true;
        }

        private void Intermission() // * Intermission: intermission scene or moment of pause where players can choose to continue or not (all press A to continue or something)
        {
            //intermissionComplete = false;
            if (m_passIntermissionAutomatically)
            {
                intermissionComplete = true;
            }

            if (intermissionComplete)
            {
                CupState targetState = IsCupComplete() == true ? CupState.CupEnd : CupState.StartRound;
                intermissionComplete = false;
                m_levelSelected = false;
                ChangeCupState(targetState);
                if (targetState == CupState.StartRound)
                {
                    SceneManager.instance.ChangeScene(scn_LevelSelect);
                }
            }
        }

        public void CompleteIntermission()
        {
            intermissionComplete = true;
        }

        public bool IsCupComplete()
        {
            if (m_currentRound == m_numberOfRounds && m_roundComplete)
            {
                return true;
            }
            return false;
        }

        private void CupEnd() // * CupEnd: all rounds in a Cup have been played, determine victor and go to win screen. Save stats.
        {
            if (m_cupData != null)
            {
                m_cupData.CompleteCup();
                Debug.Log("Ultimate Victory goes to: Player " + m_cupData.cupWinner.playerID.ToString());
                SaveCup();
                ChangeCupState(CupState.Idle);
                //! Change This once levels are actually implemented
                SceneManager.instance.ChangeScene(scn_CupEnd);
            }
        }

        #endregion

        #region Cup Saving/Loading

        private void CreateSave()
        {
            //Get Folder Path
            // ! Change to Application.persistentDataPath for final build and update variables in Inspector
            string path = Application.dataPath + m_cupSavePath;

            //Assign Cup ID - dynamic so that it will be 'Cup 1', 'Cup 2', and so on
            m_cupID = (Directory.GetFiles(path, "*.json").Length + 1).ToString();

            //Update Path and set m_cupFilePath
            path = path + m_cupSaveFileName + " " + m_cupID + ".json";
            m_cupFilePath = path;

            //Create cupSave
            CupData cupSave = new CupData(m_cupID, m_numberOfRounds, m_playersInCup);
            m_cupData = cupSave;

            Debug.Log(path);
            //Save
            SaveCup(cupSave, path);
        }

        private void SaveCup()
        {
            SaveCup(m_cupData, m_cupFilePath);
        }

        private void SaveCup(CupData cupDataToSave, string path)
        {
            //Convert CupSave to JSON
            string saveContent = JsonUtility.ToJson(cupDataToSave, true);

            //Write to Path
            File.WriteAllText(path, saveContent);
        }

        private void UnloadCupData()
        {
            m_cupData = null;
        }

        private void LoadCup()
        {
            LoadCup(m_cupID);
        }

        private void LoadCup(string cupID)
        {
            // ! Change to Application.persistentDataPath for final build and update variables in Inspector
            string path = Application.dataPath + m_cupSavePath;

            //Technically the direct file path is already saved in m_cupFilePath, but this is practice to find it by just ID and folder path
            //Find the file by its cupID
            DirectoryInfo directory = new DirectoryInfo(path);
            FileInfo[] fileInfo = directory.GetFiles("*.json");
            foreach (FileInfo file in fileInfo)
            {
                if (file.Name.Contains(cupID))
                {
                    path = path + file.Name;
                }
            }

            string data = File.ReadAllText(path);
            m_cupData = JsonUtility.FromJson<CupData>(data);
        }

        #endregion

        #region Round Saving/Loading
        private void SaveRound(RoundData data, int roundNumber)
        {
            if (m_cupData == null)
            {
                Debug.Log("Load Cup");
                LoadCup();
            }

            if (m_cupData != null)
            {
                data.roundNumber = roundNumber;
                m_cupData.rounds[roundNumber] = data;
            }
        }

        #endregion
    }

    [System.Serializable]
    public class CupData
    {
        public RoundData[] rounds;
        public bool complete = false;
        public string cupID;
        public int numOfPlayers;
        public float cupTime;
        public PlayerData[] playerCumulativeCupData;
        public PlayerData cupWinner;

        public CupData(string cupIDString, int numberOfRounds, int numberOfPlayers)
        {
            //Initialize Array
            cupID = cupIDString;
            rounds = new RoundData[Mathf.Abs(numberOfRounds)];
            numOfPlayers = numberOfPlayers;
            for (int i = 0; i < rounds.Length; i++)
            {
                //Populate Array
                rounds[i] = new RoundData(numberOfPlayers);
            }
        }

        public void CompleteCup()
        {
            complete = true;

            //Determine Winner by determining list of players and adding up all their scores
            List<PlayerData> players = new List<PlayerData>();
            cupTime = 0;

            foreach (RoundData roundData in rounds)
            {
                cupTime += roundData.roundTime;
                //This should populate the list of players initially
                if (players.Count == 0)
                {
                    for (int i = 0; i < numOfPlayers; i++)
                    {
                        players.Add(new PlayerData(i));
                    }
                }
                
                if (players.Count != 0) //Afterwards, check for matching player IDs and add score up
                {
                    for (int i = 0; i < players.Count; i++) //Loop through all players found
                    {
                        for (int x = 0; x < roundData.players.Length; x++) //Loop through each round and Cup player IDs, adding up scores as you go
                        {
                            if (players[i].playerID == roundData.players[x].playerID) //If Matching IDs
                            {
                                players[i].AddScore(roundData.players[x].playerScore); //Add up scores and stats
                                players[i].punchCount += roundData.players[x].punchCount;
                                players[i].uppercutCount += roundData.players[x].uppercutCount;
                                players[i].hitEnemiesCount += roundData.players[x].hitEnemiesCount;
                                players[i].hitByEnemiesCount += roundData.players[x].hitByEnemiesCount;
                                players[i].timeAlive += roundData.players[x].timeAlive;
                                players[i].alive = false;
                            }
                        }
                    }
                }
            }

            //Find one with largest score and set them as Winner
            int largestPlayerScore = 0;
            int winningPlayerIndex = 0;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerScore >= largestPlayerScore)
                {
                    largestPlayerScore = players[i].playerScore;
                    winningPlayerIndex = i;
                }
            }

            playerCumulativeCupData = new PlayerData[numOfPlayers];
            playerCumulativeCupData = players.ToArray();
            cupWinner = players[winningPlayerIndex];
        }
    }

    [System.Serializable]
    public class RoundData
    {
        public int roundNumber = 0;
        public PlayerData[] players;
        public PlayerData roundWinner;
        public float roundTime;
        public int level;

        public RoundData(int numOfPlayers)
        {
            players = new PlayerData[numOfPlayers];
            for (int i = 0; i < numOfPlayers; i++)
            {
                players[i] = new PlayerData(i);
            }
        }

        public RoundData(int numOfPlayers, int _roundNumber)
        {
            players = new PlayerData[numOfPlayers];
            roundNumber = _roundNumber;
            for (int i = 0; i < numOfPlayers; i++)
            {
                players[i] = new PlayerData(i);
            }
        }

        public void SetPlayers(PlayerData[] newPlayers)
        {
            players = newPlayers;
        }
    }

    [System.Serializable]
    public class PlayerData
    {
        public int playerID;
        public int playerScore;
        public int punchCount;
        public int uppercutCount;
        public int hitEnemiesCount;
        public int hitByEnemiesCount;
        public bool alive = true;
        public float timeAlive;

        public PlayerData(int ID)
        {
            playerID = ID;
            ResetScore();
        }

        public PlayerData(int ID, int score)
        {
            playerID = ID;
            playerScore = score;
            punchCount = 0;
            uppercutCount = 0;
            hitEnemiesCount = 0;
            hitByEnemiesCount = 0;
        }

        public void ResetScore()
        {
            playerScore = 0;
        }

        public void AddScore(int amount)
        {
            playerScore += amount;
        }
    }
}
