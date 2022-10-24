using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam,Battle,Dialog,CutScene}

public class GameController : MonoBehaviour
{
    public GameState state;
    [SerializeField] PlayerMove playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    public static GameController Instance { get; private set; }

    TrainerController trainer;

    private void Awake()
    {
        Instance = this;
        ConditionsDB.Init();
    }

    public void Start()
    {
        playerController.onEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
        playerController.onEnterTrainerView += (Collider2D trainerCollider) =>
        {
            var trainer = trainerCollider.GetComponentInParent<TrainerController>(); 
            if(trainer!= null)
            {
                state = GameState.CutScene;
                StartCoroutine(trainer.triggerTrainerBattle(playerController));
            }
        };

        //Bat su kien 

        

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };


        DialogManager.Instance.OnCloseDialog += () =>
        {
            if(state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }
            
        };
    }

    

    void EndBattle(bool won)
    {

        if(trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        
        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
        
        battleSystem.StartBattle(playerParty,wildPokemon);
    }

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    private void Update()
    {
        if(state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else
        {
            if(state == GameState.Battle)
            {
                battleSystem.HandleUpdate();
            }
            else
            {
                if(state == GameState.Dialog)
                {
                    DialogManager.Instance.HandleUpdate();
                }
            }
        }
    }
}
