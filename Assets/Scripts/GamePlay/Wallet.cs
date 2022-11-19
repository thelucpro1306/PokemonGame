using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallet : MonoBehaviour,ISavable
{
    [SerializeField] float money;

    public event Action OnMoneyChanged;

    public static Wallet Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    

    public float Money => money;

    public void AddMoney(float amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke();
    }

    public void TakeMoney(float amount)
    {
        money -= amount;
        OnMoneyChanged?.Invoke();
    }

    public bool HasMoney(float amount)
    {
        return amount <= money;  
    }

    public object CaptureState()
    {
        return money;
    }

    public void RestoreState(object state)
    {
        money = (float)state;
    }
}
