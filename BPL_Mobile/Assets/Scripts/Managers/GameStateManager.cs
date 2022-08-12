using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    //Gameobject Singleton
    public static GameStateManager Instance { 
        get {
            GameObject gsm = new GameObject("GameStateManager");
            Instance_ = gsm.AddComponent<GameStateManager>();
            GameObject.DontDestroyOnLoad(gsm);
            return Instance_;
        }}
    static GameStateManager Instance_ = null;


    //This just tells unity to RunOn when the game loads, accessing Instance which will create one if not available
    [RuntimeInitializeOnLoadMethod]
    static void RunOn() => _ = Instance;



    #region Game related variables

    GamemodeInfo CurrentGamemode;

    #region Teams
    public Team_struct Team_1 { get; private set; }
    public Team_struct Team_2 { get; private set; }

    public void UpdateTeam(uint TeamNumber, TeamScriptable team)
    {
        if (TeamNumber > 2)
            throw new Exception($"Team number {TeamNumber} is invalid. Please enter 1 or 2");

        if (TeamNumber == 1)
            Team_1 = new Team_struct() { BaseTeam = team };
        else
            Team_2 = new Team_struct() { BaseTeam = team };
    }
    #endregion

    #region Turn
    private TurnBasedManager TurnManager = new TurnBasedManager();

    public TurnBasedManager.Turn whosTurn() => TurnManager.CurrentTurn;

    private TurnBasedManager.Turn LastValidTurn = TurnBasedManager.Turn.Team1;
    public void changeTurn(TurnBasedManager.Turn v)
    {
        if(v != TurnBasedManager.Turn.Null)
            LastValidTurn = v;

        TurnManager.UpdateTurn(v);
        TurnChanged.Invoke(TurnManager.CurrentTurn);
    }

    [Obsolete]
    public void nextTurn()
    {
        TurnManager.UpdateTurn(TurnManager.CurrentTurn == TurnBasedManager.Turn.Team1 ? TurnBasedManager.Turn.Team2 : TurnBasedManager.Turn.Team1);
        TurnChanged.Invoke(TurnManager.CurrentTurn);
    }
    #endregion

    #endregion


    #region Event Delegates for posterity
    public delegate void turnChanged_(TurnBasedManager.Turn turn);
    public event turnChanged_ TurnChanged;
    #endregion


    #region Session Saving

    //Session saving is disabled for now
    private void OnApplicationPause(bool paused)
    {
        //-- subject to change, reports of this not working 100% since 2019. Consider OnAppliationFocus instead
        //-- also might change because its uncertain if we're maintaining an in-match manager of this external manager

        //--true means we've lost focus,
        //--false means we've gained it back

        //--so on a false load the file and on a true save it
        //--only load tho, if the active scene isn't the game scene, and the use decides to resume?

        GameObject[] trackings = GameObject.FindGameObjectsWithTag("Trackable");
        TrackedObject[] tracking = new TrackedObject[trackings.Length];
        for(int i = 0; i < trackings.Length; i++)
            tracking[i] = new TrackedObject(trackings[i]);


        //SaveSystem.saveGeneric(new SavedSession()
        //{
        //    CurrentTurn = TurnManager.CurrentTurn,
        //    LastTurn = LastValidTurn,
        //    Team1_state = Team_1,
        //    Team2_state = Team_2,
        //    trackedGameObjects = tracking
        //}, "lastSession.sav");
    }

    #endregion
}

#region Team and turn manager
[Serializable]
public struct Team_struct
{
    public int Score;
    public TeamScriptable BaseTeam;
    public string Name() => BaseTeam.TeamName;
    public object[] ChosenPlayers;//-- this will need to change
    public bool HasPowerPlay;
    public List<GameObject> Teams_Active_Bowls;
}

public class TurnBasedManager
{
    public enum Turn
    {
        Team1,
        Null,
        Team2
    }

    public Turn CurrentTurn { get; private set; }

    /// <summary>
    /// When updating, transition to the null state if needed.
    /// </summary>
    /// <param name="CurrentTurn"></param>
    public void UpdateTurn(Turn CurrentTurn) => this.CurrentTurn = CurrentTurn;
}
#endregion

#region summary Tracked
struct SavedSession
{
    public TrackedObject[] trackedGameObjects;
    public Team_struct Team1_state;
    public Team_struct Team2_state;
    public TurnBasedManager.Turn LastTurn;
    public TurnBasedManager.Turn CurrentTurn;
}

struct TrackedObject
{
    public TrackedObject(GameObject obj)
    {
        Position = obj.transform.position;
        Rotation = obj.transform.rotation;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if(rb != null)
        {
            Velocity = rb.velocity;
            AngularVelocity = rb.angularVelocity;
        }
        else
        {
            Velocity = Vector3.zero;
            AngularVelocity = Vector3.zero;
        }
    }

    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;            //Including this
    public Vector3 AngularVelocity;    //And this because in the rare case something is moving when the app loses focus we can maintain that when it comes back if available
}
#endregion