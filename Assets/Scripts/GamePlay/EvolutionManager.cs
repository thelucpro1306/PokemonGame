using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    public event Action OnStartEvolution; 
    public event Action OnCompletedEvolution; 


    public static EvolutionManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        var oldPokemon = pokemon.Base;
        evolutionUI.SetActive(true);

        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} đang tiến hoá ");
        pokemon.Evolve(evolution);
        Color originalColor = pokemonImage.color;
        yield return EvolutionAnimation(originalColor);
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldPokemon.Name} tiến hoá thành {pokemon.Base.Name}");

        evolutionUI.SetActive(false);

        OnCompletedEvolution?.Invoke();
    }

    IEnumerator EvolutionAnimation(Color originalColor)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(pokemonImage.DOColor(Color.red, 0.5f));
        sequence.Append(pokemonImage.DOColor(originalColor, 0.5f));
        sequence.Append(pokemonImage.DOColor(Color.red, 0.5f));
        sequence.Append(pokemonImage.DOColor(originalColor, 0.5f));
        sequence.Append(pokemonImage.DOColor(Color.red, 0.5f));
        sequence.Append(pokemonImage.DOColor(originalColor, 0.5f));
        yield return sequence.WaitForCompletion();
    }


}
