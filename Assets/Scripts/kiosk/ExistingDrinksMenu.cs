using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExistingDrinksMenu : MonoBehaviour
{
    [SerializeField] private GameObject ExistingDrinkScroll, ExistingDrinkPrefab;
    [SerializeField] private TextMeshProUGUI DrinkTitle, DrinkCost, BaseDonationText;
    [SerializeField] private Button MakeAnotherButton;

    public void ShowExistingDrinks ()
    {
        ExistingDrinkScroll.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        for (int i = 0; i < ExistingDrinkScroll.transform.childCount; i++)
            Destroy(ExistingDrinkScroll.transform.GetChild(i).gameObject);

        var row = GlobalOrderData.ActiveItemChunk;
        DrinkTitle.text = row[0].ToString();

        DrinkCost.enabled = BaseDonationText.enabled = !GlobalOrderData.EVENT;
        DrinkCost.text = $"{float.Parse(row[2].ToString()):0.00}";

        var orders = GlobalOrderData.InsideBasket[row[0].ToString()];
        foreach (var order in orders)
        {
            var existingDrink = Instantiate(ExistingDrinkPrefab, ExistingDrinkScroll.transform);
            existingDrink.transform.SetAsLastSibling();

            existingDrink.GetComponent<Button>().onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                GetComponentInParent<GenerateMenu>().gameObject.SetActive(false);
                GlobalOrderData.ExistingQuantity = order.Quantity;
                GlobalOrderData.Details = order.Details;
                GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.ITEM);
            });

            existingDrink.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{order.JoinedDetails}";
            existingDrink.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = order.Quantity.ToString();

            existingDrink.transform.GetChild(2).GetComponent<TextMeshProUGUI>().enabled = !GlobalOrderData.EVENT;
            float totalCost = order.Quantity * order.BaseDonationCost;
            existingDrink.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{totalCost:0.00}";
        }

        MakeAnotherButton.onClick.RemoveAllListeners();
        MakeAnotherButton.onClick.AddListener(() =>
        {
            GlobalOrderData.ClearActiveItem();
            GlobalOrderData.ActiveItem = row[0].ToString();
            GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.ITEM);
        });

        LayoutRebuilder.ForceRebuildLayoutImmediate(MakeAnotherButton.transform.parent.GetComponent<RectTransform>());

        gameObject.SetActive(true);
    }
}
