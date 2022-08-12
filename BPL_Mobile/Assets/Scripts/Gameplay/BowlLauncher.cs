using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BowlLauncher : MonoBehaviour
{
    public LineRenderer lineRenderer;

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
    
    // used to define the relationship between inputs 
    // and angle of bowl and initial velocity
    // 
    // rotation of bowl is calculated as a scaled 
    const float MAX_ROTATION = 35;
    const float MAX_VELOCITY = 9;
    // input will only be considered when it is equal to or bellow this value
    float VALID_Y_INPUT = 1/5f * Screen.height; 

    void Start(){
        points = new Vector3[pointsSize];
    }

    void FixedUpdate(){
        if(deliver && !delivered){    
            time += Time.deltaTime;

            // TODO: change the angle of the bowl as its moving along the path
            // TODO: add rotation to the bowl
            if(time < DeliveryEndTime){
                // find the position we should currently be in
                Vector3 pos = delivery_path(initialVelocity, deliveryAngle, 1, time);

                // mode the bowls position
                Vector3 bPos = new Vector3(pos.x, transform.position.y, pos.z);

                transform.position = bPos;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(delivered){
            return;
        }

        float middle = Screen.width/2;

        if(Input.touchCount > 0){
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

                        DeliveryEndTime = stop_time(initialVelocity, deliveryAngle, 1);
                        
                    } else{

                        lineRenderer.enabled = false;
                    }
                    break;
                case TouchPhase.Began:
                    goto case TouchPhase.Moved;
                case TouchPhase.Moved:
                    // touch input should only be valid on the bottom (1/3)rd-ish of the phone
                    if(touch.position.y < VALID_Y_INPUT){
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
                float PredictorEndTime = stop_time(initialVelocity, deliveryAngle, 1);
                int steps = (int)Math.Ceiling(PredictorEndTime/PredictorTimeStep);
                    
                // if points isn't large enough resize it so it is
                if(pointsSize < steps){
                    points = new Vector3[(int)Math.Ceiling((float)pointsSize/steps)];
                }
                
                for(int step = 0; step < steps; step++){
                    points[step] = delivery_path(initialVelocity, deliveryAngle, 1, PredictorTimeStep * step);
                }

                lineRenderer.positionCount = steps;
                lineRenderer.SetPositions(points);
                lineRenderer.enabled = true;
            }
        }
        else{

            lineRenderer.enabled = false;
        }
    }

    // returns the time step when the velocity of the bowl will be zero
    float stop_time(float initVel, float angle, float MuScale){
        // velocity = 0 when t = initVecl/(MuScale * Gravity)
        return initVel/((0.025f + (MuScale * 0.003f)) * 9.8f);
    }

    Vector3 delivery_path(float initVel, float angle, float MU_scale, float t){

        angle = angle *(Mathf.PI / 180);
        Bias bias;

        if(angle < 0){
            bias = Bias.Left;
        }else{
            bias = Bias.Right;
        }

        var MU = 0.025f + (MU_scale * 0.003f);
        var G = 9.8f; //(m/s^2) velocity due to gravity
        //var R = 6f; // (cm) radius of the bowl
        //var Mass = 1.5f; // (kg)
        //var d = 0.7f; // (mm) distance of the center of gravity to the geometric center of the bowl
        var p = 3.8f; //(2.8*MU*R)/d 

        var r0 = (p * (initVel*initVel)) / (2*MU*G); // initial radius of curvature of the path of the bowl
    
        var v = initVel - MU*G*t; // velocity at particular time step
        float phi = (2/p)*Mathf.Log(initVel/v); // angle between the tangent of the bowls path with the x-axis
        var lamba = Mathf.Exp(-p*phi); // 

        float x;
        float y;

        // end points
        float Y_e = r0/(1+(p*p));
        float X_e = p*Y_e;

        // bias is on the left
        if(bias == Bias.Left){
            x = (r0/(1+p*p))*(p - p*lamba* Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
            y = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
        }
        // bias is on the right
        else{
            x = -(r0/(1+p*p))*(p - p*lamba*Mathf.Cos(phi)+lamba*Mathf.Sin(phi));
            y = (r0/(1+p*p))*(1 - lamba*Mathf.Cos(phi) - p*lamba*Mathf.Sin(phi));
        }

        if(bias == Bias.Right){
            angle += Mathf.PI;
        }
        
        float rotatedX = x*Mathf.Cos(angle) - y*Mathf.Sin(angle);
        float rotatedY = x*Mathf.Sin(angle) + y*Mathf.Cos(angle);

        return new Vector3(rotatedY, 0.01f, rotatedX-9);
    }

    float angle_between_points(Vector2 P1, Vector2 P2){
        // if points are 180 degrees from each other then there will be no
        // triangle to construct, we can make sure this isn't the case by 
        // checking if P2 is some multiple of P1
        if(P2.y - P1.y*(P2.x/P1.x) == 0){
            return Mathf.PI;
        }

        Vector2 P1ToP2 = P1 - P2;

        float angle = Mathf.Acos(((-Mathf.Pow(P1ToP2.magnitude, 2) + Mathf.Pow(P1.magnitude, 2) + Mathf.Pow(P2.magnitude, 2))/ (2*P1.magnitude*P2.magnitude)));
        Debug.Log(angle);
        float rot_x = P1.x * Mathf.Cos(angle) - P1.y * Mathf.Sin(angle);
        float rot_y = P1.x * Mathf.Sin(angle) + P1.y * Mathf.Cos(angle);

        if(rot_x != P2.x && rot_y != P2.y){
            angle = -angle;
        }

        return angle;
    }
}

public enum Bias{
    Left,
    Right
}