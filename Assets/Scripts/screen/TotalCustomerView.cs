using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public class TotalCustomerView : MonoBehaviour
{
    [SerializeField] private GameObject CustomerPrefab;
    [SerializeField] private RectTransform CompleteScroll, PrepareScroll;
    private readonly List<string> CompleteCustomers = new List<string>();
    private readonly List<string> PrepareCustomers = new List<string>();
    private int LastRecordedCustomer = 0;

    private async Task RefreshCustomers()
    {
        var values = await GlobalOrderData.RefreshCustomers();
        if (values?.Count > 0)
        {
            if (values.Count > LastRecordedCustomer) //Add new customers
            {
                for (int i = LastRecordedCustomer; i < values.Count; i++)
                {
                    var row = values[i];
                    int Quantity = int.Parse(row[3].ToString());
                    if (Quantity <= 0) continue; //Don't render 0 drinks.

                    string name = row[0].ToString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        string customerString = $"{i + 1}. {name}";
                        if (row.Count >= 6) //Completed
                            CompleteCustomers.Add(customerString);
                        else
                            PrepareCustomers.Add(customerString);
                    }
                }
                LastRecordedCustomer = values.Count;
            }
            for (int i = GlobalOrderData.LatestCustomer; i < values.Count; i++)
            {
                var row = values[i];
                if (row.Count >= 6) //Completed
                {
                    string customerString = $"{i + 1}. {name}";
                    if (PrepareCustomers.Remove(customerString)) CompleteCustomers.Add(customerString);
                    if (GlobalOrderData.LatestCustomer + 1 == i) GlobalOrderData.LatestCustomer++;
                }
            }
        }

        for (var i = 0; i < CompleteScroll.childCount; i++) Destroy(CompleteScroll.GetChild(i).gameObject);
        for (int CustomerIndex = CompleteCustomers.Count - 1; CustomerIndex > 0; CustomerIndex--)
            Instantiate(CustomerPrefab, CompleteScroll).GetComponent<TextMeshProUGUI>().text = CompleteCustomers[CustomerIndex];
        LayoutRebuilder.ForceRebuildLayoutImmediate(CompleteScroll);

        for (var i = 0; i < PrepareScroll.childCount; i++) Destroy(PrepareScroll.GetChild(i).gameObject);
        for (int CustomerIndex = PrepareCustomers.Count - 1; CustomerIndex > 0; CustomerIndex--)
            Instantiate(CustomerPrefab, PrepareScroll).GetComponent<TextMeshProUGUI>().text = PrepareCustomers[CustomerIndex];
        LayoutRebuilder.ForceRebuildLayoutImmediate(PrepareScroll);

        await Task.Delay(1000);
        await RefreshCustomers();
    }

    private async void OnEnable() => await RefreshCustomers();
    private void OnDisable() => CancelInvoke();
}
