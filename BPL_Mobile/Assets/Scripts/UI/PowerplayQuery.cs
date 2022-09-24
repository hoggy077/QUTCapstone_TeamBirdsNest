using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerplayQuery : MonoBehaviour
{
    [Header("Animation Parameters")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float timerMax = 10f;
    private float currentTimeRemaining = 0f;
    private bool displaying = false;

    [Header("Groups to Scale")]
    [SerializeField] private Transform[] timerLoadingBars;
    [SerializeField] private RectTransform[] teamGroups;

    [Header("Adjustable Elements")]
    [SerializeField] private Button[] activationButtons;
    [SerializeField] private TextMeshProUGUI[] buttonTexts;
    [SerializeField] private Image[] teamLogos;

    [HideInInspector] public static PowerplayQuery instance;

    // Variables to Handle Storing of Information and communication with scoring manager
    [HideInInspector] public ScoringManager sm;

    // Start is called before the first frame update
    void Start()
    {
        // Allowing for references from anywhere
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        // Setup of Display
        displaying = false;
        transform.localScale = new Vector3(0f, 1f, 1f);
        sm = FindObjectOfType<ScoringManager>();

        // If not multiplayer, deactivate second activation button
        if(!GameStateManager.Instance.isMultiplayerMode)
        {
            teamGroups[1].localScale = new Vector3(0f, 1f, 1f);
        }

        teamLogos[0].sprite = GameStateManager.Instance.Team_1.BaseTeam.TeamIcon;
        teamLogos[1].sprite = GameStateManager.Instance.Team_2.BaseTeam.TeamIcon;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateVisualState();

        // Closing Display if Time Runs out
        if(currentTimeRemaining == 0)
        {
            CloseDisplay();
        }
        else
        {
            currentTimeRemaining = Mathf.Clamp(currentTimeRemaining - Time.deltaTime, 0f, timerMax);
        }
    }

    // Updating Animation and Displays
    private void UpdateVisualState()
    {
        // If inactive, deactivate 
        if(!displaying)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, new Vector3(0f, 1f, 1f), Time.deltaTime / animationDuration);
            foreach (Button butt in activationButtons)
            {
                butt.interactable = false;
            }
        }
        else
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, new Vector3(1f, 1f, 1f), Time.deltaTime / animationDuration);
        }

        // Updating Loading Bar
        foreach (Transform bar in timerLoadingBars)
        {
            bar.localScale = new Vector3(currentTimeRemaining / timerMax, 1f, 1f);
        }
    }

    // Function to allow for displaying of UI
    public bool OfferPowerplay(bool team1PowerplayAvailable, bool team2PowerplayAvailable)
    {
        bool powerplayPossible = false;

        if(team1PowerplayAvailable)
        {
            powerplayPossible = true;
            displaying = true;
            currentTimeRemaining = timerMax;
            activationButtons[0].interactable = true;
        }
        else
        {
            activationButtons[0].interactable = false;
        }

        if (team2PowerplayAvailable && GameStateManager.Instance.isMultiplayerMode)
        {
            powerplayPossible = true;
            displaying = true;
            currentTimeRemaining = timerMax;
            activationButtons[1].interactable = true;
        }
        else
        {
            activationButtons[1].interactable = false;
        }

        return powerplayPossible;
    }

    // Function to close display
    public void CloseDisplay()
    {
        displaying = false;
        activationButtons[0].interactable = false;
        activationButtons[1].interactable = false;
    }

    // Take Input
    public void ActivatePowerplay(int team)
    {
        activationButtons[team - 1].interactable = false;

        sm.ActivatePowerplay(team);
    }

    // For match manager managing
    public bool CurrentlyOpen()
    {
        return displaying;
    }
}
