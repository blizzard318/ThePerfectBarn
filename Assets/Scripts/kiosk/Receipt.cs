using TMPro;
using System;
using UnityEngine;
using System.Collections;
using Unity.Notifications;
using System.Threading.Tasks;

public class Receipt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Greeting, TotalText, Loading;
    [SerializeField] private GameObject NextOrder;
    [SerializeField] private bool EnableNotification = false;

    public async void OnEnable()
    {
        NextOrder.SetActive(false);
        var coroutine = StartCoroutine(LoadingText());
        Greeting.text = $"God Bless You,\r\n{GlobalOrderData.CustomerName}!\r\n\r\nYour order is being prepared.";

        string TotalDonationCost = $"Total: ${GlobalOrderData.TotalDonationCost:0.00}";

        TotalText.text = GlobalOrderData.EVENT ? string.Empty : TotalDonationCost;
        var cell = await GlobalOrderData.PlaceOrder();
        if (coroutine != null) StopCoroutine(coroutine);
        NextOrder.SetActive(true);
        if (EnableNotification) await Task.Run(async () =>
        {
            while (!await GlobalOrderData.IsOrderDone(cell)) await Task.Delay(1000);
            var notif = new Notification()
            {
                Badge = 1,
                ShowInForeground = true,
                Title = "Your drink is ready!",
                Group = GlobalOrderData.NotificationGroup,
                Text = $"God bless you, {GlobalOrderData.CustomerName}" + Environment.NewLine + "Please proceed to Level 3 to collect your drink.",
            };
            var time = new NotificationDateTimeSchedule(DateTime.Now);
            NotificationCenter.ScheduleNotification(notif, GlobalOrderData.NotificationGroup, time);
            Debug.Log(notif.Title);
        });

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
