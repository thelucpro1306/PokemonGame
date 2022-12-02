using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hello : MonoBehaviour,IPlayerTriggerable
{
    [SerializeField] Dialog dialog;
    bool isUsed = false;
    public bool TriggerRepeatly => throw new System.NotImplementedException();

    public void onPlayerTriggered(PlayerMove player)
    {
        throw new System.NotImplementedException();
    }


}
