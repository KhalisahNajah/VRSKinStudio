using UnityEngine;
using System.Collections;
using TMPro; // Required for TextMeshPro

/// <summary>
/// Manages switching from a current canvas to a next canvas.
/// Can be timed or manually triggered.
/// Can also activate/deactivate multiple GameObjects, TextMeshProUGUI elements,
/// and hide additional specified canvases during the switch.
/// </summary>
public class TimedCanvasSwitcher : MonoBehaviour
{
    [Header("Canvas Settings")]
    [Tooltip("The canvas that is initially active. It will be hidden when the switch occurs.")]
    public Canvas currentCanvas;

    [Tooltip("The canvas that will be activated when the switch occurs. Can be null if no next canvas.")]
    public Canvas nextCanvas;

    [Tooltip("If true, the switch will occur automatically after 'displayDuration'. If false, call ManualSwitch() to trigger.")]
    public bool timedSwitch = true;

    [Tooltip("How long (in seconds) the currentCanvas will be displayed before an automatic switch (if timedSwitch is true).")]
    public float displayDuration = 3.0f;

    [Tooltip("Additional canvases that will be hidden when the switch occurs.")]
    public Canvas[] canvasesToHideOnSwitch;

    [Header("GameObject Control on Switch (Optional)")]
    [Tooltip("GameObjects in this list will be activated when the switch occurs.")]
    public GameObject[] gameObjectsToActivate;

    [Tooltip("GameObjects in this list will be deactivated when the switch occurs.")]
    public GameObject[] gameObjectsToDeactivate;

    [Header("TextMeshPro Text Control on Switch (Optional)")]
    [Tooltip("TextMeshPro UGUI elements in this list will be activated (made visible) when the switch occurs.")]
    public TextMeshProUGUI[] textMeshProElementsToActivate;

    [Tooltip("TextMeshPro UGUI elements in this list will be deactivated (made hidden) when the switch occurs.")]
    public TextMeshProUGUI[] textMeshProElementsToDeactivate;

    void Start()
    {
        // Initial setup: Ensure currentCanvas is active and nextCanvas is inactive (if they exist)
        if (currentCanvas != null)
        {
            currentCanvas.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("TimedCanvasSwitcher: Current Canvas is not assigned. The script might not work as expected.", this);
        }

        if (nextCanvas != null)
        {
            nextCanvas.gameObject.SetActive(false);
        }

        // If timedSwitch is enabled, start the coroutine for automatic switching.
        if (timedSwitch)
        {
            StartCoroutine(TimedSwitchRoutine());
        }
    }

    /// <summary>
    /// Coroutine for the timed switch. Waits for the display duration then executes the switch.
    /// </summary>
    private IEnumerator TimedSwitchRoutine()
    {
        // Wait for the specified duration only if timedSwitch is true (which it is if this coroutine is running)
        yield return new WaitForSeconds(displayDuration);
        ExecuteSwitchLogic();
    }

    /// <summary>
    /// Public method to manually trigger the canvas switch and associated actions.
    /// Useful if timedSwitch is set to false, or to trigger the switch earlier.
    /// </summary>
    public void ManualSwitch()
    {
        Debug.Log("TimedCanvasSwitcher: ManualSwitch called.", this);
        ExecuteSwitchLogic();
    }

    /// <summary>
    /// Contains the core logic for switching canvases and managing GameObjects/TextMeshPro elements.
    /// </summary>
    private void ExecuteSwitchLogic()
    {
        // --- Hide Current Canvas ---
        if (currentCanvas != null)
        {
            Debug.Log($"TimedCanvasSwitcher: Hiding Current Canvas: {currentCanvas.name}", this);
            currentCanvas.gameObject.SetActive(false);
        }

        // --- Show Next Canvas ---
        if (nextCanvas != null)
        {
            Debug.Log($"TimedCanvasSwitcher: Showing Next Canvas: {nextCanvas.name}", this);
            nextCanvas.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("TimedCanvasSwitcher: No Next Canvas assigned to show.", this);
        }

        // --- Hide Additional Canvases ---
        if (canvasesToHideOnSwitch != null)
        {
            foreach (Canvas canvasToHide in canvasesToHideOnSwitch)
            {
                if (canvasToHide != null)
                {
                    Debug.Log($"TimedCanvasSwitcher: Hiding additional Canvas: {canvasToHide.name}", this);
                    canvasToHide.gameObject.SetActive(false);
                }
            }
        }

        // --- Control GameObjects ---
        // Activate specified GameObjects
        if (gameObjectsToActivate != null)
        {
            foreach (GameObject obj in gameObjectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"TimedCanvasSwitcher: Activated GameObject '{obj.name}'.", this);
                }
            }
        }

        // Deactivate specified GameObjects
        if (gameObjectsToDeactivate != null)
        {
            foreach (GameObject obj in gameObjectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"TimedCanvasSwitcher: Deactivated GameObject '{obj.name}'.", this);
                }
            }
        }

        // --- Control TextMeshPro Text ---
        // Activate specified TextMeshPro elements
        if (textMeshProElementsToActivate != null)
        {
            foreach (TextMeshProUGUI tmpElement in textMeshProElementsToActivate)
            {
                if (tmpElement != null)
                {
                    tmpElement.gameObject.SetActive(true);
                    Debug.Log($"TimedCanvasSwitcher: Activated TextMeshPro element '{tmpElement.name}'.", this);
                }
            }
        }

        // Deactivate specified TextMeshPro elements
        if (textMeshProElementsToDeactivate != null)
        {
            foreach (TextMeshProUGUI tmpElement in textMeshProElementsToDeactivate)
            {
                if (tmpElement != null)
                {
                    tmpElement.gameObject.SetActive(false);
                    Debug.Log($"TimedCanvasSwitcher: Deactivated TextMeshPro element '{tmpElement.name}'.", this);
                }
            }
        }

        Debug.Log("TimedCanvasSwitcher: Switch logic execution complete.", this);
    }
}
