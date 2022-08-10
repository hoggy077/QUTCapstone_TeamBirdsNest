using UnityEngine;

[CreateAssetMenu(fileName = "NewTeam", menuName = "Bowls Aus/Create New Team")]
public class TeamScriptable : ScriptableObject
{
    public Sprite TeamIcon;
    public string TeamName;
    public Color32[] TeamColors;
}

