using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start,PlayerAction,PlayerMove,EnemyMove, Busy, PartyScreen}

public class BattleSystem : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    BattleState state;
    
    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public event Action<bool> OnBattleOver;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(setUpBattle());
    }
        
    public IEnumerator setUpBattle()
    {
        playerUnit.setUp(playerParty.GetHealthyPokemon());
        enemyUnit.setUp(wildPokemon);
        playerHud.SetData(playerUnit.pokemon);
        enemyHud.SetData(enemyUnit.pokemon);

        partyScreen.Init();
        
        dialogBox.setMoveNames(playerUnit.pokemon.Moves);

        yield return dialogBox.TypeDialog($"Pokemon {playerUnit.pokemon.Base.Name} xuat hien!");
        
        PlayerAction();

    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.setDialog("Chon mot hanh dong");
        dialogBox.enableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    public void HandleUpdate()
    {
        if(state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else
        {
            if(state == BattleState.PlayerMove)
            {
                HandleMoveSelection();
            }
            else
            {
                if(state == BattleState.PartyScreen)
                {
                    HandlePartyScreenSelection();
                }   
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentAction = Mathf.Clamp(currentMove, 0, playerUnit.pokemon.Moves.Count - 1);

        dialogBox.updateMoveSelection(currentMove,playerUnit.pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.enableMoveSelector(false);
            dialogBox.enableDialogText(true);

            StartCoroutine(PerformPlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            dialogBox.enableMoveSelector(false);
            dialogBox.enableDialogText(true);
            PlayerAction();
        }
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.enableActionSelector(false);
        dialogBox.enableDialogText(false);
        dialogBox.enableMoveSelector(true);

    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.pokemon.Moves[currentMove];
        move.PP--;

        yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} used {move.Base.Name}");

        playerUnit.PlayerAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayerHitAnimation();

        var damageDetails = enemyUnit.pokemon.TakeDamage(move, playerUnit.pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.Name} fainted");
            enemyUnit.PlayerFaintAnimation();

            yield return new WaitForSeconds(2f);


            
            OnBattleOver(true);

        }
        else
        {
            StartCoroutine(EnemyMove());
        }

    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.pokemon.GetRandomMove();

        move.PP--;
        
        yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.Name} used {move.Base.Name}");
        enemyUnit.PlayerAttackAnimation();
        yield return new WaitForSeconds(1f);
        playerUnit.PlayerHitAnimation();
        var damageDetails = playerUnit.pokemon.TakeDamage(move, playerUnit.pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} fainted");
            playerUnit.PlayerFaintAnimation();

            yield return new WaitForSeconds(1f);
           
            var nextPokemon = playerParty.GetHealthyPokemon();
            if(nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                OnBattleOver(true);
            }
            
            
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A Critical Hit");
        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective");
        else if(damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not effective");
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.updateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(currentAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if(currentAction == 1)
            {

            }

            else if (currentAction == 2)
            {
                OpenPartyScreen();
            }

            else if (currentAction == 3)
            {

            }
        }
    }

    void HandlePartyScreenSelection()
    {
       
        if (Input.GetKeyDown(KeyCode.RightArrow))
        { 
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count -1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.setMessageText("You can't use a fainted Pokemon!");
                return;
            }
            if(selectedMember == playerUnit.pokemon)
            {
                partyScreen.setMessageText("You can't switch same Pokemon!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy; 
            StartCoroutine(SwitchPokemon(selectedMember));  


        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            partyScreen.gameObject.SetActive(false);
            PlayerAction();
        }


    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if(playerUnit.pokemon.HP  >  0 )
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.pokemon.Base.name}");
            playerUnit.PlayerFaintAnimation();
            yield return new WaitForSeconds(1f);
        }

        playerUnit.setUp(newPokemon);
        playerHud.SetData(newPokemon);
        dialogBox.setMoveNames(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        StartCoroutine(EnemyMove());

    }

}
