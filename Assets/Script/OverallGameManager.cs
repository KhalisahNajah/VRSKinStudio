using UnityEngine;
using UnityEngine.UI; // Required for UI elements
using System.Collections.Generic; // Required for List
using TMPro; // Required for TextMeshPro
using Leap; // Required for Chirality (if Leap Motion is used)

public class OverallGameManager : MonoBehaviour
{
    // Assign your InteractableArrow GameObjects here in the Inspector
    public List<InteractableArrow> interactableArrows;

    // UI Elements - Assign these in the Inspector
    public TMP_Text countdownText;
    public GameObject donePanel; // A GameObject (e.g., Panel) that holds the "We're Done!" text

    public int totalSwipesRequired = 3; // How many successful two-hand swipes are needed
    private int currentSwipesCompleted = 0;

    // New variables to track individual hand swipes for a combined swipe
    private bool leftHandSwipedThisRound = false;
    private bool rightHandSwipedThisRound = false;

    void Start()
    {
        // Initial setup for UI
        if (countdownText == null)
        {
            Debug.LogError("Countdown Text UI not assigned in OverallGameManager!");
            enabled = false;
            return;
        }
        if (donePanel == null)
        {
            Debug.LogError("Done Panel UI not assigned in OverallGameManager!");
            enabled = false;
            return;
        }

        // Hide the done panel initially
        donePanel.SetActive(false);

        // Subscribe to each arrow's OnArrowSwiped event
        if (interactableArrows != null)
        {
            foreach (InteractableArrow arrow in interactableArrows)
            {
                if (arrow != null)
                {
                    // *** CRUCIAL CHANGE HERE: Subscribe to the new event signature ***
                    arrow.OnArrowSwiped += HandleSpecificArrowSwipe;
                }
            }
        }
        else
        {
            Debug.LogError("No InteractableArrows assigned in OverallGameManager!");
        }

        UpdateCountdownUI(); // Display initial countdown
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (interactableArrows != null)
        {
            foreach (InteractableArrow arrow in interactableArrows)
            {
                if (arrow != null)
                {
                    // *** CRUCIAL CHANGE HERE: Unsubscribe from the new event signature ***
                    arrow.OnArrowSwiped -= HandleSpecificArrowSwipe;
                }
            }
        }
    }

    // *** CRUCIAL CHANGE HERE: Method signature now matches the event's signature ***
    private void HandleSpecificArrowSwipe(InteractableArrow swipedArrow, SwipeDetector.SwipeDirection direction)
    {
        // Only count if we haven't already completed all two-hand swipes
        if (currentSwipesCompleted < totalSwipesRequired)
        {
            // Use swipedArrow.expectedHand to determine which hand swiped
            if (swipedArrow.expectedHand == Chirality.Left)
            {
                leftHandSwipedThisRound = true;
                Debug.Log("Left hand swiped!");
            }
            else if (swipedArrow.expectedHand == Chirality.Right)
            {
                rightHandSwipedThisRound = true;
                Debug.Log("Right hand swiped!");
            }

            // Check if both hands have swiped in this "round"
            if (leftHandSwipedThisRound && rightHandSwipedThisRound)
            {
                currentSwipesCompleted++;
                Debug.Log($"OverallGameManager: Two-hand swipes completed: {currentSwipesCompleted} / {totalSwipesRequired}");
                UpdateCountdownUI();

                // Reset for the next two-hand swipe
                leftHandSwipedThisRound = false;
                rightHandSwipedThisRound = false;

                if (currentSwipesCompleted >= totalSwipesRequired)
                {
                    Debug.Log("Task Completed! All two-hand swipes done!");
                    ShowDonePanel();
                    // Optionally, disable further input or reset scene here
                    DisableAllArrows(); // Stop arrows from being interacted with
                }
            }
        }
    }

    private void UpdateCountdownUI()
    {
        countdownText.text = $"Swipes Left: {totalSwipesRequired - currentSwipesCompleted}";
    }

    private void ShowDonePanel()
    {
        donePanel.SetActive(true);
    }

    private void DisableAllArrows()
    {
        if (interactableArrows != null)
        {
            foreach (InteractableArrow arrow in interactableArrows)
            {
                if (arrow != null)
                {
                    arrow.enabled = false; // Disable the script
                    // Optionally reset their color or hide them
                    Renderer arrowRenderer = arrow.GetComponent<Renderer>();
                    if (arrowRenderer != null)
                    {
                        arrowRenderer.material.color = arrow.normalColor; // Reset to normal if you want
                    }
                }
            }
        }
    }

    // Optional: A way to reset the game
    public void ResetGame()
    {
        currentSwipesCompleted = 0;
        leftHandSwipedThisRound = false; // Reset these flags on game reset
        rightHandSwipedThisRound = false; // Reset these flags on game reset
        UpdateCountdownUI();
        donePanel.SetActive(false);
        // Re-enable arrows
        if (interactableArrows != null)
        {
            foreach (InteractableArrow arrow in interactableArrows)
            {
                if (arrow != null)
                {
                    arrow.enabled = true;
                    Renderer arrowRenderer = arrow.GetComponent<Renderer>();
                    if (arrowRenderer != null)
                    {
                        arrowRenderer.material.color = arrow.normalColor;
                    }
                }
            }
        }
        Debug.Log("Game Reset!");
    }
}