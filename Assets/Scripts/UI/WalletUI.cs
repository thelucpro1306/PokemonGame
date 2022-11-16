using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalletUI : MonoBehaviour
{
    [SerializeField] Text moneyTxt;

    private void Start()
    {
        Wallet.Instance.OnMoneyChanged += setMoneyText;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        setMoneyText();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void setMoneyText()
    {
        moneyTxt.text = "G" + Wallet.Instance.Money;
    }

}
