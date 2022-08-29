using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [Header("Debugging")]
    // Placeholder Team SOs to allow for quick testing without heading back to menu
    public TeamScriptable debugTestTeam1;
    public TeamScriptable debugTestTeam2;

    public MatchScore currentScore;
    private ScorecardUI scorecard;
    private GameStateManager gsm;

    // Structure to manage current scoring variables
    public struct MatchScore
    {
        public int team1EndsWon;
        public int team1SetsWon;
        public int team1CurrentShots;

        public int team2EndsWon;
        public int team2SetsWon;
        public int team2CurrentShots;
    }

    // Initialize Scoring 
    private void Start()
    {
        currentScore = new MatchScore();

        scorecard = FindObjectOfType<ScorecardUI>();
        gsm = GameStateManager.Instance_;
        Debug.Log(gsm.name);

        // Setup Starting Scores and Teams
        SetupStartingScore();
    }

    // Zeroing out scores, setting teams to correct
    public void SetupStartingScore()
    {
        // Resetting Scores
        currentScore.team1EndsWon = 0;
        currentScore.team1SetsWon = 0;
        currentScore.team1CurrentShots = 0;

        currentScore.team2EndsWon = 0;
        currentScore.team2SetsWon = 0;
        currentScore.team2CurrentShots = 0;

        // Setting Team Information to Display, and setting placeholders if the menu has not been used
        if(debugTestTeam1 != null)
        {
            gsm.UpdateTeam(1, debugTestTeam1);
        }
        
        if(debugTestTeam2 != null)
        {
            gsm.UpdateTeam(2, debugTestTeam2);
        }

        scorecard.UpdateTeam1Info(gsm.Team_1.Name(), gsm.Team_1.BaseTeam.TeamColors[0]);
        scorecard.UpdateTeam2Info(gsm.Team_2.Name(), gsm.Team_2.BaseTeam.TeamColors[0]);
    }
}
