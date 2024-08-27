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
        if (GlobalOrderData.TotalQuantityOfItem <= 0)
        {
            GetComponentInParent<PageManager>().GoToPage(PageManager.Page.PageTitle.MENU);
            return;
        }

        Scroll.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        foreach (var order in Orders) Destroy(order.gameObject);
        Orders.Clear();
        string ItemText = GlobalOrderData.TotalQuantityOfItem > 1 ? "Item" : "Items";

        BasketQuantity.text = $"Orders ({GlobalOrderData.TotalQuantityOfItem} {ItemText})";
        TotalText.enabled = BasketCost.enabled = !GlobalOrderData.EVENT;
        BasketCost.text = $"${GlobalOrderData.TotalDonationCost:0.00}";

        GlobalOrderData.ForEachBasket(order =>
        {
            foreach (var variant in order.Value)
            {
                var _order = Instantiate(OrderPrefab, Scroll.transform);
                Orders.Add(_order);
                _order.GenerateOrder(variant);
            }
        });
    }
}
