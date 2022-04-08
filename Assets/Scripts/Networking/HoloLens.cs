using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RiptideNetworking;
using UnityEditor;

public class Log
{
    public string Text { get; set; }
    public LogType Type { get; set; }

    public static List<Log> logs = new List<Log>();

    public Log(string _text, LogType _logType)
    {
        Text = _text;
        Type = _logType;
    }

    public static void AddLog(Log log)
    {
        logs.Add(log);
    }

    public enum LogType
    {
        Normal = 0,
        Warning = 1,
        Error = 2
    }
}

public class HoloLens
{
    public ushort Id { get; set; }
    public DateTime ConnectionTime { get; private set; }

    public static Dictionary<ushort, HoloLens> HoloLenses = new Dictionary<ushort, HoloLens>();

    public static string editorWindowLog = "";

    public HoloLens(ushort _id)
    {
        Id = _id;
        HoloLenses[Id] = this;
        ConnectionTime = DateTime.Now;
    }


    /// <summary>
    /// A new hololens connected to the network
    /// </summary>
    /// <param name="fromHololensId"></param>
    /// <param name="message"></param>
    [MessageHandler((ushort)ClientToServerId.hololensConnected)]
    private static void HoloLensConnected(ushort fromHoloLensId, Message message)
    {
        Log.AddLog(new Log("HoloLens [" + fromHoloLensId + "] connected", Log.LogType.Normal));

        HoloLens holoLens = new HoloLens(fromHoloLensId);


        /// TEST CODE
        /// use MessageSendMode.unrealiable for data that is send multiple times per seconds -> e.g. optitrack data
        Message testMessage = Message.Create(MessageSendMode.reliable, ServerToClientId.testMessage);
        testMessage.AddString("test string");
        testMessage.AddInt(2);
        // the receiving end must retrieve the data in the same order it was send -> so first the string then the int
        SendAllHoloLenses(testMessage);
    }

    /// <summary>
    /// A log was issued on one of the connected hololens's
    /// </summary>
    /// <param name="fromHololens"></param>
    /// <param name="message"></param>
    [MessageHandler((ushort)ClientToServerId.log)]
    private static void HololensLog(ushort fromHoloLensId, Message message)
    {
        Log.LogType logType = (Log.LogType)message.GetInt();
        string messageText = message.GetString();

        Log.AddLog(new Log("[" + fromHoloLensId + "]: " + messageText, logType));
    }

    /// <summary>
    /// Sends a Riptide message to all connected hololenses
    /// </summary>
    /// <param name="message"></param>
    public static void SendAllHoloLenses(Message message)
    {
        foreach (KeyValuePair<ushort, HoloLens> holoLens in HoloLenses)
        {
            NetworkManager.Singleton.Server.Send(message, holoLens.Key);
        }
    }
}
