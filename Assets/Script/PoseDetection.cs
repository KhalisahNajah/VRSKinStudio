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


        public void PoseDetected(string inputString)
        {
            // Only proceed if the correct pose hasn't been detected yet
            if (hasCorrectPoseBeenDetected)
            {
                return; // Exit the method if already detected
            }

            foreach (var pose in poseList)
            {
                if (string.Equals(inputString, pose.poseName, StringComparison.OrdinalIgnoreCase))
                {
                    // Set the flag to true as the correct pose is now detected
                    hasCorrectPoseBeenDetected = true;

                    if (pose.poseViewer != null)
                    {
                        Destroy(pose.poseViewer); // Remove the pose viewer
                    }

                    if (initialCanvas != null)
                        initialCanvas.SetActive(false);

                    // Start the coroutine to display correctCanvas, then trigger the next canvas switch
                    if (correctCanvas != null)
                    {
                        StartCoroutine(ShowCorrectCanvasAndThenSwitch(correctCanvas, correctCanvasDisplayDuration));
                    }
                    else
                    {
                        // If there's no correctCanvas to show, immediately trigger the next switch
                        ExecuteNextCanvasSwitchLogic();
                    }

                    break; // Exit the loop once the pose is found and processed
                }
            }
        }

        private IEnumerator ShowCorrectCanvasAndThenSwitch(GameObject canvasToShow, float duration)
        {
            canvasToShow.SetActive(true); // Show the 'correctCanvas'
            yield return new WaitForSeconds(duration); // Wait for its specified duration
            canvasToShow.SetActive(false); // Hide the 'correctCanvas'

            // Now that the 'correctCanvas' has hidden, trigger the next canvas switch logic
            ExecuteNextCanvasSwitchLogic();
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