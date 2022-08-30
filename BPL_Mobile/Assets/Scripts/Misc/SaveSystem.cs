using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using UnityEngine;

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
    private static void verifyDirectory()
    {
        if (!Directory.Exists(PersistentPath))
            Directory.CreateDirectory(PersistentPath);
    }
}
