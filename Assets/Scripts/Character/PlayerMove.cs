using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerMove : MonoBehaviour, ISavable
{

    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    public Vector2 input;
  

    private Character character;

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
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffSetY), 0.2f, GameLayers.i.TriggerableLayer);

        foreach(var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if(triggerable != null)
            {
                triggerable.onPlayerTriggered(this);
                break;
            }
        }
    }

    public object CaptureState()
    {
        float[] position = new float[] { 
            transform.position.x,  
            transform.position.y
        };
        return position;


    }

    public void RestoreState(object state)
    {
        var position = (float[])state;
        transform.position = new Vector3((float)position[0], (float)position[1]);
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