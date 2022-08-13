using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientBackground : MonoBehaviour
{
    [Header("Default Colours (use 'ChangeColours()' to change at runtime)")]
    public Color colour1;
    private Color defaultColour1;
    public Color colour2;
    private Color defaultColour2;

    private float targetRotation = -70f;
    private bool flipColours = false;
    private UnityEngine.UI.Image display;

    [Header("Animation Variables")]
    public float animationSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        // Setting up ideal main menu
        display = GetComponent<UnityEngine.UI.Image>();
        flipColours = true;
        targetRotation = -70f;
        defaultColour1 = colour1;
        defaultColour2 = colour2;
    }

    // Update is called once per frame
    void Update()
    {
        // Apply Target Rotation Lerping
        float newRot = Mathf.Lerp(display.material.GetFloat("_CurrentRot"), targetRotation, animationSpeed * Time.deltaTime);
        display.material.SetFloat("_CurrentRot", newRot);

        // Apply Colour Changes, and flipping if requested
        Color target1;
        Color target2;

        if(flipColours)
        {
            target1 = colour2;
            target2 = colour1;
        }
        else
        {
            target1 = colour1;
            target2 = colour2;
        }

        Color newCol1 = Color.Lerp(display.material.GetColor("_Color1"), target1, animationSpeed * Time.deltaTime);
        Color newCol2 = Color.Lerp(display.material.GetColor("_Color2"), target2, animationSpeed * Time.deltaTime);

        display.material.SetColor("_Color1", newCol1);
        display.material.SetColor("_Color2", newCol2);
    }

    // Function to handle creation of new gradient background and randomization
    public void ChangeColours(Color col1, Color col2)
    {
        colour1 = col1;
        colour2 = col2;
        RandomiseGradient();
    }

    // Function to randomise gradient orientation
    public void RandomiseGradient()
    {
        if(Random.Range(0f,1f) > 0.5f)
        {
            flipColours = true;
        }
        else
        {
            flipColours = false;
        }

        targetRotation = Random.Range(-90f, 20f);
    }

    // Function to return to default menu colours
    public void ResetColours()
    {
        colour1 = defaultColour1;
        colour2 = defaultColour2;
        RandomiseGradient();
    }
}
