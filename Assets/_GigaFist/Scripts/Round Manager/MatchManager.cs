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
        public string m_matchSavePath = "matchData.json";
        [Space]

        [Range(2, 4)]
        public int m_playersInMatch;
        [Range(1, 15)]
        public int m_numberOfRounds;
        public bool m_passIntermissionAutomatically = false;

        //Tracking
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
        }

        void Update()
        {

        }

        private void CreateSaves()
        {
            MatchSave matchSave = new MatchSave(m_numberOfRounds);
            string saveContent = JsonUtility.ToJson(matchSave);
            File.WriteAllText(Application.dataPath + m_matchSavePath, saveContent);
        }

        private void SaveRound()
        {

        }

        private void SaveMatch()
        {

        }
    }

    [System.Serializable]
    public class MatchSave
    {
        public RoundSave[] rounds;
        public PlayerSave matchWinner;

        public MatchSave(int numberOfRounds)
        {
            rounds = new RoundSave[Mathf.Abs(numberOfRounds)];
        }
    }

    [System.Serializable]
    public class RoundSave
    {
        public PlayerSave[] players;
        public PlayerSave roundWinner;
        public float matchTime;
        public int level;
    }

    [System.Serializable]
    public class PlayerSave
    {
        public int playerID;
        public int playerScore;
    }
}
