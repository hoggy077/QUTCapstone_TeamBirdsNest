using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamLeaderboardSlot : MonoBehaviour
{
    public Image logoDisplay;
    public TextMeshProUGUI teamPoints;
    public TextMeshProUGUI teamName;

    public void UpdateTeam(Sprite logo, string newTeamName, string teamScore)
    {
        logoDisplay.sprite = logo;
        teamPoints.text = teamScore;
        teamName.text = newTeamName;
    }
}
