using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;
using System;

public static class GlobalOrderData
{
    private static SheetsService _sheetsService;
    private const string _spreadsheetId = "1BO_oYW57E_xQZqt2UcAIrpRa6s5ZgK_nh4UD8Q5coZQ";

    public static string CustomerName;
    public static bool EVENT { get; private set; } = false;
    public static readonly Dictionary<string, IList<object>> MenuItems = new Dictionary<string, IList<object>>();
    public static readonly Dictionary<string, List<OrderData>> InsideBasket = new Dictionary<string, List<OrderData>>();

    public static string ActiveItem;
    public static int ExistingQuantity;
    public static HashSet<string> Details;

    public static int TotalQuantityOfItem { get; private set; } = 0;
    public static float TotalDonationCost { get; private set; } = 0;

    public static void EmptyBasket()
    {
        foreach (var entry in InsideBasket) entry.Value.Clear();
        TotalDonationCost = TotalQuantityOfItem = 0;
    }
    public static void UpdateBasket ()
    {
        TotalDonationCost = TotalQuantityOfItem = 0;
        foreach (var item in InsideBasket) //Different details
        {
            foreach (var variant in item.Value)
            {
                TotalQuantityOfItem += variant.Quantity;
                TotalDonationCost += variant.Quantity * variant.BaseDonationCost;
            }
        }
    }

    public static async Task Initialize(string json)
    {
        if (_sheetsService != null) return;
        var credential = GoogleCredential.FromJson(json).CreateScoped(new[] { SheetsService.Scope.Spreadsheets });
        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "perfect-barn"
        });

        var getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, "Menu!A1:B");
        var getResponse = await getRequest.ExecuteAsync();
        EVENT = !string.IsNullOrWhiteSpace(getResponse.Values[0][0].ToString());
    }
    public async static Task<IList<IList<object>>> RefreshSheets()
    {
        var getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, "Menu!A1:E");

        var getResponse = await getRequest.ExecuteAsync();
        var values = getResponse.Values;

        EVENT = !string.IsNullOrWhiteSpace(values[0][0].ToString());

        MenuItems.Clear();
        InsideBasket.Clear();

        return values;
    }
    public static async Task PlaceOrder()
    {
        const string TodayRange = "Today!A:Z";
        var valueRange = new ValueRange() { Range = TodayRange, Values = new List<IList<object>>() };

        string DateRow = DateTime.Now.ToString("f");
        string CustomerRow = CustomerName;
        foreach (var item in InsideBasket) //Different drinks
        {
            string ItemRow = item.Key;
            for (int i = 0; i < item.Value.Count; i++) //Different details
            {
                string details = item.Value[i].JoinedDetails;
                int quantity = item.Value[i].Quantity;
                float _cost = quantity * item.Value[i].BaseDonationCost;
                string cost = EVENT ? string.Empty : $"${_cost:0.00}";

                if (quantity <= 0) continue;

                valueRange.Values.Add(new List<object> { DateRow, CustomerRow, ItemRow, details, quantity, cost });
                DateRow = CustomerRow = ItemRow = string.Empty;
            }
        }

        var request = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, TodayRange);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
        await request.ExecuteAsync();

        const string TotalRange = "Total!A:E";
        var TotalValueRange = new ValueRange() { Range = TotalRange, Values = valueRange.Values };
        var TotalRequest = _sheetsService.Spreadsheets.Values.Append(TotalValueRange, _spreadsheetId, TotalRange);
        TotalRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        TotalRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
        await TotalRequest.ExecuteAsync();
    }

    public async static Task<IList<IList<object>>> RefreshCustomers()
    {
        //var clrreqval = new ClearValuesRequest();
        //var DeleteRequest = _sheetsService.Spreadsheets.Values.Clear(, _spreadsheetId, TotalRange);
        var getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, "Today!B2:F");

        var getResponse = await getRequest.ExecuteAsync();
        var values = getResponse.Values;

        return values;
    }

    public static void ClearActiveItem()
    {
        ActiveItem = string.Empty;
        ExistingQuantity = 0;
        Details = null;
    }

    public static IList<object> ActiveItemChunk => MenuItems[ActiveItem];
    public static List<OrderData> ActiveBasket => InsideBasket[ActiveItem];
}
