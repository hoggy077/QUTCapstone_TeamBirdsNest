//#define TEST
using System;
using System.Collections;
using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;

public class AI
{
    public List<GameObject> lrs; // currently used for testing

    private GameStateManager gsm = GameStateManager.Instance;
    public AIDifficulty difficulty = AIDifficulty.HARD;
    private int turns = 0;
    private int accurateShots = 0;
    //private bool HavePowerPlay = true;
    
    public AI(){
        lrs = new List<GameObject>();
    }

    // TODO: remove bowls that are in the ditch and arne't still marked as active or "chalked"
    public bool TakeTurn(GameObject CurrentBowl, Vector3 _JackPos, List<GameObject> PlayerBowls, List<GameObject> AIBowls, float biasStrength){
        CurrentBowl.GetComponent<TrackThisThing>().IncludeInSave = true;
        Vector2 JackPos = BowlPhysics.UnityToGameCoords(_JackPos);
        var (PlayerPositions, AIPositions) = gameObjectToBowlPosition(PlayerBowls, AIBowls, JackPos);
        bool playerCloser;
        bool takeAccurateShot = GetAccurateShotDecision(difficulty);

        if(AIPositions.Count == 0 && PlayerPositions.Count == 0){ // first turn for anyone
            TakeRandomShot(CurrentBowl, 0.5f, JackPos);
        }

        if( AIPositions.Count == 0 && PlayerPositions.Count == 1){ // first turn for AI, Second delivery overall
            playerCloser = true;
        }
        else{
            playerCloser = AIPositions[0].MagFromJack < PlayerPositions[0].MagFromJack ? false : true;
        }

        if(takeAccurateShot){
            var (availablePolygons, bias) = GetCloserPolygons(JackPos, PlayerPositions[0].MagFromJack, PlayerPositions, AIPositions, biasStrength);
            if(availablePolygons.Count > 0){ // The AI can get the bowl closer
                DeliverBowlWithinPolygons(CurrentBowl, availablePolygons, bias);
                return false;
            }
        }
        else{
            var (availablePolygons, bias) = GetFurtherPolygons(JackPos, PlayerPositions[0].MagFromJack, PlayerPositions, AIPositions, biasStrength);
            if(availablePolygons.Count > 0){
                DeliverBowlWithinPolygons(CurrentBowl, availablePolygons, bias);
                return false;
            }
        }

        if(playerCloser){ // player has the closest bowl
            // can we hit key opponents bowl away?
            // can we hit the jack away to a ?
        }
        else{ // AI has the closest bowl
            // place a bowl to protect a bowl close to jack
        }

        Debug.Log("Error, shouldn't get here, taking random shot");
        TakeRandomShot(CurrentBowl, 0.5f, JackPos);
        return false;
    }

    // decide if this shot will be accurate or not using the AI difficulty 
    // and a random number between 0 and 1
    // returns true if the shot will be accurate
    // false if not
    private bool GetAccurateShotDecision(AIDifficulty difficulty){
        float outcome = UnityEngine.Random.value;
        
        switch(difficulty){
            case AIDifficulty.EASY:
                if(outcome <= 2f/6f){
                    return true;
                }
            break;
            case AIDifficulty.MEDIUM:
                if(outcome <= 3f/6f){
                    return true;
                }
            break;
            case AIDifficulty.HARD:
                if(outcome <= 4f/6f){
                    return true;
                }
            break;
            case AIDifficulty.HARDER:
                if(outcome <= 5f/6f){
                    return true;
                }
            break;
        }

        return false;
    }
    
    // deliver the bowl to a random point within a collection of polygons so that each point within the given polygons
    // has the same probability of being selected
    public void DeliverBowlWithinPolygons(GameObject CurrentBowl, List<List<PointD>> availablePolygons, Bias bias){
        List<Triangle> triangles = new List<Triangle>();
        foreach(List<PointD> poly in availablePolygons){
            triangles.AddRange(Polygon.TriangulatePolygon(poly));            
        }
        // sort the triangles in ascending order based on their area
        triangles.Sort();

        // foreach(GameObject g in lrs){
        //     GameObject.Destroy(g);
        // }
        // lrs = new List<GameObject>();
        // find the total area of all the triangles
        float totalArea = 0;
        foreach(Triangle t in triangles){
            totalArea += t.area;

            // GameObject g = new GameObject();
            // g.AddComponent<LineRenderer>();
            // lrs.Add(g);
            //TestingUtils.DrawTriangle(t, g.GetComponent<LineRenderer>());
        }
        
        // choose a random triangle taking into account the area of the triangle
        // since we want to uniformly choose a random point within the available space
        float rnd = UnityEngine.Random.value;
        int ti = 0;
        float runningPercentage = 0;
        for(int i = 0; i < triangles.Count; i++){
            runningPercentage += triangles[i].area / totalArea;

            if(rnd < runningPercentage){
                ti = i;
                break;
            }
        }
        
        // find a random point inside the chosen triangle
        Vector2 position = triangles[ti].RndPointInsideTriangle();
        TakeAccurateShot(CurrentBowl, position, bias);
    }

