using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GenerateMenu : MonoBehaviour
{
    [SerializeField] private GameObject MenuItemPrefab;
    [SerializeField] private RectTransform Scroll;

    void Awake()
    {
        string Filepath = Path.Combine(Application.persistentDataPath, "menu.txt");

        MenuItem[] items = null;
        if (!File.Exists(Filepath))
        {
            items = new MenuItem[]
            {
                new MenuItem("ESP", 100),
                new MenuItem("AM", 100, 150),
                new MenuItem("VAM", 130, 180),
                new MenuItem("FW", 150, 200),
                new MenuItem("Lat", 150, 200),
                new MenuItem("VLat", 180, 230),
                new MenuItem("Tea", 100),
            };
        }

        foreach (var item in items)
        {
            var go = Instantiate(MenuItemPrefab, Scroll);
            foreach (var side in go.GetComponentsInChildren<MenuEntry>())
            {
                side.MenuName.text = item.Name;
                side.Price = side.IsHot ? item.HotPrice : item.ColdPrice;
            }
        }
    }

    [Serializable] private sealed class MenuItem //For JSON serialization
    {
        public string Name;
        public int HotPrice, ColdPrice; // $1.00 = 100.
        public MenuItem (string Name, int HotPrice, int ColdPrice)
        {
            this.Name = Name;
            this.HotPrice = HotPrice;
            this.ColdPrice = ColdPrice;
        }
        public MenuItem(string Name, int Price) : this(Name, Price, Price) { }
    }
}
