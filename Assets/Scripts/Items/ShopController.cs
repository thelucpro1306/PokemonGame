using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ShopState { Menu, Buying, Selling, Busy }

public class ShopController : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    [SerializeField] Vector2 shopCameraOffset;
    public static ShopController Instance { get; private set; }

    ShopState state;
    public event Action onStart;
    public event Action onFinnsh;


    Merchant merchant;


    private void Awake()
    {
        Instance = this;
    }

    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();
    }


    public IEnumerator StartTrading(Merchant merchant)
    {
        this.merchant = merchant;
        onStart?.Invoke();
        yield return StartMenuState();


    }

    public IEnumerator StartMenuState()
    {

        state = ShopState.Menu;
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("Tôi có thể giúp gì cho bạn?"
            , choices: new List<string>() { "Mua", "Bán", "Rời đi" }
            , onChoiceSelected: choiIndex => selectedChoice = choiIndex);

        if (selectedChoice == 0)
        {
            //buy
            
            yield return GameController.Instance.MoveCamera(shopCameraOffset);
            walletUI.Show();
            shopUI.Show(
                merchant.AvailableItem
                , (item) => StartCoroutine(BuyItem(item))
                , () => StartCoroutine(OnBackFromBuying())
                );
            state = ShopState.Buying;
        }
        else
        {
            if (selectedChoice == 1)
            {
                //sell
                state = ShopState.Selling;
                inventoryUI.gameObject.SetActive(true);
            }
            else
            {
                if (selectedChoice == 2)
                {
                    //thoat
                    onFinnsh?.Invoke();
                    yield break;

                }
            }
        }
    }

    public void HandleUpdate()
    {
        if (state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) =>
            {
                StartCoroutine(SellingItem(selectedItem));
            });
        }
        else
        {
            if (state == ShopState.Buying)
            {
                shopUI.HandleUpdate();
            }
        }
    }

    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenuState());
    }

    IEnumerator SellingItem(ItemBase item)
    {
        state = ShopState.Buying;

        if (!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText($"Bạn không thể bán vật phẩm {item.Name} ");
            state = ShopState.Selling;
            yield break;
        }
        walletUI.Show();
        var sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;
        var itemCount = inventory.GetItemCount(item);
        if (itemCount > 1)
        {
            yield return DialogManager.Instance.ShowDialogText($" Số lượng bạn muốn bán là? "
                , waitForInput: false, autoClose: false);
            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice
                , (selectedCount) => countToSell = selectedCount);

            DialogManager.Instance.CloseDialog();
        }

        sellingPrice = sellingPrice * countToSell;


        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($" Tôi có thể mua x{countToSell} {item.Name} với giá {sellingPrice}G. Bạn có muốn bán nó không?"
           , choices: new List<string>() { "Bán", "Rời đi" }
           , onChoiceSelected: choiIndex => selectedChoice = choiIndex);


        if (selectedChoice == 0)
        {
            // yes
            inventory.RemoveItem(item, countToSell);
            // cong tien cho nguoi choi
            Wallet.Instance.AddMoney(sellingPrice);
            yield return DialogManager.Instance
                .ShowDialogText($"Đã bán {item.Name} và nhận được {sellingPrice}G!");
        }

        walletUI.Close();

        state = ShopState.Selling;

    }

    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;
        yield return DialogManager.Instance
                .ShowDialogText($"Số lượng bạn muốn mua là?", waitForInput: false, autoClose: false);
        int countToBuy = 0;
        yield return countSelectorUI.ShowSelector(100, item.Price, (selectedCount) => countToBuy = selectedCount);

        DialogManager.Instance.CloseDialog();

        var totalPrice = item.Price * countToBuy;
       
        if (Wallet.Instance.HasMoney(totalPrice))
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"Bạn có muốn mua x{countToBuy} {item.Name} với giá {totalPrice} không?"
                , waitForInput: false
           , choices: new List<string>() { "Mua", "Rời đi" }
           , onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

            if (selectedChoice == 0)
            {
                //mua
                inventory.AddItem(item, countToBuy);
                Wallet.Instance.TakeMoney(totalPrice);

                yield return DialogManager.Instance.ShowDialogText("Cảm ơn đã mua hàng!");
            }

        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText("Bạn không đủ tiền! Hãy quay lại sau.");
        }
        state = ShopState.Buying;
    }



    IEnumerator OnBackFromBuying()
    {
        yield return GameController.Instance.MoveCamera(-shopCameraOffset);
        shopUI.Close();
        walletUI.Close();
        StartCoroutine(StartMenuState());
    }

}
