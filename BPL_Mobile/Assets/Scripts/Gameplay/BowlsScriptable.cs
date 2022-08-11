using UnityEngine;

[CreateAssetMenu(fileName = "NewBowl", menuName = "Bowls Aus/Create New Bowl")]
public class BowlsScriptable : ScriptableObject
{
    public Mesh DefaultMesh;
    public Texture BowlTexture;

    public double Bias;
    public Rigidbody rigidbody;
    public LineRenderer lineRenderer;

    private bool delivered = false;
    private bool deliver = false;
    private float delivery_end_time = 0;
    private float time = 0;
    private float deliveryAngle = 0;
    private float initialVelocity = 0;

    void FixedUpdate(){

        if(deliver && !delivered){
            // BOWL DELIVERY USING INBUILT PHYSICS ENGINE
            // // make sure the bowl is rotated correctly
            // // transform.rotation = Quaternion.Euler(0, deliveryAngle , 0);
            // Bounds bounds = GetComponent<Renderer>().bounds;

            // // find the center of mass offset
            // float offset = bounds.extents.y * 0.31f;

            // Vector3 com = new Vector3();
            // // add the center of mass offset for the bias of the bowl
            // if(deliveryAngle < 0){
            //     com.x = -offset;
            // }else{
            //     com.x = offset;
            // }
            // rigidbody.centerOfMass = com;
            // rigidbody.AddForce(transform.forward*initialVelocity*6.2f, ForceMode.VelocityChange);
                
            time += Time.deltaTime;

            // TODO: change the angle of the bowl as its moving along the path
            // TODO: add rotation to the bowl
            if(time < delivery_end_time){
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

        const float MAX_ROTATION = 35;
        const float MAX_VELOCITY = 9;
        float VALID_Y_INPUT = 1/5f * Screen.height;
        float middle = Screen.width/2;

        if(Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
    
            // TODO: make sure the code for the bowl path uses the sign of the angle as the bias
            
            
            bool updatePredictor = true;

            switch(touch.phase){
                case TouchPhase.Ended:
                    // if the position is within the bottom (1/3)rd-ish then launch the bowl
                    // otherwise don't launch it
                    if(touch.position.y < VALID_Y_INPUT){
                        delivered = false;
                        deliver = true;

                        delivery_end_time = stop_time(initialVelocity, deliveryAngle, 1);
                    }
                    goto case TouchPhase.Moved;
                case TouchPhase.Moved:
                    // TODO: don't update predictor if the position hasn't moved enough

                    goto case TouchPhase.Began;
                case TouchPhase.Began:
                    // touch input should only be valid on the bottom (1/3)rd-ish of the phone
                    if(touch.position.y < VALID_Y_INPUT){
                        // if the bowl is being rolled to the righ t of the center line then
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
                        Debug.Log(deliveryAngle);
                        Debug.Log("\n");
                        Debug.Log(initialVelocity);
                    }
                    break;

                default:
                    updatePredictor = false;
                    break;
            }

            if(!deliver && updatePredictor){
                float timeStepDiff = 0.5f;
                float endTime = stop_time(initialVelocity, deliveryAngle, 1);
                int steps = (int)Math.Ceiling(endTime/timeStepDiff);
                
                Vector3[] points = new Vector3[steps];

                for(int step = 0; step < steps; step++){
                    points[step] = delivery_path(initialVelocity, deliveryAngle, 1, timeStepDiff * step);
                }

                lineRenderer.positionCount = steps;
                lineRenderer.SetPositions(points);
            }
            
            
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
            angle = angle;
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