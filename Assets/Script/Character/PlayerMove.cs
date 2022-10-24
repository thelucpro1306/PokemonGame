using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerMove : MonoBehaviour
{

    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    public Vector2 input;
  

    private Character character;

    public event Action onEncountered;
    public event Action<Collider2D> onEnterTrainerView;

    private void Awake()
    {
        
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
                StartCoroutine(character.Move(input,OnMoveOver));
            }
        }
        
        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Interact();
        }

    }

  
    void Interact()
    {
        var x = character.Animator.MoveX;
        var y = character.Animator.MoveY;
       
        var faceDir = new Vector3(x, y);
        
        var interactPos = transform.position + faceDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if(collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);  
        }
    }

    private void OnMoveOver()
    {
        CheckForEncounter();
        CheckIfInTrainerView();
    }

    private void CheckIfInTrainerView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.FovLayer);
        if (collider != null)
        {
            character.Animator.isMoving = false;
            onEnterTrainerView?.Invoke(collider);
        }
    }

    private void CheckForEncounter()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.GrassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                character.Animator.isMoving =  false;
                onEncountered();
                
            }
        }

    }
    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}