using TMPro;
using UnityEngine;

public class NameInput : MonoBehaviour
{
    [SerializeField] private GameObject ProceedToMenu;

    private void OnEnable()
    {
        GetComponentInChildren<TMP_InputField>().text = GlobalOrderData.CustomerName = string.Empty;
        ProceedToMenu.SetActive(false);
    }
    private void OnDisable() => GlobalOrderData.CustomerName = GetComponentInChildren<TMP_InputField>().text;
    public void OnNameInput(string name) => ProceedToMenu.SetActive(!string.IsNullOrWhiteSpace(name));
    public void SaveName() => PlayerPrefs.SetString("CustomerName", GetComponentInChildren<TMP_InputField>().text);
}
