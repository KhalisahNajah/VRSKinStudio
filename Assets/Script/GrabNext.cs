using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Required for UnityEvents
using TMPro; // Required for TextMeshPro

namespace Leap.Examples
{
    public class GrabNext : MonoBehaviour
    {
        [Header("Grab Detection Settings")]
        [Tooltip("Assign the GrabObjectDetector script instance here.")]
        public GrabObjectDetector grabDetector;

        [SerializeField]
        private GameObject initialCanvas; // The canvas shown before the specific object is grabbed

        [SerializeField]
        private GameObject correctCanvas; // The canvas that briefly appears when the specific object is grabbed

        [SerializeField]
        private float correctCanvasDisplayDuration = 1f; // Duration for how long the 'correctCanvas' stays active

        private bool hasCorrectGrabBeenDetected = false; // Flag to track if the correct object grab has been detected

        // --- NEW: Grab Lost Settings ---
        [Header("Grab Lost Settings")]
        [SerializeField]
        private GameObject grabLostCanvas; // The canvas that appears when the specific object grab is lost

        private Coroutine grabLostCheckCoroutine; // Reference to the grab lost coroutine (though GrabObjectDetector handles the primary logic now)

        [Header("Next Canvas Switch Settings (after 'correctCanvas' hides)")]
        [Tooltip("The canvas that will be activated AFTER the 'correctCanvas' has hidden.")]
        public Canvas nextCanvas;

        [Tooltip("Additional canvases that will be hidden when the next canvas switch occurs.")]
        public Canvas[] canvasesToHideOnNextSwitch;

        [Tooltip("GameObjects in this list will be activated when the next canvas switch occurs.")]
        public GameObject[] gameObjectsToActivateOnNextSwitch;

        [Tooltip("GameObjects in this list will be deactivated when the next canvas switch occurs.")]
        public GameObject[] gameObjectsToDeactivateOnNextSwitch;

        [Tooltip("TextMeshPro UGUI elements in this list will be activated (made visible) when the next canvas switch occurs.")]
        public TextMeshProUGUI[] textMeshProElementsToActivateOnNextSwitch;

        [Tooltip("TextMeshPro UGUI elements in this list will be deactivated (made hidden) when the next canvas switch occurs.")]
        public TextMeshProUGUI[] textMeshProElementsToDeactivateOnNextSwitch;

