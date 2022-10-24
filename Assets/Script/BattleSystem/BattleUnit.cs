using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    // Start is called before the first frame update
    //[SerializeField] PokemonBase _base;
    //[SerializeField] int Level;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

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
        originalPos =image.transform.localPosition;
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

        image.color = originalColor;    
        PlayerEnterAnimation();
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

}
