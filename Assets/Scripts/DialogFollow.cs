using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogFollow : MonoBehaviour
{

    public float FollowSpeed = 5f;

    public Transform FollowedObject;

    // Update is called once per frame
    void Update()
    {
        if (!FollowedObject) return;
        transform.position = Vector3.Lerp(transform.position, FollowedObject.position, FollowSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, FollowedObject.rotation, FollowSpeed * Time.fixedDeltaTime);
    }
}
