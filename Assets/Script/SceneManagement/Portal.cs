using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    public void onPlayerTriggered(PlayerMove player)
    {
        Debug.Log("Player entered the portal");
    }


}
