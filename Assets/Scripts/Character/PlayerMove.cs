using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PlayerMove : MonoBehaviour, ISavable
{

    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    public Vector2 input ;
    public static PlayerMove instance { get; private set; }

    private Character character;

    private void Awake()
    {
        instance = this;
        character = GetComponent<Character>();
    }


    public void HandleUpdate()
    {
        if (!character.isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            
            if (input.x != 0)
            {
                input.y = 0;

            }


            if (input != Vector2.zero)
            {
                
                StartCoroutine(character.Move(input , OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(Interact());
        }

    }


    IEnumerator Interact()
    {
        var x = character.Animator.MoveX;
        var y = character.Animator.MoveY;

        var faceDir = new Vector3(x, y);

        var interactPos = transform.position + faceDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    IPlayerTriggerable currentlyTrigger;

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffSetY), 0.2f, GameLayers.i.TriggerableLayer);

        IPlayerTriggerable triggerable = null;

        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if(triggerable == currentlyTrigger && triggerable.TriggerRepeatly == false)
                {
                    break;
                }

                triggerable.onPlayerTriggered(this);
                currentlyTrigger = triggerable;
                break;
            }
        }
        if(colliders.Count() == 0 || triggerable != currentlyTrigger)
        {
            currentlyTrigger = null;
        }
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[]
            {
                transform.position.x,
                transform.position.y
            },
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList(),


        };


        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        //load vi tri nhan vat
        var pos = saveData.position;
        transform.position = new Vector3(pos[0],pos[1]);
        //load pokemon party

        GetComponent<PokemonParty>().Pokemons =  saveData.pokemons.Select(p => new Pokemon(p)).ToList();

    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;

}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}