using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;

public sealed class OrderData
{
    public string Name;
    public int Quantity;
    public float BaseDonationCost;
    public readonly HashSet<string> Details = new HashSet<string>();
}

public class GenerateMenu : MonoBehaviour
{
    [SerializeField] private GameObject CategoryPrefab, ItemEntryPrefab;
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private TextMeshProUGUI Subtitle, Basket;
    [SerializeField] private ItemEntry ItemEntry;
    [SerializeField] private Basket BasketMenu;

    [SerializeField] private GameObject ExistingDrinks;
    [SerializeField] private TextMeshProUGUI ExistingDrinkTitle, ExistingDrinkCost;
    [SerializeField] private Button ExistingDrinkButton;

    public readonly Dictionary<string, GameObject> ItemEntries = new Dictionary<string, GameObject>();
    public readonly Dictionary<string, List<OrderData>> InsideBasket = new Dictionary<string, List<OrderData>>();

    private SheetsService _sheetsService;
    private const string _spreadsheetId = "1BO_oYW57E_xQZqt2UcAIrpRa6s5ZgK_nh4UD8Q5coZQ";
    private bool EVENT = false, Refreshed = false;
    const string range = "Menu!A1:F";

    public async Task<string> UpdateData(List<List<string>> data)
    {
        var objectList = new List<object>() { "updated", "Test" };
        var objectList2 = new List<object>() { "updated2", "Test2" };
        var valueRange = new ValueRange();
        valueRange.Range = range;
        valueRange.Values = new List<IList<object>>
        {
            objectList,
            objectList2
        };

        // The new values to apply to the spreadsheet.
        var requestBody = new BatchUpdateValuesRequest()
        {
            ValueInputOption = "USER_ENTERED",
            Data = new List<ValueRange> { valueRange }
        };

        var request = _sheetsService.Spreadsheets.Values.BatchUpdate(requestBody, _spreadsheetId);

        var response = await request.ExecuteAsync(); // For async 

        return JsonUtility.ToJson(response);
    }

    public async void OnValueChanged (Vector2 _)
    {
        if (!Refreshed && Scroll.anchoredPosition.y < -300) await RefreshMenu();
        else if (Scroll.anchoredPosition.y > 0) Refreshed = false;
    }

    private async Task RefreshMenu()
    {
        Refreshed = true;

        for (var i = Scroll.transform.childCount - 1; i >= 3; i--)
            Destroy(Scroll.transform.GetChild(i).gameObject);

        var getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

        var getResponse = await getRequest.ExecuteAsync();
        var values = getResponse.Values;
        EVENT = string.IsNullOrWhiteSpace(values[0][0].ToString());
        Subtitle.text = values[0][1].ToString();

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
                var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = row[0].ToString(); //Name
                ItemEntries.Add(texts[0].text, item);
                InsideBasket.Add(texts[0].text, new List<OrderData>());
                texts[1].text = string.IsNullOrWhiteSpace(row[1].ToString()) ? row[0].ToString() : row[1].ToString(); //Description
                if (!EVENT) texts[2].text = $"${row[2]} <size=80%>Donation</size>";
                item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    if (InsideBasket[row[0].ToString()].Count > 0)
                    {
                        ExistingDrinks.SetActive(true);

                        ExistingDrinkTitle.text = row[0].ToString();
                        ExistingDrinkCost.text = row[2].ToString();

                        ExistingDrinkButton.onClick.RemoveAllListeners();
                        ExistingDrinkButton.onClick.AddListener(() => ItemEntry.GenerateSegments(row));

                        LayoutRebuilder.ForceRebuildLayoutImmediate(ExistingDrinkButton.transform.parent.GetComponent<RectTransform>());
                    }
                    else
                    {
                        ItemEntry.GenerateSegments(row);
                        gameObject.SetActive(false);
                    }
                });
            }
        }
    }

    private async void Awake()
    {
        Application.targetFrameRate = 60;
        var json = Resources.Load<TextAsset>("credentials").text;
        var credential = GoogleCredential.FromJson(json).CreateScoped(new[] { SheetsService.Scope.Spreadsheets });

        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "perfect-barn"
        });

        await RefreshMenu();
        //await UpdateData(new List<List<string>> { new List<string> { "test1" } });
    }

    public void AddToBasket (OrderData data)
    {
        bool unique = true;
        foreach (var entry in InsideBasket[data.Name])
        {
            if (entry.Details.SetEquals(data.Details))
            {
                entry.Quantity = data.Quantity;
                unique = false;
                if (data.Quantity == 0) InsideBasket[data.Name].Remove(entry);
                break;
            }
        }
        if (unique) InsideBasket[data.Name].Add(data);

        ColorUtility.TryParseHtmlString("#5CBD5A", out var Green);
        int BasketQuantity = 0;
        float TotalDonationCost = 0;
        foreach (var itemEntry in InsideBasket)
        {
            var image = ItemEntries[itemEntry.Key].transform.GetChild(5).GetComponent<Image>();

            if (itemEntry.Value.Count == 0)
            {
                image.color = Green;
                image.GetComponentInChildren<TextMeshProUGUI>().text = "+";
                image.GetComponentInChildren<TextMeshProUGUI>().fontSize = 100;
                image.GetComponentInChildren<TextMeshProUGUI>().color = UnityEngine.Color.white;
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

                image.color = UnityEngine.Color.white;
                image.GetComponentInChildren<TextMeshProUGUI>().text = TotalQuantityOfItem.ToString();
                image.GetComponentInChildren<TextMeshProUGUI>().fontSize = 65;
                image.GetComponentInChildren<TextMeshProUGUI>().color = UnityEngine.Color.black;
            }
        }

        Basket.transform.parent.gameObject.SetActive(BasketQuantity > 0);
        string ItemString = BasketQuantity == 1 ? "Item" : "Items";
        Basket.text = $"Basket ({BasketQuantity} {ItemString}): ${TotalDonationCost:0.00}";
    }

    public void OpenBasket()
    {
        var basket = new List<OrderData>();
        foreach (var order in InsideBasket) basket.AddRange(order.Value);
        BasketMenu.GenerateOrders(basket.ToArray());
        gameObject.SetActive(false);
    }
}
