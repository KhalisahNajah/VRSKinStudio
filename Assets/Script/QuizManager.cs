using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    public string skinType;
    public List<string> concerns = new List<string>();
    public int age;
    public string sensitivity;

    public List<string> finalIngredients = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep it between scenes if needed
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetAll()
    {
        skinType = "";
        concerns.Clear();
        sensitivity = "";
        age = 0;
        finalIngredients.Clear();
    }
}
