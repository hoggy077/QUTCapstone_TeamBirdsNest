using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public static class CareerRecordManager
{

    public static PlayerCareer playerCareer { get; private set; }// = new PlayerCareer();
    public static void UpdateCareerValues(string name, uint? gamesWon, uint? bowlsRolled, uint? roundsWon, uint? matchesPlayed)
    {
        if (name != null)
            playerCareer.Name = name;

        //before you ask, it gets pissy without the cast // But Why tho???? 
        if (gamesWon != null)
            playerCareer.GamesWon = (uint)gamesWon;

        if (bowlsRolled != null)
            playerCareer.BowlsRolled = (uint)bowlsRolled;

        if (roundsWon != null)
            playerCareer.RoundsWon = (uint)roundsWon;

        if (matchesPlayed != null)
            playerCareer.GamesPlayed = (uint)matchesPlayed;
    }

    public static void UpdateTournamentStatistics(bool tournamentRunning, bool? previousTournamentFinished, uint? matchesRemaining, uint[]? teamScores, uint? playerIndex, bool? midMatch, uint[]? bowlSelection)
    {
        if (tournamentRunning != null)
            playerCareer.TournamentRunning = (bool)tournamentRunning;

        //before you ask, it gets pissy without the cast // But Why tho???? 
        if (previousTournamentFinished != null)
            playerCareer.PreviousTournamentFinished = (bool)previousTournamentFinished;

        if (matchesRemaining != null)
            playerCareer.TournamentMatchesRemaining = (uint)matchesRemaining;

        if (teamScores != null)
            playerCareer.TeamScores = teamScores;

        if (playerIndex != null)
            playerCareer.playerTeamIndex = (uint)playerIndex;

        if (midMatch != null)
            playerCareer.MidMatch = (bool)midMatch;

        if (bowlSelection != null)
            playerCareer.teamSelectedBowls = bowlSelection;
    }


    public static void SaveCareer() => SaveSystem.saveGeneric(playerCareer, "careerInfo.career");

    public static void LoadCareer()
    {
        try
        {
            playerCareer = SaveSystem.loadGeneric<PlayerCareer>(false, "careerInfo.career");
        }
        catch(Exception e)
        {
            playerCareer = new PlayerCareer();
        }
    }



    /// <summary>
    /// Provides an internal common for loading or saving a career file in any definable extension
    /// </summary>
    /// <param name="isSave">Defines the type of operation</param>
    /// <param name="FileName">Excludes path, must include the extension</param>
    /// <returns></returns>
    [Obsolete("Method is obsolete, please refer to SaveSystem.saveGeneric and SaveSystem.loadGeneric")]
    private static PlayerCareer PlayerCareerInteraction(bool isSave, string FileName)
    {
        throw new Exception("Function obsolete and no valid");
        //XmlSerializer serializer = new XmlSerializer(typeof(PlayerCareer));
        //using (MemoryStream stream = new MemoryStream())
        //{
        //    if (isSave)
        //    {
        //        serializer.Serialize(stream, playerCareer);
        //        using FileStream fStream = File.OpenWrite($"{PersistentPath}/{FileName}");
        //        fStream.Write(stream.ToArray());
        //        fStream.Close();
        //        return null;
        //    }
        //    else
        //    {
        //        verifyDirectory();

        //        if (!File.Exists($"{PersistentPath}/{FileName}"))
        //        {
        //            File.Create($"{PersistentPath}/{FileName}");
        //            return new PlayerCareer();
        //        }

        //        using FileStream fStream = File.OpenRead($"{PersistentPath}/{FileName}");
        //        return (PlayerCareer)serializer.Deserialize(fStream);
        //    }
        //}
    }
}

public class PlayerCareer
{
    public string Name = "";

    public uint GamesWon = 0;
    public uint RoundsWon = 0;
    public uint BowlsRolled = 0;
    public uint GamesPlayed = 0;

    public bool TournamentRunning = false;
    public bool PreviousTournamentFinished = false;
    public uint TournamentMatchesRemaining = 0;
    public uint[] TeamScores = new uint[1];
    public uint playerTeamIndex = 0;
    public bool MidMatch = false;
    public uint[] teamSelectedBowls;
}