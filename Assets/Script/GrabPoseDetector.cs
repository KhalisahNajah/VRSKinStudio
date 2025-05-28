/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2024.                                  *
 * *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/Lgrabense-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using System;
using UnityEngine;

namespace Leap
{
    /// <summary>
    /// A detector that identifies a grab action,
    /// and can specifically detect if the grab is occurring on a designated target object.
    /// Utilizes hysteresis for grab and ungrab thresholds.
    /// </summary>
    public class GrabPoseDetector : ActionDetector
    {
        [SerializeField, Range(0, 1), Tooltip("The grab strength needed to initiate a grab. 1 being hand fully closed, 0 being open hand.")]
        public float activateStrength = 0.8f;

        [SerializeField, Range(0, 1), Tooltip("The grab strength needed to release a grab. 1 being hand fully closed, 0 being open hand.")]
        public float deactivateStrength = 0.6f;

        [Header("Specific Object Grab Detection")]
        [SerializeField, Tooltip("The GameObject that the user needs to grab for the grab action to be detectable as a 'grab pose'.")]
        private GameObject targetGrabObject;

        [SerializeField, Tooltip("The maximum distance from the hand (palm position) to the target object's center for a 'grab pose' to be detected.")]
        public float grabProximityThreshold = 0.1f; // Adjust this value in the Inspector

        /// <summary>
        /// Did the general grab action start this frame?
        /// </summary>
        public bool GrabStartedThisFrame => actionStartedThisFrame;

        /// <summary>
        /// Is the general grab action currently active?
        /// </summary>
        public bool IsGrabbing => IsDoingAction;

        /// <summary>
        /// Is the hand currently performing a grab *and* is within proximity of the specified target object?
        /// This indicates a successful "grab pose" on the object.
        /// </summary>
        public bool IsGrabbingSpecificObject { get; private set; }

        // General grab events (inherited from ActionDetector)
        public Action<Hand> OnGrabStart => onActionStart;
        public Action<Hand> OnGrabEnd => onActionEnd;
        public Action<Hand> OnGrabbing => onAction;

        /// <summary>
        /// Event fired when a grab on the **specific target object** begins.
        /// Provides the Hand and the grabbed GameObject.
        /// </summary>
        public event Action<Hand, GameObject> OnSpecificObjectGrabStart;

        /// <summary>
        /// Event fired when a grab on the **specific target object** ends.
        /// Provides the Hand (can be null if hand was lost) and the previously grabbed GameObject.
        /// </summary>
        public event Action<Hand, GameObject> OnSpecificObjectGrabEnd;

        protected override void UpdateActionStatus(Hand _hand)
        {
            // Handle hand loss first
            if (_hand == null)
            {
                if (IsGrabbingSpecificObject)
                {
                    // If a specific object was being grabbed and the hand is lost, end the specific grab
                    IsGrabbingSpecificObject = false;
                    OnSpecificObjectGrabEnd?.Invoke(null, targetGrabObject); // Invoke with null hand as it's no longer tracked
                }
                // Also reset general grab state if hand is lost
                if (IsDoingAction)
                {
                    onActionEnd?.Invoke(null);
                    IsDoingAction = false;
                }
                return;
            }

            float _grabStrength = _hand.GrabStrength;
            bool isNearTargetObject = targetGrabObject != null &&
                                      Vector3.Distance(_hand.PalmPosition, targetGrabObject.transform.position) < grabProximityThreshold;

            bool newIsDoingGeneralGrab = false;

            // Determine general grab state with hysteresis
            if (_grabStrength >= activateStrength)
            {
                newIsDoingGeneralGrab = true;
            }
            else if (_grabStrength <= deactivateStrength)
            {
                newIsDoingGeneralGrab = false;
            }
            else
            {
                // Maintain current state if strength is between thresholds
                newIsDoingGeneralGrab = IsDoingAction;
            }

            // Fire general grab start/end events
            actionStartedThisFrame = newIsDoingGeneralGrab && !IsDoingAction;
            if (actionStartedThisFrame)
            {
                onActionStart?.Invoke(_hand);
            }
            if (IsDoingAction && !newIsDoingGeneralGrab) // General grab just ended
            {
                onActionEnd?.Invoke(_hand);
            }
            IsDoingAction = newIsDoingGeneralGrab; // Update the general grab state

            // Determine and fire specific object grab events
            bool newIsGrabbingSpecificObject = IsDoingAction && isNearTargetObject;

            if (newIsGrabbingSpecificObject && !IsGrabbingSpecificObject) // Specific object grab just started
            {
                OnSpecificObjectGrabStart?.Invoke(_hand, targetGrabObject);
            }
            else if (!newIsGrabbingSpecificObject && IsGrabbingSpecificObject) // Specific object grab just ended
            {
                OnSpecificObjectGrabEnd?.Invoke(_hand, targetGrabObject);
            }
            IsGrabbingSpecificObject = newIsGrabbingSpecificObject;

            // Fire continuous general grab event if active
            if (IsDoingAction)
            {
                onAction?.Invoke(_hand);
            }
        }
    }
}