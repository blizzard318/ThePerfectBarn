using TMPro;
using UnityEngine;

public class Receipt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Greeting, TotalText, TotalCost;
    public void OnEnable()
    {
        Greeting.text = $"God Bless You,\r\n{GlobalOrderData.CustomerName}!\r\n\r\nYour order is being prepared.";
        TotalText.enabled = TotalCost.enabled = !GlobalOrderData.EVENT;

        float TotalDonationCost = 0;
        foreach (var item in GlobalOrderData.InsideBasket) //Different details
            foreach (var variant in item.Value)
                TotalDonationCost += variant.Quantity * variant.BaseDonationCost;
        TotalCost.text = $"${TotalDonationCost:0.00}";
    }

    public void ReturnToMenu()
    {
        foreach (var item in GlobalOrderData.InsideBasket) item.Value.Clear();
        GlobalOrderData.CustomerName = string.Empty;
        GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.MENU);
    }
}
