using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoringManager : MonoBehaviour
{
    public MatchScore currentScore;
    private ScorecardUI scorecard;
    private GameStateManager gsm;
    private MatchManager mm;
    public ClosestBowlRing ringPrefab;
    private ClosestBowlRing bowlUIRing;

    public TeamScriptable debugTeam1;
    public TeamScriptable debugTeam2;

    private int currentEnd = 1;
    public int endsPerSet = 5;

    public struct MatchScore
    {
        public int team1Sets;
        public int team2Sets;
        public int team1Ends;
        public int team2Ends;
    }

    // Initializing Match
    private void Start()
    {
        gsm = GameStateManager.Instance;
        scorecard = FindObjectOfType<ScorecardUI>();
        mm = FindObjectOfType<MatchManager>();
        SetupStartingScores();
        bowlUIRing = Instantiate(ringPrefab.gameObject, transform.position, Quaternion.Euler(Vector3.zero)).GetComponent<ClosestBowlRing>();
        bowlUIRing.ToggleRing(false);
    }

    public void ReadTheHead()
    {
        // Assembling bowls and jack references
        List<GameObject> bowls = new List<GameObject>();
        GameObject jack = mm.GetJack();

        // Calculating and Rearranging list from closest to furthest away from jack
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

        // Checking which team holds the shots
        int[] teamAndScore = UpdateShots(bowls);

        // Placing UI ring at closest bowl if it exists
        if (bowls.Count > 0)
        {
            bowlUIRing.ToggleRing(true);
            bowlUIRing.UpdateRing(jack.transform, bowls[0].GetComponent<BowlID>());
        }

        // Updating Current Bowls remaining for each team, finding if the end has concluded
        bool continueEnd = UpdateShotsRemaining(mm.GetLiveBowls());

        // Checking if the end is over, and if so, updating scorecard
        if(!continueEnd)
        {
            // Adding current shots to end score
            if(teamAndScore[0] == 1)
            {
                currentScore.team1Ends += teamAndScore[1];
            }
            else
            {
                currentScore.team2Ends += teamAndScore[1];
            }

            // Updating Scorecard End Scores
            scorecard.UpdateEndsWon(currentScore.team1Ends, currentScore.team2Ends);

            // Updating Scorecard Current End
            currentEnd++;

            // If still within set update scorecard and reset counts
            if(currentEnd <= endsPerSet)
            {
                scorecard.UpdateEndNumber(currentEnd);
                StartNewEnd();
            }
            else
            {
                StartNewSet();
            }
        }
    }

    // For the start of a new match
    private void SetupStartingScores()
    {
        currentScore.team1Ends = 0;
        currentScore.team2Ends = 0;
        currentScore.team1Sets = 0;
        currentScore.team2Sets = 0;

        // Setting teams for scorecard, or placing debug teams in there if menu was not used
        if (GameStateManager.Instance_.Team_1 != null && GameStateManager.Instance_.Team_2 != null)
        {
            gsm.UpdateTeam(1, gsm.Team_1.BaseTeam);
            gsm.UpdateTeam(2, gsm.Team_2.BaseTeam);
        }
        else
        {
            // Getting Debug Testing Team Info
            gsm.UpdateTeam(1, debugTeam1);
            gsm.UpdateTeam(2, debugTeam2);
        }

        // Updating Scorecard
        scorecard.UpdateTeam1Info(gsm.Team_1.Name(), gsm.Team_1.BaseTeam.TeamColors[0]);
        scorecard.UpdateTeam2Info(gsm.Team_2.Name(), gsm.Team_2.BaseTeam.TeamColors[0]);

        scorecard.UpdateCurrentShots(1, 0);

        // Updating End Number
        currentEnd = 1;
        scorecard.UpdateEndNumber(currentEnd);
    }

    // For starting a new end
    private void StartNewEnd(bool tiebreaker = false)
    {
        // Delete and reset bowl list
        mm.CleanUpBowls();

        // Resetting Score
        scorecard.UpdateCurrentShots(1, 0);

        // Update Displays
        scorecard.UpdateEndNumber(currentEnd, tiebreaker);
        bowlUIRing.ToggleRing(false);
    }

    // For the finishing and starting of a new set
    private void StartNewSet()
    {
        // Determining winner of set, and updating scorecard
        if (currentScore.team1Ends > currentScore.team2Ends)
        {
            currentScore.team1Sets++;
        }
        else if (currentScore.team1Ends < currentScore.team2Ends)
        {
            currentScore.team2Sets++;
        }
        else
        {
            // Resetting End for Tiebreaker
            currentEnd--;
            StartNewEnd();
            scorecard.UpdateEndNumber(currentEnd, true);
            return;
        }

        // Update Scorecard
        scorecard.UpdateSetsWon(currentScore.team1Sets, currentScore.team2Sets);

        // Resetting End Scores
        currentScore.team1Ends = 0;
        currentScore.team2Ends = 0;

        // If two sets completed and there is a clean cut winner, end the game
        if(currentScore.team1Sets + currentScore.team2Sets >= 2 && currentScore.team1Sets != currentScore.team2Sets)
        {
            FinishMatch();
        }

        // Start Tiebreaker FINALE if required
        else if(currentScore.team1Sets + currentScore.team2Sets >= 2)
        {
            currentEnd--;
            scorecard.UpdateEndsWon(0, 0);
            StartNewEnd(true);
        }

        // If not final set, move onto next one
        else
        {
            currentEnd = 1;
            scorecard.UpdateEndsWon(0, 0);
            StartNewEnd();
        }
    }

    // Function to handle the updating and calculation of the current shots held by a leading team
    private int[] UpdateShots(List<GameObject> bowls)
    {
        int[] output = new int[2];
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

        // Generating output for tracking
        output[0] = currentWinningTeam;
        output[1] = shots;

        return output;
    }

    // Function to handle concluding of match
    private void FinishMatch()
    {
        SceneManager.LoadScene(0);
    }

    // Function to handle updating of scorecard and calculation of remaining shots for each team
    private bool UpdateShotsRemaining(List<GameObject> bowls)
    {
        // Start with total shots for each team allowed
        int team1ShotsRemaining = 6;
        int team2ShotsRemaining = 6;

        // Negate one shot for each team bowl present on the field
        foreach(GameObject bowl in bowls)
        {
            if(bowl.GetComponent<BowlID>().GetTeam() == 1)
            {
                team1ShotsRemaining -= 1;
            }
            else
            {
                team2ShotsRemaining -= 1;
            }
        }

        // Updating Shots on Scorecard
        scorecard.UpdateShotsRemaining(team1ShotsRemaining, team2ShotsRemaining);

        // Return true or false depending on the current shots remaining, if false, the game must move to the next end
        if(team1ShotsRemaining == 0 && team2ShotsRemaining == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private float DistanceToJack(GameObject bowl, GameObject jack)
    {
        return Vector3.Distance(bowl.transform.position, jack.transform.position);
    }
}
