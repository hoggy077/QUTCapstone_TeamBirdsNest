using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BowlMovement : MonoBehaviour
{
    //spublic Rigidbody rb;
    public Transform tr;
    public float mass;
    public bool isJack;
    private MeshCollider bowlmc;

    public bool externalySet = true;
    public bool isMoving = false;

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

            if(speed > 0){
                tr.position += v * Time.deltaTime;

                Vector3 av = -v / BowlRadius;
                av.x = av.x / (MathF.PI/180);
                av.y = av.y / (MathF.PI/180);
                av.z = av.z / (MathF.PI/180);

                //tr.eulerAngles += av * Time.deltaTime;
                float rotAngle = (speed / BowlRadius) / (MathF.PI/180);
                tr.RotateAround(tr.position, rd,  rotAngle * Time.deltaTime);

               // make sure the bowl is touching the ground
                
                Vector3 pos = tr.position;
                pos.y = distanceFromFloor();
                tr.position = pos;
                
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
            Debug.Log(dist);

            return dist;
        }

        return 0.0315f;
    }

    void OnCollisionEnter(Collision collision){
        if(externalySet){
            externalySet = false;
            return;
        }
        if(collision.gameObject.name != "Rink"){
            tr = GetComponent<Transform>();
            BowlMovement bm2 = collision.gameObject.GetComponent<BowlMovement>();
            
            if(bm2 == null){
                Debug.Log("object we collided with doesn't have a BowlMovement script, exit");
                return;
            }
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

            v = colliderFinalVel;
            iv = colliderFinalVel.magnitude;
    
            Vector3 angularVel = -colliderFinalVel / BowlRadius;
            
            // angularVel.x = angularVel.x / (MathF.PI/180);
            // angularVel.y = angularVel.y / (MathF.PI/180);
            // angularVel.z = angularVel.z / (MathF.PI/180);
            av = angularVel;
            rd = Vector3.Cross(angularVel, Vector3.up).normalized;

            isMoving = true;
        
        }
    }
}
