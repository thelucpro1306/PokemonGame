using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] float price;
    [SerializeField] bool isSellable;
    public virtual string Name => name;
    public string Description => description;
    public Sprite Icon => icon;

    public float Price => price;    
    public bool IsSellable => isSellable;


    public virtual bool Use(Pokemon pokemon)
    {
        return false;
    }

    public virtual bool canUsedInBattle => true;
    public virtual bool canUsedOutSideBattle => true;

    public virtual bool isReuseable => false;



}
