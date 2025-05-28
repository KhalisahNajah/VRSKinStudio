using UnityEngine;
using System.Collections.Generic; // Required for List

public class CanvasControllers : MonoBehaviour
{
    // Assign all your UI Canvas GameObjects (or panels/any game objects you want to control visibility) here in the Inspector
    public List<GameObject> managedObjects;

    // Optional: Indices of the objects to show initially when the scene starts
    // Use this if you want multiple elements visible at the start.
    public List<int> initialObjectIndices; 

    void Start()
    {
        // Ensure all managed objects are assigned
        if (managedObjects == null || managedObjects.Count == 0)
        {
            Debug.LogError("CanvasController: No objects assigned to 'Managed Objects' list! Please assign them in the Inspector.");
            enabled = false; // Disable the script if no objects are set
            return;
        }

        // Hide all objects first to ensure a clean state
        HideAllManagedObjects();

        // Show the initial objects specified
        if (initialObjectIndices != null && initialObjectIndices.Count > 0)
        {
            ShowManagedObjects(initialObjectIndices);
        } else {
             // If no initial indices are specified, you might still want to show a default one
             // Or leave all hidden if that's the desired start state.
             // For example, if you want the first object to be visible by default:
             // ShowManagedObject(0);
        }
    }

    /// <summary>
    /// Activates the GameObject at the specified single index and deactivates all others in the list.
    /// This is useful for single-screen transitions.
    /// </summary>
    /// <param name="objectIndexToShow">The index of the GameObject to activate.</param>
    public void ShowManagedObject(int objectIndexToShow)
    {
        // Simply call the multi-index method with a single index
        ShowManagedObjects(new List<int> { objectIndexToShow });
    }

    /// <summary>
    /// Activates the GameObjects at the specified indices and deactivates all others in the list.
    /// This allows multiple objects to be shown simultaneously.
    /// </summary>
    /// <param name="objectIndicesToShow">A list of indices of the GameObjects to activate.</param>
    public void ShowManagedObjects(List<int> objectIndicesToShow)
    {
        if (managedObjects == null || managedObjects.Count == 0)
        {
            Debug.LogError("CanvasController: Managed Objects list is empty or null. Cannot show objects.");
            return;
        }

        // Create a HashSet for efficient lookup of objects to show
        HashSet<int> indicesToActivate = new HashSet<int>(objectIndicesToShow);

        for (int i = 0; i < managedObjects.Count; i++)
        {
            if (managedObjects[i] != null)
            {
                // If the current index is in the set of indices to activate, set active to true, otherwise false
                managedObjects[i].SetActive(indicesToActivate.Contains(i));
            }
            else
            {
                Debug.LogWarning($"CanvasController: Managed Object at index {i} is null in the list. Please check your assignments.");
            }
        }

        string activeIndices = "";
        foreach(int index in objectIndicesToShow)
        {
            activeIndices += index.ToString() + ", ";
        }
        Debug.Log($"CanvasController: Displaying objects at indices: {activeIndices.TrimEnd(',', ' ')}.");
    }


    /// <summary>
    /// Activates only the GameObjects at the specified indices, without affecting others.
    /// This is useful if you want to add an object to an already visible set.
    /// </summary>
    /// <param name="objectIndicesToAdd">A list of indices of the GameObjects to activate.</param>
    public void ActivateOnlySelectedObjects(List<int> objectIndicesToAdd)
    {
        if (managedObjects == null || managedObjects.Count == 0)
        {
            Debug.LogError("CanvasController: Managed Objects list is empty or null. Cannot activate objects.");
            return;
        }

        foreach (int index in objectIndicesToAdd)
        {
            if (index >= 0 && index < managedObjects.Count && managedObjects[index] != null)
            {
                managedObjects[index].SetActive(true);
            }
            else
            {
                Debug.LogWarning($"CanvasController: Invalid or null object index {index} to activate.");
            }
        }
        Debug.Log($"CanvasController: Activated specific objects at indices: {string.Join(", ", objectIndicesToAdd)}.");
    }


    /// <summary>
    /// Deactivates only the GameObjects at the specified indices, without affecting others.
    /// This is useful if you want to remove an object from a visible set.
    /// </summary>
    /// <param name="objectIndicesToDeactivate">A list of indices of the GameObjects to deactivate.</param>
    public void DeactivateOnlySelectedObjects(List<int> objectIndicesToDeactivate)
    {
        if (managedObjects == null || managedObjects.Count == 0)
        {
            Debug.LogError("CanvasController: Managed Objects list is empty or null. Cannot deactivate objects.");
            return;
        }

        foreach (int index in objectIndicesToDeactivate)
        {
            if (index >= 0 && index < managedObjects.Count && managedObjects[index] != null)
            {
                managedObjects[index].SetActive(false);
            }
            else
            {
                Debug.LogWarning($"CanvasController: Invalid or null object index {index} to deactivate.");
            }
        }
        Debug.Log($"CanvasController: Deactivated specific objects at indices: {string.Join(", ", objectIndicesToDeactivate)}.");
    }

    /// <summary>
    /// Hides all GameObjects managed by this controller.
    /// </summary>
    public void HideAllManagedObjects()
    {
        if (managedObjects == null || managedObjects.Count == 0) return;

        foreach (GameObject obj in managedObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        Debug.Log("CanvasController: All managed objects hidden.");
    }
}