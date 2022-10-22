using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BowlLauncher : MonoBehaviour
{
    public Collider rinkFloor;
    public LineRenderer lineRenderer;
    private Rigidbody rb;
    private Transform tr;
    private float BowlRadius = 0.0635f;
    private float lastAngle = 0;
    public float bowlBiasStrength = 1f;

    private bool collided = false;
    private bool deliver = false; // when true the bowl is moving toward its resting place
    // keeping track of the bowls trajectory in relation to time
    private float DeliveryEndTime = 0;
    private float time = 0;
    // initial conditions for the bowls delivery
    private float deliveryAngle = 0;
    private float initialVelocity = 0;
    private Bias bias = Bias.Left;

    // predictor line
    private int pointsSize = 50;
    private Vector3[] points;
    private float PredictorTimeStep = 0.1f;

    private ScorecardUI sUI;
    private MatchManager mm;

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

    // UI Handling
    private bool setupUI = false;

    void Start(){
        points = new Vector3[pointsSize];
        
        Bounds bounds = GetComponent<Renderer>().bounds;
        //BowlRadius = bounds.extents.y;
        rb = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        sUI = FindObjectOfType<ScorecardUI>();
        mm = FindObjectOfType<MatchManager>();
        BowlOverlay.instance.scorecard.Reposition(false);
    }

    void FixedUpdate(){
        if(deliver && !collided){ 
            if(time < DeliveryEndTime){
                Vector2 direction = BowlPhysics.GetCurrentDirection(initialVelocity, deliveryAngle, bias, 0, time, bowlBiasStrength);
                direction = direction.normalized;
                Vector3 velocity = new Vector3(direction.x, 0, direction.y) * BowlPhysics.GetCurrentVelocity(initialVelocity, deliveryAngle, 0, time);
                rb.velocity = velocity;

                // update the angular velocity
                float rotAngle = (velocity.magnitude / BowlRadius) / (MathF.PI/180);
                Vector3 dr = tr.TransformPoint(Vector3.right);
                tr.RotateAround(tr.position, dr, rotAngle * Time.deltaTime);
                
                float angularVelocitySpeed = rotAngle * Time.deltaTime;
                Vector3 angularVelocityVec = new Vector3(0, 0, angularVelocitySpeed);
                
                rb.AddRelativeTorque(-angularVelocityVec, ForceMode.VelocityChange);
            }
            else{
                destroyScript();
            }
        }
        else if(deliver && collided && rb.velocity.magnitude < 0.001){
            destroyScript();
        }
    }

    public void destroyScript(){
        GetComponent<LineRenderer>().enabled = false;
        //rb.isKinematic = false;
        GetComponent<Rigidbody>().mass = 1;
        Destroy(GetComponent<BowlLauncher>());
    }

    // Update is called once per frame
    void Update()
    {
        if(deliver){
            BowlOverlay.instance.scorecard.Reposition(true);
            HandleDelivery();
        }
        else if(Input.touchCount > 0){
            HandleInput();
        }
    }

    private void HandleDelivery(){
        if(collided){
            return;
        }

        time += Time.deltaTime;
        if(time < DeliveryEndTime){
            // // find the position the bowl should currently be in
            Vector3 pos = BowlPhysics.GameToUnityCoords(BowlPhysics.DeliveryPath(initialVelocity, deliveryAngle, bias, 0, time, bowlBiasStrength));;
            Vector3 pos_diff = pos - transform.position;
            transform.position = new Vector3(pos.x, transform.position.y, pos.z);

            // set the rotation around the y-axis of the bowl so that is follows the trajectory correctly
            Vector3 euler_angles = transform.localEulerAngles;
            Vector3 direction = BowlPhysics.GetCurrentDirection(initialVelocity, deliveryAngle, bias, 0, time, bowlBiasStrength);
            if(direction.magnitude > 0.02f){
                float angle = BowlPhysics.GetBowlAngle(direction);
                euler_angles.y = angle;
            }
            transform.localEulerAngles = euler_angles;
            
            // rotate the bowl around the local x-axis to create the illusion of it rolling
            euler_angles = transform.localEulerAngles;
            lastAngle = lastAngle % 360;
            float angle_percentage = 1 - time/DeliveryEndTime; // used to slow the amount of rotation toward the end of the trajectory
            euler_angles.x = lastAngle + (pos_diff.magnitude/(BowlRadius*2*Mathf.PI)) * 360 * angle_percentage;
            lastAngle = euler_angles.x;
            transform.localEulerAngles = euler_angles;
            
            // make sure the bowl is upright
            Vector3 globalEuler = transform.eulerAngles;
            globalEuler.z = 0f;
            transform.eulerAngles = globalEuler;
        }
    }

    private void HandleInput(){
        Touch touch = Input.GetTouch(0); 
        bool updatePredictor = false;

        // handle any touch input
        switch(touch.phase){
            case TouchPhase.Ended:
                // if the position is within the bottom (1/3)rd-ish then launch the bowl
                // otherwise don't launch it
                if(touch.position.y < VALID_Y_INPUT && sUI.submenuState == ScorecardUI.SubmenuState.None)
                {
                    deliver = true;
                    DeliveryEndTime = BowlPhysics.DeliveryEndTime(initialVelocity, deliveryAngle, 0) - 0.75f;
                    //lineRenderer.enabled = false;

                    rb.mass = 1;
                    rb.useGravity = false;
                    rb.isKinematic = false;
                    rb.detectCollisions = true;
                    rb.drag = 1000;
                    rb.angularDrag = 1000;
                    GetComponent<BowlMovement>().inDelivery = true;
                    GetComponent<TrackThisThing>().IncludeInSave = true;

                    BowlOverlay.instance.ToggleOpacity(false, true);
                    CameraZoom.instance.zoom = false;
                }
                BowlOverlay.instance.ToggleOpacity(false, false);
                CameraZoom.instance.zoom = false;
                break;
            case TouchPhase.Began:
                if (setupUI == false)
                {
                    BowlOverlay.instance.MoveToBowl(transform.position);
                }
                goto case TouchPhase.Moved;
            case TouchPhase.Moved:
                // touch input should only be valid on the bottom (1/3)rd-ish of the phone
                if(touch.position.y < VALID_Y_INPUT && sUI.submenuState == ScorecardUI.SubmenuState.None)
                {
                    CameraZoom.instance.zoom = true;
                    float middle = Screen.width/2;
                    float distFromMidX = touch.position.x - middle;
                    float distFromValidY = System.Math.Abs(touch.position.y - VALID_Y_INPUT);

                    //initialVelocity = 2 + (MAX_VELOCITY-2) * (distFromValidY/VALID_Y_INPUT);
                    initialVelocity = (MAX_VELOCITY) * (distFromValidY/VALID_Y_INPUT);
                    deliveryAngle = -MAX_ROTATION * (distFromMidX/middle);

                    if(deliveryAngle < 0){
                        bias = Bias.Right;
                    }
                    else{
                        bias = Bias.Left;
                    }

                    transform.rotation = Quaternion.Euler(0, deliveryAngle , 0);
                    
                    updatePredictor = true;
                    BowlOverlay.instance.UpdateLinePullback(touch.position);

                    BowlOverlay.instance.ToggleOpacity(true, true);
                    mm.ToggleHidePlayers(true);
                }
                else{
                    updatePredictor = false;
                    CameraZoom.instance.zoom = false;
                    BowlOverlay.instance.ToggleOpacity(false, false);
                    mm.ToggleHidePlayers(false);
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
        float PredictorEndTime = BowlPhysics.DeliveryEndTime(initialVelocity, deliveryAngle, 0) - 0.75f;
        int steps = (int)Math.Ceiling(PredictorEndTime/PredictorTimeStep);
            
        // if points isn't large enough resize it so it is
        if(pointsSize < steps){
            points = new Vector3[steps];
        }
        
        for(int step = 0; step < steps; step++){
            points[step] = BowlPhysics.GameToUnityCoords(BowlPhysics.DeliveryPath(initialVelocity, deliveryAngle, bias, 0, PredictorTimeStep * step, bowlBiasStrength)) + new Vector3(0f, 0.05f);
        }

        lineRenderer.positionCount = steps;
        lineRenderer.SetPositions(points);
        lineRenderer.enabled = true;
    }

    public void MakeDelivery(float angle, float InitVel, Bias bias){
        initialVelocity = InitVel;
        deliveryAngle = angle;
        this.bias = bias;
        transform.rotation = Quaternion.Euler(0, deliveryAngle , 0);
        deliver = true;
        DeliveryEndTime = BowlPhysics.DeliveryEndTime(initialVelocity, deliveryAngle, 0);
        GetComponent<BowlMovement>().inDelivery = true;
        Handheld.Vibrate();
        rb = GetComponent<Rigidbody>();
        rb.mass = 1;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.drag = 1000;
        rb.angularDrag = 1000;
        rb.detectCollisions = true;
    }

    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.name != "Rink" && !collided){
            lineRenderer.enabled = false;
            collided = true;

            BowlMovement bm1 = GetComponent<BowlMovement>();
            BowlMovement bm2 = collision.gameObject.GetComponent<BowlMovement>();

            if(bm2 == null){ // colliding with the environment
                // the environment won't have rigidbodies
                Vector2 dir = BowlPhysics.GetCurrentDirection(initialVelocity, deliveryAngle, bias, 0, time, bowlBiasStrength);
                dir = dir.normalized;
                Vector3 velocity = new Vector3(dir.x, 0, dir.y) * BowlPhysics.GetCurrentVelocity(initialVelocity, deliveryAngle, 0, time);
                
                Vector3 direction;
                if(collision.transform.position.x < 0){
                    direction = Vector3.left;
                } else{
                    direction = Vector3.right;
                }
                
                Vector3 colliderProj = (Vector3.Dot(direction, velocity) / Vector3.Dot(direction, direction)) * direction;
                Vector3 colliderOrtho = velocity - colliderProj;
                
                Vector3 colliderFinalVel = -colliderProj + colliderOrtho;
                
                Vector3 angularVel = -colliderFinalVel / BowlRadius; // 
                Vector3 rd = Vector3.Cross(angularVel, Vector3.up).normalized; // rotational axis

                bm1.av = angularVel;
                bm1.v = colliderFinalVel;
                bm1.iv = colliderFinalVel.magnitude;
                bm1.rd = rd;
                bm1.externalySet = true;
                bm1.isMoving = true;
            }else{ // colliding with another bowl or the jack
                Vector2 dir = BowlPhysics.GetCurrentDirection(initialVelocity, deliveryAngle, bias, 0, time, bowlBiasStrength);
                dir = dir.normalized;
                Vector3 velocity = new Vector3(dir.x, 0, dir.y) * BowlPhysics.GetCurrentVelocity(initialVelocity, deliveryAngle, 0, time);
                float mass1 = 2;
                float mass2 = collision.rigidbody.mass;
                
                Vector3 direction = collision.transform.position - transform.position;
                direction.y = 0;
                Vector3 colliderProj = (Vector3.Dot(direction, velocity) / Vector3.Dot(direction, direction)) * direction;
                Vector3 colliderOrtho = velocity - colliderProj;
                
                Vector3 collideeProj = (Vector3.Dot(-direction, collision.rigidbody.velocity) / Vector3.Dot(-direction, -direction)) * -direction;
                
                Vector3 collideeOrtho = collision.rigidbody.velocity - collideeProj;
                Vector3 colliderFinalVel = ((((mass1 - mass2) * colliderProj) + 2 * mass2 * collideeProj)/(mass1 + mass2)) + colliderOrtho;
                Vector3 collideeFinalVel = ((((mass2 - mass1) * collideeProj) + 2 * mass1 * colliderProj)/(mass1 + mass2)) + collideeOrtho;

                
                Vector3 angularVel = -colliderFinalVel / BowlRadius;
                Vector3 rd = Vector3.Cross(angularVel, Vector3.up).normalized;

                bm1.av = angularVel;
                bm1.v = colliderFinalVel;
                bm1.iv = colliderFinalVel.magnitude;
                bm1.rd = rd;
                bm1.externalySet = true;
                bm1.isMoving = true;
            
                Vector3 angularVel2 = -collideeFinalVel / BowlRadius;
                Vector3 rd2 = Vector3.Cross(angularVel2, Vector3.up).normalized;

                bm2.av = angularVel2;
                bm2.v = collideeFinalVel;
                bm2.iv = collideeFinalVel.magnitude;
                bm2.rd = rd2;
                bm2.externalySet = true;
                bm2.isMoving = true;
            }
        }
    }
}