/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Moves the selected interactable around a fixed point at its current position.
/// </summary>
public class AdvancedMoveAtSourceProvider : MonoBehaviour, IMovementProvider
{
    public Vector3 MovementScale = Vector3.one;

    public Vector3 RotationScale = Vector3.one;
    [Serializable]
    public class StringEvent : UnityEvent<String> { }
    [Serializable]
    public class PosEvent : UnityEvent<Vector3> { }
    [Serializable]
    public class RotEvent : UnityEvent<Quaternion> { }

    public PosEvent UpdatePos;
    public RotEvent UpdateRot;
    public Transform ScaleReferenceTransform;

    public IMovement CreateMovement()
    {
        return new AdvancedMoveRelativeToTarget(this);
    }
}

public class AdvancedMoveRelativeToTarget : IMovement
{

    public Pose Pose => _current;
    public bool Stopped => true;

    private Pose _current = Pose.identity;
    private Quaternion _startRotation;
    private Quaternion _startRotDif;
    private Pose _originalTarget;
    private Pose _originalSource;
    private Vector3 MovementScale = Vector3.one;

    private Vector3 RotationScale = Vector3.one;
    private AdvancedMoveAtSourceProvider _reference;

    public AdvancedMoveRelativeToTarget(AdvancedMoveAtSourceProvider reference)
    {
        MovementScale = reference.MovementScale;
        RotationScale = reference.RotationScale;
        _reference = reference;
    }

    public void MoveTo(Pose target)
    {
        _originalTarget = target;
    }

    public void UpdateTarget(Pose target)
    {
        _reference.UpdateRot?.Invoke(_startRotation);
        Pose grabberDelta = PoseUtils.Delta(_originalTarget, target);
        var rotDif = Quaternion.RotateTowards(_startRotation, _originalSource.rotation * grabberDelta.rotation, 360);
        
        var scaledLocalRotDif = ScaleQuaternion(Quaternion.Inverse(_reference.ScaleReferenceTransform.rotation * rotDif), _reference.RotationScale);
        var scaledRotDif = _reference.ScaleReferenceTransform.rotation * scaledLocalRotDif;



        // Convert global delta to local delta
        Vector3 localPositionDelta = _originalTarget.rotation * grabberDelta.position;
        Quaternion localRotationDelta = Quaternion.Inverse(_originalTarget.rotation) * grabberDelta.rotation;

        // Apply scaling to the delta
        localPositionDelta = Vector3.Scale(localPositionDelta, Vector3.Scale(_reference.ScaleReferenceTransform.right + _reference.ScaleReferenceTransform.up + _reference.ScaleReferenceTransform.forward, MovementScale));
        grabberDelta.position = /*_reference.ScaleReferenceTransform.TransformDirection*/(localPositionDelta);
        //grabberDelta.rotation = ScaleQuaternion(localRotationDelta, RotationScale);

        // Apply the scaled, local deltas back to the object's transform
        //PoseUtils.Multiply(_originalSource, grabberDelta, ref _current);
        _current.position = _originalSource.position + grabberDelta.position;
        //_current.rotation = scaledRotDif;
        _current.rotation = _startRotDif * rotDif;
    }

    Quaternion ScaleQuaternion(Quaternion quaternion, Vector3 scale)
    {
        Quaternion scaled = quaternion;
        scaled.x *= scale.x;
        scaled.y *= scale.y;
        scaled.z *= scale.z;
        scaled.w = Mathf.Clamp01(scaled.w); // Ensure the quaternion remains normalized
        return scaled;
    }

    /// <summary>
    /// called once when grab is initiated
    /// </summary>
    /// <param name="source"></param>
    public void StopAndSetPose(Pose source)
    {
        _reference.UpdatePos?.Invoke(source.position);
        _reference.UpdateRot?.Invoke(source.rotation);
        _current = _originalSource = source;
        _startRotation = _reference.ScaleReferenceTransform.rotation;
        _startRotDif = Quaternion.RotateTowards(_startRotation, _originalSource.rotation, 360);
    }

    public void Tick()
    {

    }
}
