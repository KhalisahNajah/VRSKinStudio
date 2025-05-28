using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Required for TextMeshPro

namespace Leap.Examples
{
    public class GrabbingDetection : MonoBehaviour
    {
        [Header("Grab Detection Settings")]
        [SerializeField]
        [Tooltip("The canvas shown before any grab is detected.")]
        private GameObject initialCanvas;

        [SerializeField]
        [Tooltip("The canvas that briefly appears when a grab is successfully detected.")]
        private GameObject grabSuccessCanvas;

        [SerializeField]
        [Tooltip("Duration for how long the 'grabSuccessCanvas' stays active.")]
        private float grabSuccessCanvasDisplayDuration = 1f;

        // Flag to track if the initial grab has been detected and the sequence has started.
        // This prevents the initial flow from re-triggering if the grab is lost and re-gained.
        private bool hasGrabBeenDetected = false;

        // --- Grab Lost Settings ---
        [Header("Grab Lost Settings")]
        [SerializeField]
        [Tooltip("The canvas that appears when the grab is lost after it has been initially detected.")]
        private GameObject grabLostCanvas;

        [SerializeField]
        [Tooltip("How often to check if the grab is lost (after 'NoGrabDetected' is called).")]
        private float grabLostCheckInterval = 0.5f;

        // Reference to the coroutine that checks if the grab is lost, allowing it to be stopped.
        private Coroutine grabLostCheckCoroutine;

        // --- Next Canvas Switch Settings (after 'grabSuccessCanvas' hides) ---
        [Header("Next Canvas Switch Settings")]
        [Tooltip("The canvas that will be activated AFTER the 'grabSuccessCanvas' has hidden.")]
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

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the states of canvases and GameObjects.
        /// </summary>
        private void Start()
        {
            // Ensure grabSuccessCanvas is initially off
            if (grabSuccessCanvas != null)
                grabSuccessCanvas.SetActive(false);

            // Ensure nextCanvas is initially off
            if (nextCanvas != null)
                nextCanvas.gameObject.SetActive(false);

            // Ensure grabLostCanvas is initially off
            if (grabLostCanvas != null)
                grabLostCanvas.SetActive(false);

            // Set the initial active/inactive states for all elements that will be toggled later.
            SetInitialStatesForNextSwitchElements();
        }

        /// <summary>
        /// Sets the initial active/inactive states for GameObjects, Canvases, and TextMeshPro elements
        /// that are part of the 'next canvas switch' logic. This ensures they are in the correct
        /// starting state before the grab detection sequence begins.
        /// </summary>
        private void SetInitialStatesForNextSwitchElements()
        {
            if (gameObjectsToActivateOnNextSwitch != null)
            {
                foreach (GameObject obj in gameObjectsToActivateOnNextSwitch)
                {
                    if (obj != null) obj.SetActive(false); // Elements to activate later should be off initially
                }
            }
            if (gameObjectsToDeactivateOnNextSwitch != null)
            {
                foreach (GameObject obj in gameObjectsToDeactivateOnNextSwitch)
                {
                    if (obj != null) obj.SetActive(true); // Elements to deactivate later should be on initially
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
                    if (canvasToHide != null) canvasToHide.gameObject.SetActive(true); // Canvases to hide later should be on initially
                }
            }
        }

        /// <summary>
        /// This method should be called by an external system (e.g., a Leap Motion grab detector)
        /// when a hand is detected as performing a grab action.
        /// </summary>
        public void GrabDetected()
        {
            // If the initial grab has already been detected and the sequence has started,
            // we only react to re-detecting the grab if the grabLostCanvas is currently active.
            if (hasGrabBeenDetected)
            {
                // If the grab was lost and the 'grabLostCanvas' is showing, hide it and stop the check.
                if (grabLostCanvas != null && grabLostCanvas.activeSelf)
                {
                    grabLostCanvas.SetActive(false);
                    Debug.Log($"GrabbingDetection: Re-detected grab. Hiding Grab Lost Canvas.", this);
                    // Stop the grab lost check coroutine when the grab is found again
                    if (grabLostCheckCoroutine != null)
                    {
                        StopCoroutine(grabLostCheckCoroutine);
                        grabLostCheckCoroutine = null;
                    }
                }
                return; // Exit if the sequence has already been initiated (initial grab handled).
            }

            // This block executes only for the *first* successful grab detection.
            hasGrabBeenDetected = true; // Set the flag to true as the initial grab is now detected

            // Hide the initial canvas if it's assigned.
            if (initialCanvas != null)
            {
                initialCanvas.SetActive(false);
                Debug.Log("GrabbingDetection: Hiding Initial Canvas.", this);
            }

            // Start the coroutine to display the grabSuccessCanvas, then trigger the next canvas switch.
            if (grabSuccessCanvas != null)
            {
                Debug.Log("GrabbingDetection: Showing Grab Success Canvas.", this);
                StartCoroutine(ShowGrabSuccessCanvasAndThenSwitch(grabSuccessCanvas, grabSuccessCanvasDisplayDuration));
            }
            else
            {
                // If there's no grabSuccessCanvas to show, immediately trigger the next switch logic.
                Debug.LogWarning("GrabbingDetection: No Grab Success Canvas assigned. Immediately triggering next switch.", this);
                ExecuteNextCanvasSwitchLogic();
            }
        }

