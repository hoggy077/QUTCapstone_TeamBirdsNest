using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlID : MonoBehaviour
{
    public bool chalked = false;
    public bool inDitch = false;
    public bool enteredDitch = false;
    private int team = 0;
    private MeshRenderer mr;
    public bool toucher = false;

    public void SetTeam(int newTeam)
    {
        team = newTeam;

        mr = GetComponent<MeshRenderer>();

        if(team == 1)
        {
            mr.materials[0].SetColor("_BaseColour", GameStateManager.Instance.Team_1.BaseTeam.TeamColors[0]);
            mr.materials[1].SetColor("_BaseColour", GameStateManager.Instance.Team_1.BaseTeam.TeamColors[0]);
            mr.materials[2].color = GameStateManager.Instance.Team_1.BaseTeam.TeamColors[1];
        }
        else
        {
            mr.materials[0].SetColor("_BaseColour", GameStateManager.Instance.Team_2.BaseTeam.TeamColors[0]);
            mr.materials[1].SetColor("_BaseColour", GameStateManager.Instance.Team_2.BaseTeam.TeamColors[0]);
            mr.materials[2].color = GameStateManager.Instance.Team_2.BaseTeam.TeamColors[1];
        }
    }

    public int GetTeam()
    {
        return team;
    }

    public void SetToucher()
    {
        toucher = true;
        mr.materials[0].SetFloat("_ToucherChalk", 1f);
        mr.materials[1].SetFloat("_ToucherChalk", 1f);
    }
}
