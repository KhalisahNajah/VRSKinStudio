using UnityEngine;

public class CanvasOpener : MonoBehaviour
{
    public GameObject canvasToOpen;

    public void OnActive()
    {
        if (canvasToOpen != null)
        {
            canvasToOpen.SetActive(true);
        }
    }
}
