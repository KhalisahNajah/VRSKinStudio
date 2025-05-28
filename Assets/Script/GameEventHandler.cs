using UnityEngine;

public class GameEventHandler : MonoBehaviour
{
    // Reference to your SwipeDetector script.
    // Assign this in the Inspector.
    public SwipeDetector swipeDetector; 

    void Start()
    {
        if (swipeDetector != null)
        {
            // Subscribe to the OnSwipeDetected event
           
            Debug.Log("GameEventHandler subscribed to SwipeDetector events.");
        }
        else
        {
            Debug.LogError("SwipeDetector reference not set in GameEventHandler! Please assign it in the Inspector.");
        }
    }

    void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks or errors when objects are destroyed
        if (swipeDetector != null)
        {
           
            Debug.Log("GameEventHandler unsubscribed from SwipeDetector events.");
        }
    }

    // This method will be called when a swipe is detected
    private void HandleSwipe(SwipeDetector.SwipeDirection direction)
    {
        Debug.Log($"GameEventHandler received swipe: {direction}");

        // --- YOUR GAME LOGIC HERE ---
        switch (direction)
        {
            case SwipeDetector.SwipeDirection.Left:
                Debug.Log("Player swiped left! Move character or change menu.");
                // Example: GetComponent<CharacterController>().Move(Vector3.left * 5f);
                break;
            case SwipeDetector.SwipeDirection.Right:
                Debug.Log("Player swiped right! Next item or turn right.");
                break;
            case SwipeDetector.SwipeDirection.Up:
                Debug.Log("Player swiped up! Jump or scroll up.");
                break;
            case SwipeDetector.SwipeDirection.Down:
                Debug.Log("Player swiped down! Duck or scroll down.");
                break;
            case SwipeDetector.SwipeDirection.None:
                // Should not happen if event is only invoked for actual swipes
                break;
        }
    }
}