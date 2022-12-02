using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class Pokemon
{

    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel; 

        Init();
    }

    public PokemonBase Base
    {
        get
        {
            return _base;
        }
    }
    public int Level
    {
        get
        {
            return level;
        }
    }

    public int Exp { get; set; }

    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }   
    public int HP { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Conditions Status { get; private set; }
    public int StatusTime { get; set; }
    public Conditions VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public Queue<string> StatusChanges { get; private set; } 
    

    public event System.Action OnStatusChanged;
    public event System.Action OnHpChanged;

    int dmgTake;
    bool isCritical = true;

    public bool IsCritical => isCritical;

    public int DmgTake => dmgTake;

    public void Init()
    {
        //khoi tao chieu cho pokemon
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                break;
        }
        
        Exp = Base.GetExpForLevel(Level);

        CaculateStats();
        StatusChanges = new Queue<string>();
        HP = MaxHP;
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;
            
        if (saveData.statusId != null)
        {
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        }
        else
        {
            Status = null;
        }

        //Khoi tao chieu
        Moves = saveData.moves.Select(s=> new Move(s)).ToList();

        CaculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();        
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m=>m.getSaveData()).ToList(),
        };


        return saveData;
    }

    void CaculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        int oldMaxHp = MaxHP;
        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;

        if(oldMaxHp != 0)
        {
            HP += MaxHP - oldMaxHp;
        }

    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0 },
            {Stat.Evasion, 0 },
        };
    }

    int GetStat(Stat stat)
    {
        int staval = Stats[stat];

        int boost = StatBoosts[stat];
        var boostValues = new float[] {1f,1.5f,2f,2.5f,3f,3.5f,4f};

        if(boost >= 0)
        {
            staval = Mathf.FloorToInt(staval * boostValues[boost]);
        }
        else
        {
            staval = Mathf.FloorToInt(staval / boostValues[-boost]);
        }

        return staval;
    }

    public void ApplyBoost(List<StatBoost> statBoosts)
    {
        foreach(var StatBoost in statBoosts)
        {
            var stat = StatBoost.stat;
            var boost = StatBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6,6);
            
            if(boost > 0)
            {
                StatusChanges.Enqueue($"Pokemon {Base.Name} tăng "+ ChangeLang( stat)+"!");
                
            }
            else
            {
                StatusChanges.Enqueue($"Pokemon {Base.Name} giảm " + ChangeLang(stat) + "!");
            }

        }
    }

    public Evolution CheckForEvolution()
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= level);
    }

    public string ChangeLang(Stat stat)
    {
        if (stat == Stat.Accuracy)
        {
            return "Chính xác";
        }
        else
        {
            if (stat == Stat.Evasion)
            {
                return "Né tránh";
            }
            else
            {
                if (stat == Stat.Attack)
                {
                    return "Tấn công";
                }
                else
                {
                    if (stat == Stat.Defense)
                    {
                        return "thủ";
                    }
                    else
                    {
                        if (stat == Stat.SpAttack)
                        {
                            return "Tấn công đặc biệt";
                        }
                        else
                        {
                            if (stat == Stat.SpDefense)
                            {
                                return "thủ đặc biệt";
                            }
                            else
                            {
                                return "tốc độ";
                            }
                        }
                    }
                }
            }
        }
    }
    public Evolution CheckForEvolution(ItemBase item)
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }

    public void Evolve(Evolution evolution)
    {
        _base = evolution.EvolvesInto;
        CaculateStats();
    }

    public bool CheckForLevelUp()
    {
        if(Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            CaculateStats();
            return true;
        }

        return false;
    }

    public int MaxHP { get; private set; }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public Move GetRandomMove()
    {
        var moveWithPP = Moves.Where(x => x.PP > 0).ToList();


        int r = Random.Range(0, moveWithPP.Count);
        return Moves[r];
    }

    public LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.Where(p => p.Level == level).FirstOrDefault();
    }

    public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;
        Moves.Add(new Move(moveToLearn));
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if(Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
                canPerformMove = false;
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }

        return canPerformMove;
    }
    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 2f;
        CheckCrit(critical);
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);
        GetDamage(damage);
        DecreaseHP(damage);
        
        return damageDetails;

    }

    public void GetDamage(int dmg)
    {
        dmgTake = dmg;  
    }

    public void CheckCrit(float crit)
    {
        if(crit == 2)
        {
            isCritical = true;
        }
        else
        {
            isCritical = false;
        }
    }

    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        OnHpChanged?.Invoke();
        
    }

    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHP);
        OnHpChanged?.Invoke();
        

    }

    public void Heal()
    {
        HP = MaxHP;
        OnHpChanged?.Invoke();
        CureStatus();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null) return;

        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}{Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}{VolatileStatus.StartMessage}");
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }

    public float TypeEffectiveness { get; set; }
}
[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}