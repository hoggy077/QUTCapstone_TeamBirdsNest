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
    public bool secondPlayerSelecting = false;

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
    }

    // Update is called once per frame
    void Update()
    {
        // Title Screen
        if(currentScreen.Equals(MenuState.TitleScreen))
        {
            if(Input.GetMouseButtonDown(0))
            {
                UpdateMenuDisplay(MenuState.MainMenu);
            }
        }
    }

    // Function to move to play menu
    public void ButtonPress(string targetAction)
    {
        // Depending on command, perform action
        switch(targetAction)
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
                break;

            case "SelectTeam_2P":
                UpdateMenuDisplay(MenuState.SelectTeam);
                multiplayer = true;
                secondPlayerSelecting = false;
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
        if(currentScreen != newState || forceReset)
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
            if(bg && !forceReset)
            {
                bg.RandomiseGradient();
            }
        }
    }
}