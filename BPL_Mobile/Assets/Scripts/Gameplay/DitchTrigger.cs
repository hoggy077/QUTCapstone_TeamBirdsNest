using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class DitchTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider collider){
        GameObject bowl = collider.gameObject;
        BowlID bi = bowl.GetComponent<BowlID>();

        bi.inDitch = true;

        collider.attachedRigidbody.angularDrag = 1;
        collider.attachedRigidbody.drag = 1;
        collider.attachedRigidbody.useGravity = true;
    }
}