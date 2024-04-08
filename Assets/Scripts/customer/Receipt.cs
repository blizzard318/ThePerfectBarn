using TMPro;
using UnityEngine;

public class Receipt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Greeting, TotalText;
    public void OnEnable()
    {
        Greeting.text = $"God Bless You,\r\n{GlobalOrderData.CustomerName}!\r\n\r\nYour order is being prepared.";

        string TotalDonationCost = $"Total: ${GlobalOrderData.TotalDonationCost:0.00}";
        TotalText.text = GlobalOrderData.EVENT ? string.Empty : TotalDonationCost;
    }

    public void EmptyBasket() => GlobalOrderData.EmptyBasket();
}
