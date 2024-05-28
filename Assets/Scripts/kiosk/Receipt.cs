using TMPro;
using UnityEngine;
using System.Collections;

public class Receipt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Greeting, TotalText, Loading;
    [SerializeField] private GameObject NextOrder;

    public async void OnEnable()
    {
        NextOrder.SetActive(false);
        var coroutine = StartCoroutine(LoadingText());
        Greeting.text = $"God Bless You,\r\n{GlobalOrderData.CustomerName}!\r\n\r\nYour order is being prepared.";

        string TotalDonationCost = $"Total: ${GlobalOrderData.TotalDonationCost:0.00}";
        TotalText.text = GlobalOrderData.EVENT ? string.Empty : TotalDonationCost;
        await GlobalOrderData.PlaceOrder();
        if (coroutine != null) StopCoroutine(coroutine);
        NextOrder.SetActive(true);

        IEnumerator LoadingText()
        {
            const string AddOn = ".";
            var duration = new WaitForSeconds(0.5f);
            while (!NextOrder.activeSelf)
            {
                Loading.text = "Now sending your order";
                for (int i = 0; i <= 3; i++)
                {
                    yield return duration;
                    Loading.text += AddOn;
                }
            }
        }
    }

    public void EmptyBasket() => GlobalOrderData.EmptyBasket();
}
