using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;

    public List<ItemSlot> Slots => slots;

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerMove>().GetComponent<Inventory>();
    }

}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item => item;
    public int Count => count;  

}
