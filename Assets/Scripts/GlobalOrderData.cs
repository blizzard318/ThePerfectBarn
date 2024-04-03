using System.Collections.Generic;

public static class GlobalOrderData
{
    public static string CustomerName;
    public static bool EVENT = false;
    public static readonly Dictionary<string, List<OrderData>> InsideBasket = new Dictionary<string, List<OrderData>>();
    public static readonly Dictionary<string, IList<object>> MenuItems = new Dictionary<string, IList<object>>();

    public static void ClearActiveItem()
    {
        ActiveItem = string.Empty;
        ExistingQuantity = 0;
        Details = null;
    }
    public static string ActiveItem;
    public static int ExistingQuantity;
    public static HashSet<string> Details;

    public static IList<object> ActiveItemChunk => MenuItems[ActiveItem];
}
