using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderEntry : MonoBehaviour
{
    private MenuEntry original;
    private bool Complete;
    [SerializeField] private GameObject Toggle;
    public TextMeshProUGUI OrderItemName;

    public void Init (MenuEntry original)
    {
        this.original = original;
        var itemName = (original.IsHot ? "Hot " : "Iced ") + original.MenuName.text;

        var dollars = original.Price / 100;
        var cents = original.Price % 100;

        OrderItemName.text = $"{itemName}:${dollars}.{cents:00}";
        GetComponent<RawImage>().color = original.IsHot ? new Color(0.7f, 0, 0) : new Color(0.7f, 1, 1);
    }

    public void ToggleStrikeThrough()
    {
        Complete = !Complete;

        Toggle.SetActive(Complete);
        if (Complete)
        {
            GetComponent<RawImage>().color = Color.gray;
            OrderItemName.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
        else
        {
            GetComponent<RawImage>().color = original.IsHot ? Color.red : Color.cyan;
            OrderItemName.color = Color.white;
        }
    }
    public void DestroySelf()
    {
        original.ChangeValue(-1);
        Destroy(gameObject);
        GetComponentInParent<Tabulate>().UpdateReceipt();
    }
}
