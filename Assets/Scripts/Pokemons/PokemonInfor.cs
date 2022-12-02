using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokemonInfor : MonoBehaviour
{
    [SerializeField] Text txtName;
    [SerializeField] Text txtStatus = null;
    [SerializeField] Text txtType1;
    [SerializeField] Text txtType2;
    [SerializeField] Text txtHp;
    [SerializeField] Text txtActtack;
    [SerializeField] Text txtSpActtack;
    [SerializeField] Text txtSpDefense;
    [SerializeField] Text txtDefense;
    [SerializeField] Text txtSpeed;
    [SerializeField] Image pokemonImage;

    [SerializeField] GameObject info;

    public static PokemonInfor instance { get; set; }

    public event Action onStart;
    public event Action onBack;

    private void Awake()
    {
        instance = this;   
    }

    

    public void SetPokemonSelectedData(Pokemon pokemon)
    {
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        txtName.text = pokemon.Base.Name;
        txtType1.text = pokemon.Base.Type1.ToString();
        txtType2.text = pokemon.Base.Type2.ToString();
        txtHp.text = pokemon.HP.ToString() + "/" +pokemon.MaxHP.ToString();
        txtActtack.text = pokemon.Attack.ToString();
        txtSpActtack.text = pokemon.SpAttack.ToString();
        txtDefense.text = pokemon.Defense.ToString();
        txtSpDefense.text = pokemon.SpDefense.ToString();
        txtSpeed.text = pokemon.Speed.ToString();
    }

    public void Open()
    {
        onStart?.Invoke();
        gameObject.SetActive(true);    
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    public void HandleUpdate()
    { 
        if(Input.GetKeyDown(KeyCode.K))
        {
            CloseMenu();
            onBack?.Invoke();            
        }
    }

}