    // returns a set of polygons that contain valid points for the bowl to be delivered to without a collision that is closer to the
    // jack than the given radius
    public (List<List<PointD>> polygons, Bias bias) GetFurtherPolygons(Vector3 position, float radius, List<BowlPosition> bowls1, List<BowlPosition> bowls2, float biasStrength){
        List<List<PointD>> circle = Polygon.GetCirclePolygon(position, radius-0.05f, 30);
        
        // check if AI can get another bowl closer than the closest players bowl
        // meaning the AI gets another point
        Bias bias = Bias.Left;
        if(UnityEngine.Random.value < 0.5){
            bias = Bias.Right;
        }
        
        List<List<PointD>> validPolygons = new List<List<PointD>>();

        for(int i = 0; i < 2; i++){
            if(i == 1){
                if(bias == Bias.Left){
                    bias = Bias.Right;
                }
                else{
                    bias = Bias.Left;
                }
            }

            List<List<PointD>> rinkBounds = Polygon.GetBiasRinkBoundary(bias, biasStrength);
            List<List<PointD>> polygons = Clipper.Difference(rinkBounds, circle, FillRule.EvenOdd);

            validPolygons = GetValidPolygons(position, polygons, bowls1, bowls2, bias, biasStrength);

            if(validPolygons.Count != 0){
                break;
            }
        }

        return (validPolygons, bias);
    }

    // returns a set of polygons that contain valid points for the bowl to be delivered to without a collision that is closer to the
    // jack than the given radius
    public (List<List<PointD>> polygons, Bias bias) GetCloserPolygons(Vector3 position, float radius, List<BowlPosition> bowls1, List<BowlPosition> bowls2, float biasStrength){
        
        List<List<PointD>> circle = Polygon.GetCirclePolygon(position, radius-0.05f, 30);

        // check if AI can get another bowl closer than the closest players bowl
        // meaning the AI gets another point
        Bias bias = Bias.Left;
        if(UnityEngine.Random.value < 0.5){
            bias = Bias.Right;
        }
        
        List<List<PointD>> validPolygons = new List<List<PointD>>();

        for(int i = 0; i < 2; i++){
            if(i == 1){
                if(bias == Bias.Left){
                    bias = Bias.Right;
                }
                else{
                    bias = Bias.Left;
                }
            }

            validPolygons = GetValidPolygons(position, circle, bowls1, bowls2, bias, biasStrength);

            if(validPolygons.Count != 0){
                break;
            }
        }

        return (validPolygons, bias);
    }

    private List<List<PointD>> GetValidPolygons(Vector3 position, List<List<PointD>> polygons, List<BowlPosition> bowls1, List<BowlPosition> bowls2, Bias bias, float biasStrength){
        List<List<PointD>> pathBoundaryPolygons = new List<List<PointD>>();
        List<List<PointD>> availablePointsPolygons = new List<List<PointD>>();

        pathBoundaryPolygons = Polygon.GetPolygonPaths(bowls1, bowls2, bias, biasStrength);

        availablePointsPolygons = Clipper.Difference(polygons, pathBoundaryPolygons, FillRule.NonZero);
        availablePointsPolygons = Clipper.Intersect(availablePointsPolygons, Polygon.GetInternalRinkBoundary(), FillRule.NonZero);
        
        return availablePointsPolygons;
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

    private void TakeAccurateShot(GameObject bowl, Vector2 endPoint, Bias bias){
        InitialConditions ics = BowlPhysics.GetInitialConditions(endPoint, bias, 0);
        BowlLauncher bl = bowl.GetComponent<BowlLauncher>();
        bl.MakeDelivery(ics.Angle, ics.InitVel, bias);
    }

    private (List<BowlPosition> playerPositions, List<BowlPosition> AIPositions) gameObjectToBowlPosition(List<GameObject> PlayerObjects, List<GameObject> AIObjects, Vector2 JackPos){
        List<BowlPosition> PlayerPositions = new List<BowlPosition>();
        List<BowlPosition> AIPositions = new List<BowlPosition>();

        // for both teams bowls get the vector3 position vector 
        // update it so it's in the correct coordinate system
        // and add it to the new list
        foreach(var bowl in PlayerObjects){
            Transform transform = bowl.GetComponent<Transform>();
            Vector3 bowlPos = BowlPhysics.UnityToGameCoords(transform.position);
            PlayerPositions.Add(new BowlPosition(bowlPos, JackPos));
        }

        foreach(var bowl in AIObjects){
            Transform transform = bowl.GetComponent<Transform>();
            Vector3 bowlPos = BowlPhysics.UnityToGameCoords(transform.position);
            AIPositions.Add(new BowlPosition(bowlPos, JackPos));
        }
    
        // make sure the bowls are sorted in lowest to highest order
        PlayerPositions.Sort();
        AIPositions.Sort();

        return (PlayerPositions, AIPositions);
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