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
        }
    };
}

public enum ConditionID
{
   none, psn, brn, slp, par, frz,
}
