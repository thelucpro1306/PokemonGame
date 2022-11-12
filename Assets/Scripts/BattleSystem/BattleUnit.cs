using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;
    [SerializeField] GameObject moveAnimation;
    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }

    public BattleHud Hud
    {
        get { return hud; }
    }

    public Pokemon pokemon { get; set; }
    Image image;
    Vector3 originalPos;
    Color originalColor;
    


    private void Awake()
    {
        image = GetComponent<Image>();  
       
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void setUp(Pokemon nPokemon)
    {
        pokemon = nPokemon;  
        if (isPlayerUnit)
        {
           image.sprite = pokemon.Base.BackSprite;   
        }
        else 
        {
            image.sprite = pokemon.Base.FrontSprite;   
        }
        hud.gameObject.SetActive(true);
        hud.SetData(pokemon);
        transform.localScale = new Vector3(1, 1, 1);
        image.color = originalColor;    
        PlayerEnterAnimation();
    }


    public IEnumerator MoveEffectsAnimation(Move move,Vector3 targetPos)
    {
        
        var image = moveAnimation.GetComponentInChildren<Image>();
        image.sprite = move.Base.Sprite;
        
        if (image.sprite != null)
        {
            moveAnimation.gameObject.SetActive(true);
            Debug.Log(image.sprite.name);
            moveAnimation.transform.localPosition = originalPos;//-116 56         
            yield return moveAnimation.transform.DOLocalMove(targetPos + new Vector3(1.5f,0,0), 0.85f).WaitForCompletion();//224 85
            moveAnimation.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Fail");
            moveAnimation.gameObject.SetActive(false);
            yield break;
        }
        
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    public void PlayerEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-500f,originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(500f, originalPos.y);
        }

        image.transform.DOLocalMoveX(originalPos.x,1.5f);
    }

    public void PlayerAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.5f));
        }
        else
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.5f));
        }

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.5f));
    }

    

    public void PlayerHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.red, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));

    }

    public void PlayerFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 150f, 0.5f));
        sequence.Join(image.DOFade(0f,0.5f));
    }

    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y + 50f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f,0.3f,1f),0.7f));
        yield return sequence.WaitForCompletion();
    }



    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y , 0.5f));
        sequence.Join(transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

}
