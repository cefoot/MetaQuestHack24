using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Composer;
using Meta.WitAi.Composer.Data;
using UnityEngine;

public class ComposerEventLogger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    string FormatResponseData(ComposerResponseData responseData)
    {
        // use multiline string interpolation
        return $@"Response:
Is Final: {responseData.responseIsFinal}
Phrase: {responseData.responsePhrase}
TTS: {responseData.responseTts}
TTS Settings: {responseData.responseTtsSettings}
    ";
    }

    string FormatErrorResponseData(ComposerResponseData responseData)
    {
        // use multiline string interpolation
        return $@"Response:
        Error: {responseData.error}";

    }

    public void HandleComposerResponse(ComposerSessionData sessionData)
    {
        Debug.Log("Composer Response: " + FormatResponseData(sessionData.responseData));
    }

    public void HandleComposerError(ComposerSessionData sessionData)
    {
        Debug.LogError("Composer Error: " + FormatErrorResponseData(sessionData.responseData));
    }

    public void HandleComposerSpeakPhrase(ComposerSessionData sessionData)
    {
        Debug.Log("Composer Speak Phrase: " + FormatResponseData(sessionData.responseData));
    }
}
