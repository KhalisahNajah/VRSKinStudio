using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class IngredientTextDisplay : MonoBehaviour
{
    public TMP_Text ingredientText; // Assign your Text UI element in Inspector

    void OnEnable()
    {
        if (QuizManager.Instance == null || QuizManager.Instance.finalIngredients == null)
        {
            ingredientText.text = "No ingredients found.";
            return;
        }

        var ingredients = QuizManager.Instance.finalIngredients;
        int count = Mathf.Min(5, ingredients.Count); // Limit to 5

        StringBuilder sb = new StringBuilder(" ");

        for (int i = 0; i < count; i++)
        {
            sb.AppendLine("" + ingredients[i]);
        }

        ingredientText.text = sb.ToString();
    }
}