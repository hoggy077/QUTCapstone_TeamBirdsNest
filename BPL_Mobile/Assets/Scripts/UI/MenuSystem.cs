using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    [Header("Flow into Game")]
    private string gameScene = "TempQuickPlay";
    private Gamemode gamemode;
    public enum Gamemode
    {
        Tournament,
        Quickplay
    }

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
    public List<BowlsScriptable> team1Bowls = new List<BowlsScriptable>();
    public List<BowlsScriptable> team2Bowls = new List<BowlsScriptable>();

    // Variables to manage swiping
    private Vector2 fingerDownPosition;
    private Vector2 fingerUpPosition;

    [Header("Team Selection Screen")]
    [SerializeField] private UnityEngine.UI.Image logoDisplay;
    private Vector3 logo1OGPosition;
    private Vector3 logo1OGScale;
    private List<CharacterAppearanceUpdater> team1CharacterDisplays = new List<CharacterAppearanceUpdater>();
    private List<CharacterAppearanceUpdater> team2CharacterDisplays = new List<CharacterAppearanceUpdater>();
    [SerializeField] private Image frontTeamProfileDisplay;
    [SerializeField] private Image backTeamProfileDisplay;
    [SerializeField] private Material team1Display;
    [SerializeField] private Material team2Display;

    // Variables to handle logo 2's animations
    [SerializeField] private UnityEngine.UI.Image logoDisplay2;
    [SerializeField] private UnityEngine.UI.Image vsImage;
    [SerializeField] private TMPro.TextMeshProUGUI selectTeamText;
    private Vector3 logo2OGPosition;
    private Vector3 logo2OGScale;
    private int currentSelectionIndex = 0;
    private bool firstPlayerSelected = false;
    private bool bothPlayersSelected = false;

    // Team Information
    [SerializeField] private TeamScriptable[] teams;

    [Header("Bowl Selection Screen")]
    [SerializeField] private BowlsScriptable[] bowls;
    [SerializeField] private UnityEngine.UI.RawImage bowlLogoDisplay;
    [SerializeField] private MeshRenderer bowlDisplay;
    [SerializeField] private MeshRenderer bowlModelLogoDisplay;
    [SerializeField] private TMPro.TextMeshProUGUI bowlBiasLabel;
    [SerializeField] private UnityEngine.UI.Image[] bowlsSelectedDisplay;
    [SerializeField] private UnityEngine.UI.RawImage[] bowlsBrandsSelectedDisplay;
    [SerializeField] private RectTransform bowlSelectionShopfrontMenu;
    [SerializeField] private RectTransform singleplayerSelectedItemsMenu;
    [SerializeField] private RectTransform multiplayerSelectedItemsMenu;
    [SerializeField] private TMPro.TextMeshProUGUI bowlSelectionButtonText;

    // Variables to store selection summary UI of each possible case
    [SerializeField] private SelectionsSummaryUI singleplayerSS;
    [SerializeField] private SelectionsSummaryUI player1SS;
    [SerializeField] private SelectionsSummaryUI player2SS;

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

        // Looping through and finding all character appearance updaters for future use
        foreach(CharacterAppearanceUpdater character in FindObjectsOfType<CharacterAppearanceUpdater>())
        {
            if(character.teamID == 1)
            {
                team1CharacterDisplays.Add(character);
            }
            else
            {
                team2CharacterDisplays.Add(character);
            }
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
        int previousSelectionIndex = currentSelectionIndex;

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
                if(currentScreen.Equals(MenuState.SelectTeam) || currentScreen.Equals(MenuState.SelectBowls))
                {
                    // If Swipe Left
                    if (swipeDirection == 180f)
                    {
                        currentSelectionIndex++;
                    }

                    // If Swipe Right
                    else if(swipeDirection == 0f)
                    {
                        currentSelectionIndex--;
                    }
                }
            }
        }

        // If in team selection screen, perform associated tasks
        if(currentScreen.Equals(MenuState.SelectTeam))
        {
            // If player has swiped to a different team and is allowed to, handle it for both directions
            if (previousSelectionIndex != currentSelectionIndex && !bothPlayersSelected)
            {
                if (previousSelectionIndex > currentSelectionIndex)
                {
                    currentSelectionIndex = previousSelectionIndex;
                    NextTeam(-1);
                }
                else
                {
                    currentSelectionIndex = previousSelectionIndex;
                    NextTeam(1);
                }

                UpdateTeamSelectionScreen();
                previousSelectionIndex = currentSelectionIndex;
            }

            currentSelectionIndex = previousSelectionIndex;

            // If Player 1 has selected a team, preform screen animations for logo
            if (firstPlayerSelected && multiplayer)
            {
                logoDisplay2.enabled = true;
                logoDisplay2.sprite = teams[player1TeamIndex].TeamIcon;

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
                logoDisplay.sprite = teams[player2TeamIndex].TeamIcon;
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

        // If in bowl selection screen, perform associated tasks
        if (currentScreen.Equals(MenuState.SelectBowls))
        {
            // If player has swiped to a different team and is allowed to, handle it for both directions
            if (previousSelectionIndex != currentSelectionIndex && !bothPlayersSelected)
            {
                if (previousSelectionIndex > currentSelectionIndex)
                {
                    currentSelectionIndex = previousSelectionIndex;
                    NextBowl(-1);
                }
                else
                {
                    currentSelectionIndex = previousSelectionIndex;
                    NextBowl(1);
                }

                UpdateBowlSelectionScreen();
                previousSelectionIndex = currentSelectionIndex;
            }

            currentSelectionIndex = previousSelectionIndex;

            int currentIndex = 0;
            float shopfrontScreenTargetLocation = 1000f;
            float singleFinalPreviewTargetX = -1000f;
            float multiFinalPreviewTargetX = 1000f;
            // Resetting Bowl Selected Display
            foreach (UnityEngine.UI.Image bowlImage in bowlsSelectedDisplay)
            {
                bowlImage.color = new Color(0f, 0f, 0f, 128f / 255f);
                bowlsBrandsSelectedDisplay[currentIndex].enabled = false;
                bowlImage.enabled = true;
                currentIndex++;
            }

            // Display Current Bowl Selection Phase for player currently choosing
            if(!firstPlayerSelected && !bothPlayersSelected)
            {
                currentIndex = 0;
                shopfrontScreenTargetLocation = 0f;
                while (currentIndex < team1Bowls.Count)
                {
                    bowlsSelectedDisplay[currentIndex].color = new Color(1f, 1f, 1f, 128f / 255f);
                    bowlsBrandsSelectedDisplay[currentIndex].enabled = true;
                    bowlsBrandsSelectedDisplay[currentIndex].texture = team1Bowls[currentIndex].BowlTexture;
                    currentIndex++;
                }

                if(team1Bowls.Count < 3)
                {
                    bowlSelectionButtonText.text = "Select Bowl";
                }
                else
                {
                    bowlSelectionButtonText.text = "CONFIRM SELECTION";
                }
            }
            else if(multiplayer && firstPlayerSelected && !bothPlayersSelected)
            {
                currentIndex = 0;
                shopfrontScreenTargetLocation = 0f;
                while (currentIndex < team2Bowls.Count)
                {
                    bowlsSelectedDisplay[currentIndex].color = new Color(1f, 1f, 1f, 128f / 255f);
                    bowlsBrandsSelectedDisplay[currentIndex].enabled = true;
                    bowlsBrandsSelectedDisplay[currentIndex].texture = team2Bowls[currentIndex].BowlTexture;
                    currentIndex++;
                }

                if (team2Bowls.Count < 3)
                {
                    bowlSelectionButtonText.text = "Select Bowl";
                }
                else
                {
                    bowlSelectionButtonText.text = "CONFIRM SELECTION";
                }
            }

            // If bowls have all been selected, display a final summary before progressing to the game
            else if(firstPlayerSelected && bothPlayersSelected)
            {
                currentIndex = 0;
                // Resetting Bowl Selected Display
                foreach (UnityEngine.UI.Image bowlImage in bowlsSelectedDisplay)
                {
                    bowlsBrandsSelectedDisplay[currentIndex].enabled = false;
                    bowlImage.enabled = false;
                    currentIndex++;
                }

                if(multiplayer)
                {
                    multiFinalPreviewTargetX = 0f;
                    currentIndex = 0;
                    foreach (BowlsScriptable bowl in team1Bowls)
                    {
                        player1SS.brands[currentIndex].texture = bowl.BowlTexture;
                        currentIndex++;
                    }

                    player1SS.teamLogo.sprite = teams[player1TeamIndex].TeamIcon;

                    currentIndex = 0;
                    foreach (BowlsScriptable bowl in team2Bowls)
                    {
                        player2SS.brands[currentIndex].texture = bowl.BowlTexture;
                        currentIndex++;
                    }

                    player2SS.teamLogo.sprite = teams[player2TeamIndex].TeamIcon;
                }
                else
                {
                    singleFinalPreviewTargetX = 0f;
                    currentIndex = 0;
                    foreach(BowlsScriptable bowl in team1Bowls)
                    {
                        singleplayerSS.brands[currentIndex].texture = bowl.BowlTexture;
                        currentIndex++;
                    }

                    singleplayerSS.teamLogo.sprite = teams[player1TeamIndex].TeamIcon;
                }

                bowlSelectionButtonText.text = "START MATCH";
            }

            // Moving Bowl Selection Sub Menus To Correct Positions
            bowlSelectionShopfrontMenu.anchoredPosition = Vector3.Lerp(bowlSelectionShopfrontMenu.anchoredPosition, new Vector3(0f, shopfrontScreenTargetLocation, 0f), 5f * Time.deltaTime);
            singleplayerSelectedItemsMenu.anchoredPosition = Vector3.Lerp(singleplayerSelectedItemsMenu.anchoredPosition, new Vector3(singleFinalPreviewTargetX, 0f, 0f), 5f * Time.deltaTime);
            multiplayerSelectedItemsMenu.anchoredPosition = Vector3.Lerp(multiplayerSelectedItemsMenu.anchoredPosition, new Vector3(multiFinalPreviewTargetX, 0f, 0f), 5f * Time.deltaTime);
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
                gamemode = Gamemode.Quickplay;
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
                currentSelectionIndex = 0;
                UpdateTeamSelectionScreen();
                break;

            case "SelectTeam_2P":
                UpdateMenuDisplay(MenuState.SelectTeam);
                multiplayer = true;
                currentSelectionIndex = 0;
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

    // Function to update character models
    private void UpdateCharacterModels()
    {
        if (!firstPlayerSelected)
        {
            foreach (CharacterAppearanceUpdater display in team1CharacterDisplays)
            {
                display.GetCoreInformation(teams[currentSelectionIndex]);
                display.AssembleAppearance();
                display.ModifyBody();
            }

            backTeamProfileDisplay.enabled = false;
            frontTeamProfileDisplay.enabled = true;

            frontTeamProfileDisplay.material = team1Display;
        }
        else if (!bothPlayersSelected)
        {
            foreach (CharacterAppearanceUpdater display in team1CharacterDisplays)
            {
                display.GetCoreInformation(teams[player1TeamIndex]);
                display.AssembleAppearance();
                display.ModifyBody();
            }

            foreach (CharacterAppearanceUpdater display in team2CharacterDisplays)
            {
                display.GetCoreInformation(teams[currentSelectionIndex]);
                display.AssembleAppearance();
                display.ModifyBody();
            }

            backTeamProfileDisplay.enabled = false;
            frontTeamProfileDisplay.enabled = true;

            frontTeamProfileDisplay.material = team2Display;
            backTeamProfileDisplay.material = team1Display;
        }
        else
        {
            foreach (CharacterAppearanceUpdater display in team1CharacterDisplays)
            {
                display.GetCoreInformation(teams[player1TeamIndex]);
                display.AssembleAppearance();
                display.ModifyBody();
            }

            foreach (CharacterAppearanceUpdater display in team2CharacterDisplays)
            {
                display.GetCoreInformation(teams[player2TeamIndex]);
                display.AssembleAppearance();
                display.ModifyBody();
            }

            if (multiplayer)
            {
                backTeamProfileDisplay.enabled = true;
                frontTeamProfileDisplay.enabled = true;
            }
            else
            {
                backTeamProfileDisplay.enabled = false;
                frontTeamProfileDisplay.enabled = true;
            }

            frontTeamProfileDisplay.material = team2Display;
            backTeamProfileDisplay.material = team1Display;
        }
    }

    // If on team selection screen roll through to correct team and colours
    private void UpdateTeamSelectionScreen()
    {
        // Setting Correct Logo
        if (teams.Length > 0)
        {
            logoDisplay.sprite = teams[currentSelectionIndex].TeamIcon;
            bg.ChangeColours(teams[currentSelectionIndex].TeamColors[0], teams[currentSelectionIndex].TeamColors[1]);

            UpdateCharacterModels();
        }

        team1Bowls = new List<BowlsScriptable>();
        team2Bowls = new List<BowlsScriptable>();
    }

    // If on bowl selection screen roll through to correct team and colours
    private void UpdateBowlSelectionScreen()
    {
        // Setting Correct Logo
        if (bowls.Length > 0)
        {
            bowlLogoDisplay.texture = bowls[currentSelectionIndex].BowlTexture;
            bowlModelLogoDisplay.material.mainTexture = bowls[currentSelectionIndex].BowlTexture;

            // Updating Bias Tooltip Text
            string biasText = "NO BIAS";
            switch(currentSelectionIndex)
            {
                case 0:
                    biasText = "LOWEST BIAS";
                    break;

                case 1:
                    biasText = "LOW BIAS";
                    break;

                case 2:
                    biasText = "MEDIAN BIAS";
                    break;

                case 3:
                    biasText = "HIGH BIAS";
                    break;

                case 4:
                    biasText = "HIGHEST BIAS";
                    break;
            }

            bowlBiasLabel.text = biasText;

            // Setting correct background and bowl colours
            if (!firstPlayerSelected)
            {
                bg.ChangeColours(teams[player1TeamIndex].TeamColors[0], teams[player1TeamIndex].TeamColors[1]);
                bowlDisplay.materials[2].color = teams[player1TeamIndex].TeamColors[1];
                bowlDisplay.materials[1].SetColor("_BaseColour", teams[player1TeamIndex].TeamColors[0]);
                bowlDisplay.materials[0].SetColor("_BaseColour", teams[player1TeamIndex].TeamColors[0]);
            }
            else if (!bothPlayersSelected)
            {
                bg.ChangeColours(teams[player2TeamIndex].TeamColors[0], teams[player2TeamIndex].TeamColors[1]);
                bowlDisplay.materials[2].color = teams[player2TeamIndex].TeamColors[1];
                bowlDisplay.materials[1].SetColor("_BaseColour", teams[player2TeamIndex].TeamColors[0]);
                bowlDisplay.materials[0].SetColor("_BaseColour", teams[player2TeamIndex].TeamColors[0]);
            }
        }
    }

    // Function to handle the selecting of teams
    public void SelectTeam() 
    {
        // If singleplayer, set team and move on
        if(!multiplayer)
        {
            player1TeamIndex = currentSelectionIndex;
            team1Bowls = new List<BowlsScriptable>();
            team2Bowls = new List<BowlsScriptable>();
            firstPlayerSelected = false;
            bothPlayersSelected = false;
            bowlSelectionShopfrontMenu.anchoredPosition = Vector3.zero;
            singleplayerSelectedItemsMenu.anchoredPosition = new Vector3(-1000f, 0f, 0f);
            multiplayerSelectedItemsMenu.anchoredPosition = new Vector3(1000f, 0f, 0f);
            UpdateMenuDisplay(MenuState.SelectBowls);
            currentSelectionIndex = 0;
            UpdateBowlSelectionScreen();
        }

        // If multiplayer, and second player has not selected yet
        else if(!firstPlayerSelected)
        {
            player1TeamIndex = currentSelectionIndex;
            currentSelectionIndex++;

            // If Index out of range, loop back around
            if (currentSelectionIndex >= teams.Length)
            {
                currentSelectionIndex = 0;
            }
            else if (currentSelectionIndex < 0)
            {
                currentSelectionIndex = teams.Length - 1;
            }

            UpdateTeamSelectionScreen();
            UpdateMenuDisplay(MenuState.SelectTeam, true);
            firstPlayerSelected = true;
            UpdateCharacterModels();
        }

        // If second player has just selected, move on
        else if(firstPlayerSelected && !bothPlayersSelected)
        {
            player2TeamIndex = currentSelectionIndex;
            bothPlayersSelected = true;
            UpdateMenuDisplay(MenuState.SelectTeam);
            UpdateCharacterModels();
        }

        // If both players have selected, and the button is pressed again, progress to bowl selection
        else if(firstPlayerSelected && bothPlayersSelected)
        {
            firstPlayerSelected = false;
            bothPlayersSelected = false;
            UpdateMenuDisplay(MenuState.SelectBowls);
            bowlSelectionShopfrontMenu.anchoredPosition = Vector3.zero;
            singleplayerSelectedItemsMenu.anchoredPosition = new Vector3(-1000f, 0f, 0f);
            multiplayerSelectedItemsMenu.anchoredPosition = new Vector3(1000f, 0f, 0f);
            team1Bowls = new List<BowlsScriptable>();
            team2Bowls = new List<BowlsScriptable>();
            currentSelectionIndex = 0;
            UpdateBowlSelectionScreen();
        }
    }

    // Function to handle the selecting of bowls
    public void SelectBowl()
    {
        // If singleplayer, set bowls and move on
        if (!multiplayer && !bothPlayersSelected)
        {
            if(team1Bowls.Count < 3)
            {
                team1Bowls.Add(bowls[currentSelectionIndex]);
            }

            // If three bowls selected, move on
            if(team1Bowls.Count == 3)
            {
                firstPlayerSelected = true;
                bothPlayersSelected = true;
            }

            UpdateMenuDisplay(MenuState.SelectBowls, true);
            UpdateBowlSelectionScreen();
        }

        // If multiplayer, and second player has not selected yet
        else if (!firstPlayerSelected && !bothPlayersSelected)
        {
            if (team1Bowls.Count < 3)
            {
                team1Bowls.Add(bowls[currentSelectionIndex]);
            }

            // If three bowls selected, move on
            if (team1Bowls.Count == 3)
            {
                firstPlayerSelected = true;
            }

            UpdateMenuDisplay(MenuState.SelectBowls, true);
            UpdateBowlSelectionScreen();
        }

        // If second player has just selected, move on
        else if (firstPlayerSelected && !bothPlayersSelected)
        {
            if (team2Bowls.Count < 3)
            {
                team2Bowls.Add(bowls[currentSelectionIndex]);
            }

            // If three bowls selected, move on
            if (team2Bowls.Count == 3)
            {
                bothPlayersSelected = true;
            }

            UpdateMenuDisplay(MenuState.SelectBowls, true);
            UpdateBowlSelectionScreen();
        }

        // If both players have selected, and the button is pressed again, progress to game
        else if (firstPlayerSelected && bothPlayersSelected)
        {
            firstPlayerSelected = false;
            bothPlayersSelected = false;
            HandoverToGameScene();
        }
    }

    // Roll To Next Team
    private void NextTeam(int direction)
    {
        bool adjustIndex = true;

        while(adjustIndex)
        {
            // Adjust current index
            currentSelectionIndex += direction;

            // If outside bounds, adjust
            if(currentSelectionIndex >= teams.Length)
            {
                currentSelectionIndex = 0;
            }
            else if(currentSelectionIndex < 0)
            {
                currentSelectionIndex = teams.Length - 1;
            }

            adjustIndex = multiplayer && firstPlayerSelected && player1TeamIndex == currentSelectionIndex;
        }
    }

    // Roll To Next Team
    private void NextBowl(int direction)
    {
        bool adjustIndex = true;

        while (adjustIndex)
        {
            // Adjust current index
            currentSelectionIndex += direction;

            // If outside bounds, adjust
            if (currentSelectionIndex >= bowls.Length)
            {
                currentSelectionIndex = 0;
            }
            else if (currentSelectionIndex < 0)
            {
                currentSelectionIndex = bowls.Length - 1;
            }

            adjustIndex = false;
        }
    }

    // Function to handle moving backwards in the team menu
    public void BackButtonInTeamSelect()
    {
        // If singleplayer, set team and move on
        if (!multiplayer || !firstPlayerSelected)
        {
            ButtonPress("QuickPlay");
        }

        // If first player has just selected, move back to them
        else if (firstPlayerSelected && !bothPlayersSelected)
        {
            firstPlayerSelected = false;
            bothPlayersSelected = false;
            UpdateMenuDisplay(MenuState.SelectTeam, true);
            currentSelectionIndex = player1TeamIndex;
            UpdateTeamSelectionScreen();
        }

        // If both players have selected, and the button is pressed again, progress to bowl selection
        else if (firstPlayerSelected && bothPlayersSelected)
        {
            firstPlayerSelected = true;
            bothPlayersSelected = false;
            UpdateMenuDisplay(MenuState.SelectTeam, true);
            currentSelectionIndex = player2TeamIndex;
            UpdateTeamSelectionScreen();
        }
    }

    // Function to handle the selecting of bowls
    public void BackButtonInBowlSelect()
    {
        // If singleplayer, remove bowls or move back to teams if required
        if (!multiplayer && !bothPlayersSelected)
        {
            // If no bowls left, return to select team menu
            if(team1Bowls.Count < 1)
            {
                firstPlayerSelected = false;
                bothPlayersSelected = false;
                UpdateMenuDisplay(MenuState.SelectTeam, true);
                currentSelectionIndex = player1TeamIndex;
                UpdateTeamSelectionScreen();
            }
            // If bowls remain, remove one
            else
            {
                team1Bowls.RemoveAt(team1Bowls.Count - 1);
                UpdateMenuDisplay(MenuState.SelectBowls, true);
                UpdateBowlSelectionScreen();
            }

        }

        // If multiplayer, and first player presses back button, remove bowl or move back to teams if required
        else if (!firstPlayerSelected && !bothPlayersSelected)
        {
            // If no bowls left, return to select team menu
            if (team1Bowls.Count < 1)
            {
                firstPlayerSelected = true;
                bothPlayersSelected = true;
                UpdateMenuDisplay(MenuState.SelectTeam, true);
                currentSelectionIndex = player2TeamIndex;
                UpdateTeamSelectionScreen();
            }
            // If bowls remain, remove one
            else
            {
                team1Bowls.RemoveAt(team1Bowls.Count - 1);
                UpdateMenuDisplay(MenuState.SelectBowls, true);
                UpdateBowlSelectionScreen();
            }
        }

        // If second player has pressed the back button, remove a bowl or flick back to first player selection
        else if (firstPlayerSelected && !bothPlayersSelected)
        {
            // If no bowls left, return to select team menu
            if (team2Bowls.Count < 1)
            {
                firstPlayerSelected = false;
                bothPlayersSelected = false;
                UpdateMenuDisplay(MenuState.SelectBowls, true);
                UpdateBowlSelectionScreen();
            }
            // If bowls remain, remove one
            else
            {
                team2Bowls.RemoveAt(team2Bowls.Count - 1);
                UpdateMenuDisplay(MenuState.SelectBowls, true);
                UpdateBowlSelectionScreen();
            }
        }

        // If both players have selected, move back to second player selection
        else if (firstPlayerSelected && bothPlayersSelected)
        {
            firstPlayerSelected = multiplayer;
            bothPlayersSelected = false;
        }
    }

    private void HandoverToGameScene()
    {
        // If we are in quickplay, head to quick play
        if (gamemode == Gamemode.Quickplay)
        {
            GameStateManager.Instance.UpdateTeam(1, teams[player1TeamIndex]);

            if (multiplayer)
            {
                GameStateManager.Instance.UpdateTeam(2, teams[player2TeamIndex]);
                GameStateManager.Instance.isMultiplayerMode = true;
            }
            else
            {
                GameStateManager.Instance.UpdateTeam(2, GetRandomUnpickedTeam());
                GameStateManager.Instance.isMultiplayerMode = false;
            }

            SceneManager.LoadScene(gameScene);
        }
        else
        {
            SceneManager.LoadScene(gameScene);
            GameStateManager.Instance.isMultiplayerMode = false;
        }
    }

    // Gets Random Team for Multiplayer
    private TeamScriptable GetRandomUnpickedTeam()
    {
        List<TeamScriptable> possibleTeams = new List<TeamScriptable>();

        foreach(TeamScriptable team in teams)
        {
            possibleTeams.Add(team);
        }

        possibleTeams.Remove(teams[player1TeamIndex]);

        return possibleTeams[Random.Range(0, possibleTeams.Count)];
    }
}
