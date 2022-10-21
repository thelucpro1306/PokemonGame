using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerMove : MonoBehaviour
{
    // Start is called before the first frame update

    public float moveSpeed = 15f;
    private bool IsMoving;
    public Vector2 input;
  
    public Rigidbody2D rb;
    public CharacterAnimator animator;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;
    public LayerMask grassLayer;

    public event Action onEncountered;

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
    }

    // Update is called once per frame
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        if (!IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0)
            {
                input.y = 0;
            }

            if (input != Vector2.zero)
            {
                animator.MoveX = input.x;
                animator.MoveY =  input.y; 
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;
                if(IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }
        animator.isMoving =  IsMoving;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Interact();
        }

    }

    IEnumerator Move(Vector3 targetPos)
    {
        IsMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        IsMoving = false;
        CheckForEncounter();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableLayer) != null)
        {
            return false;
        }
        return true;
    }

    void Interact()
    {
        var x = animator.MoveX;
        var y = animator.MoveY;
       
        var faceDir = new Vector3(x, y);
        
        var interactPos = transform.position + faceDir;
        //Debug.DrawLine(transform.position,interactPos,Color.green,0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, interactableLayer);
        if(collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();  
        }
    }


    private void CheckForEncounter()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.isMoving =  false;
                onEncountered();
                
            }
        }

    }
}