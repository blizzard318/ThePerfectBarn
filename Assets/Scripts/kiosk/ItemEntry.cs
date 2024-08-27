using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class ItemEntry : MonoBehaviour
{
    [SerializeField] private GameObject AddToBasketBtn, Content;
    [SerializeField] private Segment segment;
    [SerializeField] private Image DrinkImage;
    [HideInInspector] public UnityEvent OnSelection;
    private List<Segment> Segments = new List<Segment>();
    [SerializeField] private TextMeshProUGUI Name, BaseDonationQty, QuantityText;
    private int Quantity = 1;
    private float BaseDonation, SingleDonationCost;
    private bool AddOn = false;
    private HashSet<string> OldPreset;

    private void Awake() => OnSelection.AddListener(() =>
    {
        const string Green = "#53AD5A", Grey = "#808080";
        string ColorCode;
        bool AbleToOrder = true;

        if (Quantity <= 0)
        {
            Quantity = 0;
            ColorCode = Green;
            string BasketString = OldPreset == null ? "Back to Menu" : "Remove Item";
            AddToBasketBtn.GetComponentInChildren<TextMeshProUGUI>().text = BasketString;
        }
        else
        {
            SingleDonationCost = BaseDonation;
            foreach (var segment in Segments)
            {
                SingleDonationCost += segment.Values.donationCost;
                if (!segment.Chosen) AbleToOrder = false;
            }
            ColorCode = AbleToOrder ? Green : Grey;

            string BasketString = OldPreset == null ? "Add to Basket" : "Update Basket";
            AddToBasketBtn.GetComponentInChildren<TextMeshProUGUI>().text = BasketString;
            if (!GlobalOrderData.EVENT)
            {
                var TotalDonationCost = SingleDonationCost * Quantity;
                AddToBasketBtn.GetComponentInChildren<TextMeshProUGUI>().text += $" - {TotalDonationCost:0.00}";
            }
        }
        ColorUtility.TryParseHtmlString(ColorCode, out Color color);
        AddToBasketBtn.GetComponent<Image>().color = color;
        AddToBasketBtn.GetComponent<Button>().enabled = AbleToOrder;
    });
    public void OnEnable()
    {
        OldPreset = GlobalOrderData.Details;
        Content.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        foreach (var segment in Segments) Destroy(segment.gameObject);
        Segments.Clear();

        AddOn = GlobalOrderData.ExistingQuantity == 0;
        if (AddOn) GlobalOrderData.ExistingQuantity = 1;
        QuantityText.text = (Quantity = GlobalOrderData.ExistingQuantity).ToString();

        //DrinkImage.sprite = Resources.Load<Sprite>("Images/" + GlobalOrderData.ActiveItem) ?? Resources.Load<Sprite>("Images/Default");

        StartCoroutine(DrinkImage.GetTexture(GlobalOrderData.ActiveItem));

        var chunk = GlobalOrderData.ActiveItemChunk;
        Name.text = $"<b>{chunk[0]}</b>" + System.Environment.NewLine + $"<size=80%><color=\"grey\">{chunk[1]}</size>";

        BaseDonationQty.enabled = !GlobalOrderData.EVENT;
        if (GlobalOrderData.EVENT) BaseDonation = 0;
        else BaseDonationQty.text = $"{BaseDonation = float.Parse(chunk[2].ToString()):0.00}" + System.Environment.NewLine + "<i><size=60%><color=\"grey\">Base donation</size></i>";

        for (int i = 4, c = -1; i < chunk.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(chunk[i].ToString())) continue;
            var _segment = Instantiate(segment, Content.transform);
            Segments.Add(_segment);
            _segment.transform.SetSiblingIndex(_segment.transform.childCount + c);
            _segment.GenerateSelection(chunk[i].ToString(), GlobalOrderData.Details);
            c++;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());
        gameObject.SetActive(true);
        OnSelection.Invoke();
    }

    public void Plus()
    {
        QuantityText.text = (++Quantity).ToString();
        if (Quantity > GlobalOrderData.DRINKLIMIT) Quantity = GlobalOrderData.DRINKLIMIT;
        OnSelection.Invoke();
    }
    public void Minus()
    {
        Quantity--;
        if (Quantity < 0) Quantity = 0;
        QuantityText.text = Quantity.ToString();
        OnSelection.Invoke();
    }

    public void AddToBasket()
    {
        if (Quantity == 0)
        {
            if (OldPreset != null)
                GlobalOrderData.ActiveBasket.RemoveAll(entry => OldPreset.SetEquals(entry.Details));
        }
        else
        {
            var data = new OrderData()
            {
                Name = GlobalOrderData.ActiveItem,
                Quantity = Quantity,
                DonationCost = SingleDonationCost
            };
            foreach (var segment in Segments) data.Details.Add(segment.Values.name);

            bool unique = true;
            OrderData ToRemove = null;
            foreach (var entry in GlobalOrderData.ActiveBasket)
            {
                if (OldPreset?.SetEquals(entry.Details) == true)
                {
                    OldPreset = null;
                    entry.Quantity = 0;
                    if (data.Quantity > 0 && entry.Details.SetEquals(data.Details))
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
            OldPreset = null;
            if (ToRemove != null) GlobalOrderData.ActiveBasket.Remove(ToRemove);
            if (unique) GlobalOrderData.ActiveBasket.Add(data);
        }

        GlobalOrderData.UpdateBasket();
        GetComponentInParent<PageManager>().GoPrevious();
    }
}
