using TMPro;
using UnityEngine;

public class NameInput : MonoBehaviour
{
    [SerializeField] private GameObject ProceedToMenu;

    private void OnEnable()
    {
        GetComponentInChildren<TMP_InputField>().text = string.Empty;
        ProceedToMenu.SetActive(false);
    }
    public void OnNameInput(string name) => ProceedToMenu.SetActive(!string.IsNullOrWhiteSpace(name));
    public void NameDone(TMP_InputField nameInput) => GlobalOrderData.CustomerName = nameInput.text;
}
