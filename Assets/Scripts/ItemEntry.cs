using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class ItemEntry : MonoBehaviour
{
    [SerializeField] private GenerateMenu MenuScroll;
    [SerializeField] private GameObject AddToBasketBtn, Content;
    [SerializeField] private Segment segment;
    [HideInInspector] public UnityEvent OnSelection;
    private List<Segment> Segments = new List<Segment>();
    [SerializeField] private TextMeshProUGUI Name, Description, BaseDonationText, QuantityText;
    private int Quantity = 1;
    private float BaseDonation, SingleDonationCost;

    private void Awake()
    {
        OnSelection.AddListener(() =>
        {
            const string Green = "#53AD5A", Grey = "#808080";

            if (Quantity <= 0)
            {
                Quantity = 0;
                ColorUtility.TryParseHtmlString(Green, out Color green);
                AddToBasketBtn.GetComponent<Image>().color = green;
                AddToBasketBtn.GetComponent<Button>().enabled = true;
                AddToBasketBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Back to Menu";
                return;
            }

            bool AbleToOrder = true;
            SingleDonationCost = BaseDonation;
            foreach (var segment in Segments)
            {
                SingleDonationCost += segment.Values.donationCost;
                if (!segment.Chosen) AbleToOrder = false;
            }

            var ColorCode = AbleToOrder ? Green : Grey;
            AddToBasketBtn.GetComponent<Button>().enabled = AbleToOrder;
            ColorUtility.TryParseHtmlString(ColorCode, out Color color);
            AddToBasketBtn.GetComponent<Image>().color = color;

            var TotalDonationCost = SingleDonationCost * Quantity;
            AddToBasketBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Add to Basket - {TotalDonationCost:0.00}";
        });
    }
    public void GenerateSegments(IList<object> chunk)
    {
        Content.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        foreach (var segment in Segments) Destroy(segment.gameObject);
        Segments.Clear();

        QuantityText.text = (Quantity = 1).ToString();

        Name.text = chunk[0].ToString();
        Description.text = chunk[1].ToString();
        BaseDonation = float.Parse(BaseDonationText.text = chunk[2].ToString());
        for (int i = 3, c = -1; i < chunk.Count; i++, c++)
        {
            var _segment = Instantiate(segment, Content.transform);
            Segments.Add(_segment);
            _segment.transform.SetSiblingIndex(_segment.transform.childCount + c);
            _segment.GenerateSelection(chunk[i].ToString());
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());
        gameObject.SetActive(true);
        OnSelection.Invoke();
    }

    public void Close()
    {
        MenuScroll.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Plus()
    {
        QuantityText.text = (++Quantity).ToString();
        OnSelection.Invoke();
    }
    public void Minus()
    {
        Quantity--;
        if (Quantity <= 0)
        {
            Quantity = 0;
        }
        QuantityText.text = Quantity.ToString();
        OnSelection.Invoke();
    }

    public void AddToBasket()
    {
        var data = new OrderData()
        {
            Name = Name.text,
            Quantity = Quantity,
            BaseDonationCost = SingleDonationCost
        };
        foreach (var segment in Segments) data.Details.Add(segment.Values.name);

        MenuScroll.AddToBasket(data);
        Close();
    }
}
