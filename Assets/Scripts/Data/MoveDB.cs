using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB
{
    public static Dictionary<string, MoveBase> moves;

    public static void Init()
    {

        moves = new Dictionary<string, MoveBase>();


        
        var moveList = Resources.LoadAll<MoveBase>("");

        foreach (var move in moveList)
        {
            if (moves.ContainsKey(move.Name))
            {
                Debug.LogError($"Có 2 chiêu cùng tên  {move.Name}");
                continue;
            }

            moves[move.Name] = move;
        }
    }

    public static MoveBase GetMoveByName(string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.LogError($"Pokémon {name} dosen't contain in DB");
        }
        return moves[name];
    }

}
