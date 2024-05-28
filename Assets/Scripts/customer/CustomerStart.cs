using TMPro;
using UnityEngine;
using System.Collections;
using Unity.Notifications;

public class CustomerStart : MonoBehaviour
{
    [SerializeField] private PageManager pm;
    [SerializeField] private TextMeshProUGUI Greeter;
    private void Awake()
    {
        var args = NotificationCenterArgs.Default;
        args.AndroidChannelId = "default";
        args.AndroidChannelName = "Notifications";
        args.AndroidChannelDescription = "Main notifications";
        NotificationCenter.Initialize(args);
    }

    private IEnumerator Start()
    {
        var request = NotificationCenter.RequestPermission();
        if (request.Status == NotificationsPermissionStatus.RequestPending) yield return request;

        string name = GlobalOrderData.CustomerName = PlayerPrefs.GetString("CustomerName");
        if (string.IsNullOrEmpty(name)) pm.GoToINPUT();
        else
        {
            Greeter.text += name + "!";
            pm.GoToRETURN();
        }
    }
}
