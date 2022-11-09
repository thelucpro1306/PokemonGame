using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum InventoryUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI ItemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Text categoryText;


    Inventory inventory;
    int selectedItem = 0;
    int selectedCategory = 0;
    const int itemInViewport = 4;

    Action<ItemBase> onItemUsed;

    List<ItemSlotUI> slotUIList;
    RectTransform itemListRec;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;

    InventoryUIState state;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRec = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.onUpdated += UpdateItemList;

    }



    void UpdateItemList()
    {
        //Xoa cac Object item trong danh sach

        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }
        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotByCategory(selectedCategory))
        {
            var slotUIObject = Instantiate(ItemSlotUI, itemList.transform);
            slotUIObject.SetData(itemSlot);


            slotUIList.Add(slotUIObject);
        }


        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

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
                else
                {
                    if (Input.GetKeyUp(KeyCode.RightArrow))
                    {
                        ++selectedCategory;
                    }
                    else
                    {
                        if (Input.GetKeyUp(KeyCode.LeftArrow))
                        {
                            --selectedCategory;
                        }
                    }
                }
            }

            if(selectedCategory > Inventory.ItemCategorys.Count - 1)
            {
                selectedCategory = 0;
            }
            else
            {
                if(selectedCategory < 0)
                {
                    selectedCategory = Inventory.ItemCategorys.Count - 1;
                }
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotByCategory(selectedCategory).Count - 1);

            if (prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategorys[selectedCategory];
                UpdateItemList();
            }
            else
            {
                if (prevSelection != selectedItem)
                {
                    UpdateItemSelection();
                }
            }

            // Dung item
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ItemSelected();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    onBack?.Invoke();
                }
            }
        }
        else
        {
            if (state == InventoryUIState.PartySelection)
            {
                // party 

                Action onSelected = () =>
                {
                    // use Item on pokemon
                    StartCoroutine(UseItem());
                };

                Action onBackPartyScreen = () =>
                {
                    ClosePartyScreen();
                };

                partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
            }
        }
    }
    void UpdateItemSelection()
    {
        selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotByCategory(selectedCategory).Count - 1);
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

        

        if (inventory.GetSlotByCategory(selectedCategory).Count > 0)
        {
            var item = inventory.GetSlotByCategory(selectedCategory)[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }



        HandleScrolling();
    }

    void ItemSelected()
    {
        if(selectedCategory == (int) ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
        }
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

    void ResetSelection()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";

    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;
        var useItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
        if (useItem != null)
        {
            if(!(useItem is PokeballItem))
                yield return DialogManager.Instance.ShowDialogText($"Bạn đã dùng vật phẩm là {useItem.Name}");
            onItemUsed?.Invoke(useItem);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"Vật phẩm không có hiệu lực!");
        }
        ClosePartyScreen();
    }
    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
    }

}
