using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Made with the help of this tutorial: https://www.youtube.com/watch?v=Pi4SHO0IEQY
public class DebugDisplay : MonoBehaviour
{
    Dictionary<string, string> debugTexts = new Dictionary<string, string>();
    public TextMeshProUGUI display;
    void Start()
    {
        gameObject.SetActive(LevelManager.isDebug);
    }
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }
    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }
    void HandleLog(string logString, string stackTrace, LogType type){
        if (type != LogType.Log){
            return;
        }
        string[] splitString = logString.Split(new char[] { ':' }, 2);
        string debugKey = splitString[0];
        string debugValue = splitString.Length > 1 ? splitString[1] : "";
        if (debugTexts.ContainsKey(debugKey)){
            debugTexts[debugKey] = debugValue;
        } else {
            debugTexts.Add(debugKey, debugValue);
        }
        string displayText = "";
        foreach (KeyValuePair<string, string> log in debugTexts)
        {
            if(log.Value == ""){
                displayText += log.Key + "\n";
            }else{
                displayText += log.Key + ": " + log.Value + "\n";
            }
        }
        display.text = displayText;
    }
}
