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

    public void AddItem(ItemBase item, int count = 1)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotByCategory(category);


        var itemSlot = currentSlot.FirstOrDefault(slot => slot.Item == item);
        if(itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlot.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        onUpdated?.Invoke();

    }

    public ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if(item is RecoveryItem)
        {
            return ItemCategory.Items;
        }
        else
        {
            if(item is PokeballItem)
            {
                return ItemCategory.Pokeballs;
            }
            else
            {
                return ItemCategory.Tms;
            }
        }
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currentSlot = GetSlotByCategory(categoryIndex);
        return currentSlot[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon, int selectedCategory)
    {

        var item = GetItem(itemIndex, selectedCategory);
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            if(!item.isReuseable)
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

    public ItemBase Item { get =>item; set=>item = value; }
    public int Count
    {
        get => count;
        set => count = value;
    }


}

public enum ItemCategory { Items, Pokeballs, Tms}