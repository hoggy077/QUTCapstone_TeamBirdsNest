using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    private RectTransform parentTransform;

    [Header("General Display")]
    public TextMeshProUGUI endNumberDisplay;
    public Image[] shotPoints;
    public Image[] shotPointColours;

    [Header("Team 1")]
    public TextMeshProUGUI team1NameDisplay;
    public Image team1ColourDisplay;
    public TextMeshProUGUI team1ShotCountDisplay;
    public Image team1ShotCountIcon;
    public TextMeshProUGUI team1SetsWonDisplay;
    public TextMeshProUGUI team1EndsWonDisplay;
    public Image team1PP;
    public Texture[] team1CharacterProfiles;

    [Header("Team 2")]
    public TextMeshProUGUI team2NameDisplay;
    public Image team2ColourDisplay;
    public TextMeshProUGUI team2ShotCountDisplay;
    public Image team2ShotCountIcon;
    public TextMeshProUGUI team2SetsWonDisplay;
    public TextMeshProUGUI team2EndsWonDisplay;
    public Image team2PP;
    public Texture[] team2CharacterProfiles;

    [Header("Teammate Selection")]
    public RawImage[] teammateProfilesUI;
    public TextMeshProUGUI[] teammateNamesUI;
    public RawImage[] bowlProfilesUI;
    public TextMeshProUGUI[] teammateBowlCounts;

    private Vector3 targetPos = Vector3.zero;
    [HideInInspector] public bool fullyOnScreen = false;
    private bool onScreen = false;

    // Sub Menu Functionality
    public SubmenuState submenuState = SubmenuState.None;
    public enum SubmenuState
    {
        None,
        TeammateSwitch,
        PauseMenu,
        OverheadCamera
    }
    public RectTransform teammateSelectionPanel;
    private float teammaterSelectionPanelClosedPos = -1500f;
    private float teammateSelectionPanelTargetPos = 0f;
    private MatchManager mm;

    private void Start()
    {
        scorecardTransform = GetComponent<RectTransform>();
        parentTransform = transform.parent.GetComponent<RectTransform>();
        Reposition(false);
        teammateSelectionPanelTargetPos = teammaterSelectionPanelClosedPos;
        mm = FindObjectOfType<MatchManager>();
    }

    private void Update()
    {
        fullyOnScreen = onScreen && parentTransform.anchoredPosition.y == targetPos.y;

        // Animate Elements if aPPlicable
        AnimateSetsElement();

        // Moving to repositioned position
        parentTransform.anchoredPosition = Vector3.MoveTowards(parentTransform.anchoredPosition, new Vector3(parentTransform.anchoredPosition.x, targetPos.y, 0f), 400f * 3f * Time.deltaTime);

        // If aiming to be off screen, then force menu to close
        if (!fullyOnScreen)
        {
            teammateSelectionPanel.anchoredPosition = Vector3.MoveTowards(teammateSelectionPanel.anchoredPosition, new Vector3(teammaterSelectionPanelClosedPos, teammateSelectionPanel.anchoredPosition.y), 3000f * Time.deltaTime);
        }
        else
        {
            // Moving Teamate Swap Menu if Required
            teammateSelectionPanel.anchoredPosition = Vector3.MoveTowards(teammateSelectionPanel.anchoredPosition, new Vector3(teammateSelectionPanelTargetPos, teammateSelectionPanel.anchoredPosition.y), 3000f * Time.deltaTime);
        }

        // Updating Teammate Selection Screen profiles
        if (mm.PlayerTurn)
        {
            for (int i = 0; i < teammateProfilesUI.Length; i++)
            {
                teammateProfilesUI[i].texture = team1CharacterProfiles[i];
                bowlProfilesUI[i].texture = GameStateManager.Instance.Team_1.teamBowls[i].BowlTexture;
            }

            teammateNamesUI[0].text = FormatPlayerName(GameStateManager.Instance.Team_1.BaseTeam.character1.characterName);
            teammateNamesUI[1].text = FormatPlayerName(GameStateManager.Instance.Team_1.BaseTeam.character2.characterName);
            teammateNamesUI[2].text = FormatPlayerName(GameStateManager.Instance.Team_1.BaseTeam.character3.characterName);
        }
        else
        {
            for (int i = 0; i < teammateProfilesUI.Length; i++)
            {
                teammateProfilesUI[i].texture = team2CharacterProfiles[i];
                bowlProfilesUI[i].texture = GameStateManager.Instance.Team_2.teamBowls[i].BowlTexture;
            }

            teammateNamesUI[0].text = FormatPlayerName(GameStateManager.Instance.Team_2.BaseTeam.character1.characterName);
            teammateNamesUI[1].text = FormatPlayerName(GameStateManager.Instance.Team_2.BaseTeam.character2.characterName);
            teammateNamesUI[2].text = FormatPlayerName(GameStateManager.Instance.Team_2.BaseTeam.character3.characterName);
        }
    }

    public void Reposition(bool outOfView)
    {
        // Moving Scorecard off screen
        Vector3 currentScorePos = parentTransform.anchoredPosition;
        if (outOfView)
        {
            targetPos = new Vector3(currentScorePos.x, 600f, currentScorePos.z);
        }
        else
        {
            targetPos = new Vector3(currentScorePos.x, 0f, currentScorePos.z);

            if (SystemInfo.deviceModel.Contains("iPhone") || true)
            {
                float screenRatio = (1.0f * Screen.height) / (1.0f * Screen.width);

                if (screenRatio >= 2.1f)
                {
                    targetPos = new Vector3(currentScorePos.x, -70f, currentScorePos.z);
                }
            }
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
        if (!tiebreak)
        {
            endNumberDisplay.text = "END " + endNumber.ToString();
        }
        else
        {
            endNumberDisplay.text = "TIEBREAK";
        }
    }

    public void UpdateTeammateShots(int[] shots)
    {
        for (int i = 0; i < shots.Length; i++)
        {
            teammateBowlCounts[i].text = shots[i].ToString();
        }
    }

    public void UpdateCurrentTeammate(int activeTeammate)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i == activeTeammate)
            {
                teammateNamesUI[i].color = new Color(0.7f, 1f, 0.7f);
                bowlProfilesUI[i].color = new Color(0.7f, 1f, 0.7f);
                teammateBowlCounts[i].color = new Color(0.7f, 1f, 0.7f);
            }
            else
            {
                teammateNamesUI[i].color = Color.white;
                bowlProfilesUI[i].color = Color.white;
                teammateBowlCounts[i].color = Color.white;
            }
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
        foreach (Image indi in shotPoints)
        {
            indi.gameObject.SetActive(false);
        }

        int counter = 0;
        foreach (Image indi in shotPoints)
        {
            if (counter == numberOfShots)
            {
                break;
            }

            indi.gameObject.SetActive(true);

            counter++;
        }

        // Setting Colour of all shot counters
        foreach (Image indi in shotPointColours)
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
            Color currentColor = setsWonScorecardElement.GetComponent<Image>().color;
            Color targetColor = currentColor;
            targetColor.a = targetOpacity;

            setsWonScorecardElement.GetComponent<Image>().color = Color.Lerp(currentColor, targetColor, setsWonChangeAnimationSpeed * Time.deltaTime * 2f);
            currentColor = setsWonScorecardElement.GetComponent<Image>().color;

            team1SetsWonDisplay.color = currentColor;
            team2SetsWonDisplay.color = currentColor;
        }
    }
    #endregion

    #region Sub Menu

    public void ToggleTeammateMenu()
    {
        if(submenuState == SubmenuState.None)
        {
            teammateSelectionPanelTargetPos = 0f;
            submenuState = SubmenuState.TeammateSwitch;
        }
        else
        {
            teammateSelectionPanelTargetPos = teammaterSelectionPanelClosedPos;
            submenuState = SubmenuState.None;
        }
    }

    private string FormatPlayerName(string input)
    {
        string output;

        string[] segmentedInput = input.Split(' ');

        output = segmentedInput[0][0].ToString() + ". " + segmentedInput[1];

        return output;
    }

    #endregion
}
