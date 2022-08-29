using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BowlLauncher : MonoBehaviour
{
    public Collider rinkFloor;
    public LineRenderer lineRenderer;
    private Rigidbody rigidbody;
    private float BowlRadius;

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
    const float MAX_VELOCITY = 5;

    void Start(){
        points = new Vector3[pointsSize];

        Bounds bounds = GetComponent<Renderer>().bounds;
        BowlRadius = bounds.extents.y;

        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate(){
        if(deliver){    
            if(time < DeliveryEndTime){
                Vector3 direction = BowlPhysics.GetCurrentDirection(initialVelocity, deliveryAngle, 0, time);
                direction = direction.normalized;
                Vector3 velocity = direction * BowlPhysics.GetCurrentVelocity(initialVelocity, deliveryAngle, 0, time);
                rigidbody.velocity = velocity;

                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, rigidbody.position);
                lineRenderer.SetPosition(1, rigidbody.position+velocity);
                lineRenderer.enabled = true;
            }
            else{
                lineRenderer.enabled = false;
                rigidbody.useGravity = true;
                //rigidbody.velocity = new Vector3(0,0,0);
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
        else if(Input.touchCount > 0){
            HandleInput();
        }
    }

    private void HandleDelivery(){
        if(time == 0){
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            //rigidbody.useGravity = true;
        }

        time += Time.deltaTime;
        if(time < DeliveryEndTime){
            // // find the position the bowl should currently be in
            Vector3 pos = BowlPhysics.DeliveryPath(initialVelocity, deliveryAngle, 0, time);
            pos = BowlPhysics.GameToUnityCoords(pos);
            Vector3 pos_diff = pos - transform.position;
            transform.position = new Vector3(pos.x, transform.position.y, pos.z);

            //Conditions conditions = BowlPhysics.GetBowlConditions(initialVelocity, deliveryAngle, 0, time);
            Vector3 euler_angles = transform.localEulerAngles;
            
            Vector3 direction = BowlPhysics.GetCurrentDirection(initialVelocity, deliveryAngle, 0, time);
            if(direction.magnitude > 0.02){
                float angle = BowlPhysics.GetBowlAngle(direction);
            
                euler_angles.y = angle;
            }
            Debug.Log((pos_diff.magnitude/BowlRadius) / (Mathf.PI/180));
            euler_angles.x += (pos_diff.magnitude/BowlRadius) / (Mathf.PI/180);
            transform.localEulerAngles = euler_angles;
        }
    }

    private void HandleInput(){
        Touch touch = Input.GetTouch(0); 
        bool updatePredictor = true;

        // handle any touch input
        switch(touch.phase){
            case TouchPhase.Ended:
                // if the position is within the bottom (1/3)rd-ish then launch the bowl
                // otherwise don't launch it
                if(touch.position.y < VALID_Y_INPUT){
                    deliver = true;
                    DeliveryEndTime = BowlPhysics.DeliveryEndTime(initialVelocity, deliveryAngle, 0);
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

                    initialVelocity = 2 + (MAX_VELOCITY-2) * (distFromValidY/VALID_Y_INPUT);
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

    private void UpdatePathPrediction(){
        float PredictorEndTime = BowlPhysics.DeliveryEndTime(initialVelocity, deliveryAngle, 0);
        int steps = (int)Math.Ceiling(PredictorEndTime/PredictorTimeStep);
            
        // if points isn't large enough resize it so it is
        if(pointsSize < steps){
            points = new Vector3[steps];
        }
        
        for(int step = 0; step < steps; step++){
            points[step] = BowlPhysics.GameToUnityCoords(BowlPhysics.DeliveryPath(initialVelocity, deliveryAngle, 0, PredictorTimeStep * step));
        }

        lineRenderer.positionCount = steps;
        lineRenderer.SetPositions(points);
        lineRenderer.enabled = true;
    }

    public void MakeDelivery(float angle, float InitVel){
        initialVelocity = InitVel;
        deliveryAngle = angle;
        transform.rotation = Quaternion.Euler(0, deliveryAngle , 0);
        deliver = true;
        DeliveryEndTime = BowlPhysics.DeliveryEndTime(initialVelocity, deliveryAngle, 0);
    }

    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.name != "Rink"){
            lineRenderer.enabled = false;
            rigidbody.useGravity = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            // delete this script off of the bowl to stop following the path and let the physics
            // system do the rest
            Destroy(GetComponent<BowlLauncher>());
        }
    }
}