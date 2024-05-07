using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEditor;
using UnityEngine;

public class Helper : MonoBehaviour
{
    
    [MenuItem("GameObject/Custom Actions/Tools/Update Distance Hand Grab", false, 11)]
    static void AddRigidbody(MenuCommand menuCommand)
    {
        // This function adds a Rigidbody component to the selected GameObject
        GameObject gameObject = menuCommand.context as GameObject;
        if (gameObject != null)
        {
            Undo.DestroyObjectImmediate(gameObject.GetComponentInParent<Rigidbody>());
            var rigidBdy = gameObject.GetComponentInParent<Rigidbody>();
            gameObject.GetComponent<DistanceHandGrabInteractable>().InjectRigidbody(rigidBdy);
            gameObject.GetComponent<PhysicsGrabbable>().InjectRigidbody(rigidBdy);
            gameObject.GetComponent<DistanceGrabInteractable>().InjectRigidbody(rigidBdy);
            Undo.DestroyObjectImmediate(gameObject.GetComponent<MoveTowardsTargetProvider>());
            var mover = Undo.AddComponent<AdvancedMoveAtSourceProvider>(gameObject);
            var handInteractable = gameObject.GetComponent<DistanceHandGrabInteractable>();
            Undo.RecordObject(handInteractable, "Change Hand Grab Mover");
            handInteractable.InjectOptionalMovementProvider(mover);
            var interactable = gameObject.GetComponent<DistanceGrabInteractable>();
            Undo.RecordObject(interactable, "Change Grab Mover");
            interactable.InjectOptionalMovementProvider(mover);
        }
    }
}
