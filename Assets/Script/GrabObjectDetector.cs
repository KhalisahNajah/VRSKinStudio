using UnityEngine;
using UnityEngine.Events;
using Leap;
using System;
using System.Collections; // <--- ADD THIS LINE for IEnumerator

namespace Leap
{
    public class GrabObjectDetector : MonoBehaviour
    {
        [Tooltip("The specific GameObject that needs to be grabbed.")]
        public GameObject targetGrabObject;

        [Tooltip("Optional: Specify a particular LeapProvider. If none is selected, the script will automatically find one in the scene.")]
        [SerializeField]
        private LeapProvider leapProvider = null;

        /// <summary>
        /// The minimum strength required for a hand to be considered "grabbing".
        /// Adjust this value based on your desired grab sensitivity (0.0 to 1.0).
        /// </summary>
        [Range(0f, 1f)]
        [Tooltip("The minimum strength required for a hand to be considered 'grabbing'. Adjust this value based on your desired grab sensitivity (0.0 to 1.0).")]
        public float grabStrengthThreshold = 0.8f;

        /// <summary>
        /// The maximum distance a hand can be from the target object to be considered grabbing it.
        /// </summary>
        [Tooltip("The maximum distance a hand can be from the target object to be considered grabbing it.")]
        public float grabDistanceThreshold = 0.1f; // Adjust as needed, e.g., 0.1 meters

        /// <summary>
        /// Delay before triggering OnGrabLost to prevent flickering if the grab is momentarily released.
        /// </summary>
        [Tooltip("Delay before triggering OnGrabLost to prevent flickering if the grab is momentarily released.")]
        public float grabLostHysteresisTime = 0.2f;

        // Events for grab detection
        [Serializable] public class GrabEvent : UnityEvent<GameObject> { }
        [Serializable] public class GrabLostEvent : UnityEvent<GameObject> { }

        /// <summary>
        /// Event fired when the target object is grabbed by a Leap Motion hand.
        /// The GameObject grabbed will be passed as a parameter.
        /// </summary>
        public GrabEvent OnGrabDetected;

        /// <summary>
        /// Event fired while the target object is being continuously grabbed.
        /// The GameObject grabbed will be passed as a parameter.
        /// </summary>
        public GrabEvent WhileGrabDetected;

        /// <summary>
        /// Event fired when the target object is released after being grabbed.
        /// The GameObject that was grabbed will be passed as a parameter.
        /// </summary>
        public GrabLostEvent OnGrabLost;

        private bool isObjectGrabbed = false;
        private Coroutine grabLostCoroutine;

        private void Start()
        {
            if (leapProvider == null)
            {
                leapProvider = Hands.Provider;
                if (leapProvider == null)
                {
                    Debug.LogError("GrabObjectDetector: No LeapProvider found in the scene or assigned. Please add a LeapProvider to your scene.", this);
                    enabled = false; // Disable the script if no provider is found
                    return;
                }
            }

            if (targetGrabObject == null)
            {
                Debug.LogError("GrabObjectDetector: No Target Grab Object assigned. Please assign the GameObject you want to detect grabbing.", this);
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (leapProvider == null || targetGrabObject == null) return;

            bool currentlyGrabbingTarget = false;

            foreach (var hand in leapProvider.CurrentFrame.Hands)
            {
                // CORRECTED: Direct cast from Leap.Vector to UnityEngine.Vector3
                // Assuming hand.PalmPosition is a Leap.Vector
                if (Vector3.Distance(hand.PalmPosition, targetGrabObject.transform.position) < grabDistanceThreshold &&
                    hand.GrabStrength >= grabStrengthThreshold)
                {
                    currentlyGrabbingTarget = true;
                    // You might want to add a check here for which object is actually being held by a physics-based grab if you have one.
                    // For now, we are relying on proximity and grab strength.
                    break;
                }
            }

            if (currentlyGrabbingTarget && !isObjectGrabbed)
            {
                // Grab Detected
                isObjectGrabbed = true;
                if (grabLostCoroutine != null)
                {
                    StopCoroutine(grabLostCoroutine);
                    grabLostCoroutine = null;
                }
                OnGrabDetected.Invoke(targetGrabObject);
                Debug.Log($"GrabObjectDetector: Grab Detected on '{targetGrabObject.name}'!", this);
            }
            else if (currentlyGrabbingTarget && isObjectGrabbed)
            {
                // While Grab Detected
                WhileGrabDetected.Invoke(targetGrabObject);
            }
            else if (!currentlyGrabbingTarget && isObjectGrabbed)
            {
                // Grab Lost, start the hysteresis timer
                if (grabLostCoroutine == null)
                {
                    grabLostCoroutine = StartCoroutine(HandleGrabLostHysteresis());
                }
            }
            else if (!currentlyGrabbingTarget && !isObjectGrabbed)
            {
                // Not grabbing and already in 'not grabbed' state, ensure poseLostCanvas is off if it somehow got active
                if (grabLostCoroutine != null)
                {
                    StopCoroutine(grabLostCoroutine);
                    grabLostCoroutine = null;
                }
            }
        }

        private IEnumerator HandleGrabLostHysteresis()
        {
            yield return new WaitForSeconds(grabLostHysteresisTime);

            // Re-check if the grab is still lost after the delay
            bool recheckGrab = false;
            foreach (var hand in leapProvider.CurrentFrame.Hands)
            {
                // CORRECTED: Direct cast from Leap.Vector to UnityEngine.Vector3
                if (Vector3.Distance(hand.PalmPosition, targetGrabObject.transform.position) < grabDistanceThreshold &&
                    hand.GrabStrength >= grabStrengthThreshold)
                {
                    recheckGrab = true;
                    break;
                }
            }

            if (!recheckGrab)
            {
                isObjectGrabbed = false;
                OnGrabLost.Invoke(targetGrabObject);
                Debug.Log($"GrabObjectDetector: Grab Lost from '{targetGrabObject.name}'.", this);
            }
            else
            {
                // Grab was re-established during hysteresis, so don't trigger OnGrabLost
                Debug.Log($"GrabObjectDetector: Grab re-established on '{targetGrabObject.name}' during hysteresis.", this);
            }
            grabLostCoroutine = null; // Clear the coroutine reference
        }

        public bool IsObjectCurrentlyGrabbed()
        {
            return isObjectGrabbed;
        }
    }
}