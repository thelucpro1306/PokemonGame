using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> tmSlots;


    List<List<ItemSlot>> allSlots;


    public static List<string> ItemCategorys { get; private set; } = new List<string>()
    {
        "V?t ph?m", 
        "POKEBALLS", //this is v?t ph?m
        "TMs & HMs"
    };



    public event Action onUpdated;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>()
        {
            slots,pokeballSlots,tmSlots
        };
    }

    public List<ItemSlot> GetSlotByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerMove>().GetComponent<Inventory>();
    }

    public ItemBase UseItem(int itemIdex, Pokemon selectedPokemon, int selectedCategory)
    {
        var currentSlot =  GetSlotByCategory(selectedCategory);
        var item = currentSlot[itemIdex].Item;
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            RemoveItem(item,selectedCategory);
            return item;
        }
        return null;
    }

    public void RemoveItem(ItemBase item, int category)
    {
        var currentSlot = GetSlotByCategory(category);
        var itemSlot = currentSlot.First(slot => slot.Item == item);
        itemSlot.Count--;
        if(itemSlot.Count == 0)
        {
            currentSlot.Remove(itemSlot);
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
