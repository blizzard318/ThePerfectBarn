using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;

public class ItemEntry : MonoBehaviour
{
    [SerializeField] private GenerateMenu MenuScroll;
    [SerializeField] private GameObject AddToBasketBtn, Content;
    [SerializeField] private Segment segment;
    [HideInInspector] public UnityEvent OnSelection;
    private List<Segment> Segments = new List<Segment>();
    [SerializeField] private TextMeshProUGUI Name, Description, BaseDonation;
    private float TotalDonationCost = 0;

    private void Awake()
    {
        OnSelection.AddListener(() =>
        {
            TotalDonationCost = 0;
            foreach (var segment in Segments)
            {
                TotalDonationCost += segment.Values.donationCost;
                if (!segment.Chosen) return;
            }
            ColorUtility.TryParseHtmlString("#53AD5A", out Color color);
            AddToBasketBtn.GetComponent<Image>().color = color;
            AddToBasketBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Add to Basket - {TotalDonationCost:0.00}";
            AddToBasketBtn.SetActive(true);
        });
    }
    public void GenerateSegments(IList<object> chunk)
    {
        foreach (var segment in Segments) Destroy(segment.gameObject);
        Segments.Clear();

        AddToBasketBtn.SetActive(false);
        Name.text = chunk[0].ToString();
        Description.text = chunk[1].ToString();
        BaseDonation.text = chunk[2].ToString();
        for (int i = 3; i < chunk.Count; i++)
        {
            var _segment = Instantiate(segment, Content.transform);
            Segments.Add(_segment);
            _segment.transform.SetAsLastSibling();
            _segment.GenerateSelection(chunk[i].ToString());
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());
        gameObject.SetActive(true);
    }

    public void Close()
    {
        MenuScroll.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void AddToBasket()
    {
        MenuScroll.AddToBasket(Name.text, TotalDonationCost);
        Close();
    }
}
