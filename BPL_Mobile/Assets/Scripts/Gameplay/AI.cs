//#define TEST
using System;
using System.Collections;
using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;

public class AI
{
    public List<GameObject> lrs; // currently used for testing

    private GameStateManager gsm = GameStateManager.Instance_;
    public AIDifficulty difficulty = AIDifficulty.HARD;
    private int turns = 0;
    private int accurateShots = 0;
    //private bool HavePowerPlay = true;
    
    public AI(){
    }

    // TODO: remove bowls that are in the ditch and arne't still marked as active or "chalked"
    public bool TakeTurn(GameObject CurrentBowl, Vector3 JackPos, List<GameObject> PlayerBowls, List<GameObject> AIBowls){
        turns++;
        #if TEST // TESTING {
            if(Input.touchCount > 0){ 
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
            } 
        #endif // TESTING }
        
        List<BowlPosition> PlayerPositions = new List<BowlPosition>();
        List<BowlPosition> AIPositions = new List<BowlPosition>();

        Vector2 JackPos2 = BowlPhysics.UnityToGameCoords(JackPos);

        // for both teams bowls get the vector3 position vector 
        // update it so it's in the correct coordinate system
        // and add it to the new list
        foreach(var bowl in PlayerBowls){
            Transform transform = bowl.GetComponent<Transform>();
            Vector3 bowlPos = BowlPhysics.UnityToGameCoords(transform.position);
            PlayerPositions.Add(new BowlPosition(bowlPos,JackPos2));
        }

        foreach(var bowl in AIBowls){
            Transform transform = bowl.GetComponent<Transform>();
            Vector3 bowlPos = BowlPhysics.UnityToGameCoords(transform.position);
            AIPositions.Add(new BowlPosition(bowlPos,JackPos2));
        }
    
        // make sure the bowls are sorted in lowest to highest order
        PlayerPositions.Sort();
        AIPositions.Sort();

        if(AIPositions.Count == 0 && PlayerPositions.Count == 0){ // first turn for anyone
            Bias bias;
            // get random radius
            float radius = UnityEngine.Random.value  + 0.5f;

            // get random end position
            Vector2 target = UnityEngine.Random.insideUnitCircle.normalized * radius;

            if(target.y > 0){
                bias = Bias.Left;
            }else{
                bias = Bias.Right;
            }

            // take the shot
            TakeAccurateShot(CurrentBowl, JackPos2 + target, bias);

            return false;
        }

        bool takeAccurateShot = false;
        float outcome = UnityEngine.Random.value;
        switch(difficulty){
            case AIDifficulty.EASY:
                if(outcome <= 2/6){
                    takeAccurateShot = true;
                    accurateShots++;
                }
            break;
            case AIDifficulty.MEDIUM:
                if(outcome <= 3/6){
                    takeAccurateShot = true;
                    accurateShots++;
                }
            break;
            case AIDifficulty.HARD:
                if(outcome <= 4/6){
                    takeAccurateShot = true;
                    accurateShots++;
                }
            break;
            case AIDifficulty.HARDER:
                if(outcome <= 5/6){
                    takeAccurateShot = true;
                    accurateShots++;
                }
            break;
        }

        bool playerCloser;
        if( AIPositions.Count == 0 && PlayerPositions.Count == 1){ // first turn for AI, Second delivery overall
            playerCloser = true;
        }
        else{
            playerCloser = AIPositions[0].MagFromJack < PlayerPositions[0].MagFromJack ? false : true;
        }
       
        if(playerCloser){ // player has the closest bowl

            var (availablePolygons, bias) = GetValidPolygons(JackPos2, PlayerPositions[0].MagFromJack, PlayerPositions, AIPositions);
            if(availablePolygons.Count > 0){ // The AI can get the bowl closer
                //if(takeAccurateShot){
                    DeliverBowlCloser(CurrentBowl, availablePolygons, bias);
                // }else{
                //     TakeRandomShot(CurrentBowl, PlayerPositions[0].MagFromJack, JackPos2);
                // }
                
                return false;
            }
            else if(false){ // can we hit key opponents bowl away?

            }
            else if(false){ // can we hit the jack away to a ?

            }
            else{
                
            }
        }
        else{ // AI has the closest bowl
            var (availablePolygons, bias) = GetValidPolygons(JackPos2, PlayerPositions[0].MagFromJack, PlayerPositions, AIPositions);
            if(availablePolygons.Count > 0){ // The AI can score another point
                //if(takeAccurateShot){
                    DeliverBowlCloser(CurrentBowl, availablePolygons, bias);
                // }else{
                //     TakeRandomShot(CurrentBowl, PlayerPositions[0].MagFromJack, JackPos2);
                // }
                return false;
            }
            else { // place a bowl to protect a bowl close to jack

            }
        }

        if(turns == 6){
            turns = 0;
            accurateShots = 0;
        }

        return false;
    }

