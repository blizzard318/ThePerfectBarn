using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PageManager : MonoBehaviour
{
    [Serializable] public class Page
    {
        public enum PageTitle { INPUT, MENU, ITEM, BASKET, RECEIPT, CUSTOMERS, INDIVIDUAL };
        public PageTitle Title;
        public GameObject page;
    }
    [SerializeField] private Page.PageTitle StartingPage;
    [SerializeField] private Page[] Pages;
    [SerializeField] private GameObject CurrentPage, PreviousPage;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        GlobalOrderData.Initialize(Resources.Load<TextAsset>("credentials").text);
        foreach (var page in Pages)
        {
            if (page.Title == StartingPage)
            {
                PreviousPage = CurrentPage = page.page;
                CurrentPage.SetActive(true);
            }
            else page.page.SetActive(false);
        }
    }

    public void GoToPage(Page.PageTitle title)
    {
        PreviousPage = CurrentPage;
        foreach (var page in Pages)
        {
            if (title == page.Title) (CurrentPage = page.page).SetActive(true);
            else page.page.SetActive(false);
        }
    }
    public void GoPrevious()
    {
        var interim = PreviousPage;
        PreviousPage = CurrentPage;
        CurrentPage = interim;
        PreviousPage.SetActive(false);
        CurrentPage.SetActive(true);
    }
    public void GoToINPUT() => GoToPage(Page.PageTitle.INPUT);
    public void GoToMENU() => GoToPage(Page.PageTitle.MENU);
    public void GoToITEM() => GoToPage(Page.PageTitle.ITEM);
    public void GoToBASKET() => GoToPage(Page.PageTitle.BASKET);
    public void GoToRECEIPT() => GoToPage(Page.PageTitle.RECEIPT);
    public void GoToCUSTOMERS() => GoToPage(Page.PageTitle.CUSTOMERS);
    public void GoToINDIVIDUAL() => GoToPage(Page.PageTitle.INDIVIDUAL);

    public void LoadCustomer() => SceneManager.LoadScene("Customer");
    public void LoadCashier() => SceneManager.LoadScene("Cashier");
}
