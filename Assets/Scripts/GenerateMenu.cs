using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Auth.OAuth2;

public class GenerateMenu : MonoBehaviour
{
    [SerializeField] private GameObject MenuItemPrefab;
    [SerializeField] private RectTransform Scroll;

    private SheetsService _sheetsService;
    private const string _spreadsheetId = "1BO_oYW57E_xQZqt2UcAIrpRa6s5ZgK_nh4UD8Q5coZQ";

    private void ConnectToGoogle()
    {
        GoogleCredential credential;

        // Put your credentials json file in the root of the solution and make sure copy to output dir property is set to always copy 
        var filepath = Path.Combine(Application.streamingAssetsPath, "credentials.json");
        using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(new[] { SheetsService.Scope.Spreadsheets });
        }

        // Create Google Sheets API service.
        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "perfect-barn"
        });
    }
    public async Task<string> UpdateData(List<List<string>> data)
    {
        var objParams = new List<IList<object>>();
        foreach (var row in data) objParams.Add(new List<object> { row });

        const string range = "Menu!A1:B";
        
        // The new values to apply to the spreadsheet.
        var requestBody = new BatchUpdateValuesRequest()
        {
            ValueInputOption = "USER_ENTERED",
            Data = new List<ValueRange>
            {
                new ValueRange() { Range = range, Values = objParams }
            }
        };

        var request = _sheetsService.Spreadsheets.Values.BatchUpdate(requestBody, _spreadsheetId);

        var response = await request.ExecuteAsync(); // For async 

        return JsonUtility.ToJson(response);
    }

    async void Awake()
    {
        ConnectToGoogle();

        SpreadsheetsResource.ValuesResource.GetRequest getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, "Menu!A1:B");

        var getResponse = await getRequest.ExecuteAsync();
        var values = getResponse.Values;
        if (values != null && values.Count > 0)
        {
            foreach (var row in values)
            {
                Debug.Log(row[0]);
                Debug.Log(row[1]);
            }
        }

        await UpdateData(new List<List<string>> { new List<string> { "test1", "test2" }, new List<string> { "test3" } });
        return;
        string Filepath = Path.Combine(Application.persistentDataPath, "menu.txt");

        MenuItem[] items = null;
        if (!File.Exists(Filepath))
        {
            items = new MenuItem[]
            {
                new MenuItem("ESP", 100),
                new MenuItem("AM", 100, 150),
                new MenuItem("VAM", 130, 180),
                new MenuItem("FW", 150, 200),
                new MenuItem("Lat", 150, 200),
                new MenuItem("VLat", 180, 230),
                new MenuItem("Tea", 100),
            };
        }

        foreach (var item in items)
        {
            var go = Instantiate(MenuItemPrefab, Scroll);
            foreach (var side in go.GetComponentsInChildren<MenuEntry>())
            {
                side.MenuName.text = item.Name;
                side.Price = side.IsHot ? item.HotPrice : item.ColdPrice;
            }
        }
    }

    [Serializable] private sealed class MenuItem //For JSON serialization
    {
        public string Name;
        public int HotPrice, ColdPrice; // $1.00 = 100.
        public MenuItem (string Name, int HotPrice, int ColdPrice)
        {
            this.Name = Name;
            this.HotPrice = HotPrice;
            this.ColdPrice = ColdPrice;
        }
        public MenuItem(string Name, int Price) : this(Name, Price, Price) { }
    }
}
