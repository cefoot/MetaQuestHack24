using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Composer;
using Meta.WitAi.Events;
using Oculus.Voice;
using Oculus.Voice.Composer;
using Oculus.Voice.Toolkit;
using UnityEngine;
using UnityEngine.Events;



public class LanguageManager : MonoBehaviour
{

    public ConversationHandler ConversationHandler;

    public BuildingBlockBridge BuildingBlockBridge;

    public AppComposerExperience AppComposerExperience;


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
        if (composerSessionData.responseData.responseIsFinal)
        {

            // hack: wait for 0.5 second before AI speaks, because the human transcription event is (strangely) fired after the Composer Speak Phrase event
            StartCoroutine(AISpeakRoutine(composerSessionData));
        }
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

    public void HandleComposerExpectsInput(ComposerSessionData composerSessionData)
    {
        if (composerSessionData.contextMap.HasData("composer_wants_state") && composerSessionData.contextMap.GetData<bool>("composer_wants_state"))
        {
            // nothing
        }
        else
        {
            // AppComposerExperience.AppVoiceExperience.Activate();
            BuildingBlockBridge.TriggerInvocation(true);
        }
    }

    public void HandleActionFindThing(ComposerSessionData composerSessionData)
    {
        StartCoroutine(HandleActionFindThingRoutine(composerSessionData));
    }

    IEnumerator HandleActionFindThingRoutine(ComposerSessionData composerSessionData)
    {
        yield return new WaitForSeconds(5f);
        composerSessionData.contextMap.SetData("location_found", true);
        composerSessionData.composer.SendContextMapEvent();
        // composerSessionData.contextMap.SetData("composer_wants_state", false);

        // set location navigation target
    }

    public void HandleActionNavigateToThing(ComposerSessionData composerSessionData)
    {
        StartCoroutine(HandleActionNavigateToThingRoutine(composerSessionData));
    }

    IEnumerator HandleActionNavigateToThingRoutine(ComposerSessionData composerSessionData)
    {
        yield return new WaitForSeconds(5f);
        composerSessionData.contextMap.SetData("location_reached", true);
        composerSessionData.composer.SendContextMapEvent();

    }
}
