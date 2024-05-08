using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Anchoring BoxPrefab;
    private float _lastBtn = 0f;

    public void SpawnBox()
    {//prevent double click only spawn at most once per second
        if (Time.fixedTime - _lastBtn < 1f) return;
        _lastBtn = Time.fixedTime;
        Instantiate(BoxPrefab, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity, null);
    }

    public void LoadAndSpawnAnchors(GameObject parent)
    {
        if (Time.fixedTime - _lastBtn < 1f) return;
        _lastBtn = Time.fixedTime;
        StartCoroutine(Anchoring.LoadAnchorsFromPrefs(parent.transform, BoxPrefab));
    }

    public void SavelAllAnchorsToPrefs()
    {
        if (Time.fixedTime - _lastBtn < 1f) return;
        _lastBtn = Time.fixedTime;
        Anchoring.SaveAnchorsToPrefs();
    }
}
