using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Bowls Aus/Teams/Create New Character")]
[Serializable]
public class CharacterSO : ScriptableObject
{
    [Header("Head")]
    public Mesh hairStyle;
    public Color hairColour;
    public Color alternateHairColour;
    public bool flipHair = false;

    [Header("Jersey")]
    public Color beltColour;
    public Color beltBuckleColour;
    public Color pantsColour;
    public Color sockColour;
    public Color shoeColour;

    [Header("Identifications")]
    public string characterName;

    [Header("Physical Attributes")]
    public Color skinColour;
    public float shoulderScale = 1f;
    public float waistScale = 1f;
    public float heightScale = 1f;
}