    public void TakeRandomShot(GameObject CurrentBowl, float innerRadius, Vector2 JackPos){
        Bias bias;

        float r = 1.5f - innerRadius;
        // get random radius
        float radius = UnityEngine.Random.value * r;
        radius = radius + innerRadius;
        // get random end position
        Vector2 target = UnityEngine.Random.insideUnitCircle.normalized * radius;

        if(target.y > 0){
            bias = Bias.Left;
        }else{
            bias = Bias.Right;
        }

        // take the shot
        TakeAccurateShot(CurrentBowl, JackPos + target, bias);
    }

    public void DeliverBowlCloser(GameObject CurrentBowl, List<List<PointD>> availablePolygons, Bias bias){
        int polygonIndex = (int)MathF.Ceiling((availablePolygons.Count-1) * UnityEngine.Random.value);
        var triangles = Polygon.TriangulatePolygon(availablePolygons[polygonIndex]);
        int triangleIndex = (int)MathF.Ceiling((triangles.Count-1) * UnityEngine.Random.value);
        
        Vector2 position = triangles[triangleIndex].RndPointInsideTriangle();
        TakeAccurateShot(CurrentBowl, position, bias);
    }

    public (List<List<PointD>> polygons, Bias bias) GetValidPolygons(Vector3 position, float radius, List<BowlPosition> bowls1, List<BowlPosition> bowls2){
        // check if AI can get another bowl closer than the closest players bowl
        // meaning the AI gets another point
        Bias bias = Bias.Left;
        if(UnityEngine.Random.value < 0.5){
            bias = Bias.Right;
        }
        
        List<List<PointD>> circle = Polygon.GetCirclePolygon(position, radius-0.05f, 30);
        List<List<PointD>> pathBoundaryPolygons = new List<List<PointD>>();
        List<List<PointD>> availablePointsPolygons = new List<List<PointD>>();
        bool found = false;

        for(int i = 0; i < 2; i++){
            if(i == 1){
                if(bias == Bias.Left){
                    bias = Bias.Right;
                }
                else{
                    bias = Bias.Left;
                }
            }

            pathBoundaryPolygons = Polygon.GetPolygonPaths(bowls1, bowls2, bias);
            availablePointsPolygons = Clipper.Difference(circle, pathBoundaryPolygons, FillRule.NonZero);
            if(availablePointsPolygons.Count != 0){
                found = true;
                break;
            }
        }

        if(found){
            if(lrs != null){
                foreach(GameObject go in lrs){
                    GameObject.Destroy(go);
                }
            }

            // lrs = new List<GameObject>();
            // foreach(List<PointD> path in availablePointsPolygons){
                
            //     GameObject go = new GameObject();
            //     go.AddComponent<LineRenderer>();
            //     lrs.Add(go);
            //     TestingUtils.drawPolygon(Polygon.PathToVec2(path), go.GetComponent<LineRenderer>());
            // }
        }

        return (availablePointsPolygons, bias);
    }

    private void TakeDifficultyScaledShot(GameObject bowl, Vector2 endPoint, Bias bias, AIDifficulty difficulty){
        float radius = 1;
        switch(difficulty){
            case(AIDifficulty.EASY):
                radius = 0.8f;
            break;
            case(AIDifficulty.MEDIUM):
                radius = 0.5f;
            break;
            case(AIDifficulty.HARD):
                radius = 0.2f;
            break;
            case(AIDifficulty.HARDER):
                radius = 0;
            break;
        }
        
        TakeAccurateShot(bowl, endPoint + (UnityEngine.Random.insideUnitCircle * radius), bias);
    }

    private void TakeAccurateShot(GameObject bowl, Vector2 endPoint, Bias bias){
        InitialConditions ics = BowlPhysics.GetInitialConditions(endPoint, bias, 0);
        BowlLauncher bl = bowl.GetComponent<BowlLauncher>();
        bl.MakeDelivery(ics.Angle, ics.InitVel, bias);
    }
}

//TODO: update this to consider the mesh of the bowl, since this assumes that the 
//      distance from the center of the bowl to the outside is the same for every angle 
//      but this isn't true since the bowl isnt a perfect sphere
//
public class BowlPosition : IComparable<BowlPosition>{
    public float MagFromJack {get;}
    public Vector3 BowlPos {get;}

    public BowlPosition(Vector3 BowlPos, Vector3 JackPos){
        this.MagFromJack = (JackPos - BowlPos).magnitude;
        this.BowlPos = BowlPos;
    }

    public int CompareTo(BowlPosition otherBowl){
        if(MagFromJack < otherBowl.MagFromJack){
            return -1;
        }
        else if(MagFromJack > otherBowl.MagFromJack){
            return 1;
        }
        else{
            return 0;
        }
    }
}

public enum AIDifficulty{
    EASY,
    MEDIUM,
    HARD,
    HARDER
}