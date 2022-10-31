using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI ItemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    Inventory inventory;
    int selectedItem = 0;
    const int itemInViewport = 8;


    List<ItemSlotUI> slotUIList;
    RectTransform itemListRec;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRec = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();
    }

    void UpdateItemList() 
    {
        //Xoa cac Object item trong danh sach

        foreach(Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }
        slotUIList = new List<ItemSlotUI> ();
        foreach(var itemSlot in inventory.Slots)
        {
            var slotUIObject = Instantiate(ItemSlotUI, itemList.transform);
            slotUIObject.SetData(itemSlot);


            slotUIList.Add(slotUIObject);   
        }


        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack)
    {
        int prevSelection = selectedItem;
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            ++selectedItem;
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                --selectedItem;
            }
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, inventory.Slots.Count - 1);

        if (prevSelection != selectedItem)
        {
            UpdateItemSelection();
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            onBack?.Invoke();
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
            {
                slotUIList[i].NameText.color = GlobalSetting.i.HighlightedColor;
            }
            else
            {
                slotUIList[i].NameText.color = Color.black;
            }
        }

        var item = inventory.Slots[selectedItem].Item;
        itemIcon.sprite = item.Icon;
        itemDescription.text = item.Description;

        HandleScrolling();
    }

    void HandleScrolling()
    {
        float scrollPos = Mathf.Clamp(selectedItem - itemInViewport/2, 0, selectedItem) * slotUIList[0].Height;
        itemListRec.localPosition = new Vector2(itemListRec.localPosition.x,scrollPos);

        bool showUpArrow = selectedItem > itemInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);  


    }

}
