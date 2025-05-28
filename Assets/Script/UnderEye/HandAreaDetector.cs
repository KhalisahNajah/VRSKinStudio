using UnityEngine;

public class HandAreaDetector : MonoBehaviour
{
    [SerializeField] private Transform detectionCenter; // Assign "UnderEyeArea" GameObject here
    [SerializeField] private Vector3 detectionBoxSize = new Vector3(0.1f, 0.05f, 0.1f); // Size in meters

    // Example Leap hand (palm or finger) reference
    public Transform handReference; // Assign the hand or fingertip transform

    void Update()
    {
        if (IsInDetectionBox(handReference.position))
        {
            Debug.Log("Hand is in the under-eye detection zone!");
            // You can trigger swipe detection logic here
        }
    }

    private bool IsInDetectionBox(Vector3 handPosition)
    {
        Vector3 localPos = handPosition - detectionCenter.position;

        return Mathf.Abs(localPos.x) <= detectionBoxSize.x / 2 &&
               Mathf.Abs(localPos.y) <= detectionBoxSize.y / 2 &&
               Mathf.Abs(localPos.z) <= detectionBoxSize.z / 2;
    }

    void OnDrawGizmos()
{
    if (detectionCenter == null) return;
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireCube(detectionCenter.position, detectionBoxSize);
}

}
