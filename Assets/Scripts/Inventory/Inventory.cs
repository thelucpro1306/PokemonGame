using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;

    public List<ItemSlot> Slots => slots;

    public event Action onUpdated;

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerMove>().GetComponent<Inventory>();
    }

    public ItemBase UseItem(int itemIdex, Pokemon selectedPokemon)
    {
        var item =  slots[itemIdex].Item;
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            RemoveItem(item);
            return item;
        }
        return null;
    }

    public void RemoveItem(ItemBase item)
    {
        var itemSlot = slots.First(slot => slot.Item == item);
        itemSlot.Count--;
        if(itemSlot.Count == 0)
        {
            slots.Remove(itemSlot);
        }

        onUpdated?.Invoke();

    }

}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item => item;
    public int Count
    {
        get => count;
        set => count = value;
    }


}