        /// <summary>
        /// This method should be called by an external system (e.g., a Leap Motion grab detector)
        /// when a hand is no longer detected as performing a grab action.
        /// </summary>
        public void NoGrabDetected()
        {
            // Only consider showing the 'grabLostCanvas' if a grab was previously detected
            // and the 'grabLostCanvas' is not already active.
            if (hasGrabBeenDetected && (grabLostCanvas != null && !grabLostCanvas.activeSelf))
            {
                // If the grab lost check coroutine is not already running, start it.
                // This prevents multiple instances of the coroutine from running.
                if (grabLostCheckCoroutine == null)
                {
                    grabLostCheckCoroutine = StartCoroutine(CheckForGrabLost());
                }
            }
        }

        /// <summary>
        /// Coroutine to check for a lost grab after a short delay.
        /// If the grab is still lost after the delay, the 'grabLostCanvas' is shown.
        /// </summary>
        private IEnumerator CheckForGrabLost()
        {
            // Wait for the specified interval before potentially showing the "grab lost" message.
            yield return new WaitForSeconds(grabLostCheckInterval);

            // If the coroutine hasn't been stopped by 'GrabDetected()' in the meantime,
            // and the 'grabLostCanvas' is assigned and not already active, show it.
            if (grabLostCanvas != null && !grabLostCanvas.activeSelf)
            {
                grabLostCanvas.SetActive(true);
                Debug.Log("GrabbingDetection: Grab Lost! Showing Grab Lost Canvas.", this);
            }
            // The coroutine finishes here. It will only be restarted if 'NoGrabDetected()' is called again.
        }

        /// <summary>
        /// Coroutine to display the 'grabSuccessCanvas' for a duration, then hide it,
        /// and finally trigger the main logic for switching to the next set of UI/GameObjects.
        /// </summary>
        /// <param name="canvasToShow">The GameObject representing the 'grabSuccessCanvas'.</param>
        /// <param name="duration">How long the 'grabSuccessCanvas' should be displayed.</param>
        private IEnumerator ShowGrabSuccessCanvasAndThenSwitch(GameObject canvasToShow, float duration)
        {
            canvasToShow.SetActive(true); // Show the 'grabSuccessCanvas'
            yield return new WaitForSeconds(duration); // Wait for its specified duration
            canvasToShow.SetActive(false); // Hide the 'grabSuccessCanvas'

            // Stop any ongoing grab lost check coroutine, as the successful sequence is complete.
            if (grabLostCheckCoroutine != null)
            {
                StopCoroutine(grabLostCheckCoroutine);
                grabLostCheckCoroutine = null;
            }

            // Now that the 'grabSuccessCanvas' has hidden, trigger the next canvas switch logic.
            ExecuteNextCanvasSwitchLogic();

            // 'hasGrabBeenDetected' remains true to prevent re-triggering the initial flow,
            // assuming this is a one-time transition to a new state.
        }

        /// <summary>
        /// Contains the core logic for switching to the next canvas and managing GameObjects/TextMeshPro elements.
        /// This method is called AFTER the 'grabSuccessCanvas' has been displayed and hidden.
        /// </summary>
        private void ExecuteNextCanvasSwitchLogic()
        {
            Debug.Log("GrabbingDetection: Triggering Next Canvas Switch Logic.", this);

            // --- Show Next Canvas ---
            if (nextCanvas != null)
            {
                Debug.Log($"GrabbingDetection: Showing Next Canvas: {nextCanvas.name}", this);
                nextCanvas.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("GrabbingDetection: No Next Canvas assigned to show after grab detected.", this);
            }

            // --- Hide Additional Canvases ---
            if (canvasesToHideOnNextSwitch != null)
            {
                foreach (Canvas canvasToHide in canvasesToHideOnNextSwitch)
                {
                    if (canvasToHide != null)
                    {
                        Debug.Log($"GrabbingDetection: Hiding additional Canvas: {canvasToHide.name} during next switch.", this);
                        canvasToHide.gameObject.SetActive(false);
                    }
                }
            }

            // --- Control GameObjects ---
            // Activate specified GameObjects
            if (gameObjectsToActivateOnNextSwitch != null)
            {
                foreach (GameObject obj in gameObjectsToActivateOnNextSwitch)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"GrabbingDetection: Activated GameObject '{obj.name}'.", this);
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
                        Debug.Log($"GrabbingDetection: Deactivated GameObject '{obj.name}'.", this);
                    }
                }
            }

            // --- Control TextMeshPro Text ---
            // Activate specified TextMeshPro elements
            if (textMeshProElementsToActivateOnNextSwitch != null)
            {
                foreach (TextMeshProUGUI tmpElement in textMeshProElementsToActivateOnNextSwitch)
                {
                    if (tmpElement != null)
                    {
                        tmpElement.gameObject.SetActive(true);
                        Debug.Log($"GrabbingDetection: Activated TextMeshPro element '{tmpElement.name}'.", this);
                    }
                }
            }

            // Deactivate specified TextMeshPro elements
            if (textMeshProElementsToDeactivateOnNextSwitch != null)
            {
                foreach (TextMeshProUGUI tmpElement in textMeshProElementsToDeactivateOnNextSwitch)
                {
                    if (tmpElement != null)
                    {
                        tmpElement.gameObject.SetActive(false);
                        Debug.Log($"GrabbingDetection: Deactivated TextMeshPro element '{tmpElement.name}'.", this);
                    }
                }
            }

            Debug.Log("GrabbingDetection: Next canvas switch logic execution complete.", this);
        }
    }
}
