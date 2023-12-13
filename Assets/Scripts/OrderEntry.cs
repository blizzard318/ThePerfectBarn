using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderEntry : MonoBehaviour
{
    public MenuEntry original;
    [SerializeField] private Color Hot, Iced;

    public void ToggleStrikeThrough()
    {
        var text = GetComponentInChildren<TextMeshProUGUI>();
        if (text.fontStyle == FontStyles.Normal)
        {
            text.fontStyle = FontStyles.Strikethrough;
            GetComponent<RawImage>().color = Color.gray;
        }
        else
        {
            text.fontStyle = FontStyles.Normal;
            GetComponent<RawImage>().color = original.IsHot ? Hot : Iced;
        }
    }
}
