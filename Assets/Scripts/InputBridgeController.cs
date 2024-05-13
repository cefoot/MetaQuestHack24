using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Voice.Toolkit.Samples
{
    public class InputBridgeController : MonoBehaviour
    {
        [SerializeField] private InputBridge _inputBridge;
        [SerializeField] private OVRHand _leftHand;
        [SerializeField] private OVRHand _rightHand;
        [SerializeField] private Camera _camera;

        private bool _leftHandPinching;
        private bool _rightHandPinching;

        protected void OnEnable()
        {
            if (!_camera)
            {
                _camera = Camera.main;
            }
        }


        // Update is called once per frame
        void Update()
        {
            UpdatePinchState(_leftHand, ref _leftHandPinching);
            UpdatePinchState(_rightHand, ref _rightHandPinching);
        }

        void UpdatePinchState(OVRHand hand, ref bool handPinching)
        {
            bool currentlyPinching = hand.IsTracked && hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > 0.9f;

            if (currentlyPinching && !handPinching)
            {
                _inputBridge.Click(true, GetHandRay(hand));
                handPinching = true;
            }
            else if (!currentlyPinching && handPinching)
            {
                _inputBridge.Click(false, GetHandRay(hand));
                handPinching = false;
            }
        }


        Ray GetHandRay(OVRHand hand)
        {
            // Get the default pinch ray for the hand
            Vector3 pinchRayOrigin = hand.PointerPose.position;
            Vector3 pinchRayDirection = hand.PointerPose.forward;

            // Return the hand ray
            return new Ray(pinchRayOrigin, pinchRayDirection);
        }
    }
}