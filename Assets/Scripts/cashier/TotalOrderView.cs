using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public sealed class Customer
{
    public sealed class DrinkDetails
    {
        public string Name, Details;
        public int Quantity;
        public bool Complete = false;
    }
    public string Name;
    public int DrinksCompleted = 0, TotalDrinks = 0;
    public float TotalCost = 0;
    public List<DrinkDetails> Drinks;

}
public class TotalOrderView : MonoBehaviour
{
    [SerializeField] private GameObject CustomerPrefab, BlackOut;
    [SerializeField] private RectTransform Scroll;

    private async Task RefreshCustomers()
    {
        for (var i = 0; i < Scroll.childCount; i++) Destroy(Scroll.GetChild(i).gameObject);

        var values = await GlobalOrderData.RefreshCustomers();

        var Customers = new List<Customer>();
        string StoredDrinkName = string.Empty;
        foreach (var row in values)
        {
            int Quantity = int.Parse(row[3].ToString());
            if (Quantity <= 0) continue; //Don't render 0 drinks.

            string name = row[0].ToString();
            if (!string.IsNullOrWhiteSpace(name))
            {
                Customers.Add(new Customer
                {
                    Name = name,
                    Drinks = new List<Customer.DrinkDetails>()
                });
            }

            string drinkName = row[1].ToString();
            if (!string.IsNullOrWhiteSpace(drinkName)) StoredDrinkName = drinkName;

            Customers[Customers.Count - 1].Drinks.Add(new Customer.DrinkDetails
            {
                Name = drinkName,
                Details = row[2].ToString(),
                Quantity = Quantity,
            });

            string row4 = row[4].ToString();
            float Cost = float.Parse(row4.Substring(row4.IndexOf("$") + 1));
            Customers[Customers.Count - 1].TotalCost += Cost;
            Customers[Customers.Count - 1].TotalDrinks += Quantity;
        }

        foreach (var customer in Customers)
        {
            var customerPanel = Instantiate(CustomerPrefab, Scroll);
            customerPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{customer.Name}" + System.Environment.NewLine + $"${customer.TotalCost}   0/{customer.TotalDrinks}";

            customerPanel.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                CustomerMenu.ActiveCustomer = customer;
                GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.INDIVIDUAL);
            });

            customerPanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                var ConfirmPrompt = BlackOut.transform.GetChild(0);
                ConfirmPrompt.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Have you completed {customer.Name}'s order?";
                ConfirmPrompt.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
                ConfirmPrompt.GetChild(1).GetComponent<Button>().onClick.AddListener(() => Destroy(customerPanel));
                BlackOut.SetActive(true);
            });
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll);
    }

    private void OnEnable() => InvokeRepeating("RefreshCustomers", 0, 5);
    private void OnDisable() => CancelInvoke();
}
