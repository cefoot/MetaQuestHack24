using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectDescription : MonoBehaviour
{
    public TextMeshProUGUI LabelTMP;
    public LanguageManager LanguageManager;

    const string EMPTY_LOCATION = "[EMPTY LOCATION]";

    // Start is called before the first frame update
    void Start()
    {
        LanguageManager = FindObjectOfType<LanguageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        string desc = GetComponent<Anchoring>().Description;
        LabelTMP.text = desc.Length > 0 ? desc : EMPTY_LOCATION;
    }

    public void HandleModifyLocation()
    {
        LanguageManager.HandleUpdateLocationContents(GetComponent<Anchoring>());
    }
}
