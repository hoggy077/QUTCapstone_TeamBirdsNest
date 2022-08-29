using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    public MatchScore currentScore;
    private ScorecardUI scorecard;
    private GameStateManager gsm;
    private MatchManager mm;
    private List<GameObject> bowlsList;

    public TeamScriptable debugTeam1;
    public TeamScriptable debugTeam2;

    private int bowlsRolled = 0;

    public struct MatchScore
    {
        public int team1Sets;
        public int team2Sets;
        public int team1Ends;
        public int team2Ends;
        public int team1Shots;
        public int team2Shots;
    }

    // Initializing Match
    private void Start()
    {
        gsm = GameStateManager.Instance_;
        scorecard = FindObjectOfType<ScorecardUI>();
        mm = FindObjectOfType<MatchManager>();
        SetupStartingScores();
    }

    public void ReadTheHead()
    {
        List<GameObject> bowls = new List<GameObject>();
        GameObject jack = mm.GetJack();

        foreach(GameObject bowl in mm.GetLiveBowls())
        {
            // If its the first bowl to be read, add it on and move on
            if(bowls.Count < 1)
            {
                bowls.Add(bowl);
            }
            else
            {
                int currentIndex = 0;
                bool furthestAway = true;
                // Looping through bowls and sorting closest to the front
                foreach(GameObject preSavedBowl in bowls)
                {
                    if(DistanceToJack(bowl, jack) < DistanceToJack(preSavedBowl, jack))
                    {
                        furthestAway = false;
                        break;
                    }

                    currentIndex++;
                }

                // Once ideal order is identified, add to the list
                if(!furthestAway)
                {
                    bowls.Insert(currentIndex, bowl);
                }
                else
                {
                    bowls.Add(bowl);
                }
            }
        }

        UpdateShots(bowls);
    }

    // For the start of a new match
    private void SetupStartingScores()
    {
        currentScore.team1Ends = 0;
        currentScore.team2Ends = 0;
        currentScore.team1Shots = 0;
        currentScore.team2Shots = 0;
        currentScore.team1Sets = 0;
        currentScore.team2Sets = 0;

        // Setting teams for scorecard, or placing debug teams in there if menu was not used
        if(debugTeam1 != null)
        {
            gsm.UpdateTeam(1, debugTeam1);
        }

        if(debugTeam2 != null)
        {
            gsm.UpdateTeam(2, debugTeam2);
        }

        // Updating Scorecard
        scorecard.UpdateTeam1Info(gsm.Team_1.Name(), gsm.Team_1.BaseTeam.TeamColors[0]);
        scorecard.UpdateTeam2Info(gsm.Team_2.Name(), gsm.Team_2.BaseTeam.TeamColors[0]);

        scorecard.UpdateCurrentShots(1, 0);
    }

    private void UpdateShots(List<GameObject> bowls)
    {
        int currentWinningTeam = 0;
        int shots = 0;

        // Looping through the bowls and detecting the number of bowls between the first alternative team
        foreach(GameObject bowl in bowls)
        {
            if(currentWinningTeam == 0)
            {
                currentWinningTeam = bowl.GetComponent<BowlID>().GetTeam();
            }
            else if(currentWinningTeam != bowl.GetComponent<BowlID>().GetTeam())
            {
                break;
            }
            shots++;
        }

        scorecard.UpdateCurrentShots(currentWinningTeam, shots);
    }

    private float DistanceToJack(GameObject bowl, GameObject jack)
    {
        return Vector3.Distance(bowl.transform.position, jack.transform.position);
    }
}
