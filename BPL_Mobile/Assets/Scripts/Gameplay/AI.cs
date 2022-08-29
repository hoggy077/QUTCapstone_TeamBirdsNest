using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI
{
    //private bool HavePowerPlay = true;
    private GameStateManager gsm = GameStateManager.Instance_;

    public AI(){
    }

    // TODO: remove bowls that are in the ditch and arne't still marked as active or "chalked"
    public void TakeTurn(GameObject CurrentBowl, Vector3 JackPos, List<GameObject> Team1Bowls, List<GameObject> Team2Bowls){

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

        }
        else{
            BowlLauncher bl = CurrentBowl.GetComponent<BowlLauncher>();
            bl.MakeDelivery(ics.Angle, ics.InitVel);
        }
    }

    // TODO: change return type to include a bail out return value when the search has been going on to long
    private (InitialConditions ics, bool aborted) GetTargetPosition(GameObject CurrentBowl, Vector3 JackPos, List<Vector3> Team1Bowls, List<Vector3> Team2Bowls){
        LineRenderer lineRenderer = CurrentBowl.GetComponent<LineRenderer>();
        Vector2 TargetPos = new Vector2();
        Bias bias = Bias.Left;
        float radius = 0.127f;
        float radius_step = 0.06135f;
        float angle_step = 1;
        float angle = angle_step;

        // get the first proposed end position
        TargetPos.x = JackPos.z + radius_step;
        TargetPos.y = JackPos.x;
        InitialConditions ics = BowlPhysics.GetInitialConditions(TargetPos, bias, 0);

        // This is the first turn, there are no bowls
        if(Team1Bowls.Count == 0 && Team2Bowls.Count == 0){
            return (ics, false);
        }
        
        // do until either:
        //      1. an end position without collisions along trajectory is found
        //      or
        //      2. an upper limit of time or attempts has been reached then
        //          switch over to power delivery and find a bowl or the jack
        //          to hit out of the way
        for(int tries = 0; tries < 1000; tries++){
            // get next target position
            angle += angle_step;
            if(angle >= 360){
                Debug.Log(tries);
                angle = 0;
                radius += radius_step;
            }

            TargetPos = GetProposedTarget(JackPos, angle, radius);
            
            for(int run = 1; run <= 2; run++){

                if(run == 1){
                    bias = Bias.Left;
                }
                else{
                    bias = Bias.Right;
                }

                ics = BowlPhysics.GetInitialConditions(TargetPos, bias, 0);

                Vector3[] BowlTrajectory = BowlPhysics.GetBowlTrajectory(ics.InitVel, ics.Angle, 0);
                

                // if(lineRenderer != null){
                //     // update line renderer
                //     lineRenderer.positionCount = BowlTrajectory.Length;
                //     for(int i = 0; i < BowlTrajectory.Length; i++){
                //         lineRenderer.SetPosition(i, BowlPhysics.GameToUnityCoords(BowlTrajectory[i]));
                //     }

                //     //System.Threading.Thread.Sleep(2000);
                //     //lineRenderer.SetPositions(BowlTrajectory);
                //     lineRenderer.enabled = true;
                // }



                float x_bound = 0;
                // if the trajectory goes outside the rink continue to the next iteration
                for(int i = 0; i < BowlTrajectory.Length; i++){
                    if(MathF.Abs(BowlTrajectory[i].x) >= 2.5) continue;
                    if(BowlTrajectory[i].x > x_bound) x_bound = BowlTrajectory[i].x;
                }

                // check the end position for collisions
                List<Vector3> collisions = GetCollisions(BowlTrajectory, ics, x_bound, JackPos, Team1Bowls, Team2Bowls);
                
                if(collisions.Count == 0){
                    return (ics, false);
                }
                else{
                    // try and resolve the collisions by moving the end position in a 
                    // way that avoids the collisions
                    // (and possibly changing the biased side on the bowl)

                    // if the collision can be resolved return the new end position

                    // otherwise collision can't be resolved so set a new target position
                }
            }
        }

        return (ics, true);
    }


    private Vector2 GetProposedTarget(Vector3 JackPos, float angle, float radius){
        Vector2 JackPosV2 = new Vector2(JackPos.z, JackPos.x);
        angle = angle / (MathF.PI/180);
        Vector2 pos = new Vector2(radius * MathF.Cos(angle), radius * MathF.Sin(angle));
        return JackPosV2 - pos;
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
            BowlHaystack.Add(position);
        }
        foreach(var position in Team2Positions){
            BowlHaystack.Add(position);
        }
        
        foreach(var position in BowlHaystack){
            if(CheckForCollisionInPath(BowlTrajectory, ics, position)){
                collisions.Add(position);
            }
        }

        return collisions;
    }

    // TODO: probably some easy optimisations to do here
    private bool CheckForCollisionInPath(Vector3[] BowlTrajectory, InitialConditions ics, Vector3 BowlPosition){
        Bounds BowlPositionBound = new Bounds(BowlPosition, new Vector3(0.0635f, 10f, 0.0635f));
        // Debug.Log(String.Format("z = {0}, x = {1}", BowlPosition.z, BowlPosition.y));

        for(int i = BowlTrajectory.Length-1; i >= 0; i--){
            Bounds TrajectoryBound = new Bounds(BowlTrajectory[i], new Vector3(0.0635f, 10f, 0.0635f));
            if(TrajectoryBound.Intersects(BowlPositionBound)){   
                    return true;
            }
        }
        return false;
    }

    private bool CheckForCollision(Vector3 trajectoryPosition, float time, InitialConditions ics, Vector3 restingBowl){
        //Conditions conditions = BowlPhysics.GetBowlConditions(ics.InitVel, ics.Angle, 0, time);
        
        return false;
    }

    // returns the best possible strategy to use for the current turn based on all the available information
    // 
    // position of jack, bowls,
    // private TurnStrategy GetTurnStrategy(Vector3 JackPos, List<GameObject> Team1Bowls, List<GameObject> Team2Bowls){
    // }

    private enum TurnStrategy{
        PowerDrive, // smash other bowls or the jack away
        StandardDelivery // avoid any collisions whilst getting the bowl as close as possible
    }
}