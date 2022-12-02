using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Dialog dialog;
    [SerializeField] Vector2 backPos;
    [SerializeField] bool isDestroy = false;

    public void onPlayerTriggered(PlayerMove player)
    {   
        player.Character.Animator.isMoving = false;

        StartCoroutine(player.Character.Move(backPos));
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog));

    }
    public bool TriggerRepeatly => false;
}
