using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable
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
    Healer healer;
    Merchant merchant;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            if (questToComplete != null)
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
                            if (healer != null)
                            {
                                yield return healer.Heal(initiator, dialog);
                            }
                            else
                            {
                                if(merchant != null)
                                {
                                    yield return merchant.Trade();
                                }
                                else
                                {
                                    yield return DialogManager.Instance.ShowDialog(dialog);
                                }
                            }
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

        if (oldPos != transform.position)
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

        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0;
                if (movementPattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }

            }
        }

        character.HandleUpdate();
    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();

        saveData.activeQuest = activeQuest?.GetSaveData();
        if (questToStart != null)
        {
            saveData.activeQuest = (new Quest(questToStart)).GetSaveData();
        }
        if (questToComplete != null)
        {
            saveData.activeQuest = (new Quest(questToComplete)).GetSaveData();
        }

        return saveData;

    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCQuestSaveData;
        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;
            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;

        }

    }
}

[System.Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}


public enum NPCState { Idle, Walking, Dialog }