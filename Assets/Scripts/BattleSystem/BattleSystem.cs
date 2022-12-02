using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            yield return dialogBox.TypeDialog($"Pokemon {playerUnit.pokemon.Base.Name} xuất hiện!");
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
            yield return dialogBox.TypeDialog($"{trainer.Name} muốn chiến đấu với bạn!!");

            //Send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.setUp(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} đã chọn {enemyPokemon.Base.Name} để chiến đấu");

            //Send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.setUp(playerPokemon);
            yield return dialogBox.TypeDialog($"Hãy lên nào {enemyPokemon.Base.Name} !!!");
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
        playerUnit.Hud.ClearData();
        
        enemyUnit.Hud.ClearData();
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.setDialog("Chọn một hành động");
        dialogBox.enableActionSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;

        yield return dialogBox.TypeDialog($"{ trainer.Name} chuẩn bị đưa " +
            $"{newPokemon.Base.Name} ra trận chiến.");

        yield return dialogBox.TypeDialog($"Bạn có muốn đổi Pokemon không?");


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
                    if (state == BattleState.Bag)
                    {

                        Action onBack = () =>
                        {
                            inventoryUI.gameObject.SetActive(false);
                            state = BattleState.ActionSelection;
                        };

                        Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
                        {
                            StartCoroutine(OnItemUsed(usedItem));
                        };

                        inventoryUI.HandleUpdate(onBack, onItemUsed);
                    }
                    else
                    {
                        if (state == BattleState.AboutToUse)
                        {
                            HandleAboutToUse();
                        }
                        else
                        {
                            if (state == BattleState.MoveToForget)
                            {

                                Action<int> onMoveSelected = (moveIndex) =>
                                {
                                    moveSelectionUI.gameObject.SetActive(false);
                                    if (moveIndex == PokemonBase.MaxNumOfMoves)
                                    {
                                        // khong hoc chieu moi
                                        StartCoroutine(dialogBox
                                            .TypeDialog($"{playerUnit.pokemon.Base.Name} không muốn học chiêu mới !!"));
                                    }
                                    else
                                    {
                                        //Hoc chieu moi va lang quen chieu cu

                                        var selectedMove = playerUnit.pokemon.Moves[moveIndex].Base;
                                        StartCoroutine(dialogBox
                                            .TypeDialog($"{playerUnit.pokemon.Base.Name} đã bỏ {selectedMove.Name} và học {moveToLearn.Name}"));
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



    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is PokeballItem)
        {
            yield return ThrowPokeball((PokeballItem)usedItem);
        }
        else
        {

        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
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
        {
            if(currentMove < playerUnit.pokemon.Moves.Count - 1)
            {
                ++currentMove;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (currentMove > 0)
                {
                    --currentMove;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (currentMove < playerUnit.pokemon.Moves.Count -2)
                    {
                        currentMove += 2;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow  ))
                    {
                        if (currentMove < 1)
                        {
                            currentMove -= 2;
                        }
                    }
                }
            }
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.pokemon.Moves.Count -1);
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
                //
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
        yield return dialogBox.TypeDialog($"{fanitedUnit.pokemon.Base.Name} đã bị hạ gục !!");
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
            yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} đã nhận được {expGain} kinh nghiệm.");
            yield return playerUnit.Hud.SetExpSmooth();

            //kiem tra len cap
            while (playerUnit.pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} lên cấp {playerUnit.pokemon.Level}");

                //Hoc chieu moi
                var newMove = playerUnit.pokemon.GetLearnableMoveAtCurrentLevel();
                if (newMove != null)
                {
                    if (playerUnit.pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        //Hoc chieu moi
                        playerUnit.pokemon.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} đã học {newMove.Base.Name}");
                        dialogBox.setMoveNames(playerUnit.pokemon.Moves);
                    }
                    else
                    {
                        // lang quen chieu 
                        yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} đang cố gắng học {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"Nhưng Pokémon này không thể học quá {PokemonBase.MaxNumOfMoves} chiêu");

                        yield return ChooseMoveToForget(playerUnit.pokemon, newMove.Base);

                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);

                    }
                }
                yield return playerUnit.Hud.SetExpSmooth(true);
            }
        }
        else
        {

        }
        yield return CheckForBattleOver(fanitedUnit);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Chọn một chiêu để bỏ ra:");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(p => p.Base).ToList(), newMove);
        moveToLearn = newMove;
        state = BattleState.MoveToForget;
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.pokemon);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }

        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.pokemon.Base.Name} đã sử dụng {move.Base.Name}");





        if (CheckIfMoveHits(move, sourceUnit.pokemon, targetUnit.pokemon))
        {
            sourceUnit.PlayerAttackAnimation();          
            yield return sourceUnit.MoveEffectsAnimation(move, targetUnit.transform.localPosition);
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
                    , targetUnit.pokemon.DmgTake, targetUnit.pokemon.IsCritical);
                yield return targetUnit.Hud.WaitForHPUpdate();
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
            yield return dialogBox.TypeDialog($"Đòn tấn công của {sourceUnit.pokemon.Base.Name} đã bị trượt !!!");
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
        yield return sourceUnit.Hud.WaitForHPUpdate();
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

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
            {
                BattleOver(false);      
                    GameController.Instance.OnMenuSelected(3);
            }    
            
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
                    yield return DialogManager.Instance.ShowDialogText($"Bạn đã nhận được {trainer.Money}G");
                    Wallet.Instance.AddMoney(trainer.Money);
                    BattleOver(true);
                }
            }
        }

    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("Đòn trí mạng!!!!");
        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("Siêu hiệu quả !!!!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("Không hiệu quả !!!!");
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
                partyScreen.setMessageText("Bạn không thể sử dụng Pokémon bị hạ gục!");
                return;
            }
            if (selectedMember == playerUnit.pokemon)
            {
                partyScreen.setMessageText("Bạn không thể đổi Pokémon cùng loại!");
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
                partyScreen.setMessageText("Bạn phải chọn một Pokémon để tiếp tục !");
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

        partyScreen.HandleUpdate(onSelected, onBack);


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
            yield return dialogBox.TypeDialog($"Trở lại nào {playerUnit.pokemon.Base.name}!!");
            playerUnit.PlayerFaintAnimation();
            yield return new WaitForSeconds(1f);
        }

        playerUnit.setUp(newPokemon);
        dialogBox.setMoveNames(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"Tiến lên {newPokemon.Base.Name}!");

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


        yield return dialogBox.TypeDialog($"{trainer.Name} đã đưa {nextPokemon.Base.Name} ra trận chiến !!!");
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

    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"{player.Name} Bạn không thể bắt Pokémon của trainer khác !");
            state = BattleState.RunningTurn;
            yield break;
        }
        yield return dialogBox.TypeDialog($"{player.Name} sử dụng {pokeballItem.Name.ToUpper()}!!!");
        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);

        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        //animation cua pokeball

        yield return pokeball.transform
            .DOJump(enemyUnit.transform.position + new Vector3(0, 1.4f), 2f, 1, 0.8f)
            .WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();

        pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.5f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.pokemon, pokeballItem);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 20f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // bat duoc pokemon

            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.name} đã bị bắt!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.name} đã được thêm vào nhóm của bạn!");
            Destroy(pokeball);
            BattleOver(true);

        }
        else
        {
            //khong bat duoc
            yield return new WaitForSeconds(1.5f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();
            if (shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.name} đã thoát được!");
            }
            else
            {
                yield return dialogBox.TypeDialog("Suýt thì được!!!");
            }

            Destroy(pokeball);
            state = BattleState.RunningTurn;

        }

    }

    int TryToCatchPokemon(Pokemon pokemon, PokeballItem pokeballItem)
    {
        if(pokeballItem.CatchRateModfier == 4)
        {
            return 4;
        }
        else
        {
            float a = (3 * pokemon.MaxHP - 2 * pokemon.HP)
                    * pokemon.Base.CatchRate
                    * pokeballItem.CatchRateModfier
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

    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"Bạn không thể chạy khỏi trận chiến này !!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.pokemon.Speed;
        int enemySpeed = enemyUnit.pokemon.Speed;

        if (playerSpeed > enemySpeed)
        {
            yield return dialogBox.TypeDialog($"Chạy thoát thành công!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;
            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Chạy thoát thành công!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Chạy thoát thất bại!");
                state = BattleState.RunningTurn;
            }
        }
    }

}
