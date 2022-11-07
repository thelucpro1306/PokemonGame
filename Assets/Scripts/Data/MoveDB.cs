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
                Debug.LogError($"C� 2 chi�u c�ng t�n  {move.Name}");
                continue;
            }

            moves[move.Name] = move;
        }
    }

    public static MoveBase GetMoveByName(string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.LogError($"Pok�mon {name} dosen't contain in DB");
        }
        return moves[name];
    }

}
