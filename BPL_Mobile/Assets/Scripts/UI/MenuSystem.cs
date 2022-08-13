using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : MonoBehaviour
{
    [Header("Menu Screens")]
    public GameObject[] titleScreen;
    public GameObject[] mainMenu;
    public GameObject[] options;
    public GameObject[] playMenu;
    public GameObject[] careerMenu;
    public GameObject[] tutorial;
    public GameObject[] tournament;
    public GameObject[] quickPlay;
    public GameObject[] selectTeam;
    public GameObject[] selectBowls;
    public GameObject[] postMatchBreakdown;
    public GameObject[] theBPL;
    private List<GameObject[]> allScreens = new List<GameObject[]>();

    [Header("Current Screen")]
    // Variables to manage values across screens
    public MenuState currentScreen = MenuState.TitleScreen;

    [Header("Takeaways")]
    public bool multiplayer = false;
    public int player1TeamIndex = 0;
    public int player2TeamIndex = 0;
    private bool firstPlayerSelected = false;
    private bool bothPlayersSelected = false;

    // Variables to manage swiping
    private Vector2 fingerDownPosition;
    private Vector2 fingerUpPosition;

    [Header("Team Selection Screen")]
    [SerializeField] private UnityEngine.UI.Image logoDisplay;
    private Vector3 logo1OGPosition;
    private Vector3 logo1OGScale;

    // Variables to handle logo 2's animations
    [SerializeField] private UnityEngine.UI.Image logoDisplay2;
    [SerializeField] private UnityEngine.UI.Image vsImage;
    [SerializeField] private TMPro.TextMeshProUGUI selectTeamText;
    private Vector3 logo2OGPosition;
    private Vector3 logo2OGScale;
    private int currentTeamIndex = 0;

    // Team Information
    [SerializeField]private Sprite[] teamLogos;
    [SerializeField]private Color[] teamColours1;
    [SerializeField]private Color[] teamColours2;

    // Variable to manage background gradient effect
    private GradientBackground bg;

    public enum MenuState
    {
        TitleScreen = 0,
        MainMenu = 1,
        Options = 2,
        PlayMenu = 3,
        CareerMenu = 4,
        Tutorial = 5,
        Tournament = 6,
        QuickPlay = 7,
        SelectTeam = 8,
        SelectBowls = 9,
        PostMatchBreakdown = 10,
        TheBPL = 11
    }

    // Start is called before the first frame update
    void Start()
    {
        // Adding all screens to shared list for later reference
        allScreens.Add(titleScreen);
        allScreens.Add(mainMenu);
        allScreens.Add(options);
        allScreens.Add(playMenu);
        allScreens.Add(careerMenu);
        allScreens.Add(tutorial);
        allScreens.Add(tournament);
        allScreens.Add(quickPlay);
        allScreens.Add(selectTeam);
        allScreens.Add(selectBowls);
        allScreens.Add(postMatchBreakdown);
        allScreens.Add(theBPL);

        // Saving gradient background for later use
        if (FindObjectOfType<GradientBackground>())
        {
            bg = FindObjectOfType<GradientBackground>();
        }

        // Overriding Main Menu for game start
        UpdateMenuDisplay(MenuState.TitleScreen, true);

        // Setting Logo 2's Original Variables
        logo1OGPosition = logoDisplay.rectTransform.localPosition;
        logo1OGScale = logoDisplay.rectTransform.localScale;

        // Setting Logo 2's Original Variables
        logo2OGPosition = logoDisplay2.rectTransform.localPosition;
        logo2OGScale = logoDisplay2.rectTransform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        int previousTeamIndex = currentTeamIndex;

        // Touch Detection and Management
        foreach (Touch touch in Input.touches)
        {
            // If touch has just begun
            if (touch.phase == TouchPhase.Began)
            {
                // Title Screen
                if (currentScreen.Equals(MenuState.TitleScreen))
                {
                    UpdateMenuDisplay(MenuState.MainMenu);
                }

                fingerUpPosition = touch.position;
                fingerDownPosition = touch.position;
            }

            // If touch has moved
            if(touch.phase == TouchPhase.Moved)
            {
                fingerUpPosition = touch.position;
            }

            // If touch has ended
            if (touch.phase == TouchPhase.Ended)
            {
                float swipeDirection = GetSwipeDirection(fingerDownPosition, fingerUpPosition);

                // Team Selection Screen
                if(currentScreen.Equals(MenuState.SelectTeam))
                {
                    // If Swipe Left
                    if (swipeDirection == 180f)
                    {
                        currentTeamIndex++;
                    }

                    // If Swipe Right
                    else if(swipeDirection == 0f)
                    {
                        currentTeamIndex--;
                    }
                }
            }
        }

        // If in team selection screen, perform associated tasks
        if(currentScreen.Equals(MenuState.SelectTeam))
        {
            // If player has swiped to a different team and is allowed to, handle it for both directions
            if (previousTeamIndex != currentTeamIndex && !bothPlayersSelected)
            {
                if (previousTeamIndex > currentTeamIndex)
                {
                    currentTeamIndex = previousTeamIndex;
                    NextTeam(-1);
                }
                else
                {
                    currentTeamIndex = previousTeamIndex;
                    NextTeam(1);
                }

                UpdateTeamSelectionScreen();
                previousTeamIndex = currentTeamIndex;
            }

            currentTeamIndex = previousTeamIndex;

            // If Player 1 has selected a team, preform screen animations for logo
            if (firstPlayerSelected && multiplayer)
            {
                logoDisplay2.enabled = true;
                logoDisplay2.sprite = teamLogos[player1TeamIndex];

                if(bothPlayersSelected)
                {
                    logoDisplay2.color = Color.Lerp(logoDisplay2.color, Color.white, 5f * Time.deltaTime);
                }
                else
                {
                    logoDisplay2.color = Color.Lerp(logoDisplay2.color, new Color(0f, 0f, 0f, 34f / 255f), 5f * Time.deltaTime);
                }

                logoDisplay2.rectTransform.localPosition = Vector3.Lerp(logoDisplay2.rectTransform.localPosition, new Vector3(-187f, 389f, 0f), 5f * Time.deltaTime);
                logoDisplay2.rectTransform.localScale = Vector3.Lerp(logoDisplay2.rectTransform.localScale, new Vector3(4.3f, 4.3f, 4.3f), 5f * Time.deltaTime);
                selectTeamText.text = "Select Team 2";
            }
            else
            {
                logoDisplay2.enabled = false;
                logoDisplay2.rectTransform.localPosition = logo2OGPosition;
                logoDisplay2.rectTransform.localScale = logo2OGScale;
                logoDisplay2.color = Color.white;
                

                if(multiplayer)
                {
                    selectTeamText.text = "Select Team 1";
                }
                else
                {
                    selectTeamText.text = "Select Team";
                }
            }

            // If Player 2 has selected, display both selected teams in a head to head
            if (bothPlayersSelected && multiplayer)
            {
                logoDisplay.sprite = teamLogos[player2TeamIndex];
                logoDisplay.rectTransform.localPosition = Vector3.Lerp(logoDisplay.rectTransform.localPosition, new Vector3(180f, 155f, 0f), 5f * Time.deltaTime);
                logoDisplay.rectTransform.localScale = Vector3.Lerp(logoDisplay.rectTransform.localScale, new Vector3(4.3f, 4.3f, 4.3f), 5f * Time.deltaTime);
                vsImage.enabled = true;
                selectTeamText.text = "Confirm Teams";
            }
            else
            {
                logoDisplay.rectTransform.localPosition = logo1OGPosition;
                logoDisplay.rectTransform.localScale = logo1OGScale;
                vsImage.enabled = false;
            }
        }
    }

    // Function to move to play menu
    public void ButtonPress(string targetAction)
    {
        // Depending on command, perform action
        switch (targetAction)
        {
            case "PlayMenu":
                UpdateMenuDisplay(MenuState.PlayMenu);
                break;

            case "TitleScreen":
                UpdateMenuDisplay(MenuState.TitleScreen);
                break;

            case "BPL":
                UpdateMenuDisplay(MenuState.TheBPL);
                break;

            case "Options":
                UpdateMenuDisplay(MenuState.Options);
                break;

            case "MenuScreen":
                UpdateMenuDisplay(MenuState.MainMenu);
                break;

            case "QuickPlay":
                UpdateMenuDisplay(MenuState.QuickPlay);
                firstPlayerSelected = false;
                bothPlayersSelected = false;
                bg.ResetColours();
                break;

            case "Tournament":
                UpdateMenuDisplay(MenuState.Tournament);
                multiplayer = false;
                break;

            case "Career":
                UpdateMenuDisplay(MenuState.CareerMenu);
                break;

            case "Tutorial":
                UpdateMenuDisplay(MenuState.Tutorial);
                break;

            case "SelectTeam_1P":
                UpdateMenuDisplay(MenuState.SelectTeam);
                multiplayer = false;
                currentTeamIndex = 0;
                UpdateTeamSelectionScreen();
                break;

            case "SelectTeam_2P":
                UpdateMenuDisplay(MenuState.SelectTeam);
                multiplayer = true;
                currentTeamIndex = 0;
                UpdateTeamSelectionScreen();
                firstPlayerSelected = false;
                break;

            case "":
                break;

            default:
                break;
        }
    }

    // Function to manage updating of menu display
    public void UpdateMenuDisplay(MenuState newState, bool forceReset = false)
    {
        if (currentScreen != newState || forceReset)
        {
            currentScreen = newState;

            // Looping through and hiding all screens
            foreach (GameObject[] screen in allScreens)
            {
                foreach (GameObject element in screen)
                {
                    element.SetActive(false);
                }
            }

            // Activating Current Screen
            foreach (GameObject element in allScreens[(int)currentScreen])
            {
                element.SetActive(true);
            }

            // Null Check background before randomising
            if (bg && !forceReset)
            {
                bg.RandomiseGradient();
            }
        }
    }

    // Get Swipe Direction
    private float GetSwipeDirection(Vector2 start, Vector2 end)
    {
        // Right
        if(start.x < end.x)
        {
            return 0f;
        }

        // Left
        else if(end.x < start.x)
        {
            return 180f;
        }

        return 69f;
    }

    // If on team selection screen roll through to correct team and colours
    private void UpdateTeamSelectionScreen()
    {
        // Setting Correct BG Gradient
        if(currentTeamIndex < teamColours1.Length && currentTeamIndex >= 0 && currentTeamIndex < teamColours2.Length)
        {
            bg.ChangeColours(teamColours1[currentTeamIndex], teamColours2[currentTeamIndex]);
        }

        // Setting Correct Logo
        if(teamLogos.Length > 0)
        {
            logoDisplay.sprite = teamLogos[currentTeamIndex];
        }
    }

    // Function to handle the selecting of teams
    public void SelectTeam()
    {
        // If singleplayer, set team and move on
        if(!multiplayer)
        {
            player1TeamIndex = currentTeamIndex;
            UpdateMenuDisplay(MenuState.SelectBowls);
        }

        // If multiplayer, and second player has not selected yet
        else if(!firstPlayerSelected)
        {
            player1TeamIndex = currentTeamIndex;
            currentTeamIndex++;

            // If Index out of range, loop back around
            if (currentTeamIndex >= teamLogos.Length)
            {
                currentTeamIndex = 0;
            }
            else if (currentTeamIndex < 0)
            {
                currentTeamIndex = teamLogos.Length - 1;
            }

            UpdateTeamSelectionScreen();
            UpdateMenuDisplay(MenuState.SelectTeam, true);
            firstPlayerSelected = true;
        }

        // If second player has just selected, move on
        else if(firstPlayerSelected && !bothPlayersSelected)
        {
            player2TeamIndex = currentTeamIndex;
            bothPlayersSelected = true;
            UpdateMenuDisplay(MenuState.SelectTeam);
        }

        // If both players have selected, and the button is pressed again, progress to bowl selection
        else if(firstPlayerSelected && bothPlayersSelected)
        {
            UpdateMenuDisplay(MenuState.SelectBowls);
        }
    }

    // Roll To Next Team
    private void NextTeam(int direction)
    {
        bool adjustIndex = true;

        while(adjustIndex)
        {
            // Adjust current index
            currentTeamIndex += direction;

            // If outside bounds, adjust
            if(currentTeamIndex >= teamLogos.Length)
            {
                currentTeamIndex = 0;
            }
            else if(currentTeamIndex < 0)
            {
                currentTeamIndex = teamLogos.Length - 1;
            }

            adjustIndex = multiplayer && firstPlayerSelected && player1TeamIndex == currentTeamIndex;
        }
    }
}
