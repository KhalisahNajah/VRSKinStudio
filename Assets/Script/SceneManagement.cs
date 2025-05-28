using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // This function will be called when the "Start" button is clicked
    public void SkincareRecommendation()
    {
        // Load the MainScene
        SceneManager.LoadScene("SkincareRecommendation");
    }

    public void MainPage()
    {
        // Load the MainScene
        SceneManager.LoadScene("MainPage");
    }

    public void Grab()
    {
        // Load the MainScene
        SceneManager.LoadScene("Grab");
    }

    public void PhysicalHands()
    {
        // Load the MainScene
        SceneManager.LoadScene("PhysicalHands");
    }

    public void PoseShowcase()
    {
        // Load the MainScene
        SceneManager.LoadScene("PoseShowcase2");
    }

    // This function will be called when the "Quit" button is clicked
    public void QuitGame()
    {
        // If running in the Unity editor, stop playing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // If running as a build, quit the application
        Application.Quit();
        #endif
    }
}