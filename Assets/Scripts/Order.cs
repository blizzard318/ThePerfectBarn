using TMPro;
using UnityEngine;

public class Order : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Quantity, Name, Details, DonationTotal;
    public void GenerateOrder (OrderData data)
    {
        const int BaseSize = 250;
        int AddOn = 50 * (data.Details.Count - 1);
        int NewHeight = BaseSize + AddOn;
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, NewHeight);

        Quantity.text = data.Quantity.ToString() + "x";
        Name.text = data.Name;
        Details.text = string.Join(System.Environment.NewLine, Details);
        DonationTotal.text = $"{data.BaseDonationCost * data.Quantity:0.00}";
    }
}
