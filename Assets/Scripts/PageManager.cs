using System;
using UnityEngine;
using static PageManager.Page;

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

    private void Awake()
    {
        foreach (var page in Pages)
        {
            if (page.page.activeSelf)
            {
                CurrentPage = page.page;
                break;
            }
        }
    }

    public void GoToPage(PageTitle title)
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
}
