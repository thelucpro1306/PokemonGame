using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDB 
{
    public static Dictionary<string, ItemBase> items;

    public static void Init()
    {

        items = new Dictionary<string, ItemBase>();
        var itemList = Resources.LoadAll<ItemBase>("");

        foreach (var item in itemList)
        {
            if (items.ContainsKey(item.Name))
            {
                Debug.LogError($"Có 2 vật phẩm cùng tên  {item.Name}");
                continue;
            }

            items[item.Name] = item;
        }
    }

    public static ItemBase GetItemByName(string name)
    {
        if (!items.ContainsKey(name))
        {
            Debug.LogError($"Vật phẩm {name} không có trong DB");
        }
        return items[name];
    }
}
