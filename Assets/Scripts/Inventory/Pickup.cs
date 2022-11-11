﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable
{
    [SerializeField] ItemBase item;

    public bool Used { get; private set; }  = false;

    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);
            Used = true;
            GetComponent<SpriteRenderer>().enabled = false;  
            GetComponent<BoxCollider2D>().enabled = false;
            var playerName = initiator.GetComponent<PlayerMove>().Name; 
            yield return DialogManager.Instance.ShowDialogText($"{playerName} đã tìm thấy {item.Name}!");

        }
    }
}
