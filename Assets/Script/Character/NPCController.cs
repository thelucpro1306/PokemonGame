using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour,Interactable 
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Sprite> sprites;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    
    Character character;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact()
    {
        if(state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {
                idleTimer = 0f;
                state = NPCState.Idle;
            }));

        }
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        yield return character.Move(movementPattern[currentPattern]);

        currentPattern = (currentPattern + 1) % movementPattern.Count;

        state = NPCState.Idle;

    }

    private void Update()
    {
        //if (DialogManager.Instance.isShowing)
        //{
        //    return;
        //}

        if(state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if(idleTimer > timeBetweenPattern)
            {
                idleTimer = 0;
                if(movementPattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }
                
            }
        }

        character.HandleUpdate();
    }

}

public enum NPCState { Idle, Walking, Dialog}