using TMPro;
using UnityEngine;

public class Receipt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Greeting, TotalText;
    [SerializeField] private GameObject NextOrder;
    public async void OnEnable()
    {
        NextOrder.SetActive(false);
        Greeting.text = $"God Bless You,\r\n{GlobalOrderData.CustomerName}!\r\n\r\nYour order is being prepared.";

        string TotalDonationCost = $"Total: ${GlobalOrderData.TotalDonationCost:0.00}";
        TotalText.text = GlobalOrderData.EVENT ? string.Empty : TotalDonationCost;
        await GlobalOrderData.PlaceOrder();
        NextOrder.SetActive(true);
    }

    public void EmptyBasket() => GlobalOrderData.EmptyBasket();
}
