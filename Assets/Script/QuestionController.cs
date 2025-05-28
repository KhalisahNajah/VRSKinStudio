using UnityEngine;

public class QuestionController : MonoBehaviour
{
    public GameObject nextCanvas;
    public GameObject previousCanvas;

    public void SetSkinType(string type)
    {
        QuizManager.Instance.skinType = type;
        Debug.Log("Skin Type Selected: " + type); // ✅ Debug log
        GoToNext();
    }

    public void SetAge(int age)
    {
        QuizManager.Instance.age = age;
        Debug.Log("Age Group Selected: " + age); // ✅ Debug log
        GoToNext();
    }

    public void SetSensitivity(string level)
    {
        QuizManager.Instance.sensitivity = level;
        Debug.Log("Sensitivity Level Selected: " + level); // ✅ Debug log
        GoToNext();
    }

    public void AddConcern(string concern)
    {
        if (!QuizManager.Instance.concerns.Contains(concern))
        {
            QuizManager.Instance.concerns.Add(concern);
            Debug.Log("Concern Added: " + concern); // ✅ Debug log
        }
    }

    public void GoToNext()
    {
        if (nextCanvas != null)
        {
            Debug.Log("Navigating to next canvas: " + nextCanvas.name); // Optional debug
            gameObject.SetActive(false);
            nextCanvas.SetActive(true);
        }
    }

    public void GoToPrevious()
    {
        if (previousCanvas != null)
        {
            Debug.Log("Returning to previous canvas: " + previousCanvas.name); // Optional debug
            gameObject.SetActive(false);
            previousCanvas.SetActive(true);
        }
    }
}
