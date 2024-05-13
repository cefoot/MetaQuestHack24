using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.WitAi.Composer;
using Meta.WitAi.Events;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using Oculus.Voice.Composer;
using Oculus.Voice.Toolkit;
using UnityEngine.Search;
using UnityEngine;
using UnityEngine.Events;



public class LanguageManager : MonoBehaviour
{

    public ConversationHandler ConversationHandler;

    public BuildingBlockBridge BuildingBlockBridge;

    public AppComposerExperience AppComposerExperience;
    public TTSSpeaker TTSSpeaker;

    public LineRenderer GuidanceLine;
    public Transform GuidanceReference;

    private Dictionary<string, Anchoring> _locationsMap = new Dictionary<string, Anchoring>();
    private GameObject _navigationTarget = null;


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
        ConversationHandler?.gameObject.SetActive(true);
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

    private void AddAILine(string message)
    {
        ConversationHandler?.AddNewMessage(
            new ConversationHandler.ConversationData
            {
                Type = ConversationHandler.ConversationType.AI,
                message = message
            });
    }

    IEnumerator AISpeakRoutine(ComposerSessionData composerSessionData)
    {
        yield return new WaitForSeconds(0.5f);
        AddAILine(composerSessionData.responseData.responsePhrase);
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
        string thingDescription = composerSessionData.contextMap.GetData<string>("thing_to_find[0].value");
        string locationUuid = FindUuidByDescription(thingDescription);
        if (locationUuid.Length == 0)
        {
            // thing not found
            composerSessionData.contextMap.SetData("location_found", false);
            composerSessionData.contextMap.SetData("location_uuid", locationUuid);
        }
        else
        {
            composerSessionData.contextMap.SetData("location_found", true);
            composerSessionData.contextMap.SetData("location_uuid", locationUuid);

        }

        composerSessionData.composer.SendContextMapEvent();

    }

    public static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }


    private class FuzzySearchResult
    {
        public Anchoring obj;
        public long score;
    }

    private string FindUuidByDescription(string searchString)
    {
        var allAnchorings = FindObjectsByType<Anchoring>(FindObjectsSortMode.None);

        // search for the best match using FuzzySearch.FuzzyMatch
        var results = new List<FuzzySearchResult>();
        var firstItm = allAnchorings
            .Select(itm => new
            {
                Txt = itm.Description,
                UUID = itm.GetAnchorUuid(),
                Distance = LevenshteinDistance(searchString, itm.Description)
            })
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        if (firstItm == null)
        {
            return string.Empty;
        }
        else
        {
            return firstItm.UUID;
        }
    }

    public void HandleActionNavigateToThing(ComposerSessionData composerSessionData)
    {
        StartCoroutine(HandleActionNavigateToThingRoutine(composerSessionData));
    }

    IEnumerator HandleActionNavigateToThingRoutine(ComposerSessionData composerSessionData)
    {
        string locationUuid = composerSessionData.contextMap.GetData<string>("location_uuid");
        // ConversationHandler.AddNewMessage(new ConversationHandler.ConversationData { message = "UUID:" + locationUuid, Type = ConversationHandler.ConversationType.AI });

        var obj = FindObjectsByType<Anchoring>(FindObjectsSortMode.None).Where(a => a.GetAnchorUuid().Equals(locationUuid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        if (!obj)
        {

            ConversationHandler.AddNewMessage(new ConversationHandler.ConversationData { message = "nothing found.....", Type = ConversationHandler.ConversationType.AI });
            composerSessionData.contextMap.SetData("location_reached", false);
        }
        else
        {
            GuidanceLine.enabled = true;
            while (Vector3.Distance(GuidanceReference.position, obj.transform.position) > 1)
            {
                GuidanceLine.SetPositions(new[]
                {
                    GuidanceReference.position,
                    obj.transform.position
                });
                yield return new WaitForSecondsRealtime(0.5f);
            }
            GuidanceLine.enabled = false;
            composerSessionData.contextMap.SetData("location_reached", true);
        }
        composerSessionData.composer.SendContextMapEvent();

    }

    static readonly string DESCRIPTION_UPDATE_QUESTION = "What would you like to store in this location?";
    public void HandleUpdateLocationContents(Anchoring anchoredObject)
    {

        string locationUuid = anchoredObject.GetAnchorUuid();
        _locationsMap.Add(locationUuid, anchoredObject);

        // start a new composer session, set context and activate listening
        AppComposerExperience.StartSession();
        AppComposerExperience.CurrentContextMap.ClearAllNonReservedData();
        AppComposerExperience.CurrentContextMap.SetData("modifying_object_description", true);
        AppComposerExperience.CurrentContextMap.SetData("location_uuid", locationUuid);
        AppComposerExperience.SendContextMapEvent();
    }

    public void HandleActionSaveNewDescription(ComposerSessionData composerSessionData)
    {

        string locationUuid = composerSessionData.contextMap.GetData<string>("location_uuid");
        string newDescription = composerSessionData.contextMap.GetData<string>("new_thing_description[0].value");
        Debug.Log("HandleActionSaveNewDescription: locationUuid=" + locationUuid + ", newDescription=" + newDescription);
        _locationsMap[locationUuid].Description = newDescription;
        _locationsMap.Remove(locationUuid);
    }


}
