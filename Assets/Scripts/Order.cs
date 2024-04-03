using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Order : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Quantity, Name, Details, DonationTotal;
    [SerializeField] private Button Edit;
    public void GenerateOrder (OrderData data)
    {
        transform.SetAsLastSibling();
        Quantity.text = data.Quantity.ToString() + "x";
        Name.text = data.Name;
        Details.text = string.Join(", ", data.Details);

        DonationTotal.enabled = !GlobalOrderData.EVENT;
        DonationTotal.text = $"{data.BaseDonationCost * data.Quantity:0.00}";

        Edit.onClick.AddListener(() =>
        {
            GlobalOrderData.ActiveItem = data.Name;
            GlobalOrderData.ExistingQuantity = data.Quantity;
            GlobalOrderData.Details = data.Details;
            GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.ITEM);
        });
    }
}
