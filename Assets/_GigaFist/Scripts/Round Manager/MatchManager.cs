using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace GigaFist
{
    public class MatchManager : MonoBehaviour //Responsible for keeping track of the current match and the scoring, as well as beginning 
    {

        public static MatchManager Instance;
        public enum MatchState { Idle, MatchStart, StartRound, RoundInProgress, Intermission, MatchEnd }
        // * MatchStart: Create score tracking and go to level select
        // * StartRound: Launch game into a round
        // * RoundInProgress: wait back for a report from RoundManager for final scores from input devices
        // * Intermission: intermission scene or moment of pause where players can choose to continue or not (all press A to continue or something)
        // * MatchEnd: all rounds in a match have been played, determine victor and go to win screen. Save stats.

        [Header("Match Settings")]
        public MatchState m_matchState;
        public string m_matchSavePath;
        public string m_matchSaveFileName = "Match ";
        [Space]

        [Range(2, 4)]
        public int m_playersInMatch;
        [Range(1, 15)]
        public int m_numberOfRounds;
        public bool m_passIntermissionAutomatically = false;

        [Space]
        [Header("Configuration")]
        public float stateTickRate = 0.25f;


        //Tracking
        private MatchData m_matchData;
        private string m_matchID;
        private string m_matchFilePath;

        private int m_currentRound;
        private bool m_roundComplete = false;

        private bool intermissionComplete = false;

        private bool m_levelSelected = false;
        private SceneIndexes m_selectedLevelIndex;

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
            StartCoroutine(MatchStateController());
        }

        void Update()
        {
            //! Testing
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("Unloaded Match " + m_matchID);
                UnloadMatchData();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                LoadMatch();
                Debug.Log("Loaded Match " + m_matchID);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("Saved Match " + m_matchID);
                SaveMatch();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartMatch();
            }
        }

        #region Match State

        private IEnumerator MatchStateController()
        {
            for (; ; )
            {
                switch (m_matchState)
                {
                    case MatchState.Idle:
                        break;
                    case MatchState.MatchStart:
                        MatchStart();
                        break;
                    case MatchState.StartRound:
                        StartRound();
                        break;
                    case MatchState.RoundInProgress:
                        RoundInProgress();
                        break;
                    case MatchState.Intermission:
                        Intermission();
                        break;
                    case MatchState.MatchEnd:
                        MatchEnd();
                        break;
                }
                yield return new WaitForSeconds(stateTickRate);
            }
        }

        private void ChangeMatchState(MatchState newState)
        {
            m_matchState = newState;
        }

        private void MatchStart() // * MatchStart: Create score tracking and go to level select
        {
            //Create the save data for the match
            CreateSave();

            //Reset Variables
            m_currentRound = 1;
            m_levelSelected = false;
            m_roundComplete = false;
            intermissionComplete = false;

            // Load scene selection level
            ChangeMatchState(MatchState.StartRound);
            SceneManager.instance.ChangeScene(SceneIndexes.LEVEL_SELECT);
        }

        public void StartMatch()
        {
            ChangeMatchState(MatchState.MatchStart);
            Debug.Log("Match Started!");
        }

        private void StartRound() // * StartRound: Launch game into a round
        {
            m_roundComplete = false;

            // Listen for level selection, and load appropriate level
            if (m_levelSelected)
            {
                ChangeMatchState(MatchState.RoundInProgress);
                SceneManager.instance.ChangeScene(m_selectedLevelIndex);
            }
        }

        public void SelectLevel(int index)
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
            //There's really nothing to do except wait

            if (m_roundComplete)
            {
                //Proceed onto the intermission step
                ChangeMatchState(MatchState.Intermission);
            }
        }

        public void RoundComplete(RoundData roundData)
        {
            //Save the RoundData to the match data
            SaveRound(roundData, m_currentRound - 1);
            m_roundComplete = true;
        }

        private void Intermission() // * Intermission: intermission scene or moment of pause where players can choose to continue or not (all press A to continue or something)
        {
            intermissionComplete = false;
            if (m_passIntermissionAutomatically)
            {
                intermissionComplete = true;
            }

            if (intermissionComplete)
            {
                MatchState targetState = IsMatchComplete() == true ? MatchState.MatchEnd : MatchState.StartRound;
                ChangeMatchState(targetState);
            }
        }

        public void CompleteIntermission()
        {
            intermissionComplete = true;
        }

        public bool IsMatchComplete()
        {
            if (m_currentRound == m_numberOfRounds && m_roundComplete)
            {
                return true;
            }
            return false;
        }

        private void MatchEnd() // * MatchEnd: all rounds in a match have been played, determine victor and go to win screen. Save stats.
        {
            if (m_matchData != null)
            {
                m_matchData.CompleteMatch();
                Debug.Log("Ultimate Victory goes to: " + m_matchData.matchWinner);
                SaveMatch();
                ChangeMatchState(MatchState.Idle);
                //! Change This once levels are actually implemented
                SceneManager.instance.ChangeScene(SceneIndexes.LOADING);
            }
        }

        #endregion

        #region Match Saving/Loading

        private void CreateSave()
        {
            //Get Folder Path
            // ! Change to Application.persistentDataPath for final build and update variables in Inspector
            string path = Application.dataPath + m_matchSavePath;

            //Assign Match ID - dynamic so that it will be 'Match 1', 'Match 2', and so on
            m_matchID = (Directory.GetFiles(path, "*.json").Length + 1).ToString();

            //Update Path and set m_matchFilePath
            path = path + m_matchSaveFileName + " " + m_matchID + ".json";
            m_matchFilePath = path;

            //Create MatchSave
            MatchData matchSave = new MatchData(m_matchID, m_numberOfRounds, m_playersInMatch);

            Debug.Log(path);
            //Save
            SaveMatch(matchSave, path);
        }

        private void SaveMatch()
        {
            SaveMatch(m_matchData, m_matchFilePath);
        }

        private void SaveMatch(MatchData matchDataToSave, string path)
        {
            //Convert MatchSave to JSON
            string saveContent = JsonUtility.ToJson(matchDataToSave);

            //Write to Path
            File.WriteAllText(path, saveContent);
        }

        private void UnloadMatchData()
        {
            m_matchData = null;
        }

        private void LoadMatch()
        {
            LoadMatch(m_matchID);
        }

        private void LoadMatch(string matchID)
        {
            // ! Change to Application.persistentDataPath for final build and update variables in Inspector
            string path = Application.dataPath + m_matchSavePath;

            //Technically the direct file path is already saved in m_matchFilePath, but this is practice to find it by just ID and folder path
            //Find the file by its matchID
            DirectoryInfo directory = new DirectoryInfo(path);
            FileInfo[] fileInfo = directory.GetFiles("*.json");
            foreach (FileInfo file in fileInfo)
            {
                if (file.Name.Contains(matchID))
                {
                    path = path + file.Name;
                }
            }

            string data = File.ReadAllText(path);
            m_matchData = JsonUtility.FromJson<MatchData>(data);
        }

        #endregion

        #region Round Saving/Loading
        private void SaveRound(RoundData data, int roundNumber)
        {
            if (m_matchData == null)
            {
                LoadMatch();
            }

            if (m_matchData != null)
            {
                m_matchData.rounds[roundNumber] = data;
            }
        }

        #endregion

    }

    [System.Serializable]
    public class MatchData
    {
        public RoundData[] rounds;
        public bool complete = false;
        public string matchID;
        public int numOfPlayers;
        public PlayerData matchWinner;

        public MatchData(string matchIDString, int numberOfRounds, int numberOfPlayers)
        {
            //Initialize Array
            matchID = matchIDString;
            rounds = new RoundData[Mathf.Abs(numberOfRounds)];
            numOfPlayers = numberOfPlayers;
            for (int i = 0; i < rounds.Length; i++)
            {
                //Populate Array
                rounds[i] = new RoundData(numberOfPlayers);
            }
        }

        public void CompleteMatch()
        {
            complete = true;

            //Determine Winner by determining list of players and adding up all their scores
            List<PlayerData> players = new List<PlayerData>();

            foreach (RoundData roundData in rounds)
            {
                //This should populate the list of players initially
                if (players.Count == 0)
                {
                    for (int i = 0; i < roundData.players.Length; i++)
                    {
                        players.Add(roundData.players[i]);
                    }
                }
                else //Afterwards, check for matching player IDs and add score up
                {
                    for (int i = 0; i < players.Count; i++) //Loop through all players found
                    {
                        for (int x = 0; x < roundData.players.Length; x++) //Loop through each round and match player IDs, adding up scores as you go
                        {
                            if (players[i].playerID == roundData.players[x].playerID) //If Matching IDs
                            {
                                players[i].AddScore(roundData.players[x].playerScore); //Add up scores
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

            matchWinner = players[winningPlayerIndex];
        }
    }

    [System.Serializable]
    public class RoundData
    {
        public PlayerData[] players;
        public PlayerData roundWinner;
        public float matchTime;
        public int level;

        public RoundData(int numOfPlayers)
        {
            players = new PlayerData[numOfPlayers];
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

        public PlayerData(int ID)
        {
            playerID = ID;
            ResetScore();
        }

        public PlayerData(int ID, int score)
        {
            playerID = ID;
            playerScore = score;
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