        private void OnEnable()
        {
            if (grabDetector != null)
            {
                // Subscribe to the grab detection events
                grabDetector.OnGrabDetected.AddListener(OnSpecificObjectGrabbed);
                grabDetector.OnGrabLost.AddListener(OnSpecificObjectGrabLost);
            }
            else
            {
                Debug.LogError("GrabNext: Grab Detector is not assigned. Please assign the GrabObjectDetector script in the Inspector.", this);
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (grabDetector != null)
            {
                // Unsubscribe from the grab detection events to prevent memory leaks
                grabDetector.OnGrabDetected.RemoveListener(OnSpecificObjectGrabbed);
                grabDetector.OnGrabLost.RemoveListener(OnSpecificObjectGrabLost);
            }
        }

        private void Start()
        {
            // Ensure correctCanvas is initially off
            if (correctCanvas != null)
                correctCanvas.SetActive(false);

            // Ensure nextCanvas is initially off
            if (nextCanvas != null)
                nextCanvas.gameObject.SetActive(false);

            // Ensure grabLostCanvas is initially off
            if (grabLostCanvas != null)
                grabLostCanvas.SetActive(false);

            // Ensure all objects/texts for the next switch are initially in their intended 'off' state
            SetInitialStatesForNextSwitchElements();
        }

        private void SetInitialStatesForNextSwitchElements()
        {
            if (gameObjectsToActivateOnNextSwitch != null)
            {
                foreach (GameObject obj in gameObjectsToActivateOnNextSwitch)
                {
                    if (obj != null) obj.SetActive(false); // They should be off initially
                }
            }
            if (gameObjectsToDeactivateOnNextSwitch != null)
            {
                foreach (GameObject obj in gameObjectsToDeactivateOnNextSwitch)
                {
                    if (obj != null) obj.SetActive(true); // They should be on initially to be deactivated later
                }
            }
            if (textMeshProElementsToActivateOnNextSwitch != null)
            {
                foreach (TextMeshProUGUI tmpElement in textMeshProElementsToActivateOnNextSwitch)
                {
                    if (tmpElement != null) tmpElement.gameObject.SetActive(false);
                }
            }
            if (textMeshProElementsToDeactivateOnNextSwitch != null)
            {
                foreach (TextMeshProUGUI tmpElement in textMeshProElementsToDeactivateOnNextSwitch)
                {
                    if (tmpElement != null) tmpElement.gameObject.SetActive(true);
                }
            }
            if (canvasesToHideOnNextSwitch != null)
            {
                foreach (Canvas canvasToHide in canvasesToHideOnNextSwitch)
                {
                    if (canvasToHide != null) canvasToHide.gameObject.SetActive(true); // Assume they are on to be hidden later
                }
            }
        }

        /// <summary>
        /// This method is called when the specific target object is grabbed.
        /// </summary>
        public void OnSpecificObjectGrabbed(GameObject grabbedObject)
        {
            // If the correct grab has already been detected and the next scene is triggered,
            // we don't want to reactivate the initial flow or grab lost logic unnecessarily.
            if (hasCorrectGrabBeenDetected)
            {
                // If the object is re-grabbed after being lost, hide the grab lost canvas
                if (grabLostCanvas != null && grabLostCanvas.activeSelf)
                {
                    grabLostCanvas.SetActive(false);
                    Debug.Log($"GrabNext: Re-grabbed '{grabbedObject.name}'. Hiding Grab Lost Canvas.", this);
                    // Stop any ongoing grab lost check coroutine (though GrabObjectDetector handles this)
                    if (grabLostCheckCoroutine != null)
                    {
                        StopCoroutine(grabLostCheckCoroutine);
                        grabLostCheckCoroutine = null;
                    }
                }
                return; // Exit if the final correct grab has been detected
            }

            // This is the initial correct grab detection
            hasCorrectGrabBeenDetected = true; // Set the flag to true as the correct object is now grabbed

            if (initialCanvas != null)
            {
                initialCanvas.SetActive(false);
                Debug.Log("GrabNext: Hiding Initial Canvas.", this);
            }

            // Start the coroutine to display correctCanvas, then trigger the next canvas switch
            if (correctCanvas != null)
            {
                Debug.Log("GrabNext: Showing Correct Canvas.", this);
                StartCoroutine(ShowCorrectCanvasAndThenSwitch(correctCanvas, correctCanvasDisplayDuration));
            }
            else
            {
                // If there's no correctCanvas to show, immediately trigger the next switch
                Debug.LogWarning("GrabNext: No Correct Canvas assigned. Immediately triggering next switch.", this);
                ExecuteNextCanvasSwitchLogic();
            }
        }

        /// <summary>
        /// This method is called when the specific target object is released after being grabbed.
        /// </summary>
        public void OnSpecificObjectGrabLost(GameObject releasedObject)
        {
            if (hasCorrectGrabBeenDetected && grabLostCanvas != null)
            {
                // Only show grab lost canvas if the correct grab was previously detected
                grabLostCanvas.SetActive(true);
                Debug.Log($"GrabNext: Grab Lost from '{releasedObject.name}'. Showing Grab Lost Canvas.", this);
            }
        }

        private IEnumerator ShowCorrectCanvasAndThenSwitch(GameObject canvasToShow, float duration)
        {
            canvasToShow.SetActive(true);
            yield return new WaitForSeconds(duration);
            canvasToShow.SetActive(false);
            Debug.Log("GrabNext: Hiding Correct Canvas.", this);

            ExecuteNextCanvasSwitchLogic();
        }

        private void ExecuteNextCanvasSwitchLogic()
        {
            Debug.Log("GrabNext: Executing Next Canvas Switch Logic.", this);

            // Activate the next canvas
            if (nextCanvas != null)
            {
                nextCanvas.gameObject.SetActive(true);
                Debug.Log($"GrabNext: Activating Next Canvas: {nextCanvas.name}", this);
            }

            // Hide other canvases
            if (canvasesToHideOnNextSwitch != null)
            {
                foreach (Canvas canvasToHide in canvasesToHideOnNextSwitch)
                {
                    if (canvasToHide != null)
                    {
                        canvasToHide.gameObject.SetActive(false);
                        Debug.Log($"GrabNext: Hiding Canvas: {canvasToHide.name}", this);
                    }
                }
            }

            // Activate specified GameObjects
            if (gameObjectsToActivateOnNextSwitch != null)
            {
                foreach (GameObject obj in gameObjectsToActivateOnNextSwitch)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"GrabNext: Activating GameObject: {obj.name}", this);
                    }
                }
            }

            // Deactivate specified GameObjects
            if (gameObjectsToDeactivateOnNextSwitch != null)
            {
                foreach (GameObject obj in gameObjectsToDeactivateOnNextSwitch)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"GrabNext: Deactivating GameObject: {obj.name}", this);
                    }
                }
            }

            // Activate specified TextMeshPro UGUI elements
            if (textMeshProElementsToActivateOnNextSwitch != null)
            {
                foreach (TextMeshProUGUI tmpElement in textMeshProElementsToActivateOnNextSwitch)
                {
                    if (tmpElement != null)
                    {
                        tmpElement.gameObject.SetActive(true);
                        Debug.Log($"GrabNext: Activating TextMeshPro: {tmpElement.name}", this);
                    }
                }
            }

            // Deactivate specified TextMeshPro UGUI elements
            if (textMeshProElementsToDeactivateOnNextSwitch != null)
            {
                foreach (TextMeshProUGUI tmpElement in textMeshProElementsToDeactivateOnNextSwitch)
                {
                    if (tmpElement != null)
                    {
                        tmpElement.gameObject.SetActive(false);
                        Debug.Log($"GrabNext: Deactivating TextMeshPro: {tmpElement.name}", this);
                    }
                }
            }
        }
    }
}