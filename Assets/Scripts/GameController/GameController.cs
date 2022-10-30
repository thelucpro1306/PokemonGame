using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, CutScene, Paused, Menu }

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
    public SceneDetails PrevScene { get; private set; }

    MenuController menuController;



    private void Awake()
    {
        Instance = this;
        PokemonDB.Init();
        MoveDB.Init();
        menuController = GetComponent<MenuController>();
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
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam;
            }

        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;
        

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

        if (trainer != null && won == true)
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
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
            
            if (Input.GetKeyDown(KeyCode.O))
            {
                menuController.OpenMenu();
                state = GameState.Menu;

            }

        }
        else
        {
            if (state == GameState.Battle)
            {
                battleSystem.HandleUpdate();
            }
            else
            {
                if (state == GameState.Dialog)
                {
                    DialogManager.Instance.HandleUpdate();
                }
                else
                {
                    if (state == GameState.Menu)
                    {
                        menuController.HandleUpdate();
                    }
                }
            }
        }



    }

    void OnMenuSelected(int slectedItem)
    {
        if(slectedItem == 0)
        {
            //pokemon
        }
        else
        {
            if (slectedItem == 1)
            {
                //bag
            }
            else
            {
                if (slectedItem == 2)
                {
                    //save
                    SavingSystem.i.Save("saveSlot1");
                   
                }
                else
                {
                    if (slectedItem == 3)
                    {
                        SavingSystem.i.Load("saveSlot1");
                    }
                }
            }
        }

        state = GameState.FreeRoam;

    }

}