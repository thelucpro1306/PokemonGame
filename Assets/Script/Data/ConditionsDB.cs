using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB 
{

    public static Dictionary<ConditionID, Conditions> Conditions { get; set; } = new Dictionary<ConditionID, Conditions>()
    {
        {
            ConditionID.psn,
            new Conditions()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Conditions()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHP / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to burn");
                }
            }
        },
        {
            ConditionID.par,
            new Conditions()
            {
                Name = "Paralyze",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s paralyzed and can't move");
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
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is not frozen anymore");
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
                Name = "Sleep",
                StartMessage = "has been sleep",
                OnStart = (Pokemon pokemon) =>
                {
                    //Sleep 1-3 turns
                    pokemon.StausTime = Random.Range(1,4);
                    Debug.Log($"Will be asleep for {pokemon.StausTime} move");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.StausTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up");
                        return true;
                    }

                    pokemon.StausTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping");
                    return false;
                }
            }
        },
    };
}

public enum ConditionID
{
   none, psn, brn, slp, par, frz,
}
