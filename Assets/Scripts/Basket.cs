using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Basket : MonoBehaviour
{
    [SerializeField] private GenerateMenu MenuScroll;
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private Order OrderPrefab;
    [SerializeField] private TextMeshProUGUI BasketQuantity, BasketCost;
    private readonly List<OrderData> InsideBasket = new List<OrderData>();
    private List<Order> Orders = new List<Order>();

    private void Awake()
    {
        
    }

    public void GenerateOrders(OrderData[] insideBasket)
    {
        InsideBasket.Clear();
        InsideBasket.AddRange(insideBasket);

        Scroll.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        foreach (var order in Orders) Destroy(order.gameObject);
        Orders.Clear();
        string ItemText = InsideBasket.Count > 1 ? "Item" : "Items";
        BasketQuantity.text = $"Orders ({InsideBasket.Count} {ItemText})";

        foreach (var order in InsideBasket)
        {
            var _order = Instantiate(OrderPrefab, Scroll.transform);
            Orders.Add(_order);
            _order.transform.SetAsLastSibling();
            _order.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{order.Quantity}x";
            _order.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = order.Name;
            _order.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = string.Join(System.Environment.NewLine, order.Details);
            _order.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"{order.BaseDonationCost * order.Quantity:0.00}";
            _order.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {

            });

            const int BaseSize = 250;
            int AddOn = 170 * (order.Details.Count - 1);
            int NewHeight = BaseSize + AddOn;
            GetComponent<RectTransform>().sizeDelta = new Vector2(0, NewHeight);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll.GetComponent<RectTransform>());

        gameObject.SetActive(true);
    }

    public void BackToMenu()
    {
        MenuScroll.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
