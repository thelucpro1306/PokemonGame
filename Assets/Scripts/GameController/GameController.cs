using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, CutScene, Paused, Menu, PartyScreen, Bag, Evolution, Shop,PokemonInfor }

public class GameController : MonoBehaviour
{
    GameState state;
    [SerializeField] PlayerMove playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    PokemonInfor pokemonInfor;

    public static GameController Instance { get; private set; }

    TrainerController trainer;

    GameState prevState;
    GameState stateForEvolution;
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    public GameState State => state;

    MenuController menuController;



    private void Awake()
    {

        Instance = this;
        PokemonDB.Init();
        MoveDB.Init();
        ItemDB.Init();
        QuestDB.Init();
        menuController = GetComponent<MenuController>();

        ConditionsDB.Init();
        
    }

    public void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        //Bat su kien 

        DialogManager.Instance.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };


        DialogManager.Instance.onDialogFinish += () =>
        {
            if (state == GameState.Dialog)
            {
                state = prevState;
            }

        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };
        

        menuController.onMenuSelected += OnMenuSelected;

        partyScreen.Init();

        EvolutionManager.Instance.OnStartEvolution += () =>
        {
            stateForEvolution = state;
            state = GameState.Evolution;
        };

        EvolutionManager.Instance.OnCompletedEvolution += () =>
        {
            partyScreen.SetPartyData();
            state = stateForEvolution;
        };

        ShopController.Instance.onStart += () =>
        {
            state = GameState.Shop;
        };

        ShopController.Instance.onFinnsh += () =>
        {
            state = GameState.FreeRoam;
        };


    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }

    public void StartCutSceneState()
    {
        state = GameState.CutScene;
    }

    public void StartFreeRoamState()
    {
        state = GameState.FreeRoam;
    }

    public void StartPartyScreenState()
    {
        state = GameState.PartyScreen;
    }


    void EndBattle(bool won)
    {

        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }
        partyScreen.SetPartyData();
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        var playerParty = playerController.GetComponent<PokemonParty>();
        StartCoroutine(playerParty.CheckForEvolution());
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
                    else
                    {
                        if (state == GameState.CutScene)
                        {
                            playerController.Character.HandleUpdate();
                        }
                        else
                        {
                            if (state == GameState.PartyScreen)
                            {
                                Action onSelected = () =>
                                {
                                   
                                };

                                Action onBack = () =>
                                {
                                    partyScreen.gameObject.SetActive(false);
                                    state = GameState.FreeRoam;
                                };

                                partyScreen.HandleUpdate(onSelected, onBack);
                            }
                            else
                            {
                                if (state == GameState.Bag)
                                {
                                    Action onBack = () =>
                                    {
                                        inventoryUI.gameObject.SetActive(false);
                                        walletUI.Close();
                                        state = GameState.FreeRoam;
                                    };

                                    inventoryUI.HandleUpdate(onBack);
                                }
                                else
                                {
                                    if (state == GameState.Shop)
                                    {
                                        ShopController.Instance.HandleUpdate();
                                    }
                                }
                            }
                        }
                        //
                    }
                }
            }
        }
    }

    public IEnumerator MoveCamera(Vector2 moveOffset, bool waitForFadeOut = false)
    {
        yield return Fader.Instance.FaderIn(0.5f);
        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);
        if (waitForFadeOut)
        {
            yield return Fader.Instance.FaderOut(0.5f);
        }
        else
        {
            StartCoroutine(Fader.Instance.FaderOut(0.5f));
        }
    }

    public void OnMenuSelected(int slectedItem)
    {
        if (slectedItem == 0)
        {
            //pokemon
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;

        }
        else
        {
            if (slectedItem == 1)
            {
                //bag
                inventoryUI.gameObject.SetActive(true);

                state = GameState.Bag;
            }
            else
            {
                if (slectedItem == 2)
                {
                    //save
                    SavingSystem.i.Save("saveSlot1");
                    state = GameState.FreeRoam;
                }
                else
                {
                    if (slectedItem == 3)
                    {
                        SavingSystem.i.Load("saveSlot1");
                        state = GameState.FreeRoam;
                    }
                }
            }
        }



    }

}
