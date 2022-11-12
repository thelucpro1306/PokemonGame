using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFov : MonoBehaviour, IPlayerTriggerable
{
    public bool TriggerRepeatly => false;

    public void onPlayerTriggered(PlayerMove player)
    {
        player.Character.Animator.isMoving = false;
        GameController.Instance.OnEnterTrainersView(GetComponentInParent<TrainerController>());
    }
}
