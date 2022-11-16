using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/Create new Tm or Hm")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;


    public MoveBase Move => move;
    public bool IsHM => isHM;

    public override string Name => base.Name + $": {move.Name}";
    public override bool isReuseable => isHM;
    public override bool canUsedInBattle => false;

    public override bool Use(Pokemon pokemon)
    {
        return pokemon.HasMove(move);
    }

    public bool canBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(Move);
    }

    

}
