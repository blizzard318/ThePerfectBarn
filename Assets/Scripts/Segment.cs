using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

class Segment : MonoBehaviour
{
    [SerializeField] private GameObject SelectionPrefab;
    [SerializeField] private Sprite Unselected, Selected;
    [SerializeField] private Image Tick;
    [SerializeField] private TextMeshProUGUI Name;
    private List<Image> RadioButtons = new List<Image>();

    public (string name, float donationCost) Values;
    public bool Chosen = false;

    public void GenerateSelection(string Chunk)
    {
        string[] split = Chunk.Split(":");
        Name.text = split[0];
        string[] Choices = split[1].Split("/");

        const int BaseSize = 390;
        int AddOn = 170 * (Choices.Length - 1);
        int NewHeight = BaseSize + AddOn;
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, NewHeight);

        foreach (var choice in Choices)
        {
            var selection = Instantiate(SelectionPrefab, transform).transform;
            selection.SetSiblingIndex(transform.childCount - 2);
            RadioButtons.Add(selection.GetChild(0).GetComponent<Image>());

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
