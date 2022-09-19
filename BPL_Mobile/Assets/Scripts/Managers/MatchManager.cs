using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public LineRenderer linerenderer;
    public GameObject bowlPrefab;
    public GameObject jackPrefab;

    // Basic Management of Camera Movement for Demo
    private Camera mainCam;
    private Vector3 originalCameraLocation;
    private Quaternion originalCameraRotation;
    private Vector3 cameraBowlOffset;
    private Vector3 originalCameraBowlOffset;
    private float rotationTime = 0;
    private float endRotationTime = 6f; // seconds
    private ScoringManager scm;

    //GameStateManager gsm = GameStateManager.Instance;
    GameObject currentBowl = null;
    Transform currentBowlTr = null;
    GameObject Jack;
    List<GameObject> Team1Bowls = new List<GameObject>();
    List<GameObject> Team2Bowls = new List<GameObject>();
    bool PlayerTurn = true;
    private AI ai;
    private bool ai_keep_looping = false;
    private bool spawnbowl = true;

    void Start(){
        // create the jack and set it in the correct position
        Jack = Instantiate(jackPrefab, BowlPhysics.GameToUnityCoords(new Vector2(0, 15)) + new Vector3(0, 0.0215f, 0), Quaternion.identity);
        ai = new AI();
        ai.difficulty = AIDifficulty.HARD;
        mainCam = Camera.main;
        originalCameraLocation = mainCam.transform.position;
        originalCameraRotation = mainCam.transform.rotation;
        scm = FindObjectOfType<ScoringManager>();

        Rigidbody JackRigidbody = Jack.GetComponent<Rigidbody>();
        JackRigidbody.sleepThreshold = 10f;
    }

    // Read the head for scoring purposes
    public void ReadHead(){
        if(scm)
        {
            scm.ReadTheHead();
        }
    }

    void Update(){
        Play();

        if(currentBowl != null)
        {
            if(currentBowl.GetComponent<BowlMovement>().inDelivery){
                if(rotationTime < endRotationTime){
                    rotationTime += Time.deltaTime;
                    float endAngle = 50;
                    float endRadius = 4;

                    Vector3 endVector = new Vector3(0, MathF.Cos(endAngle * (MathF.PI/180)), -MathF.Sin(endAngle * (MathF.PI/180))) * 4;
                    cameraBowlOffset = Vector3.Slerp(originalCameraBowlOffset, endVector, rotationTime/endRotationTime);
                    float angle = endAngle * (rotationTime/endRotationTime) - mainCam.transform.localEulerAngles.x;
                    mainCam.transform.Rotate(angle, 0, 0);
                }
            }

            mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, currentBowl.transform.position + cameraBowlOffset, 0.2f);
        }
    }

    private void Play(){
        if(currentBowl == null){
            // wait for all bowls and the jack to stop moving
            if(stillMoving()){
                return;
            }

            currentBowl = SpawnBowl();
            currentBowlTr = currentBowl.GetComponent<Transform>();
            mainCam.transform.position = originalCameraLocation;
            mainCam.transform.rotation = originalCameraRotation;
            cameraBowlOffset = originalCameraLocation - currentBowl.transform.position;
            originalCameraBowlOffset = cameraBowlOffset;
            rotationTime = 0;
            ReadHead();

            if(!PlayerTurn){
                currentBowl.GetComponent<BowlID>().SetTeam(2);
                Transform JackTransform = Jack.GetComponent<Transform>();
                ai.TakeTurn(currentBowl, JackTransform.position, Team1Bowls, Team2Bowls);
            }
            else{
                currentBowl.GetComponent<BowlID>().SetTeam(1);
            }
        }
        else{
            // wait for the bowl to finish its delivery
            if(currentBowl.GetComponent<BowlLauncher>() == null){
                if(PlayerTurn){
                    Team1Bowls.Add(currentBowl);
                }
                else{
                    Team2Bowls.Add(currentBowl);
                }
                
                currentBowl = null;
                PlayerTurn = !PlayerTurn;
                spawnbowl = true;
            }
        }

    }

    // returns true if any bowl or the jack is still moving
    private bool stillMoving(){
        List<GameObject> objects = GetLiveBowls();
        objects.Add(Jack);

        foreach(GameObject bowl in objects){
            BowlMovement bm = bowl.GetComponent<BowlMovement>();
            if(bm.isMoving){
                return true;
            }
        }

        return false;
    }

    private void TestAI(){
        if(currentBowl == null || ai_keep_looping){
            if(!PlayerTurn){
                if(spawnbowl){
                    // create a new bowl
                    ReadHead();
                    currentBowl = SpawnBowl();
                    currentBowl.GetComponent<BowlID>().SetTeam(2);
                    spawnbowl = false;
                    mainCam.transform.position = originalCameraLocation;
                    cameraBowlOffset = originalCameraLocation - currentBowl.transform.position;
                }
                
                Transform JackTransform = Jack.GetComponent<Transform>();
                Rigidbody JackRigidbody = Jack.GetComponent<Rigidbody>();
                JackRigidbody.sleepThreshold = 10f;
                
                ai_keep_looping = ai.TakeTurn(currentBowl, JackTransform.position, Team1Bowls, Team2Bowls);
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
            // wait for the bowl to finish its delivery
            if(currentBowl.GetComponent<BowlLauncher>() == null){
                if(PlayerTurn){
                    Team1Bowls.Add(currentBowl);
                }
                else{
                    Team2Bowls.Add(currentBowl);
                }
                
                currentBowl = null;
                PlayerTurn = !PlayerTurn;
                spawnbowl = true;
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
        List<GameObject> bowls = new List<GameObject>();
        foreach(GameObject bowl in Team1Bowls){
            bowls.Add(bowl);
        }
        foreach(GameObject bowl in Team2Bowls){
            bowls.Add(bowl);
        }
        return bowls;
    }

    // Called Externally to reset bowls and jack for new end
    public void CleanUpBowls()
    {
        // Looping through and destroying all bowls within current bowls list
        for(int index = 0; index < Team1Bowls.Count; index++)
        {
            Destroy(Team1Bowls[index]);
        }
        for(int index = 0; index < Team2Bowls.Count; index++)
        {
            Destroy(Team2Bowls[index]);
        }

        // Resetting List
        Team1Bowls = new List<GameObject>();
        Team2Bowls = new List<GameObject>();
        // Creating new Jack
        Destroy(Jack.gameObject);
        Jack = Instantiate(jackPrefab, BowlPhysics.GameToUnityCoords(new Vector2(0, 15)) + new Vector3(0, 0.0215f, 0), Quaternion.identity);
    }
}