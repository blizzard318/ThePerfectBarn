using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public sealed class OrderData //Variant
{
    public string Name;
    public int Quantity;
    public float BaseDonationCost;
    public readonly HashSet<string> Details = new HashSet<string>();
    public const string Seperator = ", ";
    public string JoinedDetails => string.Join(Seperator, Details);
}

public class GenerateMenu : MonoBehaviour
{
    [SerializeField] private GameObject CategoryPrefab, ItemEntryPrefab;
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private TextMeshProUGUI Basket;
    [SerializeField] private Basket BasketMenu;
    [SerializeField] private ExistingDrinksMenu ExistingDrinksMenu;

    public readonly Dictionary<string, GameObject> ItemEntries = new Dictionary<string, GameObject>();

    private bool Refreshed = false;

    public async void OnValueChanged (Vector2 _)
    {
        if (!Refreshed && Scroll.anchoredPosition.y < -300) await RefreshMenu();
        else if (Scroll.anchoredPosition.y > 0) Refreshed = false;
    }

    private async Task RefreshMenu()
    {
        Refreshed = true;

        for (var i = Scroll.childCount - 1; i >= 2; i--) Destroy(Scroll.GetChild(i).gameObject);

        var values = await GlobalOrderData.RefreshSheets();

        ItemEntries.Clear();
        GlobalOrderData.MenuItems.Clear();
        GlobalOrderData.InsideBasket.Clear();
        for (int i = 1; i < values.Count; i++) //Skip first row, that's handled above.
        {
            var row = values[i];
            if (row.Count == 0) continue;

            var value = row[0].ToString();

            const string CategoryPrefix = "Category:";
            if (value.Contains(CategoryPrefix))
            {
                var category = Instantiate(CategoryPrefab, Scroll);
                category.GetComponentInChildren<TextMeshProUGUI>().text = value.Split(CategoryPrefix)[1];
            }
            else
            {
                var item = Instantiate(ItemEntryPrefab, Scroll);

                item.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/" + row[0].ToString());

                var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = $"{row[0]}"; //Name
                if (!string.IsNullOrWhiteSpace(row[1].ToString()))
                    texts[0].text += System.Environment.NewLine + $"<size=70%><alpha=#AA>{row[1]}</size>";

                if (!GlobalOrderData.EVENT)
                    texts[0].text += System.Environment.NewLine + $"<alpha=#FF><size=80%>${row[2]:0.00}</size> <alpha=#AA><size=70%>Donation</size>";

                ItemEntries.Add(row[0].ToString(), item);
                GlobalOrderData.MenuItems.Add(row[0].ToString(), row);
                GlobalOrderData.InsideBasket.Add(row[0].ToString(), new List<OrderData>());

                item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    GlobalOrderData.ClearActiveItem();
                    GlobalOrderData.ActiveItem = row[0].ToString();

                    if (GlobalOrderData.InsideBasket[row[0].ToString()].Count > 0)
                        ExistingDrinksMenu.ShowExistingDrinks();
                    else GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.ITEM);
                });
            }
        }
    }

    private async void Awake() => await RefreshMenu();

    public void OnEnable ()
    {
        ColorUtility.TryParseHtmlString("#5CBD5A", out var Green);
        int BasketQuantity = 0;
        float TotalDonationCost = 0;
        foreach (var itemEntry in GlobalOrderData.InsideBasket)
        {
            var image = ItemEntries[itemEntry.Key].transform.GetChild(3).GetComponent<Image>();

            if (itemEntry.Value.Count == 0)
            {
                image.color = Green;
                image.GetComponentInChildren<TextMeshProUGUI>().text = "+";
                image.GetComponentInChildren<TextMeshProUGUI>().fontSize = 180;
                image.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            }
            else
            {
                int TotalQuantityOfItem = 0;
                foreach (var item in itemEntry.Value) //Different details
                {
                    TotalQuantityOfItem += item.Quantity;
                    TotalDonationCost += item.Quantity * item.BaseDonationCost;
                }
                BasketQuantity += TotalQuantityOfItem;

                image.color = Color.white;
                image.GetComponentInChildren<TextMeshProUGUI>().fontSize = 110;
                image.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
                image.GetComponentInChildren<TextMeshProUGUI>().text = TotalQuantityOfItem.ToString();
            }
        }

        Basket.transform.parent.gameObject.SetActive(BasketQuantity > 0);
        string ItemString = BasketQuantity == 1 ? "Item" : "Items";

        if (GlobalOrderData.EVENT)
            Basket.text = $"Basket ({BasketQuantity} {ItemString})";
        else
            Basket.text = $"Basket ({BasketQuantity} {ItemString}): ${TotalDonationCost:0.00}";
    }
}