using UnityEngine;

[CreateAssetMenu(fileName = "NewBowl", menuName = "Bowls Aus/Create New Bowl")]
public class BowlsScriptable : ScriptableObject
{
    public Mesh DefaultMesh;
    public Texture BowlTexture;

    public double Bias;
}