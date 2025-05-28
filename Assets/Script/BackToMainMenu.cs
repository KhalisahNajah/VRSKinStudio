using UnityEngine;
using UnityEngine.SceneManagement;
using Leap;

public class BackToMainMenu : MonoBehaviour
{
    Controller leapController;

    void Start()
    {
        // Initialize the Leap Motion controller
        leapController = new Controller();
    }

    void Update()
    {
        // Check if Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMainMenu();
        }

        // (Optional) You can also add Leap Motion gesture-based navigation if needed later
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("MainPage"); // Change "MainMenu" to your actual scene name
    }
}
