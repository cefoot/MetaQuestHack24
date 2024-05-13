using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Composer;
using Meta.WitAi.Events;
using Oculus.Voice.Toolkit;
using UnityEngine;
using UnityEngine.Events;



public class LanguageManager : MonoBehaviour
{

    public ConversationHandler ConversationHandler;

    public BuildingBlockBridge BuildingBlockBridge;

    // Start is called before the first frame update
    void Start()
    {
        if (BuildingBlockBridge != null)
        {
            // hack: fix the uninitialised BuildingBlockBridge action
            if (BuildingBlockBridge.voiceUIEvent == null)
            {
                BuildingBlockBridge.voiceUIEvent = new UnityAction<VoiceState, VoiceDataBase>(EmptyHandler); // Initialize with an empty handler
            }

            BuildingBlockBridge.voiceUIEvent += delegate { };

            // make asset-based BBB listen to voice service
            BuildingBlockBridge.ListenFromVoiceService();
        }
    }


    static void EmptyHandler(VoiceState state, VoiceDataBase data)
    {
        // Do nothing
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleFullTranscription(string transcription)
    {
        ConversationHandler?.AddNewMessage(
            new ConversationHandler.ConversationData
            {
                Type = ConversationHandler.ConversationType.Human,
                message = transcription
            }
        );
    }
    public void HandleComposerSpeakPhrase(ComposerSessionData composerSessionData)
    {
        // hack: wait for 0.5 second before AI speaks, because the human transcription event is (strangely) fired after the Composer Speak Phrase event
        StartCoroutine(AISpeakRoutine(composerSessionData));
    }

    IEnumerator AISpeakRoutine(ComposerSessionData composerSessionData)
    {
        yield return new WaitForSeconds(0.5f);
        ConversationHandler?.AddNewMessage(
            new ConversationHandler.ConversationData
            {
                Type = ConversationHandler.ConversationType.AI,
                message = composerSessionData.responseData.responsePhrase
            });
    }
}
