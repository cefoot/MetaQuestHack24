using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationHandler : MonoBehaviour
{

    public enum ConversationType
    {
        Human,
        AI,
    }

    [Serializable]
    public class ConversationData
    {
        public ConversationType Type;
        [Multiline]
        public string message;

        public override bool Equals(object obj)
        {
            // Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (ConversationData)obj;
            // Use string.Equals to compare message strings to handle case sensitivity or cultural differences
            return Type == other.Type && string.Equals(message, other.message, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            // Use hash code of Type enum and message string. Utilize HashCode.Combine for best practice in combining hash codes.
            return HashCode.Combine(Type, message);
        }
    }

    [Serializable]
    public class ConversationSetup
    {
        public ConversationType Type;
        public GameObject Prefab;
    }

    public Transform TextParent;

    public ConversationData[] Conversation;

    private ConversationData[] _curConversation;


    public ConversationSetup[] SetupData;

    public float SpawnDelay = 0.3f;
    private Dictionary<ConversationType, ConversationSetup> _setupDic;
    private ScrollRect _scrollRect;

    public void OnEnable()
    {
        _setupDic = SetupData.ToDictionary(dat => dat.Type);
        _scrollRect = GetComponentInChildren<ScrollRect>();
        ConversationChanged();
    }

    public void ConversationChanged()
    {
        StartCoroutine(HandleConversationChange());
    }

    public IEnumerator HandleConversationChange()
    {
        for (int i = TextParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(TextParent.GetChild(i).gameObject);
        }
        yield return null;
        _curConversation = Conversation;
        foreach (var conversation in _curConversation)
        {
            yield return StartAddNewMsg(conversation);
            yield return new WaitForSecondsRealtime(SpawnDelay * 3);
        }
    }

    public void AddNewMessage(ConversationData conversation)
    {
        StartCoroutine(StartAddNewMsg(conversation));
    }

    private IEnumerator StartAddNewMsg(ConversationData conversation)
    {
        var msg = Instantiate(_setupDic[conversation.Type].Prefab, TextParent);
        TextMeshProUGUI textMeshProUGUI = msg.GetComponentInChildren<TextMeshProUGUI>();
        textMeshProUGUI.text = "";
        yield return new WaitForSecondsRealtime(SpawnDelay);
        _scrollRect.verticalNormalizedPosition = 0f;
        foreach (var chr in conversation.message)
        {
            textMeshProUGUI.text += chr;
            _scrollRect.verticalNormalizedPosition = 0f;//scroll to bottom
            yield return new WaitForSecondsRealtime(SpawnDelay);
        }
    }
}
