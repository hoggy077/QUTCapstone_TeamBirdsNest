using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAppearanceUpdater : MonoBehaviour
{
    [Header("Aspects to Target")]
    public SkinnedMeshRenderer playerModel;
    public Transform[] hips;
    public Transform[] shoulders;
    public Transform root;
    public CharacterHair haircut;

    private CharacterSO character;
    private JerseySO jersey;

    [Header("Character Identification")]
    [Range(1, 2)] public int teamID = 0;
    [Range(1, 3)] public int characterID = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Getting References to Character and Jersey
        GetCoreInformation();

        // Applying information
        AssembleAppearance();

        // Applying Body Shape & Hair Information
        ModifyBody();
    }

    private void Update()
    {
        // Applying information
        AssembleAppearance();

        // Applying Body Shape & Hair Information
        ModifyBody();
    }

    // Function to handle recolouring of character model
    private void AssembleAppearance()
    {
        // Colouring Outfit and Skin
        playerModel.materials[0].color = character.pantsColour;
        playerModel.materials[1].color = character.beltColour;
        playerModel.materials[2].color = character.skinColour;
        playerModel.materials[3].color = character.shoeColour;
        playerModel.materials[4].color = character.sockColour;
        playerModel.materials[6].color = character.beltBuckleColour;

        // Assembling Jersey
        playerModel.materials[5].SetColor("_BaseColor", jersey.baseColour);
        playerModel.materials[5].SetColor("_Colour1", jersey.smallStripeColour);
        playerModel.materials[5].SetColor("_Colour2", jersey.shoulderStripeColour);
    }

    // Function to handle physical attributes
    private void ModifyBody()
    {
        // Setting Hair
        haircut.hairMeshFilter.mesh = character.hairStyle;
        haircut.hairMeshFilter.GetComponent<MeshRenderer>().materials[0].color = character.hairColour;
        haircut.hairMeshFilter.GetComponent<MeshRenderer>().materials[1].color = character.alternateHairColour;

        if(character.flipHair)
        {
            haircut.transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // Rescaling Hips
        foreach (Transform hip in hips)
        {
            hip.localScale = new Vector3(hip.localScale.x, character.waistScale, hip.localScale.z);
        }

        // Rescaling Shoulders
        foreach(Transform shoulder in shoulders)
        {
            shoulder.localScale = new Vector3(character.shoulderScale, character.shoulderScale, character.shoulderScale);
        }

        // Rescaling Core Character Height
        root.localScale = new Vector3(character.heightScale, character.heightScale, character.heightScale);
    }

    // Function to get information for customising of character
    private void GetCoreInformation()
    {
        TeamScriptable myTeam;

        // Getting Team Information if Available
        
        if (GameStateManager.Instance_.Team_1 != null && GameStateManager.Instance_.Team_2 != null)
        {
            // Getting Team Info
            if (teamID == 1)
            {
                myTeam = GameStateManager.Instance.Team_1.BaseTeam;
            }
            else
            {
                myTeam = GameStateManager.Instance.Team_2.BaseTeam;
            }
        }
        else
        {
            // Getting Debug Testing Team Info
            if (teamID == 1)
            {
                myTeam = FindObjectOfType<ScoringManager>().debugTeam1;
            }
            else
            {
                myTeam = FindObjectOfType<ScoringManager>().debugTeam2;
            }
        }

        // Getting Character Info
        if (characterID == 1)
        {
            character = myTeam.character1;
        }
        else if (characterID == 2)
        {
            character = myTeam.character2;
        }
        else
        {
            character = myTeam.character3;
        }

        // Getting Jersey Information
        jersey = myTeam.teamJersey;
    }
}
