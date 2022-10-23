using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;

    SpriteAnimator currentAnim;

    bool wasPreviousMoving;

    public float MoveX { get;  set; }
    public float MoveY { get;  set; }
    public bool isMoving { get;  set; }

    //States 
    SpriteAnimator WalkDownAnim;
    SpriteAnimator WalkUpAnim;
    SpriteAnimator WalkRightAnim;
    SpriteAnimator WalkLeftAnim;

    SpriteRenderer spriteRenderer;

    public FacingDirection DefaultDirection
    {
        get => defaultDirection;
    }


    private void Start() 
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        WalkDownAnim = new SpriteAnimator(walkDownSprites,spriteRenderer);
        WalkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        WalkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        WalkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);

        SetFacingDirection(defaultDirection);

        currentAnim = WalkDownAnim;
    }

    private void Update()
    {
        var preAnim = currentAnim;
        if(MoveX == 1)
        {
            currentAnim = WalkRightAnim;
        }
        else
        {
            if(MoveX == -1)
            {
                currentAnim = WalkLeftAnim;
            }
            else
            {
                if (MoveY == 1)
                {
                    currentAnim = WalkUpAnim;
                }
                else
                {
                    if (MoveY == -1)
                    {
                        currentAnim = WalkDownAnim;
                    }
                }
            }
        }

        if (isMoving)
        {
            currentAnim.HandleUpdate();
        }
        else
        {
            spriteRenderer.sprite = currentAnim.Frames[0];
        }

        if (currentAnim != preAnim || isMoving != wasPreviousMoving)
        {
            currentAnim.Start();
        }

        wasPreviousMoving = isMoving;
    }

    public void SetFacingDirection(FacingDirection dir)
    {
        if(dir == FacingDirection.Right)
        {
            MoveX = 1;
        }
        else
        {
            if(dir == FacingDirection.Left)
            {
                MoveX = -1;
            }
            else
            {
                if (dir == FacingDirection.Down)
                {
                    MoveY = -1;
                }
                else
                {
                    if(dir == FacingDirection.Up)
                    {
                        MoveY = 1;
                    }
                }
            }
        }
    }

}

public enum FacingDirection { Up, Down, Left, Right }