using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Required for TextMeshPro

namespace Leap.Examples
{
    public class PoseDetection : MonoBehaviour
    {
        [Serializable]
        struct ShowCasePose
        {
            public string poseName;
            public GameObject poseViewer;
        }

        [Header("Pose Detection Settings")]
        [SerializeField]
        private List<ShowCasePose> poseList = new List<ShowCasePose>();

        [SerializeField]
        private GameObject initialCanvas; // The canvas shown before any pose is detected

        [SerializeField]
        private GameObject correctCanvas; // The canvas that briefly appears when a correct pose is found

        [SerializeField]
        private float correctCanvasDisplayDuration = 1f; // Duration for how long the 'correctCanvas' stays active

        private bool hasCorrectPoseBeenDetected = false; // Flag to track if the correct pose has been detected

        // --- NEW: Pose Lost Settings ---
        [Header("Pose Lost Settings")]
        [SerializeField]
        private GameObject poseLostCanvas; // The canvas that appears when the correct pose is lost

        [SerializeField]
        private float poseLostCheckInterval = 0.5f; // How often to check if the pose is lost
        private string currentExpectedPoseName = ""; // Stores the name of the pose we are currently looking for
        private Coroutine poseLostCheckCoroutine; // Reference to the pose lost coroutine

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


        private void Start()
        {
            // Ensure correctCanvas is initially off
            if (correctCanvas != null)
                correctCanvas.SetActive(false);

            // Ensure nextCanvas is initially off
            if (nextCanvas != null)
                nextCanvas.gameObject.SetActive(false);

            // Ensure poseLostCanvas is initially off
            if (poseLostCanvas != null)
                poseLostCanvas.SetActive(false);

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
        /// This method is called when a pose is detected.
        /// </summary>
        /// <param name="inputString">The name of the detected pose.</param>
        public void PoseDetected(string inputString)
        {
            // If the correct pose has already been detected and the next scene is triggered,
            // we don't want to reactivate the initial flow or pose lost logic.
            if (hasCorrectPoseBeenDetected && !string.IsNullOrEmpty(currentExpectedPoseName))
            {
                // If the detected pose is the one we are expecting, hide the pose lost canvas
                if (string.Equals(inputString, currentExpectedPoseName, StringComparison.OrdinalIgnoreCase))
                {
                    if (poseLostCanvas != null && poseLostCanvas.activeSelf)
                    {
                        poseLostCanvas.SetActive(false);
                        Debug.Log($"PoseDetection: Re-detected correct pose '{inputString}'. Hiding Pose Lost Canvas.", this);
                        // Stop the pose lost check coroutine when the pose is found again
                        if (poseLostCheckCoroutine != null)
                        {
                            StopCoroutine(poseLostCheckCoroutine);
                            poseLostCheckCoroutine = null;
                        }
                    }
                }
                return; // Exit if the final correct pose has been detected and we are waiting for user to return to it
            }

            foreach (var pose in poseList)
            {
                if (string.Equals(inputString, pose.poseName, StringComparison.OrdinalIgnoreCase))
                {
                    // This is the initial correct pose detection
                    if (!hasCorrectPoseBeenDetected)
                    {
                        hasCorrectPoseBeenDetected = true; // Set the flag to true as the correct pose is now detected
                        currentExpectedPoseName = pose.poseName; // Store the name of the pose that triggered the correct sequence

                        if (pose.poseViewer != null)
                        {
                            Destroy(pose.poseViewer); // Remove the pose viewer
                            Debug.Log($"PoseDetection: Destroyed Pose Viewer for '{pose.poseName}'.", this);
                        }

                        if (initialCanvas != null)
                        {
                            initialCanvas.SetActive(false);
                            Debug.Log("PoseDetection: Hiding Initial Canvas.", this);
                        }

                        // Start the coroutine to display correctCanvas, then trigger the next canvas switch
                        if (correctCanvas != null)
                        {
                            Debug.Log("PoseDetection: Showing Correct Canvas.", this);
                            StartCoroutine(ShowCorrectCanvasAndThenSwitch(correctCanvas, correctCanvasDisplayDuration));
                        }
                        else
                        {
                            // If there's no correctCanvas to show, immediately trigger the next switch
                            Debug.LogWarning("PoseDetection: No Correct Canvas assigned. Immediately triggering next switch.", this);
                            ExecuteNextCanvasSwitchLogic();
                        }
                    }
                    // This part handles the case where the pose was lost and now re-detected correctly
                    else if (string.Equals(inputString, currentExpectedPoseName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (poseLostCanvas != null && poseLostCanvas.activeSelf)
                        {
                            poseLostCanvas.SetActive(false);
                            Debug.Log($"PoseDetection: Re-detected correct pose '{inputString}'. Hiding Pose Lost Canvas.", this);
                            // Stop the pose lost check coroutine when the pose is found again
                            if (poseLostCheckCoroutine != null)
                            {
                                StopCoroutine(poseLostCheckCoroutine);
                                poseLostCheckCoroutine = null;
                            }
                        }
                    }
                    break; // Exit the loop once the pose is found and processed
                }
            }
        }

        /// <summary>
        /// This method should be called when NO expected pose is detected.
        /// It's typically triggered by an external pose detection system.
        /// </summary>
        public void NoPoseDetected()
        {
            // Only show pose lost canvas if the correct pose was previously detected
            // and we are currently waiting for that specific pose, AND the pose lost canvas is not already active.
            if (hasCorrectPoseBeenDetected && !string.IsNullOrEmpty(currentExpectedPoseName) && (poseLostCanvas != null && !poseLostCanvas.activeSelf))
            {
                if (poseLostCheckCoroutine == null)
                {
                    poseLostCheckCoroutine = StartCoroutine(CheckForPoseLost());
                }
            }
        }

        private IEnumerator CheckForPoseLost()
        {
            while (true)
            {
                // This coroutine will repeatedly check if the currentExpectedPoseName is actively being held.
                // You will need to integrate this with your actual pose detection mechanism.
                // For demonstration, let's assume if PoseDetected is NOT called for currentExpectedPoseName, it's lost.
                // A better approach would be for your pose detection system to explicitly call NoPoseDetected
                // or provide a continuously updated "current detected pose" string.

                // For now, if currentExpectedPoseName is set and the 'PoseDetected' method hasn't confirmed it recently,
                // we'll assume it's lost. This requires your external pose detection to constantly report its state.

                // A simpler, more direct way is if your external pose detection has an "OnPoseLost" event or method.
                // If it doesn't, you might need a separate mechanism to track the last detected pose and a timer.

                // For the purpose of this script, let's assume 'NoPoseDetected()' is called by your
                // pose tracking system when no _specific_ pose (or the expected one) is being held.

                // If the poseLostCanvas is not active and we're expecting a pose, show it.
                if (poseLostCanvas != null && !poseLostCanvas.activeSelf)
                {
                    poseLostCanvas.SetActive(true);
                    Debug.Log("PoseDetection: Pose Lost! Showing Pose Lost Canvas.", this);
                }
                yield return new WaitForSeconds(poseLostCheckInterval);
            }
        }


        private IEnumerator ShowCorrectCanvasAndThenSwitch(GameObject canvasToShow, float duration)
        {
            canvasToShow.SetActive(true); // Show the 'correctCanvas'
            yield return new WaitForSeconds(duration); // Wait for its specified duration
            canvasToShow.SetActive(false); // Hide the 'correctCanvas'

            // Stop any ongoing pose lost check coroutine once the final correct sequence is done
            if (poseLostCheckCoroutine != null)
            {
                StopCoroutine(poseLostCheckCoroutine);
                poseLostCheckCoroutine = null;
            }
            // Now that the 'correctCanvas' has hidden, trigger the next canvas switch logic
            ExecuteNextCanvasSwitchLogic();

            // After the final switch, reset hasCorrectPoseBeenDetected and currentExpectedPoseName
            // if you intend for the entire process to be repeatable or for other poses to be detected.
            // If this is a one-time sequence leading to a new state, keep them as is.
            // For this scenario, we'll keep `hasCorrectPoseBeenDetected` as true to prevent re-triggering the initial flow.
        }

        /// <summary>
        /// Contains the core logic for switching to the next canvas and managing GameObjects/TextMeshPro elements.
        /// This is called AFTER the 'correctCanvas' has been displayed and hidden.
        /// </summary>
        private void ExecuteNextCanvasSwitchLogic()
        {
            Debug.Log("PoseDetection: Triggering Next Canvas Switch Logic.", this);

            // --- Show Next Canvas ---
            if (nextCanvas != null)
            {
                Debug.Log($"PoseDetection: Showing Next Canvas: {nextCanvas.name}", this);
                nextCanvas.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("PoseDetection: No Next Canvas assigned to show after correct pose detected.", this);
            }

            // --- Hide Additional Canvases ---
            if (canvasesToHideOnNextSwitch != null)
            {
                foreach (Canvas canvasToHide in canvasesToHideOnNextSwitch)
                {
                    if (canvasToHide != null)
                    {
                        Debug.Log($"PoseDetection: Hiding additional Canvas: {canvasToHide.name} during next switch.", this);
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
                        Debug.Log($"PoseDetection: Activated GameObject '{obj.name}'.", this);
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
                        Debug.Log($"PoseDetection: Deactivated GameObject '{obj.name}'.", this);
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
                        Debug.Log($"PoseDetection: Activated TextMeshPro element '{tmpElement.name}'.", this);
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
                        Debug.Log($"PoseDetection: Deactivated TextMeshPro element '{tmpElement.name}'.", this);
                    }
                }
            }

            Debug.Log("PoseDetection: Next canvas switch logic execution complete.", this);
        }
    }
}