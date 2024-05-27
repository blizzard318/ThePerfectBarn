using System;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;

public static class GlobalOrderData
{
    private static SheetsService _sheetsService;
    private const string _spreadsheetId = "1BO_oYW57E_xQZqt2UcAIrpRa6s5ZgK_nh4UD8Q5coZQ";

    public static string CustomerName;
    public static bool EVENT { get; private set; } = false;
    public static int DRINKLIMIT { get; private set; } = 20;

    public static readonly Dictionary<string, IList<object>> MenuItems = new Dictionary<string, IList<object>>();
    public static readonly Dictionary<string, List<OrderData>> InsideBasket = new Dictionary<string, List<OrderData>>();

    public static int LatestCustomer = 0;
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

    public static void Initialize(string json)
    {
        if (_sheetsService != null) return;
        var credential = GoogleCredential.FromJson(json).CreateScoped(new[] { SheetsService.Scope.Spreadsheets });
        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "perfect-barn"
        });
    }
    public async static Task<IList<IList<object>>> RefreshSheets()
    {
        var getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, "Meta!A1:B");
        var getResponse = await getRequest.ExecuteAsync();

        EVENT = getResponse.Values[0].Count >= 2;

        /*var DayChunk = getResponse.Values[1][1].ToString();
        var ValidDays = new HashSet<string>(DayChunk.Split(","));
        if (ValidDays.Contains(DateTime.Now.DayOfWeek.ToString()))
        {
            //Open
        }
        else
        {
            //Closed
        }

        //Time Open
        string[] TimeChunk = getResponse.Values[2][1].ToString().Split("-");
        int StartingHour = int.Parse(TimeChunk[0].Substring(0, 2));
        int ClosingHour = int.Parse(TimeChunk[1].Substring(0, 2));
        int CurrentHour = DateTime.Now.Hour;
        if (CurrentHour >= StartingHour && CurrentHour <= ClosingHour)
        {
            int CurrentMinute = DateTime.Now.Minute;
            int StartingMinute = int.Parse(TimeChunk[0].Substring(2));
            int ClosingMinute = int.Parse(TimeChunk[1].Substring(2));
            if (CurrentMinute >= StartingMinute && CurrentMinute <= ClosingMinute)
            {
                //Open
            }
            else
            {
                //Closed
            }
        }
        else
        {
            //Closed
        }*/

        string MENURANGE = getResponse.Values[3][1].ToString();
        DRINKLIMIT = int.Parse(getResponse.Values[4][1].ToString());

        getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, MENURANGE);
        getResponse = await getRequest.ExecuteAsync();
        var values = getResponse.Values;

        MenuItems.Clear();
        InsideBasket.Clear();

        return values;
    }
    public async static Task PlaceOrder()
    {
        const string TodayRange = "Today!A:F";
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
                string cost = EVENT ? "$0.00" : $"${_cost:0.00}";

                if (quantity <= 0) continue;

                valueRange.Values.Add(new List<object> { DateRow, CustomerRow, ItemRow, details, quantity, cost }); ;
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
        var getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, $"Today!B{LatestCustomer + 2}:G");

        var getResponse = await getRequest.ExecuteAsync();

        return getResponse.Values;
    }
    public async static Task CompleteCustomer (int CompletedCustomerIndex)
    {
        string TodayRange = $"Today!G{CompletedCustomerIndex + 2}";
        var valueRange = new ValueRange() { Range = TodayRange, Values = new List<IList<object>>() };
        valueRange.Values.Add(new List<object> { "Y" });

        var update = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, TodayRange);
        update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
        await update.ExecuteAsync();
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
