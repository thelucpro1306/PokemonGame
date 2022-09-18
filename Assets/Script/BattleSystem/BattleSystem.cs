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
        yield return dialogBox.TypeDialog($"Pokemon {playerUnit.pokemon.Base.Name} xu?t hi?n ");
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
    }
}
