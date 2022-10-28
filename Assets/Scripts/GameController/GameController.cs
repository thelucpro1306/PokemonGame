using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam,Battle,Dialog,CutScene, Paused}

public class GameController : MonoBehaviour
{
    public GameState state;
    [SerializeField] PlayerMove playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    public static GameController Instance { get; private set; }

    TrainerController trainer;

    GameState stateBeforePause;
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreScene { get; private set; }

    private void Awake()
    {
        Instance = this;
        ConditionsDB.Init();
    }

    public void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

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

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
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

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        
        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPokemonCopy);
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

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.CutScene;
        StartCoroutine(trainer.triggerTrainerBattle(playerController));
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PreScene = CurrentScene;
        CurrentScene = currScene;
    }

    private void Update()
    {
        if(state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.M))
            {
                SavingSystem.i.Save("saveSlot1");
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                SavingSystem.i.Load("saveSlot1");
            }

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
