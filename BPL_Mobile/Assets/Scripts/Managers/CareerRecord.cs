using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class CareerRecordManager
{

#if UNITY_EDITOR
    public static readonly string PersistentPath = $"{Environment.CurrentDirectory}\\gameInfo";
#else
    public static readonly string PersistentPath = $"{Application.persistentDataPath}\\gameInfo";
#endif

    public static PlayerCareer playerCareer { get; private set; } = new PlayerCareer();
    public static void UpdateValues(string name, uint? gamesWon, uint? bowlsRolled, uint? roundsWon)
    {
        if (name != null)
            playerCareer.Name = name;

        //before you ask, it gets pissy without the cast
        if (gamesWon != null)
            playerCareer.GamesWon = (uint)gamesWon;

        if (bowlsRolled != null)
            playerCareer.BowlsRolled = (uint)bowlsRolled;

        if (roundsWon != null)
            playerCareer.RoundsWon = (uint)roundsWon;
    }


    public static void SaveCareer() => HandleInteraction(true);

    public static void LoadCareer() => playerCareer = HandleInteraction(false);


    private static PlayerCareer HandleInteraction(bool isSave)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(PlayerCareer));
        using (MemoryStream stream = new MemoryStream())
        {
            if (isSave)
            {
                serializer.Serialize(stream, playerCareer);
                using FileStream fStream = File.OpenWrite($"{PersistentPath}\\careerInfo.cbf");
                fStream.Write(stream.ToArray());
                fStream.Close();
                return null;
            }
            else
            {
                if(!Directory.Exists(PersistentPath))
                    Directory.CreateDirectory(PersistentPath);

                if (!File.Exists($"{PersistentPath}\\careerInfo.cbf"))
                {
                    File.Create($"{PersistentPath}\\careerInfo.cbf");
                    return new PlayerCareer();
                }

                using FileStream fStream = File.OpenRead($"{PersistentPath}\\careerInfo.cbf");
                return (PlayerCareer)serializer.Deserialize(fStream);
            }
        }
    }
}

public class PlayerCareer
{
    public string Name = "";

    public uint GamesWon = 0;
    public uint RoundsWon = 0;
    public uint BowlsRolled = 0;
}