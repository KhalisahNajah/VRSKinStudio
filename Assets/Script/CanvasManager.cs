using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasManager : MonoBehaviour
{
    [Header("Canvas Settings")]
    public Canvas[] canvases; // Array of all canvases in order
    public float displayTime = 3f; // Time each canvas is displayed (3 seconds by default)

    [Header("Object Visibility")]
    public GameObject[] objectsToUnhide; // Objects to unhide when switching
    public GameObject[] objectsToHide;   // Objects to hide when switching

    private int currentCanvasIndex = 0;

    void Start()
    {
        // Initialize - disable all canvases except the first one
        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].enabled = (i == 0);
        }

        // Start the canvas timer
        StartCoroutine(CanvasTimer());
    }

    IEnumerator CanvasTimer()
    {
        while (true)
        {
            // Wait for the specified display time
            yield return new WaitForSeconds(displayTime);

            // Move to the next canvas
            SwitchToNextCanvas();
        }
    }

    public void SwitchToNextCanvas()
    {
        // Disable current canvas
        canvases[currentCanvasIndex].enabled = false;

        // Move to next canvas (wrap around if needed)
        currentCanvasIndex = (currentCanvasIndex + 1) % canvases.Length;

        // Enable new canvas
        canvases[currentCanvasIndex].enabled = true;

        // Handle object visibility
        UpdateObjectVisibility();
    }

    void UpdateObjectVisibility()
    {
        // Unhide specified objects
        foreach (GameObject obj in objectsToUnhide)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        // Hide specified objects
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    // Public methods to manually control objects (optional)
    public void UnhideObject(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(true);
        }
    }

    public void HideObject(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }
}