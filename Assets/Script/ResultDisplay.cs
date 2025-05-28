using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResultDisplay : MonoBehaviour
{
    public Transform ingredientBoxParent; // Invisible container (set to Active when showing result)
    public GameObject ingredientBoxPrefab; // Prefab with an Image + Text component
    public List<Sprite> ingredientSprites; // Images for ingredients
    public List<string> ingredientNames;   // Names matching sprites list order

    void OnEnable()
    {
        ShowResults(); // Automatically trigger when result canvas is shown
    }

    public void ShowResults()
    {
        Debug.Log("Final Ingredients Selected:");

        // Clear existing children
        foreach (Transform child in ingredientBoxParent)
        {
            Destroy(child.gameObject);
        }

        GenerateRecommendations();

        List<string> ingredients = QuizManager.Instance.finalIngredients;
        int count = Mathf.Min(ingredients.Count, 4);

        for (int i = 0; i < count; i++)
        {
            string ingredient = ingredients[i];
            Debug.Log("- " + ingredient);

            GameObject box = Instantiate(ingredientBoxPrefab, ingredientBoxParent);

            Image img = box.GetComponentInChildren<Image>();
            Text txt = box.GetComponentInChildren<Text>();

            int index = ingredientNames.IndexOf(ingredient);
            if (index >= 0 && index < ingredientSprites.Count)
            {
                img.sprite = ingredientSprites[index];
            }
            else
            {
                Debug.LogWarning("Missing image for: " + ingredient);
            }

            if (txt != null)
            {
                txt.text = ingredient;
            }
        }
    }

    private void GenerateRecommendations()
    {
        QuizManager.Instance.finalIngredients.Clear();
        List<string> ingredients = QuizManager.Instance.finalIngredients;

        string skinType = QuizManager.Instance.skinType;
        List<string> concerns = QuizManager.Instance.concerns;
        int age = QuizManager.Instance.age;
        string sensitivity = QuizManager.Instance.sensitivity;

        if (skinType == "Oily")
            AddRange(ingredients, new[] { "Niacinamide", "Salicylic Acid", "Tea Tree Oil" });
        else if (skinType == "Dry")
            AddRange(ingredients, new[] { "Hyaluronic Acid", "Ceramides", "Squalane" });
        else if (skinType == "Sensitive")
            AddRange(ingredients, new[] { "Aloe Vera", "Centella Asiatica", "Madecassoside" });
        else if (skinType == "Combination")
            AddRange(ingredients, new[] { "Niacinamide", "Green Tea Extract" });
        else if (skinType == "Normal")
            AddRange(ingredients, new[] { "Vitamin C", "Niacinamide" });

        bool canUseRetinol = age >= 25;

        foreach (string concern in concerns)
        {
            switch (concern)
            {
                case "Acne":
                    AddRange(ingredients, new[] { "Niacinamide", "Salicylic Acid", "Tea Tree Oil" });
                    break;
                case "Wrinkles":
                    if (canUseRetinol) ingredients.Add("Retinol");
                    else ingredients.Add("Bakuchiol");
                    AddRange(ingredients, new[] { "Peptides", "Vitamin C" });
                    break;
                case "Dark Circles":
                    AddRange(ingredients, new[] { "Vitamin C", "Caffeine", "Niacinamide" });
                    break;
                case "Redness":
                    AddRange(ingredients, new[] { "Centella Asiatica", "Aloe Vera" });
                    break;
                case "Hyperpigmentation":
                    AddRange(ingredients, new[] { "Niacinamide", "Vitamin C", "Licorice Root Extract" });
                    break;
            }
        }

        if (sensitivity == "Very sensitive")
        {
            ingredients.Remove("Retinol");
            ingredients.Remove("Salicylic Acid");
            ingredients.Remove("Benzoyl Peroxide");
        }

        List<string> uniqueIngredients = new List<string>();
        foreach (string ing in ingredients)
        {
            if (!uniqueIngredients.Contains(ing))
                uniqueIngredients.Add(ing);
        }

        ingredients.Clear();
        ingredients.AddRange(uniqueIngredients);
    }

    private void AddRange(List<string> list, string[] values)
    {
        foreach (var val in values)
            list.Add(val);
    }
}
