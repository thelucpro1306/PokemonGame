﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour,Interactable 
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Sprite> sprites;

    [Header("Movement")]
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    [Header("Quests")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;

    Character character;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    ItemGiver itemGiver;
    Quest activeQuest;
    PokemonGiver pokemonGiver;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if(state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            if(questToComplete != null)
            {
                var quest = new Quest(questToComplete);
                yield return quest.CompletedQuest(initiator);
                questToComplete = null;


                Debug.Log($"{quest.Base.QuestName} ");
            }


            if (itemGiver != null && itemGiver.CanbeGiven())
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerMove>());
            }
            else
            {
                if (pokemonGiver != null && pokemonGiver.CanbeGiven())
                {
                    yield return pokemonGiver.GivePokemon(initiator.GetComponent<PlayerMove>());
                    
                }
                else
                {
                    if (questToStart != null)
                    {
                        activeQuest = new Quest(questToStart);
                        yield return activeQuest.StartQuest();
                        questToStart = null;


                        // uncomment hoàn thành quest ngay lập tức khi có vật phẩm
                        //if (activeQuest.CanBeCompleted())
                        //{
                        //    yield return activeQuest.CompletedQuest(initiator);
                        //    activeQuest = null;
                        //}

                    }
                    else
                    {
                        if (activeQuest != null)
                        {
                            if (activeQuest.CanBeCompleted())
                            {
                                yield return activeQuest.CompletedQuest(initiator);
                                activeQuest = null;
                            }
                            else
                            {
                                yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialog);
                            }
                        }
                        else
                        {
                            yield return DialogManager.Instance.ShowDialog(dialog);
                        }

                    }
                }
                
            }
            idleTimer = 0f;
            state = NPCState.Idle;
        }
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if(oldPos != transform.position)
        {
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        }

        

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