using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4.Data;

public class GenerateMenu : MonoBehaviour
{
    [SerializeField] private GameObject CategoryPrefab, ItemEntryPrefab;
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private TextMeshProUGUI Subtitle;

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
        var getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

        var getResponse = await getRequest.ExecuteAsync();
        var values = getResponse.Values;
        if (values != null && values.Count > 0)
        {
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
                    texts[0].text = value;
                    if (!string.IsNullOrWhiteSpace(row[1].ToString())) texts[1].text = row[1].ToString();
                    if (!EVENT) texts[2].text = $"${row[2]} Donation";
                }
            }
        }
    }

    private async void Awake()
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

        await RefreshMenu();
        //await UpdateData(new List<List<string>> { new List<string> { "test1" } });
    }
}
