using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewJersey", menuName = "Bowls Aus/Teams/Create New Jersey")]
public class JerseySO : ScriptableObject
{
    public Color baseColour;
    public Color smallStripeColour;
    public Color shoulderStripeColour;
    public Sprite teamLogo;
}
