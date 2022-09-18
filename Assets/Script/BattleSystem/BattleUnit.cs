using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] PokemonBase _base;
    [SerializeField] int Level;
    [SerializeField] bool isPlayerUnit;

    public Pokemon pokemon { get; set; }

    public void setUp()
    {
        pokemon = new Pokemon(_base,Level);
        if (isPlayerUnit)
        {
            GetComponent<Image>().sprite = pokemon.Base.BackSprite;   
        }
        else
        {
            GetComponent<Image>().sprite = pokemon.Base.FrontSprite;   
        }
    }
}
