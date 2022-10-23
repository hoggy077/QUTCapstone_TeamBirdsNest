using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class BowlMovement : MonoBehaviour
{
    public Rigidbody rb;
    public Transform tr;
    public float mass;
    public bool isJack;
    private MeshCollider bowlmc;
    public LineRenderer lr;

    public bool externalySet = true;
    public bool isMoving = false;
    public bool inDelivery = false;

    float BowlRadius = 0.0635f;
    float g = 9.8f;
    public float iv;
    public Vector3 v = new Vector3(0, 0, 0);
    public Vector3 av = new Vector3(0, 0, 0); // rotation axis
    public Vector3 rd = new Vector3(0, 0, 0); // rotation axis
    float mu = 0.15f;
    float time = 0;

    void Start(){
        if(!isJack){
            bowlmc = GetComponent<MeshCollider>();
        }
    }

    void FixedUpdate(){
        if(isMoving){
            time += Time.deltaTime;
            float speed = iv - mu*g*time; // velocity at particular time step
            v = v.normalized * speed;

            if(speed > 0.001){
                
                float rotAngle = (speed / BowlRadius) / (MathF.PI/180);
                tr.RotateAround(tr.position, rd,  rotAngle * Time.deltaTime);
                //rd = Vector3.Cross(av, Vector3.up).normalized;
                Quaternion deltaRotation = Quaternion.AngleAxis(rotAngle * Time.deltaTime, rd).normalized;
                
                // make sure the bowl is touching the ground
                Vector3 pos = rb.position;
                pos.y = distanceFromFloor();

                rb.MovePosition(pos + (v * Time.deltaTime));
                //rb.MoveRotation(rb.rotation * deltaRotation);

                // show the rotational axis
                // lr.SetPosition(0, rb.position);
                // lr.SetPosition(1, rb.position + rd);
                // lr.positionCount = 2;
                // lr.enabled = true;
            }else{
                isMoving = false;
                externalySet = false;
                iv = 0;
                time = 0;
                v = new Vector3(0, 0, 0);
                av = new Vector3(0, 0, 0);
            }
        }
    }

    void Update(){
    }

    float distanceFromFloor(){
        if(!isJack){
            Vector3 csp1 = bowlmc.ClosestPoint(tr.position + Vector3.down);
            //Vector3 csp2 = rinkmc.ClosestPoint(tr.position);
            float dist = tr.position.y - csp1.y;

            return dist;
        }

        return 0.0315f;
    }

    void OnCollisionStay(Collision collision){
        if(collision.gameObject.name != "Rink"){
            Debug.Log("Collision STAY!!!");
        }
    }

    void OnCollisionEnter(Collision collision){

        // Setting toucher if that bad boy is a jack
        if (collision.gameObject.tag == "Jack" && GetComponent<BowlID>() && GetComponent<BowlLauncher>())
        {
            GetComponent<BowlID>().SetToucher();
        }

        if (externalySet){
            externalySet = false;
            return;
        }
        if(collision.gameObject.name != "Rink"){
            tr = GetComponent<Transform>();
            BowlMovement bm2 = collision.gameObject.GetComponent<BowlMovement>();
            
            if(bm2 == null){
            
                time = 0;
                Vector3 direction;
                if(tr.position.x < 0){
                    direction = Vector3.left;
                } else{
                    direction = Vector3.right;
                }

                // velocity of this object into direction along collision and orthogonal direction
                Vector3 colliderProj = (Vector3.Dot(direction, v) / Vector3.Dot(direction, direction)) * direction;
                Vector3 colliderOrtho = v - colliderProj;

                v = -colliderProj + colliderOrtho;
                iv = v.magnitude;
        
                Vector3 angularVel = -v / BowlRadius;
                
                av = angularVel;
                rd = Vector3.Cross(angularVel, Vector3.up).normalized;

                isMoving = true;

                //Confirm Collision SFX
                AudioManager.instance.PlaySound("IFB399-MetalCollisionSFX");
            }
            else{
                bm2.tr = collision.gameObject.GetComponent<Transform>();                

                time = 0;
                Vector3 direction = bm2.tr.position - tr.position;
                // velocity of this object into direction along collision and orthogonal direction
                Vector3 colliderProj = (Vector3.Dot(direction, v) / Vector3.Dot(direction, direction)) * direction;
                Vector3 colliderOrtho = v - colliderProj;
                
                // velocity of collidee 
                Vector3 collideeProj = (Vector3.Dot(-direction, bm2.v) / Vector3.Dot(-direction, -direction)) * -direction;
                Vector3 collideeOrtho = bm2.v - collideeProj;

                Vector3 colliderFinalVel = ((((mass - bm2.mass) * colliderProj) + 2 * bm2.mass * collideeProj)/(mass + bm2.mass)) + colliderOrtho;
                Vector3 collideeFinalVel = ((((bm2.mass - mass) * collideeProj) + 2 * mass * colliderProj)/(mass + bm2.mass)) + collideeOrtho;

                v = colliderFinalVel;
                iv = colliderFinalVel.magnitude;
        
                Vector3 angularVel = -colliderFinalVel / BowlRadius;
                av = angularVel;
                rd = Vector3.Cross(angularVel, Vector3.up).normalized;

                isMoving = true;

                Vector3 angularVel2 = -collideeFinalVel / BowlRadius;
                Vector3 rd2 = Vector3.Cross(angularVel2, Vector3.up).normalized;
                bm2.av = angularVel2;
                bm2.v = collideeFinalVel;
                bm2.iv = collideeFinalVel.magnitude;
                bm2.rd = rd2;
                bm2.externalySet = true;
                bm2.isMoving = true;

                //Confirm Collision SFX
                AudioManager.instance.PlaySound("IFB399-MetalCollisionSFX");
            }
        }
    }
}
