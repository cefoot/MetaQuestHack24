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
public class AnotherMoveAtSourceProvider : MonoBehaviour, IMovementProvider
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
        return new AnotherMoveRelativeToTarget(this);
    }
}

public class AnotherMoveRelativeToTarget : IMovement
{


    public Pose Pose => _current;
    public bool Stopped => true;

    private Pose _current = Pose.identity;
    private Pose _originalTarget;
    private Pose _originalSource;
    private AnotherMoveAtSourceProvider _reference;

    public AnotherMoveRelativeToTarget(AnotherMoveAtSourceProvider reference)
    {
        _reference = reference;
    }

    public void MoveTo(Pose target)
    {
        _originalTarget = target;
    }

    public void UpdateTarget(Pose target)
    {
        Pose grabberDelta = PoseUtils.Delta(_originalTarget, target);
        Vector3 localPositionDelta = _originalTarget.rotation * grabberDelta.position;
        localPositionDelta = Vector3.Scale(localPositionDelta, Vector3.Scale(_reference.ScaleReferenceTransform.right + _reference.ScaleReferenceTransform.up + _reference.ScaleReferenceTransform.forward, _reference.MovementScale));
        grabberDelta.position = Quaternion.Inverse(_originalTarget.rotation) * localPositionDelta;
        PoseUtils.Multiply(_originalSource, grabberDelta, ref _current);
    }

    public void StopAndSetPose(Pose source)
    {
        _current = _originalSource = source;
    }

    public void Tick()
    {
    }
}
