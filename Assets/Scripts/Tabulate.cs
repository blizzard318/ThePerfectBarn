using TMPro;
using UnityEngine;

public class Tabulate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TotalMoney;
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private GameObject OrderItemPrefab;

    public void UpdateReceipt()
    {
        for (int i = 0; i < Scroll.childCount; i++) Destroy(Scroll.GetChild(i).gameObject);

        int totalAmount = 0;

        var gm = GetComponentInParent<GenerateMenu>();
        foreach (var item in gm.GetComponentsInChildren<MenuEntry>())
        {
            if (item.Quantity == 0) continue;
            var subtotal = item.Price * item.Quantity;
            totalAmount += subtotal;

            for (int i = 0; i < item.Quantity; i++)
                Instantiate(OrderItemPrefab, Scroll).GetComponentInChildren<OrderEntry>().Init(item);
        }
        var dollars = totalAmount / 100;
        var cents = totalAmount % 100;
        TotalMoney.text = $"${dollars}.{cents:00}";
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) gameObject.SetActive(false);
    }
    public void Clear()
    {
        var gm = GetComponentInParent<GenerateMenu>();
        foreach (var item in gm.GetComponentsInChildren<MenuEntry>()) item.ChangeValue(-item.Quantity);
    }
}
