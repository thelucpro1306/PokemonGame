using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start,PlayerAction,PlayerMove,EnemyMove, Busy}

public class BattleSystem : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;

    BattleState state;
    int currentAction;
    int currentMove;

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
        
        dialogBox.setMoveNames(playerUnit.pokemon.Moves);

        yield return dialogBox.TypeDialog($"Pokemon {playerUnit.pokemon.Base.Name} xuat hien!");
        
        PlayerAction();

    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Chon mot hanh dong"));
        dialogBox.enableActionSelector(true);
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
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.pokemon.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(currentMove < playerUnit.pokemon.Moves.Count -2)
                currentMove +=2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(currentMove > 1)
            {
                currentMove -=2;
            }
        }
        dialogBox.updateMoveSelection(currentMove,playerUnit.pokemon.Moves[currentMove]);
        if (Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.enableMoveSelector(false);
            dialogBox.enableDialogText(true);

            StartCoroutine(PerformPlayerMove());
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
                playerUnit.setUp(nextPokemon);
                
                playerHud.SetData(playerUnit.pokemon);
                

                dialogBox.setMoveNames(nextPokemon.Moves);

                yield return dialogBox.TypeDialog($"Go {nextPokemon.Base.Name}!");

                PlayerAction();
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
            yield return dialogBox.TypeDialog("It's not super effective");
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
            {
                ++currentAction;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (currentAction > 0)
                {
                    --currentAction;
                }
            }
        }

        dialogBox.updateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Return))
        {
             if(currentAction == 0)
            {
                //Fight
                PlayerMove();
            }
             else
            {
                if(currentAction == 1)
                {

                }
            }
        }
    }
}
