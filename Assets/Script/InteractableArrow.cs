using UnityEngine;
using Leap;
using System;

public class InteractableArrow : MonoBehaviour
{
    public SwipeDetector.SwipeDirection requiredSwipeDirection = SwipeDetector.SwipeDirection.Right;
    public float interactionDistance = 0.05f; // How close hand needs to be to "touch" the arrow (in meters)
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color successColor = Color.green;

    public Chirality expectedHand = Chirality.Right; // Specify which hand this arrow expects

    public bool showInteractionGizmo = true;
    public float handSphereRadius = 0.01f;

    private Renderer arrowRenderer;
    private SwipeDetector swipeDetector;
    private bool isHandTouching = false;
    private Vector3 currentHandTipPosition; // To store the last known hand position for gizmo

    public event Action<SwipeDetector.SwipeDirection> OnArrowSwiped;

    void Start()
    {
        arrowRenderer = GetComponent<Renderer>();
        if (arrowRenderer == null)
        {
            Debug.LogError("InteractableArrow needs a Renderer component (e.g., MeshRenderer) on its GameObject!");
            enabled = false;
            return;
        }
        arrowRenderer.material.color = normalColor;

        swipeDetector = FindObjectOfType<SwipeDetector>();
        if (swipeDetector == null)
        {
            Debug.LogError("SwipeDetector not found in the scene! Make sure it's present and enabled.");
            enabled = false;
            return;
        }

        swipeDetector.onSwipeDetectedDetailed += HandleGlobalSwipeDetected;
    }

    void OnDestroy()
    {
        if (swipeDetector != null)
        {
            swipeDetector.onSwipeDetectedDetailed -= HandleGlobalSwipeDetected;
        }
    }

    void Update()
    {
        CheckHandTouching(); // Now checks for the *expected* hand
        
        if (isHandTouching)
        {
            arrowRenderer.material.color = highlightColor;
        }
        else
        {
            arrowRenderer.material.color = normalColor;
        }
    }

    private void CheckHandTouching()
    {
        if (swipeDetector.leapProvider == null) return;

        Frame frame = swipeDetector.leapProvider.CurrentFrame;
        
        Hand hand = frame.GetHand(expectedHand); 

        // --- MODIFIED HAND VALIDITY CHECK ---
        if (hand != null && hand.Index != null) // Check if the hand object exists AND if its Index finger is tracked
        {
            currentHandTipPosition = new Vector3(hand.Index.TipPosition.x, hand.Index.TipPosition.y, hand.Index.TipPosition.z);
            
            float distance = Vector3.Distance(currentHandTipPosition, transform.position); // Assuming transform.position is visual center
            isHandTouching = (distance < interactionDistance);

            // Debug logs for touch detection
            // if (isHandTouching) { Debug.Log($"{expectedHand} Hand touching {gameObject.name}! Distance: {distance}"); }
            // else { Debug.Log($"{expectedHand} Hand NOT touching {gameObject.name}. Distance: {distance}"); }

        }
        else
        {
            isHandTouching = false;
            currentHandTipPosition = Vector3.zero;
        }
    }

    private void HandleGlobalSwipeDetected(SwipeDetector.SwipeDirection direction, Hand swipingHand, Vector3 startPos, Vector3 endPos)
    {
        // Check if the swiping hand matches the expected hand for this arrow
        if ((expectedHand == Chirality.Left && !swipingHand.IsLeft) ||
            (expectedHand == Chirality.Right && !swipingHand.IsRight))
        {
            return; // This swipe was by the wrong hand for this arrow
        }

        Vector3 interactionCenter = transform.position; // Assuming transform.position is visual center

        bool swipeStartedNearArrow = Vector3.Distance(startPos, interactionCenter) < interactionDistance;
        bool swipeEndedNearArrow = Vector3.Distance(endPos, interactionCenter) < interactionDistance;

        if ((swipeStartedNearArrow || swipeEndedNearArrow) && direction == requiredSwipeDirection)
        {
            Debug.Log($"SUCCESS! {expectedHand} Hand swiped {gameObject.name} correctly in direction: {direction}");
            arrowRenderer.material.color = successColor;
            OnArrowSwiped?.Invoke(direction);
            Invoke("ResetArrowColor", 1.0f);
        }
    }

    private void ResetArrowColor()
    {
        arrowRenderer.material.color = normalColor;
    }

    void OnDrawGizmos()
    {
        if (showInteractionGizmo)
        {
            Vector3 gizmoCenter = transform.position; // Assuming transform.position is visual center
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(gizmoCenter, interactionDistance);

            if (currentHandTipPosition != Vector3.zero) // If hand was detected
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(currentHandTipPosition, handSphereRadius);
                Gizmos.DrawLine(gizmoCenter, currentHandTipPosition);
            }
        }
    }
}