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
    public GameObject currentBowl = null;
    Transform currentBowlTr = null;
    GameObject Jack;
    List<GameObject> Team1Bowls = new List<GameObject>();
    List<GameObject> Team2Bowls = new List<GameObject>();
    public bool PlayerTurn = true;
    private AI ai;

    // For functionality with new powerplay query menu and delays between throws
    private ScorecardUI sUI;
    private bool scorecardUpdateAnimationPlayed = true;
    private bool scorecardViewed = true;
    private float scorecardUpdateDelay = 0.4f;
    public UIFlyInFlyOut touchToContinue;
    private bool updatedScoring = false;

    private bool loadedBowls = true;

    [Header("Jack Skins")]
    [SerializeField] private Material normalJack;
    [SerializeField] private Material powerplayJack;

    [Header("Player Objects")]
    private List<CharacterAppearanceUpdater> team1Players = new List<CharacterAppearanceUpdater>();
    private List<CharacterAppearanceUpdater> team2Players = new List<CharacterAppearanceUpdater>();

    private void Awake()
    {
        ResumeManager.SessionLoaded += LoadFromPreviousSession;
        foreach(CharacterAppearanceUpdater player in FindObjectsOfType<CharacterAppearanceUpdater>())
        {
            if(player.teamID == 1)
            {
                team1Players.Add(player);
            }
            else
            {
                team2Players.Add(player);
            }
        }
    }

    void OnDisable()
    {
        ResumeManager.SessionLoaded -= LoadFromPreviousSession;
    }

    void Start(){
        // create the jack and set it in the correct positionc -- unless we loaded one in from the save system boiii
        if (Jack == null && GameObject.FindGameObjectsWithTag("Jack").Length == 0)
        {
            Jack = Instantiate(jackPrefab, RandomJackPosition() + new Vector3(0, 0.0215f, 0), Quaternion.identity);
        }
        else
        {
            Jack = GameObject.FindGameObjectWithTag("Jack");
        }

        // get a random position for the bowl spawning
        GameStateManager.Instance.bowlSpawnZPosition = randomBowlZPosition();
        ai = new AI();
        ai.difficulty = AIDifficulty.MEDIUM;
        mainCam = Camera.main;
        originalCameraLocation = new Vector3(0, mainCam.transform.position.y, 0) + BowlPhysics.GameToUnityCoords(new Vector2(0, 0)) + new Vector3(0, 0, -1);
        
        originalCameraRotation = mainCam.transform.rotation;
        scm = FindObjectOfType<ScoringManager>();

        Rigidbody JackRigidbody = Jack.GetComponent<Rigidbody>();
        JackRigidbody.sleepThreshold = 10f;

        mainCam.GetComponent<CameraFollow>().LookAt(Jack.transform);
        sUI = FindObjectOfType<ScorecardUI>();

        //SetNewPlayerPositions(1);
        //SetNewPlayerPositions(2);
    }

    // Read the head for scoring purposes
    public void ReadHead(){
        if(scm)
        {
            scm.ReadTheHead(Team1Bowls.Count, Team2Bowls.Count);
        }
    }

    public void PlayBetweenShotAnimation()
    {
        // Animating and updating scorecard
        if (!stillMoving() && !scorecardUpdateAnimationPlayed && !scorecardViewed)
        {
            sUI.Reposition(false);

            if(mainCam != null)
            {
                mainCam.GetComponent<CameraFollow>().enabled = true;
            }

            if(sUI.fullyOnScreen)
            {
                scorecardUpdateDelay -= Time.deltaTime;

                if(scorecardUpdateDelay < 0f)
                {
                    ReadHead();

                    touchToContinue.FlyIn();

                    if (Input.touchCount > 0)
                    {
                        if (Input.GetTouch(0).phase == TouchPhase.Ended)
                        {
                            scorecardViewed = true;
                        }
                    }
                }
            }
        }
    }

    void Update(){

        //TestAI();
        Play();

        if(currentBowl != null)
        {
            if(currentBowl.GetComponent<BowlMovement>().inDelivery){
                if(rotationTime < endRotationTime){
                    rotationTime += Time.deltaTime;
                    float endAngle = 50;
                    //float endRadius = 4;

                    Vector3 endVector = new Vector3(0, MathF.Cos(endAngle * (MathF.PI/180)), -MathF.Sin(endAngle * (MathF.PI/180))) * 4;
                    cameraBowlOffset = Vector3.Slerp(originalCameraBowlOffset, endVector, rotationTime/endRotationTime);
                    float angle = endAngle * (rotationTime/endRotationTime) - mainCam.transform.localEulerAngles.x;
                    mainCam.transform.Rotate(angle, 0, 0);
                }

                mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, currentBowl.transform.position + cameraBowlOffset, 0.2f);
            }
        }

        // If Jack has not had reference gathered after 
        if(Jack == null)
        {
            Jack = GameObject.FindGameObjectWithTag("Jack");
            mainCam.GetComponent<CameraFollow>().LookAt(Jack.transform);
        }

        // Check if bowls have been loaded from a save, if they have save the bowls to their correct groups for further useage
        if(loadedBowls == false)
        {
            loadedBowls = true;

            foreach(BowlID bowl in FindObjectsOfType<BowlID>())
            {
                if (bowl.GetComponent<BowlMovement>().inDelivery)
                {
                    if (bowl.GetTeam() == 1 && !Team1Bowls.Contains(bowl.gameObject))
                    {
                        bowl.SetTeam(1);
                        Team1Bowls.Add(bowl.gameObject);
                    }
                    else if (bowl.GetTeam() == 2 && !Team2Bowls.Contains(bowl.gameObject))
                    {
                        bowl.SetTeam(2);
                        Team2Bowls.Add(bowl.gameObject);
                    }
                }
            }

            scm.ReadTheHead(Team1Bowls.Count, Team2Bowls.Count);
        }

        // Moving Camera to Overview State if Button is Pressed
        if(Jack != null)
        {
            if (sUI.submenuState == ScorecardUI.SubmenuState.OverheadCamera)
            {
                mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, Jack.transform.position + new Vector3(0f, 5f, 0f), Time.deltaTime * 5f);
                mainCam.transform.LookAt(Jack.transform.position);
            }
            else if(currentBowl != null && !currentBowl.GetComponent<BowlMovement>().inDelivery)
            {
                mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, originalCameraLocation, Time.deltaTime * 5f);
                mainCam.transform.rotation = Quaternion.Lerp(mainCam.transform.rotation, originalCameraRotation, Time.deltaTime * 5f);
            }
        }
    }

    private void ditchNeededBowls(){
        foreach(GameObject go in Team1Bowls){
            BowlID bi = go.GetComponent<BowlID>();

            if(bi != null){
                if(bi.enteredDitch){
                    bi.inDitch = true;
                }
            }
        }
    }

    private void Play(){
        if(currentBowl == null){

            ditchNeededBowls();
            

            // Animating Scorecard if required
            PlayBetweenShotAnimation();

            // wait for all bowls and the jack to stop moving
            if (!scorecardViewed)
            {
                return;
            }

            mainCam.GetComponent<CameraFollow>().enabled = false;

            // Updating Scoring to Allow for Switchs in End and Sets
            if (!updatedScoring)
            {
                scm.CheckScore();
                updatedScoring = true;
            }

            touchToContinue.FlyOut();

            // Update Shots Display for Each Player
            List<int> shotsRemainingTeammate = new List<int>();

            if (PlayerTurn)
            {
                foreach (uint shot in scm.currentScore.team1teammateShots)
                {
                    shotsRemainingTeammate.Add((int)shot);
                }
            }
            else
            {
                foreach (uint shot in scm.currentScore.team2teammateShots)
                {
                    shotsRemainingTeammate.Add((int)shot);
                }
            }

            sUI.UpdateTeammateShots(shotsRemainingTeammate.ToArray());

            // Wait for Powerplay Selection
            if (PowerplayQuery.instance.CurrentlyOpen())
            {
                return;
            }

            // AI choose to enter Powerplay or not
            if (!GameStateManager.Instance.isMultiplayerMode && Team1Bowls.Count == 0 && Team2Bowls.Count == 0){
                if(UnityEngine.Random.value < 1/5f && scm.team2PowerplayAvailable){
                    scm.ActivatePowerplay(2);
                }
                else if(scm.currentEnd == 5 && scm.team2PowerplayAvailable){
                    scm.ActivatePowerplay(2);
                }
            }

            // Updating Jack to Reflect Powerplay State
            if (scm.CurrentlyInPowerplay())
            {
                Jack.GetComponent<MeshRenderer>().material = powerplayJack;
            }
            else
            {
                Jack.GetComponent<MeshRenderer>().material = normalJack;
            }

            // Allowing scoring to update on next runthrough
            updatedScoring = false;

            // Preparing Scorecard Animation
            scorecardUpdateAnimationPlayed = false;
            scorecardUpdateDelay = 0.4f;
            scorecardViewed = false;

            ToggleHidePlayers(false);

            currentBowl = SpawnBowl();
            currentBowlTr = currentBowl.GetComponent<Transform>();
            //originalCameraLocation = BowlPhysics.GameToUnityCoords(new Vector2(0, 0)) + new Vector3(0, 0, -1);
            mainCam.transform.rotation = originalCameraRotation;
            cameraBowlOffset = originalCameraLocation - currentBowl.transform.position;
            originalCameraBowlOffset = cameraBowlOffset;
            rotationTime = 0;
            ReadHead();
            ResumeManager.WipeSaveFile();

            if (!PlayerTurn){
                currentBowl.GetComponent<BowlID>().SetTeam(2);

                // Saving Game and Career
                ResumeManager.SaveGame();
                CareerRecordManager.SaveCareer();

                scm.SetTeammate(scm.team2CurrentTeammate);

                if (!GameStateManager.Instance.isMultiplayerMode)
                {
                    ToggleHidePlayers(true);

                    scm.team2CurrentTeammate = UnityEngine.Random.Range(0, 2);
                    scm.SetTeammate(scm.team2CurrentTeammate);

                    Transform JackTransform = Jack.GetComponent<Transform>();
                    ai.TakeTurn(currentBowl, JackTransform.position, Team1Bowls, Team2Bowls, 1);
                }
                else
                {
                    if (GetLiveBowls().Count < 2 || scm.currentScore.team2teammateShots[scm.team2CurrentTeammate] == 0)
                    {
                        sUI.ToggleTeammateMenu();
                    }
                }
            }
            else{

                ResumeManager.SaveGame();
                CareerRecordManager.SaveCareer();

                currentBowl.GetComponent<BowlID>().SetTeam(1);

                scm.SetTeammate(scm.team1CurrentTeammate);

                if (GetLiveBowls().Count < 2 || scm.currentScore.team1teammateShots[scm.team1CurrentTeammate] == 0)
                {
                    sUI.ToggleTeammateMenu();
                }
            }
        }
        else{
            // wait for the bowl to finish its delivery
            if(currentBowl.GetComponent<BowlLauncher>() == null){
                if(PlayerTurn && !Team1Bowls.Contains(currentBowl))
                {
                    Team1Bowls.Add(currentBowl);
                }
                else if(!PlayerTurn && !Team2Bowls.Contains(currentBowl)) {
                    Team2Bowls.Add(currentBowl);
                }
                
                currentBowl = null;

                // Updating Teammate Scorecard
                scm.PlayerShotTaken();
                PlayerTurn = !PlayerTurn;
            }
        }

    }

    // returns true if any bowl or the jack is still moving
    private bool stillMoving(){
        List<GameObject> objects = GetLiveBowls();
        objects.Add(Jack);

        foreach(GameObject bowl in objects){
            if (bowl != null)
            {
                BowlMovement bm = bowl.GetComponent<BowlMovement>();
                if (bm.isMoving)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void TestAI(){
        if(currentBowl == null){
            if(!PlayerTurn){
               
                // create a new bowl
                ReadHead();
                currentBowl = SpawnBowl();
                currentBowl.GetComponent<BowlID>().SetTeam(2);
                mainCam.transform.position = originalCameraLocation;
                cameraBowlOffset = originalCameraLocation - currentBowl.transform.position;
            
            }
            else{
                ReadHead();
                currentBowl = SpawnBowl();
                mainCam.transform.position = originalCameraLocation;
                cameraBowlOffset = originalCameraLocation - currentBowl.transform.position;
                currentBowl.GetComponent<BowlID>().SetTeam(1);
            }

            mainCam.transform.position = originalCameraLocation;
            mainCam.transform.rotation = originalCameraRotation;
            cameraBowlOffset = originalCameraLocation - currentBowl.transform.position;
            originalCameraBowlOffset = cameraBowlOffset;
            rotationTime = 0;
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
    public void CleanUpBowls(bool replaceJack = true)
    {
        // Looping through and destroying all bowls within current bowls list
        for(int index = 0; index < Team1Bowls.Count; index++)
        {
            Team1Bowls[index].GetComponent<TrackThisThing>().IncludeInSave = false;
            Destroy(Team1Bowls[index]);
        }
        for(int index = 0; index < Team2Bowls.Count; index++)
        {
            Team2Bowls[index].GetComponent<TrackThisThing>().IncludeInSave = false;
            Destroy(Team2Bowls[index]);
        }

        // Resetting List
        Team1Bowls = new List<GameObject>();
        Team2Bowls = new List<GameObject>();

        

        // Creating new Jack
        if (Jack != null)
        {
            Jack.GetComponent<TrackThisThing>().IncludeInSave = false;
            Destroy(Jack.gameObject);
        }

        if (replaceJack)
        {         
            Jack = Instantiate(jackPrefab, RandomJackPosition() + new Vector3(0, 0.0215f, 0), Quaternion.identity);
            // get a random new bowl spawning position
            GameStateManager.Instance.bowlSpawnZPosition = randomBowlZPosition();
            mainCam.GetComponent<CameraFollow>().LookAt(Jack.transform);
        }
    }

    private Vector3 RandomJackPosition(){
        float rand = UnityEngine.Random.value;
        if(rand < 1/3f){
            return new Vector3(0, 0, 14);
        }
        else if(rand > 1/3f && rand <= 2/3f){
            return new Vector3(0, 0, 16);
        }
        else if(rand > 2/3f && rand <= 1){
            return new Vector3(0, 0, 18);
        }

        return new Vector3(0, 14);
    }

    private float randomBowlZPosition(){
        float rand = UnityEngine.Random.value;
        if(rand < 1/3f){
            return -17;
        }
        else if(rand > 1/3f && rand <= 2/3f){
            return -15;
        }
        else if(rand > 2/3f && rand <= 1){
            return -13;
        }

        return -17;
    }

    // Function called at Loading of a previous Session
    public void LoadFromPreviousSession()
    {
        // Respawning Jack from Shadow Realm
        CleanUpBowls(false);

        // Respawning Bowls From Shadow Realm
        loadedBowls = false;

        PlayerTurn = GameStateManager.Instance.isPlayerTurnLoaded;
    }

    public void ToggleHidePlayers(bool hide)
    {
        foreach(CharacterAppearanceUpdater player in team1Players)
        {
            player.hideCharacter = hide;
        }

        foreach (CharacterAppearanceUpdater player in team2Players)
        {
            player.hideCharacter = hide;
        }
    }

    public void HideBowlingPlayer(int teamID, int characterID)
    {
        if(teamID == 1)
        {
            foreach(CharacterAppearanceUpdater player in team1Players)
            {
                if(player.characterID == characterID)
                {
                    player.playerBowling = true;
                }
                else
                {
                    player.playerBowling = false;
                }
            }
        }
        else
        {
            foreach (CharacterAppearanceUpdater player in team2Players)
            {
                if (player.characterID == characterID)
                {
                    player.playerBowling = true;
                }
                else
                {
                    player.playerBowling = false;
                }
            }
        }
    }

    public void SetNewPlayerPositions(int team)
    {
        bool outsideRink = true;
        if (team == 1)
        {
            foreach (CharacterAppearanceUpdater player in team1Players)
            {
                List<GameObject> obstacles = new List<GameObject>();
                //obstacles.AddRange(GetLiveBowls());
                foreach (CharacterAppearanceUpdater t2player in team2Players)
                {
                    obstacles.Add(t2player.gameObject);
                }

                player.RepositionCharacter(Jack.transform, GetLiveBowls(), !outsideRink);
                if (!player.playerBowling)
                {
                    outsideRink = false;
                }
            }
        }
        else
        {
            foreach (CharacterAppearanceUpdater player in team2Players)
            {
                List<GameObject> obstacles = new List<GameObject>();
                //obstacles.AddRange(GetLiveBowls());
                foreach (CharacterAppearanceUpdater t1player in team1Players)
                {
                    obstacles.Add(t1player.gameObject);
                }

                player.RepositionCharacter(Jack.transform, GetLiveBowls(), !outsideRink);
                if (!player.playerBowling)
                {
                    outsideRink = false;
                }
            }
        }
    }
}