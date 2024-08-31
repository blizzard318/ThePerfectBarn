using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Order : MonoBehaviour //Basket orders
{
    [SerializeField] private TextMeshProUGUI Quantity, Name, DonationTotal;
    public void GenerateOrder (OrderData data)
    {
        transform.SetAsLastSibling();
        Quantity.text = data.Quantity.ToString() + "x";
        Name.text = $"<b>{data.Name}</b>" + System.Environment.NewLine + $"<size=90%><color=\"grey\">{data.JoinedDetails}</size>";

        var TotalCost = data.DonationCost * data.Quantity;
        DonationTotal.text = GlobalOrderData.EVENT ? string.Empty : $"{TotalCost:0.00}";
        DonationTotal.text += System.Environment.NewLine + "<color=#419DEA><size=90%><u>Edit</u></size>";

        GetComponent<Button>().onClick.AddListener(() =>
        {
            GlobalOrderData.ActiveItem = data.Name;
            GlobalOrderData.ExistingQuantity = data.Quantity;
            GlobalOrderData.Details = data.Details;
            GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.ITEM);
        });
    }
}
