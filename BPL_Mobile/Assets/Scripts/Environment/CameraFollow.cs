using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    private Vector3 originalPosition;
    public float maxDistance = 30f;
    public float followSpeed = -1f;

    private void Start()
    {
        // Getting off any parents
        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        // If this exists look at it, otherwise, die
        if(target)
        {
            // Find the rotation to look at the thing
            Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);

            // Look at the thing
            if(followSpeed >= 0f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, followSpeed * Time.deltaTime);
            }

            // If less than zero, snap to look straight away
            else
            {
                transform.rotation = desiredRotation;
            }
        }

        else
        {
            Destroy(this.gameObject);
        }
    }
}
