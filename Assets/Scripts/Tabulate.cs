using TMPro;
using UnityEngine;

public class Tabulate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TotalMoney;
    [SerializeField] private GameObject OrderItemPrefab;
    [SerializeField] private RectTransform Scroll;
    [SerializeField] private GenerateMenu gm;

    public void UpdateReceipt()
    {
        gameObject.SetActive(true);
        int totalAmount = 0;

        foreach (var item in gm.GetComponentsInChildren<MenuEntry>(true))
        {
            var subtotal = item.Price * item.Quantity;
            var itemName = (item.IsHot ? "Hot " : "Iced ") + item.MenuName;
            //receiptText.text += $"{item.Quantity}x {itemName} = {subtotal}\n";
            totalAmount += subtotal;
        }
        var dollars = totalAmount / 100;
        var cents = totalAmount % 100;

        TotalMoney.text = $"${dollars}.{cents}";
    }
}
