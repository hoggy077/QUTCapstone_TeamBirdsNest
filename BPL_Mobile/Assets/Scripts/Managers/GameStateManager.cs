using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    //Gameobject Singleton
    public static GameStateManager Instance { 
        get {
            if(Instance_ == null)
            {
                GameObject gsm = new GameObject("GameStateManager");
                Instance_ = gsm.AddComponent<GameStateManager>();
                GameObject.DontDestroyOnLoad(gsm);
            }
            Exist = true;
            return Instance_;
        }}
    private static GameStateManager Instance_ = null;
    static bool Exist = false;
    public bool isMultiplayerMode = false;

    //This just tells unity to RunOn when the game loads, accessing Instance which will create one if not available
    [RuntimeInitializeOnLoadMethod]
    static void RunOn() => _ = Instance;
    public static bool Exists() => Exist;
    public int loadedEndNumber = 0;
    public bool isPlayerTurnLoaded;
    public GamemodeInfo gamemode;


    #region Game related variables

    #region Teams
    public Team_struct Team_1 { get; set; }
    public Team_struct Team_2 { get; set; }


    public void UpdateTeam(uint TeamNumber, TeamScriptable team, BowlsScriptable[] bowls)
    {
        if (TeamNumber > 2)
            throw new Exception($"Team number {TeamNumber} is invalid. Please enter 1 or 2");

        if (TeamNumber == 1)
            Team_1 = new Team_struct() { BaseTeam = team , teamBowls = bowls};
        else
            Team_2 = new Team_struct() { BaseTeam = team , teamBowls = bowls};
    }
    #endregion

    #region Turn
    private TurnBasedManager TurnManager = new TurnBasedManager();

    public TurnBasedManager.Turn whosTurn() => TurnManager.CurrentTurn;

    private TurnBasedManager.Turn LastValidTurn = TurnBasedManager.Turn.Team1;
    public void changeTurn(TurnBasedManager.Turn v)
    {
        if (v != TurnBasedManager.Turn.Null)
            LastValidTurn = v;

        TurnManager.UpdateTurn(v);
    }

    [Obsolete]
    public void nextTurn()
    {
        TurnManager.UpdateTurn(TurnManager.CurrentTurn == TurnBasedManager.Turn.Team1 ? TurnBasedManager.Turn.Team2 : TurnBasedManager.Turn.Team1);
    }
    #endregion

    #endregion


    #region Event Delegates for posterity

    //@Riley
    //1. scoreboardUI if you make a "void exampleFunc(Team_struct team)" function
    //2. in void Start() add
    //   GameStateManager.Instance.TeamUpdated += exampleFunc
    //3. and your good
    public delegate void TeamUpdated_(Team_struct ATeam);//insert A team theme song here
    public event TeamUpdated_ TeamUpdated;

    //@Riley
    public void UpdateTeamScores(uint TeamNumber, uint? shots = null, uint? sets = null, uint? ends = null, bool? powerplay = null, uint[]? teammateShots = null)
    {
        switch (TeamNumber)
        {
            case (1):
                if (shots != null)
                    Team_1.Shots = (uint)shots;

                if (sets != null)
                    Team_1.Sets = (uint)sets;

                if (ends != null)
                    Team_1.Ends = (uint)ends;

                if (powerplay != null)
                    Team_1.HasPowerPlay = (bool)powerplay;

                if (teammateShots != null)
                    Team_1.teammateShotsLeft = teammateShots;

                //TeamUpdated.Invoke(Team_1);
                break;

            case (2):
                if (shots != null)
                    Team_2.Shots = (uint)shots;

                if (sets != null)
                    Team_2.Sets = (uint)sets;

                if (ends != null)
                    Team_2.Ends = (uint)ends;

                if (powerplay != null)
                    Team_2.HasPowerPlay = (bool)powerplay;

                if (teammateShots != null)
                    Team_2.teammateShotsLeft = teammateShots;

                //TeamUpdated.Invoke(Team_2);
                break;
            default:
                return;
        }
    }



    #endregion


}

#region Team and turn manager
[Serializable]
public class Team_struct
{
    public uint Shots;
    public uint Sets;
    public uint Ends;
    public TeamScriptable BaseTeam;
    public string Name() => BaseTeam.TeamName;
    public bool HasPowerPlay;
    public bool UsingPowerplay;
    public BowlsScriptable[] teamBowls;
    public uint[] teammateShotsLeft;
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

