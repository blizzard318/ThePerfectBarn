using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public sealed class OrderData //Variant
{
    public string Name;
    public int Quantity;
    public float DonationCost;
    public readonly HashSet<string> Details = new HashSet<string>();
    public const string Seperator = ", ";
    public string JoinedDetails => string.Join(Seperator, Details);
}

public class GenerateMenu : MonoBehaviour
{
    [SerializeField] private GameObject CategoryPrefab, ItemEntryPrefab;
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private TextMeshProUGUI Basket;
    [SerializeField] private ExistingDrinksMenu ExistingDrinksMenu;

    public readonly Dictionary<string, GameObject> ItemEntries = new Dictionary<string, GameObject>();
    private Dictionary<string, List<GameObject>> Categories = new Dictionary<string, List<GameObject>>();

    private bool Refreshed = false;

    public async void OnValueChanged (Vector2 _)
    {
        if (!Refreshed && Scroll.anchoredPosition.y < -300) await RefreshMenu();
        else if (Scroll.anchoredPosition.y > 0) Refreshed = false;
    }

    private async Task RefreshMenu()
    {
        Refreshed = true;

        var values = await GlobalOrderData.RefreshSheets();

        for (var i = Scroll.childCount - 1; i >= 2; i--) Destroy(Scroll.GetChild(i).gameObject);

        Categories.Clear();
        ItemEntries.Clear();

        string CurrentCategory = string.Empty;
        for (int i = 0; i < values.Count; i++)
        {
            var row = values[i];
            if (row.Count == 0) continue; //Blank row
            
            var value = row[0].ToString();
            const string CategoryPrefix = "Category:";
            if (value.Contains(CategoryPrefix))
            {
                var category = Instantiate(CategoryPrefab, Scroll);
                var CatName = CurrentCategory = value.Split(CategoryPrefix)[1];

                category.GetComponentInChildren<TextMeshProUGUI>().text = CatName;
                Categories.TryAdd(CatName, new List<GameObject>());
                category.GetComponent<Button>().onClick.AddListener(() =>
                {
                    bool Toggle = !Categories[CatName][0].activeSelf;
                    category.transform.GetChild(1).gameObject.SetActive(!Toggle);
                    category.transform.GetChild(2).gameObject.SetActive(!Toggle);
                    category.transform.GetChild(3).gameObject.SetActive( Toggle);
                    category.transform.GetChild(4).gameObject.SetActive( Toggle);
                    foreach (var item in Categories[CatName]) item.gameObject.SetActive(Toggle);
                });
            }
            else
            {
                if (row[3].ToString() != "Y") continue; //Out of stock

                var item = Instantiate(ItemEntryPrefab, Scroll);

                //item.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/" + row[0].ToString()) ?? Resources.Load<Sprite>("Images/Default");

                var image = item.transform.GetChild(1).GetComponent<Image>();
                StartCoroutine(image.GetTexture(row[0].ToString()));

                var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = $"{row[0]}"; //Name
                if (!string.IsNullOrWhiteSpace(row[1].ToString()))
                    texts[0].text += System.Environment.NewLine + $"<size=70%><alpha=#AA>{row[1]}</size>";

                if (!GlobalOrderData.EVENT)
                    texts[0].text += System.Environment.NewLine + $"<alpha=#FF><size=80%>${row[2]:0.00}</size> <alpha=#AA><size=70%>Donation</size>";

                GlobalOrderData.AddMenuItem(row);
                Categories[CurrentCategory].Add(item);
                ItemEntries.Add(row[0].ToString(), item);

                item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    GlobalOrderData.ClearActiveItem();
                    GlobalOrderData.ActiveItem = row[0].ToString();

                    if (GlobalOrderData.ActiveBasket.Count > 0) ExistingDrinksMenu.ShowExistingDrinks();
                    else GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.ITEM);
                });
            }
        }
    }

    private async void Awake() => await RefreshMenu();

    public void OnEnable ()
    {
        foreach (var set in Categories)
            foreach (var item in set.Value) item.gameObject.SetActive(true);

        ColorUtility.TryParseHtmlString("#5CBD5A", out var Green);

        GlobalOrderData.ForEachBasket(itemEntry =>
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
                int QuantityOfItem = 0;
                foreach (var item in itemEntry.Value) QuantityOfItem += item.Quantity;

                image.color = Color.white;
                image.GetComponentInChildren<TextMeshProUGUI>().fontSize = 110;
                image.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
                image.GetComponentInChildren<TextMeshProUGUI>().text = QuantityOfItem.ToString();
            }
        });

        int BasketQuantity = GlobalOrderData.TotalQuantityOfItem;
        Basket.transform.parent.gameObject.SetActive(BasketQuantity > 0);
        string ItemString = BasketQuantity == 1 ? "Item" : "Items";

        Basket.text = $"Basket ({BasketQuantity} {ItemString})";
        if (!GlobalOrderData.EVENT)
            Basket.text += $": ${GlobalOrderData.TotalDonationCost:0.00}";
    }
}
