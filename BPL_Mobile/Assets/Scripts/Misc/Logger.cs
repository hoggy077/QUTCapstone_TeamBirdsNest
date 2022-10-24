using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Logger
{
#if UNITY_EDITOR
    public static readonly string PersistentPath = $"{Environment.CurrentDirectory}\\Logs";
#else
    public static readonly string PersistentPath = $"{Application.persistentDataPath}";
#endif



    public static Logger Instance { get { if (Instance_ == null) { Instance_ = new Logger(); } return Instance_; } }
    static Logger Instance_ = null;


    public static bool RunOnLaunch = false;


    [RuntimeInitializeOnLoadMethod]
    static void OnRun() => _ = Instance;

    public Logger()
    {
        if (!RunOnLaunch)
            return;

        LogFileName = $"{DateTime.Now.ToString("ddmmyy")}.txt";

        Application.logMessageReceived += Log;
        Application.quitting += () => LogFileWriter.Close();
    }


    static string LogFileName = "";

    static StreamWriter LogFileWriter { get { if (LogFileWriter_ == null) { LogFileWriter_ = new StreamWriter($"{PersistentPath}\\{LogFileName}", append: true); } return LogFileWriter_; } }
    static StreamWriter LogFileWriter_ = null;

    public static void Log(string Log, string trace, LogType type)
    {
        if (!RunOnLaunch)
            return;
        LogFileWriter.WriteLine($"[{type.ToString()} - {DateTime.Now.ToString("HH:mm:ss")}] {Log}");
    }
}
