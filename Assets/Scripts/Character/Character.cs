using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed = 15f;  
    
    CharacterAnimator animator;

    public bool isMoving { get; private set; }

    public float OffSetY { get; private set; } = 0.3f;


    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);       
    }



    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null, bool isCheckCollisions = true )
    {
        animator.MoveX = Mathf.Clamp(moveVec.x,-1f, 1f) ;
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f) ;
                
        var targetPos = transform.position;
        targetPos.x += moveVec.x ;
        targetPos.y += moveVec.y ;

        if(moveVec.x == 0 )
        {
            if(moveVec.x < 0)
                moveVec.y -= 1;
            else
                moveVec.y += 1;
        }
        else
        {
            if (moveVec.y < 0)
                moveVec.x -= 1;
            else
                moveVec.x += 1;
        }


        if ( isCheckCollisions && !IsPathClear(targetPos))
        {
            yield break;
        }

        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * (Time.deltaTime));
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
        Debug.Log(targetPos);
        OnMoveOver?.Invoke();

    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dif = diff.normalized;
        if( Physics2D.BoxCast(transform.position + dif, new Vector2(0.08f, 0.08f), 0f, dif, diff.magnitude-1
            , GameLayers.i.SoildLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer) == true)
        {
            return false;
        }
        return true;
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {

        pos.x = Mathf.Floor(pos.x) + 1f;
        pos.y = Mathf.Floor(pos.y) + 1f + OffSetY;

        transform.position = pos;
    }


    public void LookTowards(Vector3 targetPos)
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if(xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        else
        {
            Debug.Log("Error: you can't look this ");
        }

    }

    public void HandleUpdate()
    {
        animator.isMoving = isMoving;  
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SoildLayer | GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }

    public CharacterAnimator Animator
    {
        get => animator;    
    }

}
