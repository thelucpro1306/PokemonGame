using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveActorAction : CutSceneAction
{
    [SerializeField] CutSceneActor actor;
    [SerializeField] List<Vector2> movePatterns;

    public override IEnumerator Play()
    {
        var character = actor.GetCharacter();

        foreach (var item in movePatterns)
        {
            yield return character.Move(item, isCheckCollisions: false);
        }
    }

    
}

[System.Serializable]
public class CutSceneActor
{
    [SerializeField] bool isPlayer;
    [SerializeField] Character character;

    public Character GetCharacter() => (isPlayer) ? PlayerMove.instance.Character : character;

}
