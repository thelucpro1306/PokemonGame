using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB 
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Conditions> Conditions { get; set; } = new Dictionary<ConditionID, Conditions>()
    {
        {
            ConditionID.psn,
            new Conditions()
            {
                Name = "Độc",
                StartMessage = "đã bị dính độc",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} bị độc làm mất {pokemon.MaxHP / 8} máu.");
                }
            }
        },
        {
            ConditionID.brn,
            new Conditions()
            {
                Name = "Bỏng",
                StartMessage = "đã bị bỏng",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHP / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} bị lửa đốt và mất {pokemon.MaxHP / 16} máu.");
                }
            }
        },
        {
            ConditionID.par,
            new Conditions()
            {
                Name = "Tê liệt",
                StartMessage = "bị tê liệt",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} đã bị tê liệt và không thể cử động");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Conditions()
            {
                Name = "Đóng băng",
                StartMessage = "bị đóng băng",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} không còn bị đóng băng nữa");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Conditions()
            {
                Name = "Ngủ",
                StartMessage = "bị hôn mê",
                OnStart = (Pokemon pokemon) =>
                {
                    //Sleep 1-3 turns
                    pokemon.StatusTime = Random.Range(1,4);
                    Debug.Log($"Sẽ ngủ trong {pokemon.StatusTime} lượt");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} đã tỉnh dậy");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} đang ngủ");
                    return false;
                }
            }
        },

        //Volatile Status Conditions
        {
            ConditionID.cfn,
            new Conditions()
            {
                Name = "Hoang mang",
                StartMessage = "đang hoang mang",
                OnStart = (Pokemon pokemon) =>
                {
                    //Confused 1-4 turns
                    pokemon.VolatileStatusTime = Random.Range(1,5);
                    Debug.Log($"Sẽ bị hoang mang trong {pokemon.VolatileStatusTime} lượt");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} tỉnh dậy");
                        return true;
                    }

                    pokemon.VolatileStatusTime--;

                    //50% chance to do a move
                    if(Random.Range(1, 3) == 1)
                        return true;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} đang hoang mang!");
                    pokemon.DecreaseHP(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"Tự làm đau mình bởi hoang tưởng");
                    return false;
                }
            }
        },
    };

    public static float GetStatusBonus(Conditions condition)
    {
        if(condition == null)
        {
            return 1f;
        }
        else
        {
            if(condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            {
                return 2f;
            }
            else
            {
                if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn
                    || condition.Id == ConditionID.brn)
                {
                    return 1.5f;
                }
            }
        }

        return 1f;
    }
}

public enum ConditionID
{
   none, psn, brn, slp, par, frz,cfn
}
