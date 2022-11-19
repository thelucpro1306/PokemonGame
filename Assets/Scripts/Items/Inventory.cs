using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> tmSlots;


    List<List<ItemSlot>> allSlots;
    public static List<string> ItemCategorys { get; private set; } = new List<string>()
    {
        "Vật phẩm",
        "POKEBALLS", //this is vật phẩm
        "TMs & HMs",

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

    public int GetItemCount(ItemBase item)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotByCategory(category);
        var itemSlot = currentSlot.FirstOrDefault(slot => slot.Item == item);

        if(itemSlot != null)
        {
            return itemSlot.Count;
        }
        else
        {
            return 0;
        }

    }
    public void AddItem(ItemBase item, int count = 1)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotByCategory(category);
        var itemSlot = currentSlot.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
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
        if (item is RecoveryItem || item is EvolutionItem)
        {
            return ItemCategory.Items;
        }
        else
        {
            if (item is PokeballItem)
            {
                return ItemCategory.Pokeballs;
            }
            else
            {
                return ItemCategory.Tms;
            }
        }
    }

    public bool HasItem(ItemBase item)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotByCategory(category);

        //kiem tra vat pham co ton tai trong inventory khong
        return currentSlot.Exists(p=>p.Item == item);

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
            if (!item.isReuseable)
                RemoveItem(item);
            return item;
        }
        return null;
    }

    public void RemoveItem(ItemBase item,int countToRemove = 1)
    {
        // tìm vật phẩm từ danh mục và lấy slot của vật phẩm đó
        int category = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotByCategory(category);

        var itemSlot = currentSlot.First(slot => slot.Item == item);
        itemSlot.Count -= countToRemove;
        if (itemSlot.Count == 0)
        {
            currentSlot.Remove(itemSlot);
        }

        onUpdated?.Invoke();

    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList(),
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();

        

        allSlots = new List<List<ItemSlot>>()
        {
            slots,pokeballSlots,tmSlots
        };
        onUpdated?.Invoke();
    }
}




[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }

    public ItemSlot(ItemSavaData savaData)
    {
        this.item = ItemDB.GetObjectByName(savaData.name);
        count = savaData.count;
    }

    public ItemSavaData GetSaveData()
    {
        var saveData = new ItemSavaData()
        {
            name = item.name,
            count = count
        };
        return saveData;
    }

    public ItemBase Item { get => item; set => item = value; }
    public int Count
    {
        get => count;
        set => count = value;
    }


}

public enum ItemCategory { Items, Pokeballs, Tms }

[Serializable]
public class ItemSavaData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSavaData> items;
    public List<ItemSavaData> pokeballs;
    public List<ItemSavaData> tms;

}
//Sửa tí
