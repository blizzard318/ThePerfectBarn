using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class Basket : MonoBehaviour
{
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private Order OrderPrefab;
    [SerializeField] private TextMeshProUGUI BasketQuantity, TotalText, BasketCost;
    [SerializeField] private ItemEntry ItemMenu;
    private readonly List<Order> Orders = new List<Order>();

    public void OnEnable()
    {
        Scroll.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        foreach (var order in Orders) Destroy(order.gameObject);
        Orders.Clear();
        string ItemText = GlobalOrderData.InsideBasket.Count > 1 ? "Item" : "Items";

        int TotalQuantityOfItem = 0;
        float TotalDonationCost = 0;
        foreach (var item in GlobalOrderData.InsideBasket) //Different details
        {
            foreach (var variant in item.Value)
            {
                TotalQuantityOfItem += variant.Quantity;
                TotalDonationCost += variant.Quantity * variant.BaseDonationCost;
            }
        }

        BasketQuantity.text = $"Orders ({TotalQuantityOfItem} {ItemText})";
        TotalText.enabled = BasketCost.enabled = !GlobalOrderData.EVENT;
        BasketCost.text = $"${TotalDonationCost:0.00}";

        foreach (var order in GlobalOrderData.InsideBasket)
        {
            foreach (var variant in order.Value)
            {
                var _order = Instantiate(OrderPrefab, Scroll.transform);
                Orders.Add(_order);
                _order.GenerateOrder(variant);
            }
        }
        gameObject.SetActive(true);
    }
    public void ConfirmOrder()
    {
        GlobalOrderData.PlaceOrder();
        GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.RECEIPT);
    }
}
