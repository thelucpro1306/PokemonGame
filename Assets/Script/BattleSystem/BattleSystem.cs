using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start,ActionSelection,MoveSelection,RunningTurn, Busy, PartyScreen, BattleOver}

public enum BattleAction { Move, SwitchPokemon, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    BattleState state;
    BattleState? prevState;

    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerMove player;
    TrainerController trainer;

    public event Action<bool> OnBattleOver;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        isTrainerBattle = false;

        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerMove>();

        StartCoroutine(setUpBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerMove>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(setUpBattle());
    }

    public IEnumerator setUpBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            //wild pokemon battle
            playerUnit.setUp(playerParty.GetHealthyPokemon());
            enemyUnit.setUp(wildPokemon);

            dialogBox.setMoveNames(playerUnit.pokemon.Moves);
            yield return dialogBox.TypeDialog($"Pokemon {playerUnit.pokemon.Base.Name} xuat hien!");
        }
        else
        {
            //Trainer Battle

            //Show trainer and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            //Send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.setUp(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyPokemon.Base.Name}");

            //Send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.setUp(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {enemyPokemon.Base.Name}");
            dialogBox.setMoveNames(playerUnit.pokemon.Moves);
        }

        partyScreen.Init();
        ActionSelection();

    }


    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
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
        if(state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else
        {
            if(state == BattleState.MoveSelection)
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
            var move = playerUnit.pokemon.Moves[currentMove];   
            if(move.PP == 0)
            {
                return;
            }

            dialogBox.enableMoveSelector(false);
            dialogBox.enableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.enableMoveSelector(false);
            dialogBox.enableDialogText(true);
            ActionSelection();
        }
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.enableActionSelector(false);
        dialogBox.enableDialogText(false);
        dialogBox.enableMoveSelector(true);

    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        if(playerAction == BattleAction.Move)
        {
            playerUnit.pokemon.CurrentMove = playerUnit.pokemon.Moves[currentMove];
            enemyUnit.pokemon.CurrentMove = enemyUnit.pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.pokemon.CurrentMove.Base.Priority;

            //Kiem tra ai di truoc
            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else
            {
                if(enemyMovePriority == playerMovePriority)
                {
                    playerGoesFirst = playerUnit.pokemon.Speed >= enemyUnit.pokemon.Speed;
                }
            }

            

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;
            var secondPokemon = secondUnit.pokemon;
            //luot dau
            yield return RunMove(firstUnit,secondUnit,firstUnit.pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
            //luot sau
            if(secondPokemon.HP > 0)
            {
                yield return RunMove(secondUnit, firstUnit, secondUnit.pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver)
                {
                    yield break;
                }
            }

        }
        else
        {
            if(playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            var enemyMove = enemyUnit.pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
        }

        if(state != BattleState.BattleOver)
        {
            ActionSelection(); 
        }

    }



    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }

        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.pokemon.Base.Name} used {move.Base.Name}");

        if(CheckIfMoveHits(move, sourceUnit.pokemon, targetUnit.pokemon))
        {
            sourceUnit.PlayerAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayerHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.pokemon, targetUnit.pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.pokemon.TakeDamage(move, sourceUnit.pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.Base.Sencondaries != null && move.Base.Sencondaries.Count > 0 && targetUnit.pokemon.HP > 0)
            {
                foreach(var secondary in move.Base.Sencondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.pokemon, targetUnit.pokemon, secondary.Target);
                }
            }

            if (targetUnit.pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.pokemon.Base.Name} fainted");
                targetUnit.PlayerFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.pokemon.Base.Name}'s attack missed");
        }


        
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if(state == BattleState.BattleOver)
        {
            yield break;    
        }
        yield return new WaitUntil(()=> state == BattleState.RunningTurn);
        //Status
        sourceUnit.pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.pokemon.Base.Name} fainted");
            sourceUnit.PlayerFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while(pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
               var nextPokemon = trainerParty.GetHealthyPokemon();
                if(nextPokemon != null)
                {
                    StartCoroutine(SendNextTrainerPokemon(nextPokemon));
                }
                else
                {
                    BattleOver(true);
                }
            }
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
                MoveSelection();
            }
            else if(currentAction == 1)
            {

            }

            else if (currentAction == 2)
            {
                prevState = state;
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

            if(prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
            


        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }


    }

    IEnumerator RunMoveEffects(MoveEffects effects , Pokemon source, Pokemon target, MoveTarget moveTarget)
    {

        //Stat Boosting
        if (effects.Boosts != null)
        {

            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoost(effects.Boosts);

            }
            else
            {
                target.ApplyBoost(effects.Boosts);
            }
        }

        //Status Condition
        if(effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        //Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
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
        dialogBox.setMoveNames(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerPokemon(Pokemon nextPokemon)
    {
        state = BattleState.Busy;

        enemyUnit.setUp(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextPokemon.Base.Name}");

        state = BattleState.RunningTurn;
    }
}
