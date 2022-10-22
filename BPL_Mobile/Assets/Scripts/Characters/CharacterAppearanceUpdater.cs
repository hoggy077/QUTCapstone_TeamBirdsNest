using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAppearanceUpdater : MonoBehaviour
{
    [Header("Functionality Selection")]
    public bool inMenu = false;
    private MenuSystem menu;
    public Camera profileCamera;

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

    [Header("Fade Functionality")]
    public bool hideCharacter = false;
    public bool playerBowling = false;

    // Positioning variables
    private float floorHeight = 0f;
    private Vector2 minMaxSidePosition = new Vector2(3f, 19f);
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        floorHeight = transform.position.y;
        animator = GetComponent<Animator>();
        // Grabbing References for Possible Future Use
        menu = FindObjectOfType<MenuSystem>();

        if (!inMenu)
        {
            // Getting References to Character and Jersey
            GetCoreInformation();

            // Applying information
            AssembleAppearance();

            // Applying Body Shape & Hair Information
            ModifyBody();
        }
    }

    // Function to handle recolouring of character model
    public void AssembleAppearance()
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

    private void LateUpdate()
    {
        // Hiding if Required
        if ((transform.position.x > -3f && transform.position.x < 3f && hideCharacter))
        {
            haircut.hairMeshFilter.GetComponent<MeshRenderer>().enabled = false;
            playerModel.enabled = false;
        }
        else
        {
            haircut.hairMeshFilter.GetComponent<MeshRenderer>().enabled = true;
            playerModel.enabled = true;
        }
    }

    // Function to handle physical attributes
    public void ModifyBody()
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
    public void GetCoreInformation(TeamScriptable teamForUpdate = null)
    {
        TeamScriptable myTeam;

        if(!inMenu)
        {
            // Getting Team Information if Available
            if (GameStateManager.Instance.Team_1 != null && GameStateManager.Instance.Team_2 != null)
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
        }
        else
        {
            // If in the menu, check if team selection is occuring
            myTeam = teamForUpdate;
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

    // Function to allow for repositioning of characters at head
    public void RepositionCharacter(Transform Jack, List<GameObject> bowls, bool withinRink)
    {
        // Hiding if Required
        if ((transform.position.x > -3f && transform.position.x < 3f && hideCharacter) || playerBowling)
        {
            float uniquenessForcer = characterID * -20f - (teamID - 1) * -80f;

            haircut.hairMeshFilter.GetComponent<MeshRenderer>().enabled = true;
            playerModel.enabled = true;

            if (playerBowling)
            {
                transform.position = new Vector3(200f, -200f + uniquenessForcer, 200f);
            }
            else
            {
                haircut.hairMeshFilter.GetComponent<MeshRenderer>().enabled = false;
                playerModel.enabled = false;
            }
            return;
        }

        // Choosing Position outside of rink
        if(!withinRink)
        {
            animator.Play("Idle1", 0, Random.Range(0f, 1f));

            Vector3 newPosition = Vector3.zero;
            Vector3 newRotation = Vector3.zero;
            newPosition.y = floorHeight + 0.2f;

            newPosition.z = Mathf.Clamp(Jack.position.z, minMaxSidePosition.x, minMaxSidePosition.y);

            if(teamID == 1)
            {
                newPosition.x = -4f;
                newRotation.y = 90f;
            }
            else
            {
                newPosition.x = 4f;
                newRotation.y = 270f;
            }

            transform.position = newPosition;
            transform.rotation = Quaternion.Euler(newRotation);
        }
        else
        {
            Vector3 newPosition = Jack.position;
            newPosition.y = floorHeight;

            float radiusFromJack = 1f;
            while (Vector3.Distance(Jack.position, newPosition) < 1f)
            {
                newPosition.x = Random.Range(-2.5f, 2.5f);
                newPosition.z = Random.Range(Jack.position.z + 0.5f + radiusFromJack, Jack.position.z + 1.5f + radiusFromJack);

                foreach(GameObject bowl in bowls)
                {
                    if (Vector3.Distance(bowl.transform.position, newPosition) < 0.2f)
                    {
                        radiusFromJack += 1f;
                        newPosition = Jack.position;
                        break;
                    }
                }
            }

            transform.position = newPosition;
            transform.LookAt(new Vector3(Jack.position.x, transform.position.y, Jack.position.z));

            float random = Random.Range(0f, 1f);

            // Randomly Select Animation
            if (random > 0.88f)
            {
                animator.Play("BowlGander1", 0, Random.Range(0f, 1f));
            }
            else if (random > 0.44f)
            {
                animator.Play("BowlGander2", 0, Random.Range(0f, 1f));
            }
            else
            {
                animator.Play("BowlGander3", 0, Random.Range(0f, 1f));
            }

            // Resetting if Beyond Boundary
            if (newPosition.z > 18f)
            {
                newPosition.z = 15f;
                transform.position = newPosition;
                RepositionCharacter(transform, bowls, false);
            }
        }
    }
}
