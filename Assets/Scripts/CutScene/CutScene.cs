using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScene : MonoBehaviour, IPlayerTriggerable
{
    [SerializeReference]
    [SerializeField] List<CutSceneAction> actions;

    public bool TriggerRepeatly => false;

    public IEnumerator Play()
    {
        GameController.Instance.StartCutSceneState();

        foreach (CutSceneAction action in actions)
        {
            if(action.WaitForCompletion)
            {
                // doi hoan` thanh action moi toi action tiep theo
                yield return action.Play();
            }
            else
            {
                //thuc hien action song song
                StartCoroutine(action.Play());
            }
        }
        GameController.Instance.StartFreeRoamState();
    }

    public void AddAction(CutSceneAction action)
    {
        action.Name = action.GetType().ToString();
        actions.Add(action);
    }

    public void onPlayerTriggered(PlayerMove player)
    {
        player.Character.Animator.isMoving = false;

        StartCoroutine( Play());
    }
}
