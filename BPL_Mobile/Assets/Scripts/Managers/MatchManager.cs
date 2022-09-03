using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public GameObject bowlPrefab;
    public GameObject jackPrefab;

    // Basic Management of Camera Movement for Demo
    private Camera mainCam;
    private Vector3 originalCameraLocation;
    private Vector3 cameraBowlOffset;
    private ScoringManager scm;

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
        mainCam = Camera.main;

        originalCameraLocation = mainCam.transform.position;
        scm = FindObjectOfType<ScoringManager>();
    }

    // Read the head for scoring purposes
    public void ReadHead(){
        if(scm)
        {
            scm.ReadTheHead();
        }
    }

    void Update(){
        TestAI();

        if(currentBowl == null)
        {
            mainCam.transform.position = originalCameraLocation;
        }
        else
        {
            mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, currentBowl.transform.position + cameraBowlOffset, 0.2f);
        }
    }

    private void TestAI(){
        if(currentBowl == null){
            if(!PlayerTurn){
                // create a new bowl
                ReadHead();
                currentBowl = SpawnBowl();
                currentBowl.GetComponent<BowlID>().SetTeam(2);

                Transform JackTransform = Jack.GetComponent<Transform>();

                ai.TakeTurn(currentBowl, JackTransform.position, activeBowls, new List<GameObject>());
            }
            else{
                ReadHead();
                currentBowl = SpawnBowl();
                mainCam.transform.position = originalCameraLocation;
                cameraBowlOffset = originalCameraLocation - currentBowl.transform.position;
                currentBowl.GetComponent<BowlID>().SetTeam(1);
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

    // Functions to get jack and bowl list
    public GameObject GetJack()
    {
        return Jack;
    }

    public List<GameObject> GetLiveBowls()
    {
        return activeBowls;
    }

    // Called Externally to reset bowls and jack for new end
    public void CleanUpBowls()
    {
        // Looping through and destroying all bowls within current bowls list
        for(int index = 0; index < activeBowls.Count; index++)
        {
            Destroy(activeBowls[index]);
        }

        // Resetting List
        activeBowls = new List<GameObject>();

        // Creating new Jack
        Destroy(Jack.gameObject);
        Jack = Instantiate(jackPrefab, BowlPhysics.GameToUnityCoords(new Vector3(0, 0, 15)), Quaternion.identity);
    }
}