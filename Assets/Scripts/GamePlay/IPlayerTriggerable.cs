using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerTriggerable
{
    void onPlayerTriggered(PlayerMove player);

    bool TriggerRepeatly { get;}
}
