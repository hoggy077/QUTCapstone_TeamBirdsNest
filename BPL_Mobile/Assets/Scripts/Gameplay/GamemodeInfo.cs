using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTeam", menuName = "Bowls Aus/Create New Gamemode")]
[Serializable]
public class GamemodeInfo : ScriptableObject
{
    public uint RoundsPerEnd;
    public uint EndsPerMatch;
}
