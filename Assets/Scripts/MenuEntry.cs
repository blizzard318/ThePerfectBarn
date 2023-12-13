using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuEntry : MonoBehaviour
{
    public TextMeshProUGUI MenuName;
    [SerializeField] private TextMeshProUGUI QuantityText;
    public bool IsHot;
    internal int Quantity, Price;

    public void ChangeValue(int delta)
    {
        Quantity += delta;
        if (Quantity < 0) Quantity = 0;
        QuantityText.text = Quantity.ToString();
    }
}
