using UnityEngine;
using Leap;
using System.Collections.Generic; // For Dictionary

public class SwipeDetector : MonoBehaviour
{
    public LeapProvider leapProvider; // Assign in Inspector

    private Dictionary<int, HandSwipeState> _handSwipeStates = new Dictionary<int, HandSwipeState>();

    private class HandSwipeState
    {
        public bool IsSwiping = false;
        public Vector3 StartSwipePosition;
        public float StartTime;
    }

    private float _swipeThreshold = 0.05f; // Min distance for a swipe in meters
    private float _swipeTimeThreshold = 0.5f; // Max time for a swipe in seconds

    private float _horizontalSwipeThreshold = 0.7f;
    private float _verticalSwipeThreshold = 0.7f;

    private float _swipeCooldownTime = 0.3f;
    private float _lastSwipeTime = -Mathf.Infinity;

    public enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down,
        Forward,
        Backward
    }

    public delegate void OnSwipeDetectedDetailed(SwipeDirection direction, Hand swipingHand, Vector3 startPos, Vector3 endPos);
    public event OnSwipeDetectedDetailed onSwipeDetectedDetailed;

    void Start()
    {
        if (leapProvider == null)
        {
            leapProvider = FindObjectOfType<LeapProvider>();
            if (leapProvider == null)
            {
                Debug.LogError("LeapProvider not found in the scene! Please assign it or ensure one exists.");
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        if (leapProvider == null || Time.time < _lastSwipeTime + _swipeCooldownTime) return;

        Frame frame = leapProvider.CurrentFrame;

        // --- MODIFIED HAND CLEANUP LOGIC ---
        // Get the IDs of hands currently tracked in the frame
        HashSet<int> currentFrameHandIds = new HashSet<int>();
        foreach (Hand handInFrame in frame.Hands)
        {
            currentFrameHandIds.Add(handInFrame.Id);
        }

        // Clean up states for hands that are no longer in the current frame
        List<int> handsToRemove = new List<int>();
        foreach (int handId in _handSwipeStates.Keys)
        {
            if (!currentFrameHandIds.Contains(handId)) // If the hand ID from our dictionary is NOT in the current frame
            {
                handsToRemove.Add(handId);
            }
        }
        foreach (int handId in handsToRemove)
        {
            _handSwipeStates.Remove(handId);
        }
        // --- END MODIFIED HAND CLEANUP LOGIC ---


        // Process currently tracked hands
        foreach (Hand hand in frame.Hands)
        {
            ProcessHandSwipe(hand);
        }
    }

    private void ProcessHandSwipe(Hand hand)
    {
        // Get or create the swipe state for this specific hand
        if (!_handSwipeStates.TryGetValue(hand.Id, out HandSwipeState state))
        {
            state = new HandSwipeState();
            _handSwipeStates.Add(hand.Id, state);
        }

        // It's good practice to ensure the finger tip is also valid before using its position
        // This check implicitly relies on the Hand object itself being valid if its bones are valid.
        if (hand.Index != null) // Check if the index finger exists (it usually will for a tracked hand)
        {
            Vector3 currentHandPosition = new Vector3(hand.Index.TipPosition.x, hand.Index.TipPosition.y, hand.Index.TipPosition.z);

            if (state.IsSwiping)
            {
                Vector3 swipeDisplacement = currentHandPosition - state.StartSwipePosition;

                if (swipeDisplacement.magnitude > _swipeThreshold)
                {
                    DetectSwipeDirection(swipeDisplacement, hand, state.StartSwipePosition, currentHandPosition);
                    state.IsSwiping = false;
                    _lastSwipeTime = Time.time; // Apply global cooldown after any successful swipe
                }
                else if (Time.time - state.StartTime > _swipeTimeThreshold)
                {
                    state.IsSwiping = false; // Reset if too slow or small for this hand
                }
            }
            else
            {
                state.IsSwiping = true;
                state.StartTime = Time.time;
                state.StartSwipePosition = currentHandPosition;
            }
        }
        else
        {
            // If for some reason the index finger isn't valid, stop swiping for this hand
            state.IsSwiping = false;
        }
    }

    private void DetectSwipeDirection(Vector3 swipeDisplacement, Hand swipingHand, Vector3 startPos, Vector3 endPos)
    {
        SwipeDirection detectedDirection = SwipeDirection.None;

        Vector3 normalizedDisplacement = swipeDisplacement.normalized;

        float absX = Mathf.Abs(normalizedDisplacement.x);
        float absY = Mathf.Abs(normalizedDisplacement.y);
        float absZ = Mathf.Abs(normalizedDisplacement.z);

        if (absX > absY && absX > absZ)
        {
            if (absX > _horizontalSwipeThreshold)
            {
                detectedDirection = (normalizedDisplacement.x > 0) ? SwipeDirection.Right : SwipeDirection.Left;
            }
        }
        else if (absY > absX && absY > absZ)
        {
            if (absY > _verticalSwipeThreshold)
            {
                detectedDirection = (normalizedDisplacement.y > 0) ? SwipeDirection.Up : SwipeDirection.Down;
            }
        }
        else if (absZ > absX && absZ > absY)
        {
            if (absZ > 0.5f)
            {
                detectedDirection = (normalizedDisplacement.z > 0) ? SwipeDirection.Forward : SwipeDirection.Backward;
            }
        }

        if (detectedDirection != SwipeDirection.None)
        {
            onSwipeDetectedDetailed?.Invoke(detectedDirection, swipingHand, startPos, endPos);
        }
    }
}