using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PageManager : MonoBehaviour
{
    [Serializable] public class Page
    {
        public enum PageTitle { MENU, ITEM, BASKET, RECEIPT };
        public PageTitle Title;
        public GameObject page;
    }
    [SerializeField] private Page[] Pages;
    private GameObject CurrentPage, PreviousPage;

    private async void Awake()
    {
        await GlobalOrderData.Initialize(Resources.Load<TextAsset>("credentials").text);
        foreach (var page in Pages)
        {
            if (page.page.activeSelf)
            {
                CurrentPage = page.page;
                break;
            }
        }
    }

    public void GoToPage(Page.PageTitle title)
    {
        CurrentPage?.SetActive(false);
        PreviousPage = CurrentPage;
        foreach (var page in Pages)
        {
            if (title == page.Title)
            {
                (CurrentPage = page.page).SetActive(true);
                break;
            }
        }
    }
    public void GoPrevious()
    {
        CurrentPage?.SetActive(false);
        PreviousPage?.SetActive(true);
        var interim = PreviousPage;
        PreviousPage = CurrentPage;
        CurrentPage = interim;
    }

    public void LoadCustomer() => SceneManager.LoadScene("Customer");
    public void LoadCashier() => SceneManager.LoadScene("Cashier");
}
