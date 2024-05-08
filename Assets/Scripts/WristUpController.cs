using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WristUpController : MonoBehaviour
{
    [Serializable]
    public class PositionUnityEvent : UnityEvent<Vector3> { }

    public Transform handPalmLeft; // Assign this to the palm bone
    public Transform handPalmRight; // Assign this to the palm bone
    public PositionUnityEvent LeftHandPalmFacingCam;
    public Vector3 MenuPosLeftHand;
    public PositionUnityEvent RightHandPalmFacingCam;
    public Vector3 MenuPosRightHand;
    public UnityEvent HandPalmFacingAwayFromCam;
    public float Treshold = -.7F;

    public enum ShowState
    {
        /// <summary>
        /// initial state
        /// </summary>
        Zero = -1,
        /// <summary>
        /// no hand facing
        /// </summary>
        None = 0,
        /// <summary>
        /// left hand facing
        /// </summary>
        LeftHand = 1,
        /// <summary>
        /// right hand facing
        /// </summary>
        RightHand = 2,
    }

    /// <summary>
    /// will change in code only visible in Editor for debug purpose
    /// </summary>
    [Tooltip("only shown for debug purpose")]
    public ShowState CurrentShowState = ShowState.Zero;


    public ShowState GetCurrentState(ShowState lastState)
    {
        switch (lastState)
        {
            case ShowState.Zero:
            case ShowState.None:
            case ShowState.LeftHand:
                if (Vector3.Dot(handPalmLeft.up, Camera.main.transform.forward) > Treshold)
                {//facing away
                    if (Vector3.Dot(-handPalmRight.up, Camera.main.transform.forward) > Treshold)
                    {//facing away
                        return ShowState.None;
                    }
                    return ShowState.RightHand;
                }
                return ShowState.LeftHand;
            case ShowState.RightHand:
                if (Vector3.Dot(-handPalmRight.up, Camera.main.transform.forward) > Treshold)
                {//facing away
                    if (Vector3.Dot(handPalmLeft.up, Camera.main.transform.forward) > Treshold)
                    {//facing away
                        return ShowState.None;
                    }
                    return ShowState.LeftHand;
                }
                return ShowState.RightHand;
        }
        return ShowState.None;
    }

    public void OnEnable()
    {
        CurrentShowState = ShowState.Zero;//always start with zero
    }

    void Update()
    {
        var lastState = CurrentShowState;
        var newState = GetCurrentState(CurrentShowState);
        CurrentShowState = newState;
        switch (newState)
        {
            case ShowState.None:
                if (lastState != CurrentShowState) HandPalmFacingAwayFromCam?.Invoke();
                break;
            case ShowState.LeftHand:
                if (lastState != CurrentShowState) LeftHandPalmFacingCam?.Invoke(MenuPosLeftHand);
                transform.position = handPalmLeft.position;
                transform.rotation = handPalmLeft.rotation;
                break;
            case ShowState.RightHand:
                if (lastState != CurrentShowState) RightHandPalmFacingCam?.Invoke(MenuPosRightHand);
                transform.position = handPalmRight.position;
                transform.rotation = Quaternion.LookRotation(handPalmRight.forward, -handPalmRight.up);
                break;
        }
    }
}
