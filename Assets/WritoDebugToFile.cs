// Author: [full name here]
using System.IO;
using UnityEngine;

public class WritoDebugToFile : MonoBehaviour
{
    private int playSession = 1;
    private string fileName = string.Empty;

    public void Log(string logString, string stackTrace, LogType type)
    {
        TextWriter tw = new StreamWriter(fileName, true);
        tw.WriteLine("[ playsession: " + playSession.ToString() + "]" + ", [" + System.DateTime.Now + "], " + logString);
        tw.Close();
    }

    private void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }
    
    private void Start()
    {
        fileName = Application.dataPath + "/LogFile.text";
    }
}
