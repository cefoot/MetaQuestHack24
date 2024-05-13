using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Anchoring : MonoBehaviour
{

    public UnityEvent<OVRSpatialAnchor, OVRSpatialAnchor.OperationResult> OnAnchorCreateCompleted { get => _onAnchorCreateCompleted; set => _onAnchorCreateCompleted = value; }
    public UnityEvent OnAnchorLoadCompleted { get => _onAnchorLoadCompleted; set => _onAnchorLoadCompleted = value; }
    public UnityEvent<OVRSpatialAnchor.OperationResult> OnAnchorsEraseAllCompleted { get => _onAnchorsEraseAllCompleted; set => _onAnchorsEraseAllCompleted = value; }
    public UnityEvent<OVRSpatialAnchor, OVRSpatialAnchor.OperationResult> OnAnchorEraseCompleted { get => _onAnchorEraseCompleted; set => _onAnchorEraseCompleted = value; }

    private OVRSpatialAnchor _anchor;
    public string GetAnchorUuid()
    {
        return _anchor.Uuid.ToString();
    }

    // natural language description of the anchored object
    private string _description = string.Empty;

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            SaveDescription();
        }
    }

    protected virtual OVRSpatialAnchor.EraseOptions EraseOptions => new() { Storage = OVRSpace.StorageLocation.Local };
    private static HashSet<OVRSpatialAnchor> _globalAnchorList = new HashSet<OVRSpatialAnchor>();
    private const string PLAYER_PREF_ANCHOR = "StorageAnchors";

    [Header("# Status")]
    public OVRSpatialAnchor.OperationResult Result;

    [Header("# Events")]
    [SerializeField] private UnityEvent<OVRSpatialAnchor, OVRSpatialAnchor.OperationResult> _onAnchorCreateCompleted;
    [SerializeField] private UnityEvent _onAnchorLoadCompleted;
    [SerializeField] private UnityEvent<OVRSpatialAnchor.OperationResult> _onAnchorsEraseAllCompleted;
    [SerializeField] private UnityEvent<OVRSpatialAnchor, OVRSpatialAnchor.OperationResult> _onAnchorEraseCompleted;

    void OnDestroy()
    {
        if (_anchor != null)
        {
            _globalAnchorList.Remove(_anchor);
        }
    }


    #region Descriptions

    private void SaveDescription()
    {
        if (_anchor != null)
        {
            PlayerPrefs.SetString($"Desc_{_anchor.Uuid}", _description);
            PlayerPrefs.Save();
            Debug.Log("Internal description: " + _description + "; Description saved: " + PlayerPrefs.GetString($"Desc_{_anchor.Uuid}", string.Empty));
        }
    }

    private void LoadDescription()
    {
        if (_anchor != null)
        {
            _description = PlayerPrefs.GetString($"Desc_{_anchor.Uuid}", string.Empty);
        }
    }

    private void EraseDescription()
    {
        if (_anchor != null)
        {
            PlayerPrefs.DeleteKey($"Desc_{_anchor.Uuid}");
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region Loading

    public static IEnumerator LoadAnchorsFromPrefs(Transform parent, Anchoring prefab)
    {
        var prefs = PlayerPrefs.GetString(PLAYER_PREF_ANCHOR);
        if (string.IsNullOrEmpty(prefs)) yield break;
        Debug.Log($"[{nameof(Anchoring)}] Loaded Anchor List:{prefs}");
        var uuids = prefs.Split(';');
        if (uuids == null)
        {
            throw new ArgumentNullException();
        }

        if (uuids.Length == 0)
        {
            Debug.Log($"[{nameof(Anchoring)}] Uuid list is empty.");
            yield break;
        }

        var options = new OVRSpatialAnchor.LoadOptions
        {
            Timeout = 0,
            StorageLocation = OVRSpace.StorageLocation.Local,
            Uuids = uuids.Select(str => new Guid(str)).ToList()
        };

        yield return LoadAnchorsRoutine(prefab, parent, options);
    }

    protected static IEnumerator LoadAnchorsRoutine(Anchoring prefab, Transform parent, OVRSpatialAnchor.LoadOptions options)
    {
        // Load unbounded anchors
        var task = OVRSpatialAnchor.LoadUnboundAnchorsAsync(options);
        while (!task.IsCompleted)
            yield return null;

        var unboundAnchors = task.GetResult();
        if (unboundAnchors == null || unboundAnchors.Length == 0)
        {
            Debug.LogWarning($"[{nameof(Anchoring)}] Failed to load the anchors.");
            yield break;
        }


        // Localize the anchors
        foreach (var unboundAnchor in unboundAnchors)
        {
            if (!unboundAnchor.Localized)
            {
                var localizeTask = unboundAnchor.LocalizeAsync();
                while (!localizeTask.IsCompleted)
                    yield return null;

                if (!localizeTask.GetResult())
                {
                    Debug.LogWarning($"[{nameof(Anchoring)}] Failed to localize the anchor. Uuid: {unboundAnchor.Uuid}");
                    continue;
                }
            }

            var spatialAnchorGo = Instantiate(prefab, unboundAnchor.Pose.position, unboundAnchor.Pose.rotation, parent);
            var anchor = spatialAnchorGo.gameObject.AddComponent<OVRSpatialAnchor>();
            unboundAnchor.BindTo(anchor);
            spatialAnchorGo._anchor = anchor;
            // load linguistic description
            spatialAnchorGo.LoadDescription();
            _globalAnchorList.Add(anchor);
            spatialAnchorGo.OnAnchorLoadCompleted?.Invoke();
        }
    }

    #endregion

    #region Saving

    public void CreateAndSaveAnchor()
    {
        if (_anchor != null)
        {
            return;
        }

        _anchor = gameObject.AddComponent<OVRSpatialAnchor>();

        StartCoroutine(InitSpatialAnchor(_anchor));
    }

    public static void SaveAnchorsToPrefs()
    {
        string value = string.Join(';', _globalAnchorList.Select(anchor => anchor.Uuid));
        Debug.Log($"[{nameof(Anchoring)}] created anchor list:{value}");
        PlayerPrefs.SetString(PLAYER_PREF_ANCHOR, value);
        PlayerPrefs.Save();
        Debug.Log($"[{nameof(Anchoring)}] Saved List with count ({_globalAnchorList.Count}) to Prefs.");
    }

    private IEnumerator InitSpatialAnchor(OVRSpatialAnchor anchor)
    {
        yield return WaitForInit(anchor);
        if (Result == OVRSpatialAnchor.OperationResult.Failure)
        {
            OnAnchorCreateCompleted?.Invoke(anchor, Result);
            yield break;
        }

        yield return SaveLocalAsync(anchor);
        Debug.Log($"[{nameof(Anchoring)}] Anchor created and saved Uuid: ({anchor.Uuid})).");
        OnAnchorCreateCompleted?.Invoke(anchor, Result);
    }

    protected IEnumerator WaitForInit(OVRSpatialAnchor anchor)
    {
        float timeoutThreshold = 5f;
        float startTime = Time.time;

        while (anchor && !anchor.Created)
        {
            if (Time.time - startTime >= timeoutThreshold)
            {
                Debug.LogWarning($"[{nameof(Anchoring)}] Failed to create the spatial anchor.");
                Result = OVRSpatialAnchor.OperationResult.Failure;
                yield break;
            }
            yield return null;
        }

        if (anchor == null)
        {
            Debug.LogWarning($"[{nameof(Anchoring)}] Failed to create the spatial anchor.");
            Result = OVRSpatialAnchor.OperationResult.Failure;
        }
    }

    protected IEnumerator SaveLocalAsync(OVRSpatialAnchor anchor)
    {
        var saveOption = new OVRSpatialAnchor.SaveOptions
        {
            Storage = OVRSpace.StorageLocation.Local
        };
        _globalAnchorList.Add(anchor);

        var task = OVRSpatialAnchor.SaveAsync(_globalAnchorList, saveOption);
        while (!task.IsCompleted)
            yield return null;

        if (!task.TryGetResult(out var result))
        {
            Debug.LogWarning($"[{nameof(Anchoring)}] Failed to save the spatial anchor.");
            Result = result;
        }
    }

    #endregion

    #region Erasing


    /// <summary>
    /// Erase all instantiated anchors anchors.
    /// </summary>
    /// <remarks>It'll collect the uuid(s) of the instantiated anchor(s) and erase them.</remarks>
    public void EraseAllAnchors()
    {
        // Nothing to erase.
        if (_globalAnchorList.Count == 0)
            return;

        StartCoroutine(EraseAnchorsRoutine());
    }

    /// <summary>
    /// Erase a anchor by <see cref="Guid"/>.
    /// </summary>
    /// <param name="uuid">Anchor's uuid to erase.</param>
    public void EraseAnchorByUuid(Guid uuid)
    {
        // Nothing to erase.
        if (_globalAnchorList.Count == 0)
            return;


        var anchor = _globalAnchorList.FirstOrDefault(anchor => anchor.Uuid == uuid);

        if (anchor == null)
        {
            Debug.LogWarning($"[{nameof(Anchoring)}] Spatial anchor with uuid [{uuid}] not found.");
            return;
        }

        StartCoroutine(EraseAnchorByUuidRoutine(anchor));
    }

    private IEnumerator EraseAnchorsRoutine()
    {
        var anchorCopy = _globalAnchorList.ToArray();
        for (int i = 0; i < anchorCopy.Length; i++)
        {
            var anchor = anchorCopy[i];
            yield return EraseAnchorByUuidRoutine(anchor);
        }

        var result = _globalAnchorList.Count == 0
            ? OVRSpatialAnchor.OperationResult.Success
            : OVRSpatialAnchor.OperationResult.Failure;
        OnAnchorsEraseAllCompleted?.Invoke(result);
    }

    private IEnumerator EraseAnchorByUuidRoutine(OVRSpatialAnchor anchor)
    {
        var task = anchor.EraseAsync(EraseOptions);
        while (!task.IsCompleted)
            yield return null;

        if (!task.GetResult())
        {
            OnAnchorEraseCompleted?.Invoke(anchor, OVRSpatialAnchor.OperationResult.Failure);
            yield break;
        }

        Destroy(anchor.gameObject);//On Destroy will erase anchor from _globalAnchorList
        if (_globalAnchorList.Any(a => a.Uuid == anchor.Uuid))
            yield return null;

        OnAnchorEraseCompleted?.Invoke(anchor, OVRSpatialAnchor.OperationResult.Success);
    }
    #endregion
}
