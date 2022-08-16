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


    //Game related variables
    public Team_struct Team_1 { get; private set; }
    public Team_struct Team_2 { get; private set; }

    public int CurrentRound = 0;

    //Game state functions
    public void ResetGame()
    {
        Team_1 = new Team_struct();
        Team_2 = new Team_struct();
        CurrentRound = 0;
    }
    public void NextRound() => CurrentRound++;


}



public struct Team_struct
{
    public int Score;
    public TeamType Team;
    public string Name() => Team.ToString();
    public object[] ChosenPlayers;
    public List<GameObject> Teams_Active_Bowls;
}

public enum TeamType
{
    //Names here, order is less important
}