using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed = 15f;  
    
    CharacterAnimator animator;

    public bool isMoving { get; private set; }

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
    }
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x,-1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);
        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsPathClear(targetPos))
        {
            yield break;
        }

        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;

        OnMoveOver?.Invoke();

    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dif = diff.normalized;
        if( Physics2D.BoxCast(transform.position +dif, new Vector2(0.2f, 0.2f), 0f, dif, diff.magnitude
            , GameLayers.i.SoildLayer | GameLayers.i.InteractableLayer) == true)
        {
            return false;
        }
        return true;
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
