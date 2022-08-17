using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BowlLauncher : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private float BowlRadius;

    private bool delivered = false;
    private bool deliver = false; // when true the bowl is moving toward its resting place
    // keeping track of the bowls trajectory in relation to time
    private float DeliveryEndTime = 0;
    private float time = 0;
    // initial conditions for the bowls delivery
    private float deliveryAngle = 0;
    private float initialVelocity = 0;

    // predictor line
    private int pointsSize = 50;
    private Vector3[] points;
    private float PredictorTimeStep = 0.5f;
    
    // angle of bowl is calculated by scaling the MAX_ROTATION
    // by finding the distance of the input from the center of the x-axis,
    // the further away from the center the higher the magnitude of the angle
    // the distance of the input from the center is normalised between 0 and 1
    // and then the angle is calculated by multiplying that normalised value with MAX_ROTATION
    //
    // valid angles are between -MAX_ROTATION and MAX_ROTATION
    // the angle will be negative when the bowl is rotated clockwise or
    // when its aiming to the right of the center line and it will be positive
    // when its aiming to the left of the center line
    // the sign of the angle encodes the bias of the bowl so that the bowl
    // automatically switches bias to always be curving towards the center line
    const float MAX_ROTATION = 35;

    // input for controlling the bowl will only be considered when it is equal to or bellow this value
    float VALID_Y_INPUT = 1/5f * Screen.height; 
    // the initial velocity of the bowl is calculated by multiplying the MAX_VELOCITY with the
    // the difference between the inputs y value and VALID_Y_INPUT normalised 
    // between 0 and 1 if its bellow VALID_Y_INPUT
    const float MAX_VELOCITY = 9;
    
    void Start(){
        points = new Vector3[pointsSize];

        Bounds bounds = GetComponent<Renderer>().bounds;
        BowlRadius = bounds.extents.y;
    }

    void FixedUpdate(){
        if(deliver && !delivered){    
            Rigidbody rigidbody = GetComponent<Rigidbody>();

            if(time < DeliveryEndTime){
                Conditions conditions = BowlPhysics.GetBowlConditions(initialVelocity, deliveryAngle, 1, time);

                // calculate the velocity vector of the bowl at the current timestep
                Vector3 velocity = new Vector3(Mathf.Sin(conditions.BowlAngle) * conditions.CurrentSpeed, 0, Mathf.Cos(conditions.BowlAngle) * conditions.CurrentSpeed);
                rigidbody.velocity = velocity;
            }
            else{
                rigidbody.velocity = new Vector3(0,0,0);

                // delete this script off of the bowl
                Destroy(GetComponent<BowlLauncher>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(deliver){ 
           HandleDelivery();
        }
        else if(Input.touchCount > 0 && !deliver){
            HandleInput();
        }
    }

    void HandleDelivery(){
        time += Time.deltaTime;

        if(time < DeliveryEndTime){
            // // find the position the bowl should currently be in
            Vector3 pos = BowlPhysics.DeliveryPath(initialVelocity, deliveryAngle, 1, time);
            Vector3 pos_diff = pos - transform.position;
            transform.position = new Vector3(pos.x, transform.position.y, pos.z);

            Conditions conditions = BowlPhysics.GetBowlConditions(initialVelocity, deliveryAngle, 1, time);

            // rotate the bowl around the y-axis so its faceing the correct way
            float rotation;
            if(deliveryAngle < 0){ // left bias
                rotation = conditions.BowlAngle - transform.rotation.eulerAngles.y;
            }
            else{ // right bias
                rotation = -((transform.rotation.eulerAngles.y - 360) + conditions.BowlAngle);
            }
            transform.Rotate(new Vector3(0, rotation ,0), Space.World);
            
            float rotationChange = (pos_diff.magnitude/BowlRadius) / (Mathf.PI/180);
            transform.Rotate(rotationChange, 0, 0, Space.Self);
        }
    }

    void HandleInput(){
        Touch touch = Input.GetTouch(0); 
        bool updatePredictor = true;

        // handle any touch input
        switch(touch.phase){
            case TouchPhase.Ended:
                // if the position is within the bottom (1/3)rd-ish then launch the bowl
                // otherwise don't launch it
                if(touch.position.y < VALID_Y_INPUT){
                    delivered = false;
                    deliver = true;
                    DeliveryEndTime = BowlPhysics.DeliveryEndTime(initialVelocity, deliveryAngle, 1);
                    lineRenderer.enabled = false;
                }
                break;

            case TouchPhase.Began:
                goto case TouchPhase.Moved;
            case TouchPhase.Moved:
                // touch input should only be valid on the bottom (1/3)rd-ish of the phone
                if(touch.position.y < VALID_Y_INPUT){
                    float middle = Screen.width/2;
                    // if the bowl is being rolled to the right of the center line then
                    // this will be a negative value which is what we want.
                    // since we are encoding bias in the sign of the launch angle
                    // and a left bias delivery must be rolled to the right of the center line
                    // of the green.
                    float distFromMidX = touch.position.x - middle;
                    float distFromValidY = System.Math.Abs(touch.position.y - VALID_Y_INPUT);

                    initialVelocity = MAX_VELOCITY * (distFromValidY/VALID_Y_INPUT);
                    deliveryAngle = -MAX_ROTATION * (distFromMidX/middle);
                    transform.rotation = Quaternion.Euler(0, deliveryAngle , 0);
                    
                    updatePredictor = true;
                }
                break;
            default:
                updatePredictor = false;
                break;
        }

        // if needed update predictor line
        if(!deliver && updatePredictor){
            UpdatePathPrediction();
        }

    }

    void UpdatePathPrediction(){
        float PredictorEndTime = BowlPhysics.DeliveryEndTime(initialVelocity, deliveryAngle, 1);
        int steps = (int)Math.Ceiling(PredictorEndTime/PredictorTimeStep);
            
        // if points isn't large enough resize it so it is
        if(pointsSize < steps){
            points = new Vector3[steps];
        }
        
        for(int step = 0; step < steps; step++){
            points[step] = BowlPhysics.DeliveryPath(initialVelocity, deliveryAngle, 1, PredictorTimeStep * step);
        }

        lineRenderer.positionCount = steps;
        lineRenderer.SetPositions(points);
        lineRenderer.enabled = true;
    }

    void OnCollisionEnter(Collision collision){

    }
}