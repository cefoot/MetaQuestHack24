using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public GameObject BoxPrefab;
    private float _lastSpawn = 0f;

    public void SpawnBox()
    {//prevent double click only spawn at most once per second
        if (Time.fixedTime - _lastSpawn < 1f) return;
        Instantiate(BoxPrefab, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity, null);
        _lastSpawn = Time.fixedTime;
    }
}
