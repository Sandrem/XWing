﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using SquadBuilderNS;

public partial class Console : MonoBehaviour {

    public class LogEntry
    {
        public string Text;
        public float CalculatedPrefferedHeight;

        public LogEntry(string text)
        {
            Text = text;
        }
    }

    private static List<LogEntry> logs;
    public static List<LogEntry> Logs
    {
        get { return logs; }
        private set { logs = value; }
    }

    private static Dictionary<string, GenericCommand> availableCommands;
    public static Dictionary<string, GenericCommand> AvailableCommands
    {
        get { return availableCommands; }
        private set { availableCommands = value; }
    }


    private void Start()
    {
        Application.logMessageReceived += ProcessUnityLog;

        InitializeCommands();
    }

    private void InitializeCommands()
    {
        AvailableCommands = new Dictionary<string, GenericCommand>();

        List<Type> typelist = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => String.Equals(t.Namespace, "CommandsList", StringComparison.Ordinal))
            .ToList();

        foreach (var type in typelist)
        {
            if (type.MemberType == MemberTypes.NestedType) continue;
            System.Activator.CreateInstance(type);
        }

        AvailableCommands = AvailableCommands.OrderBy(n => n.Key).ToDictionary(n => n.Key, n => n.Value);
    }

    private static void InitializeLogs()
    {
        Logs = new List<LogEntry>();
    }

    public static void Write(string text, bool isBold = false, string color = "")
    {
        if (Logs == null) InitializeLogs();

        string logString = text;
        if (isBold) logString = "<b>" + logString + "</b>";
        if (color != "") logString = "<color="+ color + ">" + logString + "</color>";

        LogEntry logEntry = new LogEntry(logString + "\n");
        Logs.Add(logEntry);

        ShowLogEntry(logEntry);
    }

    private void ProcessUnityLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            if (IsHiddenError(logString)) return;

            if (!DebugManager.ErrorIsAlreadyReported)
            {
                if (DebugManager.ReleaseVersion && Global.CurrentVersionInt == Global.LatestVersionInt)
                {
                    SendReport(stackTrace);
                }
            }

            if (ErrorReporter.Instance != null)
            {
                ErrorReporter.ShowError(logString + "\n\n" + stackTrace);
            }
            else
            {
                IsActive = true;
            }

            Write("\n" + logString + "\n\n" + stackTrace, true, "red");
        }
    }

    private void SendReport(string stackTrace)
    {
        DebugManager.ErrorIsAlreadyReported = true;

        AnalyticsEvent.LevelFail(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            new Dictionary<string, object>()
            {
                { "Version", Global.CurrentVersion },
                { "Pilot", (Selection.ThisShip != null) ? Selection.ThisShip.PilotInfo.PilotName : "None" },
                { "Trigger", (Triggers.CurrentTrigger != null) ? Triggers.CurrentTrigger.Name : "None" },
                { "Subphase", (Phases.CurrentSubPhase != null) ? Phases.CurrentSubPhase.GetType().ToString() : "None" }
            }
        );

        StartCoroutine(UploadCustomReport(stackTrace));
    }

    IEnumerator UploadCustomReport(string stackTrace)
    {
        JSONObject jsonData = new JSONObject();
        jsonData.AddField("rowKey", Guid.NewGuid().ToString());
        jsonData.AddField("partitionKey", "CrashReport");
        jsonData.AddField("playerName", Options.NickName);
        jsonData.AddField("description", "No description");
        jsonData.AddField("p1pilot", (Selection.ThisShip != null) ? Selection.ThisShip.PilotInfo.PilotName : "None");
        jsonData.AddField("p2pilot", (Selection.AnotherShip != null) ? Selection.AnotherShip.PilotInfo.PilotName : "None");
        jsonData.AddField("stackTrace", stackTrace.Replace("\n", "NEWLINE"));
        jsonData.AddField("trigger", (Triggers.CurrentTrigger != null) ? Triggers.CurrentTrigger.Name : "None");
        jsonData.AddField("subphase", (Phases.CurrentSubPhase != null) ? Phases.CurrentSubPhase.GetType().ToString() : "None");
        jsonData.AddField("scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        jsonData.AddField("version", Global.CurrentVersion);

        try
        {
            jsonData.AddField("p1squad", SquadBuilder.SquadLists[0].SavedConfiguration.ToString().Replace("\"", "\\\""));
            jsonData.AddField("p2squad", SquadBuilder.SquadLists[1].SavedConfiguration.ToString().Replace("\"", "\\\""));
        }
        catch (Exception)
        {
            jsonData.AddField("p1squad", "None");
            jsonData.AddField("p2squad", "None");
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Battle")
        {
            jsonData.AddField("replay", ReplaysManager.GetReplayContent().Replace("\"", "\\\""));
        }
        else
        {
            jsonData.AddField("replay", "None");
        }        

        var request = new UnityWebRequest("https://flycasualdataserver.azurewebsites.net/api/crashreports/create", "POST");
        Debug.Log(jsonData.ToString());
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData.ToString());
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);
    }

    private bool IsHiddenError(string text)
    {
        if ((text == "ClientDisconnected due to error: Timeout") ||
            (text == "ServerDisconnected due to error: Timeout") ||
            text.StartsWith("Screen position out of view frustum")) return true;

        if (text == "SerializedObject target has been destroyed.") return true;

        if (text == "Material doesn't have a color property '_Color'") return true;

        return false;
    }

    public static void ProcessCommand(string inputText)
    {
        if (string.IsNullOrEmpty(inputText)) return;

        List<string> blocks = inputText.Split(' ').ToList();
        string keyword = blocks.FirstOrDefault();
        blocks.RemoveAt(0);

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        foreach (var item in blocks)
        {
            string[] paramValue = item.Split(':');
            if (paramValue.Length == 2) parameters.Add(paramValue[0], paramValue[1]);
            else if (paramValue.Length == 1) parameters.Add(paramValue[0], null);
        }

        if (AvailableCommands.ContainsKey(keyword))
        {
            AvailableCommands[keyword].Execute(parameters);
        }
        else
        {
            Console.Write("Unknown command, enter \"help\" to see list of commands", color: "red");
        }
    }

    public static void AddAvailableCommand(GenericCommand command)
    {
        AvailableCommands.Add(command.Keyword, command);
    }

}
