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
    public Animator animator;
    public LayerMask treeLayer;
    public LayerMask grassLayer;

    public event Action onEncountered;

    private void Awake()
    {
        animator = GetComponent<Animator>();
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
                animator.SetFloat("Horizontal", input.x);
                animator.SetFloat("Vertical", input.y); 
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;
                if(IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("IsMoving", IsMoving);
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
        if (Physics2D.OverlapCircle(targetPos, 0.2f, treeLayer) != null)
            return false;
        return true;
    }

    private void CheckForEncounter()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("IsMoving", false);
                onEncountered();
                
            }
        }

    }
}