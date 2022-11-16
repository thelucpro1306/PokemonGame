using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ShopState { Menu, Buying, Selling, Busy}

public class ShopController : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    public static ShopController instance { get; private set; }

    ShopState state;
    public event Action onStart;
    public event Action onFinnsh;

    private void Awake()  
    {
        instance = this;
    }

    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();
    }


    public IEnumerator StartTrading(Merchant merchant)
    {
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
        if(state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(onBackFromSelling, (selectedItem) =>
            {
                StartCoroutine(SellingItem(selectedItem));
            });
        }
    }

    void onBackFromSelling()
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

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($" Tôi có thể mua {item.Name} với giá {sellingPrice}G. Bạn có muốn bán nó không?"
           , choices: new List<string>() {  "Bán", "Rời đi" }
           , onChoiceSelected: choiIndex => selectedChoice = choiIndex);


        if(selectedChoice == 0)
        {
            // yes
            inventory.RemoveItem(item);
            // cong tien cho nguoi choi
            Wallet.Instance.AddMoney(sellingPrice);
            yield return DialogManager.Instance
                .ShowDialogText($"Đã bán {item.Name} và nhận được {sellingPrice}G!");
        }

        walletUI.Close();

        state = ShopState.Selling;

    }

}
