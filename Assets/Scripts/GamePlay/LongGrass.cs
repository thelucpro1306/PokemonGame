using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public bool TriggerRepeatly => true;

    public void onPlayerTriggered(PlayerMove player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            Debug.Log("It's woking");
            player.Character.Animator.isMoving = false;
            GameController.Instance.StartBattle();
        }
    }

}
