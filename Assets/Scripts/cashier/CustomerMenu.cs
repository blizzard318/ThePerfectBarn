using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

        foreach (var drink in ActiveCustomer.Drinks)
        {
            for (int i = 0; i < drink.Total; i++)
            {
                var entry = Instantiate(DrinkPrefab, Scroll);
                var drinkText = entry.GetComponentInChildren<TextMeshProUGUI>();
                drinkText.text = $"<b>{drink.Name}</b>\r\n<size=90%>{drink.Details}</size>";

                var toggle = entry.GetComponentInChildren<Toggle>();
                UnityAction<bool> function = isOn =>
                {
                    toggle.isOn = isOn;
                    ColorUtility.TryParseHtmlString("#53AD5A", out Color green);
                    entry.GetComponent<Image>().color = isOn ? green : Color.clear;
                    drinkText.fontStyle = isOn ? FontStyles.Strikethrough : FontStyles.Normal;
                    if (isOn) entry.transform.SetAsLastSibling();
                    else entry.transform.SetAsFirstSibling();
                };
                entry.GetComponent<Button>().onClick.AddListener(() => function(!toggle.isOn));
                function(i + 1 <= drink.Completed);
                entry.GetComponent<Button>().onClick.AddListener(() =>
                {
                    drink.Completed += toggle.isOn ? 1 : -1;
                    UpdateBanner();
                });
            }
        }
        UpdateBanner();
        LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll);
    }

    private void UpdateBanner()
    {
        int TotalDrinks = 0, TotalComplete = 0;
        foreach (var drink in ActiveCustomer.Drinks)
        {
            TotalDrinks += drink.Total;
            TotalComplete += drink.Completed;
        }

        CustomerName.text = ActiveCustomer.Name + $" {TotalComplete}/{TotalDrinks}";
        bool DrinksCompleted = TotalComplete >= TotalDrinks;
        Banner.color = DrinksCompleted ? new Color(0.6f, 0.9f, 0.6f) : new Color(0.8f, 0.8f, 0.8f);
    }
}
