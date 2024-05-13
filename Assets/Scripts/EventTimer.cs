using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class EventTimer : MonoBehaviour
{
    public UnityEvent TimerEvent;
    public float seconds = 3;
    
    private bool _waited = false;
    private bool _triggered = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!_waited)
        {
            StartCoroutine(Wait(seconds));
        }
        else if (_waited && !_triggered)
        {
            TimerEvent.Invoke();
            _triggered = true;
        }
    }

    void Restart()
    {
        _waited = false;
        _triggered = false;
    }

    IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _waited = true;
    }
}
