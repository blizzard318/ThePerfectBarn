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

        foreach (var choice in Choices)
        {
            selection.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
            {
                Tick.gameObject.SetActive(Chosen = true);
                foreach (var radioButton in RadioButtons) radioButton.sprite = Unselected;
                selection.GetChild(0).GetComponent<Image>().sprite = Selected;
                Values.name = selection.GetChild(1).GetComponent<TextMeshProUGUI>().text;
                Values.donationCost = float.Parse(selection.GetChild(2).GetComponent<TextMeshProUGUI>().text);
                GetComponentInParent<ItemEntry>().OnSelection.Invoke();
            });

            string[] values = choice.Split("$");
            selection.GetChild(1).GetComponent<TextMeshProUGUI>().text = values[0];
            var DonationCost = values.Length > 1 ? values[1] : "0.00";
            selection.GetChild(2).GetComponent<TextMeshProUGUI>().text = DonationCost;
        }
    }
}
