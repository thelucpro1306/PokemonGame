using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerMove : MonoBehaviour
{
    // Start is called before the first frame update

    
    
    public Vector2 input;
  
   
   
    

    private Character character;

    public event Action onEncountered;

    private void Awake()
    {
        
        character = GetComponent<Character>();   
    }

    // Update is called once per frame
    private void Start()
    {
        
    }

    // Update is called once per frame
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
                StartCoroutine(character.Move(input,CheckForEncounter));
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
            collider.GetComponent<Interactable>()?.Interact();  
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
}