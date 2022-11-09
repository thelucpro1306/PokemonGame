using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/Create new Tm or Hm")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;

    public MoveBase Move => move;

    public override bool Use(Pokemon pokemon)
    {
        return pokemon.HasMove(move);
    }

}
