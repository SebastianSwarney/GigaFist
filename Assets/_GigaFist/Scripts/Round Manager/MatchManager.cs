using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace GigaFist
{
    public class MatchManager : MonoBehaviour //Responsible for keeping track of the current match and the scoring, as well as beginning 
    {

        public static MatchManager Instance;
        public enum MatchState { MatchStart, StartRound, RoundInProgress, Intermission, MatchEnd }
        // * MatchStart: Create score tracking for each player based on Input device and anything else
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

        //Tracking
        [SerializeField]
        private MatchData m_matchData;
        [SerializeField]
        private string m_matchID;
        private string m_matchFilePath;
        private int m_currentRound;

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

            CreateSave();
        }

        void Update()
        {
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
        }

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

        public void UnloadMatchData()
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
        private void SaveRound()
        {

        }

        #endregion

    }

    [System.Serializable]
    public class MatchData
    {
        public RoundData[] rounds;
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
