using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using UnityEngine;
using static UnityEngine.JsonUtility;
using System.Text;

public static class SaveSystem
{
#if UNITY_EDITOR
    public static readonly string PersistentPath = $"{Environment.CurrentDirectory}\\gameInfo";
#else
    public static readonly string PersistentPath = $"{Application.persistentDataPath}\\gameInfo";
#endif

    public static void saveGeneric<T>(T target, string FileName)
    {
        verifyDirectory();

        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using FileStream stream = File.OpenWrite($"{PersistentPath}\\{FileName}");
        serializer.Serialize(stream, target);
        stream.Close();

    }

    public static T loadGeneric<T>(bool deletePostRead, string FileName)
    {
        verifyDirectory();

        if (!File.Exists($"{PersistentPath}\\{FileName}"))
            throw new Exception("File not found");

        XmlSerializer serializer = new XmlSerializer(typeof(T));
        object Result = null;

        using FileStream stream = File.OpenRead($"{PersistentPath}\\{FileName}");
        Result = serializer.Deserialize(stream);
        stream.Close();

        if (deletePostRead)
            performDelete(FileName);

        return (T)Result;
    }




    public static bool verifyFile<T>(string FileName)
    {
        if(!File.Exists($"{PersistentPath}\\{FileName}"))
            return false;

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            object Result = null;

            using FileStream stream = File.OpenRead($"{PersistentPath}\\{FileName}");
            Result = serializer.Deserialize(stream);
            stream.Close();

            T casted = (T)Result;
            return true;
        }
        catch (Exception err)
        {
            return false;
        }
        return false;
    }

    public static void performDelete(string FileName) => File.Delete($"{PersistentPath}\\{FileName}");
    public static void verifyDirectory()
    {
        if (!Directory.Exists(PersistentPath))
            Directory.CreateDirectory(PersistentPath);
    }
}


public static class SaveSystemJson
{
#if UNITY_EDITOR
    public static readonly string PersistentPath = $"{Environment.CurrentDirectory}\\gameInfo";
#else
    public static readonly string PersistentPath = $"{Application.persistentDataPath}\\gameInfo";
#endif

    public static void SaveGenericJson<T>(T item, string FileName)
    {
        SaveSystem.verifyDirectory();

        File.WriteAllBytes($"{PersistentPath}\\{FileName}",Encoding.UTF8.GetBytes(ToJson(item)));
    }

    public static void LoadGenericJson<T>(ref T item, bool deletePostRead, string FileName)
    {
        SaveSystem.verifyDirectory();

        if (!File.Exists($"{PersistentPath}\\{FileName}"))
            throw new Exception("File not found");

        byte[] outgoing = File.ReadAllBytes($"{PersistentPath}\\{FileName}");
        object e = new object();
        FromJsonOverwrite(Encoding.UTF8.GetString(outgoing), item);

        if (deletePostRead)
            SaveSystem.performDelete(FileName);

    }

    public static bool VerifyFile<T>(string FileName)
    {
        if (!File.Exists($"{PersistentPath}\\{FileName}"))
            return false;

        try
        {
            byte[] outgoing = File.ReadAllBytes($"{PersistentPath}\\{FileName}");
            object raw = new object();
            FromJsonOverwrite(Encoding.UTF8.GetString(outgoing), raw);

            return true;
        }
        catch (Exception err)
        {
            return false;
        }
        return false;
    }
}