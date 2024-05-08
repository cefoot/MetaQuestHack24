using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogFilter : MonoBehaviour
{
    private TextMeshProUGUI _textBox;



    // Start is called before the first frame update
    void Start()
    {
        _textBox = GetComponent<TMPro.TextMeshProUGUI>();
        Application.logMessageReceived += OnLogMessage;
    }

    public void Clear()
    {
        _textBox.text = "";
    }

    private void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        _textBox.text += condition + Environment.NewLine;

    }

}
