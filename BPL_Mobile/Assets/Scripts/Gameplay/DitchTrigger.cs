using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class DitchTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider collider){
        GameObject bowl = collider.gameObject;
        BowlID bi = bowl.GetComponent<BowlID>();

        // 
        if(bi != null){
            bi.enteredDitch = true;
            collider.attachedRigidbody.angularDrag = 1;
            collider.attachedRigidbody.drag = 1;
            collider.attachedRigidbody.useGravity = true;

            //Confirm CrowdUpset SFX
            AudioManager.instance.PlaySound("IFB399-CrowdNegativeSFX");
        }
    }
}