using System.Collections.Generic;

public static class IngredientLogic
{
    public static List<string> GetRecommendedIngredients()
    {
        List<string> result = new List<string>();

        // Sample logic
        var skin = QuizManager.Instance.skinType;
        var concerns = QuizManager.Instance.concerns;
        var age = QuizManager.Instance.age;
        var sensitivity = QuizManager.Instance.sensitivity;

        if (skin == "oily")
            result.Add("Niacinamide");

        if (concerns.Contains("acne"))
            result.Add("Salicylic Acid");

        if (age >= 25)
            result.Add("Retinol");

        if (sensitivity == "high")
            result.Add("Centella Asiatica");

        // Cap the list to 4
        if (result.Count > 4)
            result = result.GetRange(0, 4);

        return result;
    }
}
