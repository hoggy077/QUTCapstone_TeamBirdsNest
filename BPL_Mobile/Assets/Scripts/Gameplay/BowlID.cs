using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlID : MonoBehaviour
{
    private int team = 0;
    private MeshRenderer mr;

    public void SetTeam(int newTeam)
    {
        team = newTeam;

        mr = GetComponent<MeshRenderer>();

        if(team == 1)
        {
            mr.materials[0].SetColor("_BaseColour", GameStateManager.Instance_.Team_1.BaseTeam.TeamColors[0]);
            mr.materials[1].SetColor("_BaseColour", GameStateManager.Instance_.Team_1.BaseTeam.TeamColors[0]);
            mr.materials[2].color = GameStateManager.Instance_.Team_1.BaseTeam.TeamColors[1];
        }
        else
        {
            mr.materials[0].SetColor("_BaseColour", GameStateManager.Instance_.Team_2.BaseTeam.TeamColors[0]);
            mr.materials[1].SetColor("_BaseColour", GameStateManager.Instance_.Team_2.BaseTeam.TeamColors[0]);
            mr.materials[2].color = GameStateManager.Instance_.Team_2.BaseTeam.TeamColors[1];
        }
    }

    public int GetTeam()
    {
        return team;
    }
}
