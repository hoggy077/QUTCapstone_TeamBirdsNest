using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestBowlRing : MonoBehaviour
{
    private LineRenderer lr;
    private int segments = 720;
    private float lineWidth = 0.04f;
    private float ringDisplayRadiusThreshold = 0.3f;

    // Setting up variables
    private void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Function to handle the toggling of the ring display between ends and first throws
    public void ToggleRing(bool on)
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = on;
    }

    // Function to handle the updating of the ring drawing
    public void UpdateRing(Transform jack, BowlID closestBowl)
    {
        // Moving renderer to circle centre
        transform.position = new Vector3(jack.transform.position.x, 0.01f, jack.transform.position.z);

        // Setting Up Variables
        lr.useWorldSpace = true;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = segments + 1;

        // Calculating Distance to Bowl, for radius of circle
        float radius = Vector3.Distance(jack.position, closestBowl.transform.position);

        if (radius > ringDisplayRadiusThreshold)
        {
            // Turning the ring on
            ToggleRing(true);

            // Completing loop of points to allow for a full circle
            int pointCount = segments + 1;
            Vector3[] points = new Vector3[pointCount];

            // Looping through and calculating circle coordinates
            for (int i = 0; i < pointCount; i++)
            {
                var rad = Mathf.Deg2Rad * (i * 360f / segments);
                points[i] = new Vector3(Mathf.Sin(rad) * radius, 0.04f * Mathf.Sin(i * Mathf.PI/4f), Mathf.Cos(rad) * radius);
                points[i] += new Vector3(jack.transform.position.x, 0f, jack.transform.position.z);
                points[i].x = Mathf.Clamp(points[i].x, -3f, 3f);
                points[i].z = Mathf.Clamp(points[i].z, points[i].z, 18.92f);
            }

            // Sending information to renderer
            lr.SetPositions(points);

            // Setting to team colour
            lr.material.SetColor("_BaseColor", closestBowl.GetComponent<MeshRenderer>().materials[0].GetColor("_BaseColour"));
            lr.material.SetColor("_BaseColor2", closestBowl.GetComponent<MeshRenderer>().materials[2].color);
        }
        else
        {
            ToggleRing(false);
        }
    }
}
