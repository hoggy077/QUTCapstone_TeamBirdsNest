using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public GameObject bowlPrefab;
    public GameObject jackPrefab;

    //GameStateManager gsm = GameStateManager.Instance;
    GameObject currentBowl = null;
    GameObject Jack;
    List<GameObject> activeBowls = new List<GameObject>();
    bool PlayerTurn = true;
    private AI ai;

    void Start(){
        // create the jack and set it in the correct position
        Jack = Instantiate(jackPrefab, BowlPhysics.GameToUnityCoords(new Vector3(0, 0, 15)), Quaternion.identity);

        ai = new AI();
    }

    // read the head for scoring purposes
    public void ReadHead(){
    }

    void Update(){
        TestAI();
    }

    private void TestAI(){
        

        if(currentBowl == null){
            if(!PlayerTurn){
                // create a new bowl
                currentBowl = SpawnBowl();

                Transform JackTransform = Jack.GetComponent<Transform>();

                ai.TakeTurn(currentBowl, JackTransform.position, activeBowls, new List<GameObject>());
            }
            else{
                currentBowl = SpawnBowl();
            }
        }
        else{
            if(currentBowl.GetComponent<BowlLauncher>() == null){
                activeBowls.Add(currentBowl);
                currentBowl = null;
                PlayerTurn = !PlayerTurn;
            }
        }
    }

    private GameObject SpawnBowl(){
        
        // create the first bowl from the prefab, at the starting point
        GameObject currentBowl = Instantiate(bowlPrefab, BowlPhysics.GameToUnityCoords(new Vector3(0, 0, 0)), Quaternion.identity);
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