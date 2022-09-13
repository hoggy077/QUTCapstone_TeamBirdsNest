using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;

public class TestingAI : MonoBehaviour
{
    public LineRenderer linerenderer;
    public GameObject bowlPrefab;
    public GameObject jackPrefab;
    private Vector3 jackStartPos = new Vector3(0, 0, 6);
    GameObject Jack;
    int numPlayerBowls = 4;
    List<GameObject> PlayerBowls;
    int numAIBowls = 4;
    List<GameObject> AIBowls;
    GameObject[] Bowls;
    int numBowls = 8;
    bool bowlsInitialised = false;
    private AI ai;

    void Start(){
        // create the jack and set it in the correct position
        Jack = Instantiate(jackPrefab, jackStartPos, Quaternion.identity);

        ai = new AI();
        ai.difficulty = AIDifficulty.IMPOSSIBLE;
        linerenderer.enabled = false;

        Rigidbody JackRigidbody = Jack.GetComponent<Rigidbody>();
        JackRigidbody.sleepThreshold = 10f;
        Bowls = new GameObject[numBowls];

        Vector2[] pointsOne = new Vector2[4];
        // test getting a circle
        // Vector3[] circle = Polygon.PathToVec(Polygon.GetCirclePolygon(new Vector2(0, 6), 1, 20)[0]);
        // TestingUtils.drawPolygon(circle, linerenderer);

        // test getting arc
        // Vector3[] arc = Polygon.PathToVec(Polygon.GetArcPolygon(new Vector2(0, 6), 1, 0.01f, 0, MathF.PI/4, 10)[0]);
        // TestingUtils.drawPolygon(arc, linerenderer);
    }

    void Update(){
        Vector3 referencePosition = new Vector3(-1, 0, 0);
        float minRadius = 0.127f;
        float maxRadius = 1.5f;

        bool next_test = false;
        if(Input.touchCount > 0){
            Touch touch = Input.GetTouch(0); 
            // handle any touch input
            if(touch.phase == TouchPhase.Began){
                next_test = true;
            }
        }
        
        if(next_test){
            if(bowlsInitialised){
                // delete old bowlsa
                foreach(GameObject bowl in PlayerBowls){
                    Destroy(bowl);
                }
                foreach(GameObject bowl in AIBowls){
                    Destroy(bowl);
                }
            }else{
                bowlsInitialised = true;
            }

            PlayerBowls = new List<GameObject>();
            AIBowls = new List<GameObject>();

            for(int i = 1; i <= numPlayerBowls; i++){
                // get random angle
                float angle = UnityEngine.Random.value * MathF.PI;
                // get random radius
                float radius = (maxRadius-minRadius)*UnityEngine.Random.value + minRadius;

                // spawn bowl
                Vector3 BowlPosition = new Vector3(MathF.Cos(MathF.PI + angle), 0, MathF.Sin(MathF.PI + angle));
                //Bowls[i-1] = SpawnBowl(jackStartPos + BowlPosition*radius);
                PlayerBowls.Add(SpawnBowl(new Vector3(0, 0, 15) + BowlPosition*radius));
            }

            for(int i = 1; i <= numAIBowls; i++){
                // get random angle
                float angle = UnityEngine.Random.value * MathF.PI;
                // get random radius
                float radius = (maxRadius-minRadius)*UnityEngine.Random.value + minRadius;

                // spawn bowl
                Vector3 BowlPosition = new Vector3(MathF.Cos(MathF.PI + angle), 0, MathF.Sin(MathF.PI + angle));
                //Bowls[i-1] = SpawnBowl(jackStartPos + BowlPosition*radius);
                AIBowls.Add(SpawnBowl(new Vector3(0, 0, 15) + BowlPosition*radius));
            }

            ai.TakeTurn(new GameObject(), jackStartPos, PlayerBowls, AIBowls);
        }
    }

    private GameObject SpawnBowl(Vector3 startingPoint){
        // create the first bowl from the prefab, at the starting point
        GameObject currentBowl = Instantiate(bowlPrefab, BowlPhysics.GameToUnityCoords(startingPoint), Quaternion.identity);
        Transform tf = currentBowl.transform;
        
        Bounds bounds = currentBowl.GetComponent<Renderer>().bounds;
    
        // rescale bowl to be 12.7 cm
        Vector3 currentScale = tf.localScale;
        tf.localScale = tf.localScale * 0.127f;//(0.127f/bounds.max.y);

        bounds = currentBowl.GetComponent<Renderer>().bounds;
        // position bowl so the bottom is touching the green
        Vector3 pos = tf.position;
        pos.y = bounds.extents.y;

        tf.position = pos;

        return currentBowl;
    }
}