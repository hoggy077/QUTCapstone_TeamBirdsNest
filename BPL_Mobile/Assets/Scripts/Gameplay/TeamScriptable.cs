using UnityEngine;

[CreateAssetMenu(fileName = "NewTeam", menuName = "Bowls Aus/Teams/Create New Team")]
public class TeamScriptable : ScriptableObject
{
    public Sprite TeamIcon;
    public string TeamName;
    public Color32[] TeamColors;
    public JerseySO teamJersey;
    public CharacterSO character1;
    public CharacterSO character2;
    public CharacterSO character3;
}

