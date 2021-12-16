using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WritoDebugToFile : MonoBehaviour
{
    int playSession = 1;
    string fileName = "";

    private void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }
    public void Start()
    {
        fileName = Application.dataPath + "/LogFile.text";
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        TextWriter tw = new StreamWriter(fileName, true);

        tw.WriteLine("[ playsession: " + playSession.ToString() + "]" + ", [" + System.DateTime.Now + "], " + logString);

        tw.Close();
    }

}
