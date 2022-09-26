using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFlyInFlyOut : MonoBehaviour
{
    public Vector3 start;
    public Vector3 inPosition;
    public Vector3 end;
    public float speed = 0.1f;

    private Vector3 targetPosition;
    private bool exiting = false;
    private bool flownIn = false;
    private RectTransform thisTrans;
    private float distanceBetween;

    private void Start()
    {
        thisTrans = GetComponent<RectTransform>();
        distanceBetween = Vector3.Distance(start, inPosition);
        ResetFly();
    }

    private void Update()
    {
        thisTrans.anchoredPosition = Vector3.MoveTowards(thisTrans.anchoredPosition, targetPosition, Time.deltaTime / speed * distanceBetween);

        if(exiting && thisTrans.anchoredPosition == (Vector2)targetPosition)
        {
            ResetFly();
        }
    }

    public void FlyIn()
    {
        targetPosition = inPosition;
        flownIn = true;
    }

    public void FlyOut()
    {
        if (flownIn)
        {
            flownIn = false;
            exiting = true;
            targetPosition = end;
        }
    }

    private void ResetFly()
    {
        targetPosition = start;
        thisTrans.anchoredPosition = start;
        exiting = false;
    }
}
