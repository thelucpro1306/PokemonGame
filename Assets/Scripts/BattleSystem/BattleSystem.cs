using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, BattleOver, AboutToUse, MoveToForget }

public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] GameObject dmgUI;
    [SerializeField] InventoryUI inventoryUI;


    BattleState state;
    

    int currentAction;
    int currentMove;

    bool aboutToUseChoice = true;
    MoveBase moveToLearn;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerMove player;
    TrainerController trainer;

    int escapeAttempts;

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

        escapeAttempts = 0;
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
        dialogBox.setDialog("Select an action");
        dialogBox.enableActionSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{ trainer.Name} is about to use " +
            $"{newPokemon.Base.Name}. Do you want to change Pokemon?");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else
        {
            if (state == BattleState.MoveSelection)
            {
                HandleMoveSelection();
            }
            else
            {
                if (state == BattleState.PartyScreen)
                {
                    HandlePartyScreenSelection();
                }
                else
                {
                    if (state == BattleState.AboutToUse)
                    {
                        HandleAboutToUse();
                    }
                    else
                    {
                        if(state == BattleState.MoveToForget)
                        {

                            Action<int> onMoveSelected = (moveIndex) => 
                            {
                                moveSelectionUI.gameObject.SetActive(false);
                                if(moveIndex == PokemonBase.MaxNumOfMoves)
                                {
                                    // khong hoc chieu moi
                                    StartCoroutine(dialogBox
                                        .TypeDialog($"{playerUnit.pokemon.Base.Name} don't learn new move"));

                                }
                                else
                                {
                                    //Hoc chieu moi va lang quen chieu cu

                                    var selectedMove = playerUnit.pokemon.Moves[moveIndex].Base;
                                    StartCoroutine(dialogBox
                                        .TypeDialog($"{playerUnit.pokemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));

                                    playerUnit.pokemon.Moves[moveIndex] = new Move(moveToLearn);
                                }

                                moveToLearn = null;
                                state = BattleState.RunningTurn;

                            };
                            moveSelectionUI.HandleMoveSelection(onMoveSelected);
                        }
                    }
                }
            }
        }

        

    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.updateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice)
            {
                //Chon yes
                
                OpenPartyScreen();
            }
            else
            {
                // Chon no
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                dialogBox.EnableChoiceBox(false);
                StartCoroutine(SendNextTrainerPokemon());
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

        dialogBox.updateMoveSelection(currentMove, playerUnit.pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var move = playerUnit.pokemon.Moves[currentMove];
            if (move.PP == 0)
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
        if (playerAction == BattleAction.Move)
        {
            playerUnit.pokemon.CurrentMove = playerUnit.pokemon.Moves[currentMove];
            enemyUnit.pokemon.CurrentMove = enemyUnit.pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.pokemon.CurrentMove.Base.Priority;

            //Kiem tra ai di truoc
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else
            {
                if (enemyMovePriority == playerMovePriority)
                {
                    playerGoesFirst = playerUnit.pokemon.Speed >= enemyUnit.pokemon.Speed;
                }
            }



            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;
            var secondPokemon = secondUnit.pokemon;
            //luot dau
            yield return RunMove(firstUnit, secondUnit, firstUnit.pokemon.CurrentMove);
            
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
            //luot sau
            if (secondPokemon.HP > 0)
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
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.enableActionSelector(false);
                yield return ThrowPokeball();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            var enemyMove = enemyUnit.pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }

    }

    IEnumerator HandlePokemonFainted(BattleUnit fanitedUnit)
    {
        yield return dialogBox.TypeDialog($"{fanitedUnit.pokemon.Base.Name} fainted");
        fanitedUnit.PlayerFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!fanitedUnit.IsPlayerUnit)
        {
            //exp nhan duoc
            int expYield = fanitedUnit.pokemon.Base.ExpYield; // 160 7 1 
            int enemyLevel = fanitedUnit.pokemon.Level;       
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} gain {expGain} exp.");
            yield return playerUnit.Hud.SetExpSmooth();

            //kiem tra len cap
            while (playerUnit.pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox
                    .TypeDialog($"{playerUnit.pokemon.Base.Name} grew to level {playerUnit.pokemon.Level}");
                
                //Hoc chieu moi
                var newMove = playerUnit.pokemon.GetLearnableMoveAtCurrentLevel();
                if(newMove != null)
                {
                    if(playerUnit.pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        //Hoc chieu moi
                        playerUnit.pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.setMoveNames(playerUnit.pokemon.Moves); 

                    }
                    else
                    {
                        // lang quen chieu 
                        yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {PokemonBase.MaxNumOfMoves} moves");
                        
                        yield return ChooseMoveToForget(playerUnit.pokemon, newMove.Base);

                        yield return new WaitUntil(()=> state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);

                    }
                }


                yield return playerUnit.Hud.SetExpSmooth(true);
            }
        }
        else
        {

        }


        CheckForBattleOver(fanitedUnit);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(p=>p.Base).ToList(), newMove);
        moveToLearn = newMove;
        state = BattleState.MoveToForget;
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

        if (CheckIfMoveHits(move, sourceUnit.pokemon, targetUnit.pokemon))
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

                yield return ShowDmg(targetUnit.transform.position + new Vector3(0, 1, 0)
                    , targetUnit.pokemon.dmgTake, targetUnit.pokemon.isCritical);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Sencondaries != null && move.Base.Sencondaries.Count > 0 && targetUnit.pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Sencondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.pokemon, targetUnit.pokemon, secondary.Target);
                }
            }

            if (targetUnit.pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.pokemon.Base.Name}'s attack missed");
        }



    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver)
        {
            yield break;
        }
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        //Status
        sourceUnit.pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);

        }
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
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
                if (nextPokemon != null)
                {
                    StartCoroutine(AboutToUse(nextPokemon));
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
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not effective");
    }

    void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
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

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0)
            {
                //Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //bag
                //StartCoroutine(RunTurns(BattleAction.UseItem)); 
                OpenBag();
            }

            else if (currentAction == 2)
            {
                //pokemon
                
                OpenPartyScreen();
            }

            else if (currentAction == 3)
            {
                //run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandlePartyScreenSelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                partyScreen.setMessageText("You can't use a fainted Pokemon!");
                return;
            }
            if (selectedMember == playerUnit.pokemon)
            {
                partyScreen.setMessageText("You can't switch same Pokemon!");
                return;
            }
            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {

                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;

                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.pokemon.HP <= 0)
            {
                partyScreen.setMessageText("you have to choose a pokemon to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {

                StartCoroutine(SendNextTrainerPokemon());
            }
            else
            {
                ActionSelection();
            }
            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected,onBack);

        
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
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
        if (effects.Status != ConditionID.none)
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

    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {

        if (playerUnit.pokemon.HP > 0)
        {

            yield return dialogBox.TypeDialog($"Come back {playerUnit.pokemon.Base.name}");
            playerUnit.PlayerFaintAnimation();
            yield return new WaitForSeconds(1f);
        }

        playerUnit.setUp(newPokemon);
        dialogBox.setMoveNames(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        if (isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerPokemon());
        }
        else
        {
            state = BattleState.RunningTurn;
        }
    }

    IEnumerator SendNextTrainerPokemon()
    {

        var nextPokemon = trainerParty.GetHealthyPokemon();
        state = BattleState.Busy;

        enemyUnit.setUp(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextPokemon.Base.Name}");

        state = BattleState.RunningTurn;
    }

    public IEnumerator ShowDmg(Vector3 dmgPos, int dmg, bool isCrit)
    {
        dmgUI.SetActive(true);
        var text = dmgUI.GetComponentInChildren<Text>();
        text.transform.position = dmgPos;
        if (isCrit)
        {

            text.color = Color.red;  
        }
        else
        {
            text.color = Color.black;
        }
        text.text = "-" + dmg;
        yield return new WaitForSeconds(1.5f);
        dmgUI.SetActive(false);
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"{player.Name} you can't catch the trainer Pokemon!");
            state = BattleState.RunningTurn;
            yield break;
        }
        

        yield return dialogBox.TypeDialog($"{player.Name} used POKEBALL!!!");

        var pokeballObj = Instantiate(pokeballSprite,playerUnit.transform.position - new Vector3(2,0),Quaternion.identity);

        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

        //animation cua pokeball

        yield return pokeball.transform
            .DOJump(enemyUnit.transform.position + new Vector3(0, 1.4f), 2f, 1, 0.8f)
            .WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();

        pokeball.transform.DOMoveY(enemyUnit.transform.position.y -1.5f,0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.pokemon);

        for(int i = 0; i < Mathf.Min(shakeCount,3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0,0,20f),0.8f).WaitForCompletion();
        }

        if(shakeCount == 4)
        {
            // bat duoc pokemon
            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.name} was caught");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.name} has been added your party");
            Destroy(pokeball);
            BattleOver(true);

        }
        else
        {
            //khong bat duoc
            yield return new WaitForSeconds(1.5f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();
            if(shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.name} broke free");
            }
            else
            {
                yield return dialogBox.TypeDialog("Almost caught it!");
            }

            Destroy(pokeball);
            state = BattleState.RunningTurn;

        }

    }

    int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHP - 2 * pokemon.HP)
                    * pokemon.Base.CatchRate
                    * ConditionsDB.GetStatusBonus(pokemon.Status)
                    / (3 * pokemon.MaxHP);

        if (a >= 255)
        {
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;

        while (shakeCount < 4)
        {

            if (UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }

            ++shakeCount;
        }

        return shakeCount;

    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battle!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.pokemon.Speed;
        int enemySpeed = enemyUnit.pokemon.Speed;

        if (playerSpeed > enemySpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;
            if(UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Ran away failed!");
                state = BattleState.RunningTurn;
            }
        }
    }

}
