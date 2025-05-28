using UnityEngine;
using Leap.Examples; // Make sure this namespace matches where PoseDetection is
using System.Collections.Generic; // For list of detectors if needed

public class HandTrackerMonitor : MonoBehaviour
{
    public PoseDetection poseDetectionScript; // Drag your PoseDetection GameObject here in Inspector

    // Assuming you have a way to know the currently detected pose
    // This could be from a Leap.Unity.PoseDetector component, a custom gesture recognizer, etc.
    // For demonstration, let's mock it or assume a simple check.

    // A simple public property to set the currently detected pose name from your actual pose detection
    // Your actual pose detection logic would update this.
    public string currentlyDetectedPose = "";

    // You might have a list of actual PoseDetector components
    // public List<Leap.Unity.PoseDetector> activePoseDetectors;

    void Update()
    {
        // This 'Update' needs to be driven by your actual pose detection system.
        // It's crucial that 'currentlyDetectedPose' is updated by your Leap Motion integration
        // with the name of the pose currently being held, or an empty string if no pose.

        // --- Simplified Example (YOU NEED TO REPLACE THIS WITH YOUR REAL POSE DETECTION LOGIC) ---
        // For example, if you have multiple Leap.Unity.PoseDetector components, you might check them.
        // Or if you're getting a string from a custom gesture system.

        // Example: If a pose detection system sets currentlyDetectedPose:
        if (!string.IsNullOrEmpty(currentlyDetectedPose))
        {
            // If a pose is detected, tell PoseDetection about it.
            // PoseDetection will then decide if it's the expected one or if the lost canvas needs hiding.
            poseDetectionScript.PoseDetected(currentlyDetectedPose);
        }
        else
        {
            // If no pose is detected by your system, signal PoseDetection.
            poseDetectionScript.NoPoseDetected();
        }
        // --- END OF SIMPLIFIED EXAMPLE ---

        // A more robust example using actual Leap.Unity.PoseDetector if you have them:
        // You would typically have a single PoseDetector for the *target* pose,
        // and it would trigger events.
        // If your system just tells you what *is* detected:
        // if (myLeapPoseSystem.IsPoseActive("MyTargetPoseName"))
        // {
        //     poseDetectionScript.PoseDetected("MyTargetPoseName");
        // }
        // else
        // {
        //     poseDetectionScript.NoPoseDetected();
        // }
    }

    // Example public method that your actual pose detection system would call
    // when it finds a pose.
    public void SetDetectedPose(string poseName)
    {
        currentlyDetectedPose = poseName;
    }

    // Example public method that your actual pose detection system would call
    // when no relevant pose is found.
    public void ClearDetectedPose()
    {
        currentlyDetectedPose = "";
    }
}