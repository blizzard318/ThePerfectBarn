using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

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
    private readonly List<Customer> Customers = new List<Customer>();
    private int OldCount = 0;

    private async Task RefreshCustomers()
    {
        var values = await GlobalOrderData.RefreshCustomers();
        Debug.Log(OldCount + " " + values.Count);
        if (OldCount != values.Count)
        {
            OldCount = values.Count;

            string StoredDrinkName = string.Empty;
            int CustomerCounter = -1;
            bool AlreadyRecorded = false;
            foreach (var row in values)
            {
                int Quantity = int.Parse(row[3].ToString());
                if (Quantity <= 0) continue; //Don't render 0 drinks.

                string name = row[0].ToString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    CustomerCounter++;
                    if (Customers.Count > CustomerCounter && Customers[CustomerCounter].Name == name)
                        AlreadyRecorded = true;
                    else
                    {
                        AlreadyRecorded = false;
                        Customers.Add(new Customer
                        {
                            Name = name,
                            Drinks = new List<Customer.DrinkDetails>()
                        });
                    }
                }
                if (AlreadyRecorded) continue;

                string drinkName = row[1].ToString();
                if (!string.IsNullOrWhiteSpace(drinkName)) StoredDrinkName = drinkName;

                Customers[Customers.Count - 1].Drinks.Add(new Customer.DrinkDetails
                {
                    Name = StoredDrinkName,
                    Details = row[2].ToString(),
                    Total = Quantity
                });

                string row4 = row[4].ToString();
                float Cost = float.Parse(row4.Substring(row4.IndexOf("$") + 1));
                Customers[Customers.Count - 1].TotalCost += Cost;
            }
        }

        for (var i = 0; i < Scroll.childCount; i++) Destroy(Scroll.GetChild(i).gameObject);
        foreach (var customer in Customers)
        {
            if (customer.Collected) continue;

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

            customerPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{customer.Name}" + Environment.NewLine + $"${customer.TotalCost}   {CompletedDrinks}/{TotalDrinks}";

            customerPanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                var ConfirmPrompt = BlackOut.transform.GetChild(0);
                ConfirmPrompt.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Has {customer.Name} collected their order?";
                ConfirmPrompt.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
                ConfirmPrompt.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
                {
                    customer.Collected = true;
                    Destroy(customerPanel);
                });
                BlackOut.SetActive(true);
            });
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll);
    }

    private void OnEnable() => InvokeRepeating("RefreshCustomers", 0, 1);
    private void OnDisable() => CancelInvoke();
}
