using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI ItemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    List<ItemBase> availableItem;
    List<ItemSlotUI> slotUIList;
    Action <ItemBase> onItemSelected;   
    Action onBack;


    int selectedItem;
    const int itemInViewport = 4;
    RectTransform itemListRec;
    private void Awake()
    {        
        itemListRec = itemList.GetComponent<RectTransform>();
    }

    public void Show(List<ItemBase> availableItem, Action<ItemBase> onItemSelected
        , Action onBack)
    {
        this.availableItem = availableItem;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;
        gameObject.SetActive(true);
        UpdateItemList();

    }

    public void HandleUpdate()
    {
        var prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++selectedItem;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
            }
        }

        selectedItem = Mathf.Clamp(selectedItem,0,availableItem.Count - 1);

        if(selectedItem != prevSelection)
        {
            UpdateItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            onItemSelected?.Invoke(availableItem[selectedItem]);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                onBack?.Invoke();
            }
        }

    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void UpdateItemList()
    {
        //Xoa cac Object item trong danh sach

        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }
        slotUIList = new List<ItemSlotUI>();
        foreach (var item in availableItem)
        {
            var slotUIObject = Instantiate(ItemSlotUI, itemList.transform);
            slotUIObject.SetNameAndPrice(item);


            slotUIList.Add(slotUIObject);
        }
        UpdateItemSelection();
    }

    void UpdateItemSelection()
    {
        
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItem.Count - 1);
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
        if (availableItem.Count > 0)
        {
            var item = availableItem[selectedItem];
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }
        HandleScrolling();
    }


    void HandleScrolling()
    {

        if (slotUIList.Count <= itemInViewport)
        {
            return;
        }

        float scrollPos = Mathf.Clamp(selectedItem - itemInViewport / 2, 0, selectedItem) * slotUIList[0].Height;
        itemListRec.localPosition = new Vector2(itemListRec.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);


    }

}
