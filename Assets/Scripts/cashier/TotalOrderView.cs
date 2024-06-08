using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

[Serializable] public sealed class Customer
{
    [Serializable] public sealed class DrinkDetails
    {
        public string Name, Details;
        public int Completed, Total;
    }
    public string Name;
    public bool Collected = false;
    public float TotalCost = 0;
    public List<DrinkDetails> Drinks;

}
public class TotalOrderView : MonoBehaviour
{
    [SerializeField] private GameObject CustomerPrefab, BlackOut;
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private AudioClip NotificationSound;
    private readonly List<Customer> Customers = new List<Customer>();
    private int OldestIncompleteCustomerIndex = 0;

    private async Task RefreshCustomers()
    {
        bool NewCustomer = false;
        var values = await GlobalOrderData.RefreshCustomers();
        if (values?.Count > 0)
        {
            string StoredDrinkName = string.Empty;

            foreach (var row in values)
            {
                string name = row[0].ToString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (row.Count >= 6) //Completed
                    {
                        Customers.Add(new Customer() { Collected = true });
                        continue;
                    }
                    else Customers.Add(new Customer
                    {
                        Name = name,
                        Drinks = new List<Customer.DrinkDetails>()
                    });
                }

                string drinkName = row[1].ToString();
                if (!string.IsNullOrWhiteSpace(drinkName)) StoredDrinkName = drinkName;

                int Quantity = int.Parse(row[3].ToString());
                Customers[Customers.Count - 1].Drinks.Add(new Customer.DrinkDetails
                {
                    Name = StoredDrinkName,
                    Details = row[2].ToString(),
                    Total = Quantity
                });

                if (row.Count <= 3 || row[4].ToString() == "$0.00") continue;
                NewCustomer = true;
                string row4 = row[4].ToString();
                float Cost = float.Parse(row4.Substring(row4.IndexOf("$") + 1));
                Customers[Customers.Count - 1].TotalCost += Cost;
            }
            GlobalOrderData.LatestCustomer = Customers.Count;
        }
        if (NewCustomer)
            GetComponent<AudioSource>().PlayOneShot(NotificationSound);

        for (var i = 0; i < Scroll.childCount; i++) Destroy(Scroll.GetChild(i).gameObject);

        for (int CustomerIndex = OldestIncompleteCustomerIndex; CustomerIndex < Customers.Count; CustomerIndex++)
        {
            var customer = Customers[CustomerIndex];

            if (customer.Collected)
            {
                if (OldestIncompleteCustomerIndex + 1 == CustomerIndex) OldestIncompleteCustomerIndex++;
                continue;
            }

            int CompletedDrinks = 0, TotalDrinks = 0;
            foreach (var drink in customer.Drinks) //Variants
            {
                TotalDrinks += drink.Total;
                CompletedDrinks += drink.Completed;
            }

            var customerPanel = Instantiate(CustomerPrefab, Scroll);

            var green = new Color(0.3f, 0.7f, 0.4f);
            var clear = new Color(1, 1, 1, 0.1f);
            customerPanel.GetComponent<Image>().color = CompletedDrinks >= TotalDrinks ? green : clear;

            customerPanel.GetComponent<Button>().onClick.AddListener(() =>
            {
                CustomerMenu.ActiveCustomer = customer;
                GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.INDIVIDUAL);
            });

            var DisplayedName = $"{CustomerIndex + 1}. {customer.Name}" + Environment.NewLine;
            customerPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = DisplayedName + $"${customer.TotalCost}   {CompletedDrinks}/{TotalDrinks}";

            int CompletedIndex = CustomerIndex;
            //customerPanel.transform.GetChild(2).gameObject.SetActive(CompletedDrinks >= TotalDrinks);
            customerPanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                var ConfirmPrompt = BlackOut.transform.GetChild(0);
                ConfirmPrompt.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Has {customer.Name} collected their order?";
                ConfirmPrompt.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
                ConfirmPrompt.GetChild(1).GetComponent<Button>().onClick.AddListener(async () =>
                {
                    customer.Collected = true;
                    Destroy(customerPanel);
                    await GlobalOrderData.CompleteCustomer(CompletedIndex);
                });
                BlackOut.SetActive(true);
            });
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll);
        await Task.Delay(1000);
        await RefreshCustomers();
    }

    private async void OnEnable() => await RefreshCustomers();
}
