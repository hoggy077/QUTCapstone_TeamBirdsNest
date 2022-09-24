using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorecardUI : MonoBehaviour
{
    [Header("Set Animation Controls")]
    public float setsWonActiveXPosition;
    public float setsWonInactiveXPosition;
    public float setsWonChangeAnimationSpeed = 1f;
    public RectTransform scorecardTailendLocation;
    public GameObject setsWonScorecardElement;
    private bool displaySetScore = false;
    public float scorecardSetsWonActiveOffset = -3f;
    public float scorecardSetsWonInactiveOffset = 44.7f;
    private RectTransform scorecardTransform;

    [Header("General Display")]
    public TextMeshProUGUI endNumberDisplay;
    public UnityEngine.UI.Image[] shotPoints;
    public UnityEngine.UI.Image[] shotPointColours;

    [Header("Team 1")]
    public TextMeshProUGUI team1NameDisplay;
    public UnityEngine.UI.Image team1ColourDisplay;
    public TextMeshProUGUI team1ShotCountDisplay;
    public UnityEngine.UI.Image team1ShotCountIcon;
    public TextMeshProUGUI team1SetsWonDisplay;
    public TextMeshProUGUI team1EndsWonDisplay;
    public UnityEngine.UI.Image team1PP;

    [Header("Team 2")]
    public TextMeshProUGUI team2NameDisplay;
    public UnityEngine.UI.Image team2ColourDisplay;
    public TextMeshProUGUI team2ShotCountDisplay;
    public UnityEngine.UI.Image team2ShotCountIcon;
    public TextMeshProUGUI team2SetsWonDisplay;
    public TextMeshProUGUI team2EndsWonDisplay;
    public UnityEngine.UI.Image team2PP;

    private Vector3 targetPos = Vector3.zero;
    public bool fullyOnScreen = false;
    private bool onScreen = false;

    private void Start()
    {
        scorecardTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        fullyOnScreen = onScreen && scorecardTransform.anchoredPosition.y == targetPos.y;

        // Animate Elements if aPPlicable
        AnimateSetsElement();

        // Moving to repositioned position
        scorecardTransform.anchoredPosition = Vector3.MoveTowards(scorecardTransform.anchoredPosition, new Vector3(scorecardTransform.anchoredPosition.x, targetPos.y, 0f), 400f * 3f * Time.deltaTime);
    }

    public void Reposition(bool outOfView)
    {
        // Moving Scorecard off screen
        Vector3 currentScorePos = scorecardTransform.anchoredPosition;
        if (outOfView)
        {
            targetPos = new Vector3(currentScorePos.x, 400f, currentScorePos.z);
        }
        else
        {
            targetPos = new Vector3(currentScorePos.x, 0f, currentScorePos.z);
        }

        onScreen = !outOfView;
    }

    #region Front End Functions
    // Function to change team 1 information
    public void UpdateTeam1Info(string teamName, Color teamColour1)
    {
        UpdateTeamInformation(1, teamName, teamColour1);
    }

    // Function to change team 2 information
    public void UpdateTeam2Info(string teamName, Color teamColour1)
    {
        UpdateTeamInformation(2, teamName, teamColour1);
    }

    // Function to reset shot count, scoring and update display
    public void UpdateEndNumber(int endNumber, bool tiebreak = false)
    {
        if(!tiebreak)
        {
            endNumberDisplay.text = "END " + endNumber.ToString();
        }
        else
        {
            endNumberDisplay.text = "TIEBREAK";
        }
    }

    // Function to modify power play display
    /// <summary>
    /// 1 = Team 1, 2 = Team 2, 0 = No Current Powerplay
    /// </summary>
    /// <param name="teamCurrentlyPowerplaying"></param>
    public void UpdatePowerPlayStatus(bool team1, bool team2)
    {
        team1PP.gameObject.SetActive(team1);
        team2PP.gameObject.SetActive(team2);
    }

    // Function to update shot points
    public void UpdateCurrentShots(int team, int numberOfShots)
    {
        Color teamColor;

        // Getting Colour
        if(team == 1)
        {
            teamColor = team1ColourDisplay.color;
        }
        else
        {
            teamColor = team2ColourDisplay.color;
        }

        // Setting Active Shot Indicators
        foreach (UnityEngine.UI.Image indi in shotPoints)
        {
            indi.gameObject.SetActive(false);
        }

        int counter = 0;
        foreach (UnityEngine.UI.Image indi in shotPoints)
        {
            if (counter == numberOfShots)
            {
                break;
            }

            indi.gameObject.SetActive(true);

            counter++;
        }

        // Setting Colour of all shot counters
        foreach (UnityEngine.UI.Image indi in shotPointColours)
        {
            if(indi.IsActive())
            {
                indi.color = teamColor;
            }
        }
    }

    #region Scoring Update Functions
    // Shots Remaining
    public void UpdateShotsRemaining(int team1Shots, int team2Shots)
    {
        team1ShotCountDisplay.text = team1Shots.ToString();
        team2ShotCountDisplay.text = team2Shots.ToString();
    }

    // Sets
    public void UpdateSetsWon(int team1SetsWon, int team2SetsWon)
    {
        // If element of scorecard unecessary, hide from player and animate
        if(team1SetsWon == 0 && team2SetsWon == 0)
        {
            displaySetScore = false;
            setsWonScorecardElement.SetActive(false);
        }
        else
        {
            displaySetScore = true;
            setsWonScorecardElement.SetActive(true);
            team1SetsWonDisplay.text = team1SetsWon.ToString();
            team2SetsWonDisplay.text = team2SetsWon.ToString();
        }
    }

    // Ends
    public void UpdateEndsWon(int team1EndsWon, int team2EndsWon)
    {
        team1EndsWonDisplay.text = team1EndsWon.ToString();
        team2EndsWonDisplay.text = team2EndsWon.ToString();
    }
    
    #endregion

    #endregion

    #region Backend Functions
    // Function to modify team information
    private void UpdateTeamInformation(int teamNumber, string teamName, Color teamColour1)
    {
        // Change correct team values
        if(teamNumber == 1)
        {
            team1NameDisplay.text = teamName;

            // This is where setting the Shader Gradient to aPPropriate colours will occur, but for now we are just recolouring the white image
            team1ColourDisplay.color = teamColour1;
            team1ShotCountIcon.color = teamColour1;
        }
        else
        {
            team2NameDisplay.text = teamName;

            // This is where setting the Shader Gradient to aPPropriate colours will occur, but for now we are just recolouring the white image
            team2ColourDisplay.color = teamColour1;
            team2ShotCountIcon.color = teamColour1;
        }
    }

    // Function to manage animation of sets element
    private void AnimateSetsElement()
    {
        // Lerping position of scorecard tailend if sets need to be displayed, and vice versa
        float targetLocation;
        float scorecardTargetLocation;
        float targetOpacity;
        float threshold;

        if (displaySetScore)
        {
            targetLocation = setsWonActiveXPosition;
            scorecardTargetLocation = scorecardSetsWonActiveOffset;
            targetOpacity = 1f;
            threshold = 0.2f;
        }
        else
        {
            targetLocation = setsWonInactiveXPosition;
            scorecardTargetLocation = scorecardSetsWonInactiveOffset;
            targetOpacity = 0f;
            threshold = 0.2f;
        }

        scorecardTailendLocation.localPosition = new Vector3(Mathf.Lerp(scorecardTailendLocation.localPosition.x, targetLocation, setsWonChangeAnimationSpeed * Time.deltaTime), 0f, 0f);
        scorecardTransform.localPosition = new Vector3(Mathf.Lerp(scorecardTransform.localPosition.x, scorecardTargetLocation, setsWonChangeAnimationSpeed * Time.deltaTime), scorecardTransform.localPosition.y, scorecardTransform.localPosition.z);

        // If within reasonable threshold, update the opacity change of the sets won element
        if (Mathf.Abs(scorecardTailendLocation.localPosition.x - targetLocation) / (setsWonActiveXPosition - setsWonInactiveXPosition) > threshold)
        {
            Color currentColor = setsWonScorecardElement.GetComponent<UnityEngine.UI.Image>().color;
            Color targetColor = currentColor;
            targetColor.a = targetOpacity;

            setsWonScorecardElement.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(currentColor, targetColor, setsWonChangeAnimationSpeed * Time.deltaTime * 2f);
            currentColor = setsWonScorecardElement.GetComponent<UnityEngine.UI.Image>().color;

            team1SetsWonDisplay.color = currentColor;
            team2SetsWonDisplay.color = currentColor;
        }
    }
    #endregion
}
