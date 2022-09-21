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

    private void Start()
    {
        StartCoroutine(setUpBattle());
    }
        
    public IEnumerator setUpBattle()
    {
        playerUnit.setUp();
        enemyUnit.setUp();
        playerHud.SetData(playerUnit.pokemon);
        enemyHud.SetData(enemyUnit.pokemon);
        
        dialogBox.setMoveNames(playerUnit.pokemon.Moves);

        yield return dialogBox.TypeDialog($"Pokemon {playerUnit.pokemon.Base.Name} xuat hien!");
        yield return new WaitForSeconds(1f);
        PlayerAction();

    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Chon mot hanh dong"));
        dialogBox.enableActionSelector(true);
    }

    private void Update()
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
        yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} used {move.Base.Name}");
        yield return new WaitForSeconds(1f);
        bool isFainted = enemyUnit.pokemon.TakeDamage(move, playerUnit.pokemon);
        yield return enemyHud.UpdateHP();
        if (isFainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.Name} fainted");
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

        
        yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.Name} used {move.Base.Name}");
        yield return new WaitForSeconds(1f);
        bool isFainted = playerUnit.pokemon.TakeDamage(move, playerUnit.pokemon);
        yield return playerHud.UpdateHP();
        if (isFainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} fainted");
        }
        else
        {
            PlayerAction();
        }
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
