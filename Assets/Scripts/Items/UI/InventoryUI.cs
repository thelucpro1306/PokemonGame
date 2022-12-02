using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public enum InventoryUIState { ItemSelection, PartySelection, MoveToForget, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI ItemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Text categoryText;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] WalletUI walletUI;
    Inventory inventory;
    int selectedItem = 0;
    int selectedCategory = 0;
    const int itemInViewport = 4;
    MoveBase moveToLearn;

    Action<ItemBase> onItemUsed;

    List<ItemSlotUI> slotUIList;
    RectTransform itemListRec;
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

            if (selectedCategory > Inventory.ItemCategorys.Count - 1)
            {
                selectedCategory = 0;
            }
            else
            {
                if (selectedCategory < 0)
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
                StartCoroutine( ItemSelected());
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
            else
            {
                if (state == InventoryUIState.MoveToForget)
                {

                    Action<int> onMoveSelected = (int moveIndex) =>
                    {
                        StartCoroutine(OnMoveToForgetSelected(moveIndex));
                    };

                    moveSelectionUI.HandleMoveSelection(onMoveSelected);
                }
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

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if(GameController.Instance.State == GameState.Shop)
        {
            onItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }


        if(GameController.Instance.State == GameState.Battle)
        {
            // trong tran chien
            if (!item.canUsedInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"{item.Name} không thể dùng trong chiến đấu!");
                state = InventoryUIState.ItemSelection;
                yield break;    
            }
        }
        else
        {
            if (!item.canUsedOutSideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"{item.Name} không thể dùng ngoài chiến đấu!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        if (selectedCategory == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
            if(item is TmItem)
            {
                //kiểm tra xem pokemon có thể dùng item không
                partyScreen.ShowIfTmItemCanUsable(item as TmItem);
            }
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

    IEnumerator HandleTmItems()
    {
        var tmItem = inventory.GetItem(selectedItem, selectedCategory) as TmItem;
        if (tmItem == null)
        {
            yield break;
        }
        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} đã học chiêu {tmItem.Move.Name} trước đó!");
            yield break;
        }

        if (!tmItem.canBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} không thể học chiêu {tmItem.Move.Name}!");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} đã học chiêu {tmItem.Move.Name}!");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} đang cố gắng học chiêu {tmItem.Move.Name}!");
            yield return DialogManager.Instance.ShowDialogText($"Nhưng không thể học nhiều hơn {PokemonBase.MaxNumOfMoves} chiêu!");
            yield return ChooseMoveToForget(pokemon, tmItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = InventoryUIState.MoveToForget;
        yield return DialogManager.Instance.ShowDialogText($"Chọn một chiêu để lãng quên:" , true, false);
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(p => p.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleTmItems();

        var item = inventory.GetItem(selectedItem, selectedCategory);
        var pokemon = partyScreen.SelectedMember;


        // Xử lý tiến hoá bằng vật phẩm
        if (item is EvolutionItem)
        {
            var evolution = pokemon.CheckForEvolution(item);
            if(evolution!= null)
            {
                yield return EvolutionManager.Instance.Evolve(pokemon,evolution);
            }
            else
            {
                
                yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} không muốn dùng {item.Name} để tiến hoá");
                ClosePartyScreen();
                yield break;
            }
        }

        var useItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
        if (useItem != null)
        {
            if ((useItem is RecoveryItem))
                yield return DialogManager.Instance.ShowDialogText($"Bạn đã dùng vật phẩm là {useItem.Name}");
            onItemUsed?.Invoke(useItem);
        }
        else
        {
            if(selectedCategory == (int)ItemCategory.Items)
            {
                yield return DialogManager.Instance.ShowDialogText($"Vật phẩm không có hiệu lực!");
            }
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
        partyScreen.ClearMemberShotMessage();
        partyScreen.gameObject.SetActive(false);
         
    }

    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        var pokemon = partyScreen.SelectedMember;

        DialogManager.Instance.CloseDialog();
        moveSelectionUI.gameObject.SetActive(false);

        if (moveIndex == PokemonBase.MaxNumOfMoves)
        {
            // khong hoc chieu moi
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} không muốn học chiêu mới !!");
        }
        else
        {
            //Hoc chieu moi va lang quen chieu cu

            var selectedMove = pokemon.Moves[moveIndex].Base;
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} đã bỏ {selectedMove.Name} và học {moveToLearn.Name}");
            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }


}
