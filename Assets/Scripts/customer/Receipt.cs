using TMPro;
using UnityEngine;

public class Receipt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Greeting, TotalText;
    public void OnEnable()
    {
        Greeting.text = $"God Bless You,\r\n{GlobalOrderData.CustomerName}!\r\n\r\nYour order is being prepared.";

        float TotalDonationCost = 0;
        foreach (var item in GlobalOrderData.InsideBasket) //Different details
            foreach (var variant in item.Value)
                TotalDonationCost += variant.Quantity * variant.BaseDonationCost;
        TotalText.text = GlobalOrderData.EVENT ? string.Empty : $"Total: ${TotalDonationCost:0.00}";
    }

    public void EmptyBasket()
    {
        foreach (var entry in GlobalOrderData.InsideBasket) entry.Value.Clear();
    }
}
