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
    //Team access
    public Team_struct Team_1 { get; private set; }
    public Team_struct Team_2 { get; private set; }



    //Round count
    public int CurrentRound { get; private set; } = 0;



    //Turn shit
    private TurnBasedManager TurnManager = new TurnBasedManager();

    public TurnBasedManager.Turn whosTurn() => TurnManager.CurrentTurn;
    public void nextTurn()
    {
        TurnManager.UpdateTurn(TurnManager.CurrentTurn == TurnBasedManager.Turn.Team1 ? TurnBasedManager.Turn.Team2 : TurnBasedManager.Turn.Team1);
        TurnChanged.Invoke(TurnManager.CurrentTurn);
    }





    #endregion

    #region Event Delegates for posterity
    public delegate void turnChanged_(TurnBasedManager.Turn turn);
    public event turnChanged_ TurnChanged;

    //--this one will need to be hooked up specifically
    public delegate void powerplay_(Team_struct team);
    public event powerplay_ PowerPlayEvent;


    public delegate void roundUpdate_();
    public event roundUpdate_ RoundUpdated;
    #endregion

    #region Game state functions
    public void ResetGame()
    {
        Team_1 = new Team_struct();
        Team_2 = new Team_struct();
        CurrentRound = 0;
    }

    public void NextRound()
    {
        CurrentRound++;
        RoundUpdated.Invoke();
    }

    //Most important, this handles testing around the jack for score updating
    //This shouldn't be called every update unnecessarily, but probs instead after each turn
    public void UpdateField()
    {

    }
    #endregion


    #region Session Saving

    private void OnApplicationPause(bool paused)
    {
        //-- subject to change, reports of this not working 100% since 2019. Consider OnAppliationFocus instead
    }

    #endregion
}

#region Team and turn manager
public struct Team_struct
{
    public int Score;
    public TeamType Team;
    public string Name() => Team.ToString();
    public object[] ChosenPlayers;
    public bool HasPowerPlay;
    public List<GameObject> Teams_Active_Bowls;
}

public enum TeamType
{
    //Names here, order is less important
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
