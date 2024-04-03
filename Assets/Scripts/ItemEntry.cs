using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class ItemEntry : MonoBehaviour
{
    [SerializeField] private GameObject AddToBasketBtn, Content;
    [SerializeField] private Segment segment;
    [HideInInspector] public UnityEvent OnSelection;
    private List<Segment> Segments = new List<Segment>();
    [SerializeField] private TextMeshProUGUI Name, Description, BaseDonationQty, BaseDonationText, QuantityText;
    private int Quantity = 1;
    private float BaseDonation, SingleDonationCost;
    private bool AddOn = false;
    private HashSet<string> OldPreset;

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

            if (GlobalOrderData.EVENT)
                AddToBasketBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Add to Basket";
            else
            {
                var TotalDonationCost = SingleDonationCost * Quantity;
                AddToBasketBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Add to Basket - {TotalDonationCost:0.00}";
            }
        });
    }
    public void OnEnable()
    {
        OldPreset = GlobalOrderData.Details;
        Content.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        foreach (var segment in Segments) Destroy(segment.gameObject);
        Segments.Clear();

        AddOn = GlobalOrderData.ExistingQuantity == 0;
        if (AddOn) GlobalOrderData.ExistingQuantity = 1;
        QuantityText.text = (Quantity = GlobalOrderData.ExistingQuantity).ToString();

        var chunk = GlobalOrderData.ActiveItemChunk;
        Name.text = chunk[0].ToString();
        Description.text = chunk[1].ToString();

        BaseDonationText.enabled = BaseDonationQty.enabled = !GlobalOrderData.EVENT;
        if (GlobalOrderData.EVENT) BaseDonation = 0;
        else BaseDonationQty.text = $"{BaseDonation = float.Parse(chunk[2].ToString()):0.00}";

        for (int i = 3, c = -1; i < chunk.Count; i++, c++)
        {
            var _segment = Instantiate(segment, Content.transform);
            Segments.Add(_segment);
            _segment.transform.SetSiblingIndex(_segment.transform.childCount + c);
            _segment.GenerateSelection(chunk[i].ToString(), GlobalOrderData.Details);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());
        gameObject.SetActive(true);
        OnSelection.Invoke();
    }

    public void Plus()
    {
        QuantityText.text = (++Quantity).ToString();
        OnSelection.Invoke();
    }
    public void Minus()
    {
        Quantity--;
        if (Quantity <= 0) Quantity = 0;
        QuantityText.text = Quantity.ToString();
        OnSelection.Invoke();
    }

    public void AddToBasket()
    {
        if (OldPreset == null && Quantity == 0)
        {
            GetComponentInParent<PageManager>().GoPrevious();
            return;
        }

        var data = new OrderData()
        {
            Name = Name.text,
            Quantity = Quantity,
            BaseDonationCost = SingleDonationCost
        };
        foreach (var segment in Segments) data.Details.Add(segment.Values.name);

        bool unique = true;
        OrderData ToRemove = null;
        foreach (var entry in GlobalOrderData.InsideBasket[data.Name])
        {
            if (OldPreset?.SetEquals(entry.Details) == true)
            {
                OldPreset = null;
                entry.Quantity = 0;
                if (entry.Details.SetEquals(data.Details))
                {
                    unique = false;
                    entry.Quantity += data.Quantity; //Add to preset
                }
                else ToRemove = entry; //Remove old preset
            }
            else if (entry.Details.SetEquals(data.Details))
            {
                unique = false;
                entry.Quantity += data.Quantity; //Add to different existing
                if (entry.Quantity == 0) ToRemove = entry;
            }
        }
        if (ToRemove != null) GlobalOrderData.InsideBasket[data.Name].Remove(ToRemove);
        if (unique) GlobalOrderData.InsideBasket[data.Name].Add(data);

        GetComponentInParent<PageManager>().GoPrevious();
    }
}
