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
    public bool Chosen = false; //Needed to be public

    public void GenerateSelection(string Chunk, HashSet<string> Preset = null)
    {
        string[] split = Chunk.Split(":");
        Name.text = split[0];
        string[] Choices = split[1].Split("/");

        const int BaseSize = 390;
        int height = (int)SelectionPrefab.GetComponent<RectTransform>().rect.height;
        int AddOn = height * (Choices.Length - 1);
        int NewHeight = BaseSize + AddOn;
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, NewHeight);

        if (Choices.Length == 1)
        {
            var selection = Instantiate(SelectionPrefab, transform).transform;
            selection.SetSiblingIndex(transform.childCount - 2);
            RadioButtons.Add(selection.GetChild(1).GetComponent<Image>());

            string[] values = Choices[0].Split("$");
            selection.GetChild(2).GetComponent<TextMeshProUGUI>().text = values[0];

            selection.GetChild(3).GetComponent<TextMeshProUGUI>().enabled = !GlobalOrderData.EVENT;
            var DonationCost = values.Length > 1 ? "+" + values[1] : "0.00";
            selection.GetChild(3).GetComponent<TextMeshProUGUI>().text = DonationCost; //donation

            selection.GetChild(0).GetComponent<Button>().enabled = false;

            Tick.gameObject.SetActive(Chosen = true);
            selection.GetChild(1).GetComponent<Image>().sprite = Selected;
            Values.name = selection.GetChild(2).GetComponent<TextMeshProUGUI>().text;
            Values.donationCost = float.Parse(selection.GetChild(3).GetComponent<TextMeshProUGUI>().text);
            GetComponentInParent<ItemEntry>(true).OnSelection.Invoke();
        }
        else
        {
            foreach (var choice in Choices)
            {
                var selection = Instantiate(SelectionPrefab, transform).transform;
                selection.SetSiblingIndex(transform.childCount - 2);
                RadioButtons.Add(selection.GetChild(1).GetComponent<Image>());

                string[] values = choice.Split("$");
                selection.GetChild(2).GetComponent<TextMeshProUGUI>().text = values[0]; //name

                selection.GetChild(3).GetComponent<TextMeshProUGUI>().enabled = !GlobalOrderData.EVENT;
                var DonationCost = values.Length > 1 ? "+" + values[1] : "0.00";
                selection.GetChild(3).GetComponent<TextMeshProUGUI>().text = DonationCost; //donation

                selection.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                {
                    Tick.gameObject.SetActive(Chosen = true);
                    foreach (var radioButton in RadioButtons) radioButton.sprite = Unselected;
                    selection.GetChild(1).GetComponent<Image>().sprite = Selected;
                    Values.name = selection.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                    Values.donationCost = float.Parse(selection.GetChild(3).GetComponent<TextMeshProUGUI>().text);
                    GetComponentInParent<ItemEntry>(true).OnSelection.Invoke();
                });

                if (Preset?.Contains(values[0]) == true)
                {
                    Preset = null;
                    selection.GetChild(0).GetComponent<Button>().onClick.Invoke();
                }
                else Tick.gameObject.SetActive(Chosen);
            }
        }
    }
}
