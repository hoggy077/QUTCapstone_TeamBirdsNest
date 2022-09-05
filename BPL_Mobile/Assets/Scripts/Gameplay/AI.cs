//#define DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI
{
    public LineRenderer lineRenderer;
    private bool test_next_position = false;

    //private bool HavePowerPlay = true;
    private GameStateManager gsm = GameStateManager.Instance_;
    private SearchParameters sp = new SearchParameters();

    List<Vector3> Team1BowlsLast;
    List<Vector3> Team2BowlsLast;

    public AI(LineRenderer lineRenderer){
        this.lineRenderer = lineRenderer;
    }

    // TODO: remove bowls that are in the ditch and arne't still marked as active or "chalked"
    public bool TakeTurn(GameObject CurrentBowl, Vector3 JackPos, List<GameObject> Team1Bowls, List<GameObject> Team2Bowls){

        #if TEST
        Debug.Log("test is on");
        if(Input.touchCount > 0){ // TESTING {
            Touch touch = Input.GetTouch(0); 
            // handle any touch input
            switch(touch.phase){
                case TouchPhase.Began:
                    test_next_position = true;
                    break;
                default:
                    test_next_position = false;
                    break;
            }
        }
        else{
            test_next_position = false;
        } // TESTING }
        #endif

        List<Vector3> Team1Positions = new List<Vector3>();
        List<Vector3> Team2Positions = new List<Vector3>();

        // for both teams bowls get the vector3 position vector 
        // update it so it's in the correct coordinate system
        // and add it to the new list
        foreach(var bowl in Team1Bowls){
            Transform transform = bowl.GetComponent<Transform>();
            Team1Positions.Add(BowlPhysics.UnityToGameCoords(transform.position));
        }

        foreach(var bowl in Team2Bowls){
            Transform transform = bowl.GetComponent<Transform>();
            Team1Positions.Add(BowlPhysics.UnityToGameCoords(transform.position));
        }

        var (ics, aborted) = GetTargetPosition(CurrentBowl, BowlPhysics.UnityToGameCoords(JackPos), Team1Positions, Team2Positions);
        
        // maximum scan tries/time was reached
        // change tactic to finding the best bowl to hit
        if(aborted){
            return true;
        }
        else{
            BowlLauncher bl = CurrentBowl.GetComponent<BowlLauncher>();
            bl.MakeDelivery(ics.Angle, ics.InitVel);
            return false;
        }
    }

    private (InitialConditions ic, bool aborted) GetTargetPosition(GameObject CurrentBowl, Vector3 JackPos, List<Vector3> Team1Bowls, List<Vector3> Team2Bowls){
        LineRenderer lineRenderer = CurrentBowl.GetComponent<LineRenderer>();
        Vector2 TargetPos = new Vector2();
        InitialConditions ic;
        TargetPos = GetProposedTarget(JackPos, sp.angle, sp.radius);
        ic = BowlPhysics.GetInitialConditions(TargetPos, sp.bias, 0);

        // This is the first turn, there are no bowls
        if(Team1Bowls.Count == 0 && Team2Bowls.Count == 0){
            return (ic, false);
        }
        
        // do until either:
        //      1. an end position without collisions along trajectory is found
        //      or
        //      2. an upper limit of time or attempts has been reached then
        //          switch over to power delivery and find a bowl or the jack
        //          to hit out of the way
        for(int tries = 0; tries < 1000; tries++){
            #if TEST
            if(test_next_position){ // TESTING
            #endif
                sp.next();

                TargetPos = GetProposedTarget(JackPos, sp.angle, sp.radius);
                
                ic = BowlPhysics.GetInitialConditions(TargetPos, sp.bias, 0);
                Vector3[] BowlTrajectory = BowlPhysics.GetBowlTrajectory(ic.InitVel, ic.Angle, 0);

                float x_bound = 0;
                
                for(int i = 0; i < BowlTrajectory.Length; i++){
                    // if the trajectory goes outside the rink continue to the next iteration
                    if(MathF.Abs(BowlTrajectory[i].x)+0.0635 >= 2.5) continue;
                    if(BowlTrajectory[i].x > x_bound) x_bound = BowlTrajectory[i].x;
                }

                // check the end position for collisions
                List<Vector3> collisions = GetCollisions(BowlTrajectory, ic, x_bound, JackPos, Team1Bowls, Team2Bowls);
                
                if(collisions.Count == 0){
                    // valid path found
                    // reset the variables 
                    sp.reset();
                    #if TEST
                    UpdatePathPrediction(ic); // TESTING
                    #endif
                    return (ic, false);
                }
            #if TEST
                 UpdatePathPrediction(ic); // TESTING
                 return (ic, true);
            }
            #endif
        }
        return (ic, true);
    }

    private Vector2 GetProposedTarget(Vector3 JackPos, float angle, float radius){
        Vector2 JackPosV2 = new Vector2(JackPos.z, JackPos.x);
        angle = angle * (MathF.PI/180);
        Vector2 pos = new Vector2(radius * MathF.Cos(angle), radius * MathF.Sin(angle));
        return JackPosV2 + pos;
    }

    private List<Vector3> GetCollisions(Vector3[] BowlTrajectory, InitialConditions ics, float x_bound, Vector3 JackPos, List<Vector3> Team1Positions, List<Vector3> Team2Positions){
        List<Vector3> collisions = new List<Vector3>();
        int end_index = BowlTrajectory.Length-1;
        
        // find the bounding box of the bowls trajectory
        // x_bound is in the middle of the bowl, add the radius to it
        float z_bound = BowlTrajectory[end_index].z;
        x_bound += (x_bound/MathF.Abs(x_bound))*0.0635f;
        
        // make sure the crest of the bowls trajectory is within the rink
        List<Vector3> BowlHaystack = new List<Vector3>();

        // put the bowls within the bounding box into one list, which will be the list of bowls we 
        // actually care about
        foreach(var position in Team1Positions){
            if(CheckForCollisionInPath(BowlTrajectory, ics, position)){
                collisions.Add(position);
            }
        }

        foreach(var position in Team2Positions){
            if(CheckForCollisionInPath(BowlTrajectory, ics, position)){
                collisions.Add(position);
            }
        }

        return collisions;
    }

    // TODO: probably some easy optimisations to do here
    private bool CheckForCollisionInPath(Vector3[] BowlTrajectory, InitialConditions ics, Vector3 BowlPosition){
        for(int i = BowlTrajectory.Length-1; i >= 0; i--){
            if(CheckForCollision(BowlTrajectory[i], BowlPosition)){   
                    return true;
            }
        }

        return false;
    }

    // assuming that each bowl is represented by the circle of radius 0.0635
    private bool CheckForCollision(Vector3 trajectoryPosition, Vector3 restingBowl){
        trajectoryPosition.y = 0;
        restingBowl.y = 0;
        Vector3 diff = restingBowl - trajectoryPosition;

        // if the magnitude of the vector from one bowl to another 
        // is lower than 0.127 then the bowls will collide
        if(Vector3.Distance(restingBowl, trajectoryPosition) <= 0.127){
            return true;
        }
                
        return false;
    }

    private void UpdatePathPrediction(InitialConditions ics){
        float PredictorTimeStep = 0.5f;
        float PredictorEndTime = BowlPhysics.DeliveryEndTime(ics.InitVel, ics.Angle, 0);
        int steps = (int)Math.Ceiling(PredictorEndTime/PredictorTimeStep);
        Vector3[] points = new Vector3[steps];
        
        for(int step = 0; step < steps; step++){
            points[step] = BowlPhysics.GameToUnityCoords(BowlPhysics.DeliveryPath(ics.InitVel, ics.Angle, 0, PredictorTimeStep * step));
        }

        lineRenderer.positionCount = steps;
        lineRenderer.SetPositions(points);
        lineRenderer.enabled = true;
    }

    private class SearchParameters{
        public Bias bias {get; set;} = Bias.Left;
        public float radius_iterations {get; set;} = 1;
        public float radius {get; set;} = 0.0635f;
        public float radius_step {get; set;} = 0.0635f;
        public float angle_step_no {get; set;} = 0;
        public float angle_step {get; set;} = 360/6;
        public float angle {get; set;} = -360/6;

        public void next(){
            if(bias == Bias.Left){
                bias = Bias.Right;
                if(angle_step_no == radius_iterations * 6){
                        radius += radius_step;
                        radius_iterations++;
                        angle_step = 360f/(radius_iterations * 6);
                        angle = -angle_step;
                        angle_step_no = 0;
                    }
                angle_step_no++;
                angle += angle_step;
            }
            else{
                bias = Bias.Left;
            }
        }

        public void reset(){
            bias = Bias.Left;
            radius_iterations = 1;
            radius = 0.0635f;
            radius_step = 0.0635f;
            angle_step_no = 0;
            angle_step = 360/6;
            angle = -360/6;
        }
    }
}