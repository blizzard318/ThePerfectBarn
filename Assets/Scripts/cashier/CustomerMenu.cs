using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI CustomerName;
    [SerializeField] private GameObject DrinkPrefab;
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private Image Banner;
    public static Customer ActiveCustomer;

    void OnEnable()
    {
        Scroll.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        for (int i = 0; i < Scroll.childCount; i++) Destroy(Scroll.GetChild(i).gameObject);

        UpdateBanner();

        foreach (var drink in ActiveCustomer.Drinks)
        {
            for (int i = 0; i < drink.Quantity; i++)
            {
                var entry = Instantiate(DrinkPrefab, Scroll);
                var drinkText = entry.GetComponentInChildren<TextMeshProUGUI>();
                drinkText.text = $"<b>{drink.Name}</b>\r\n<size=90%>{drink.Details}</size>";
                entry.GetComponentInChildren<Toggle>().onValueChanged.AddListener(tick =>
                {
                    drinkText.fontStyle = tick ? FontStyles.Strikethrough : FontStyles.Normal;
                    ActiveCustomer.DrinksCompleted += tick ? 1 : -1;
                    UpdateBanner();
                });
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll);
    }

    private void UpdateBanner()
    {
        CustomerName.text = ActiveCustomer.Name + $" {ActiveCustomer.DrinksCompleted}/{ActiveCustomer.TotalDrinks}";
        bool DrinksCompleted = ActiveCustomer.DrinksCompleted >= ActiveCustomer.TotalDrinks;
        Banner.color = DrinksCompleted ? new Color(0.6f, 0.9f, 0.6f) : new Color(0.8f, 0.8f, 0.8f);
    }
}
