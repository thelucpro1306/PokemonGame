using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] List<Pokemon> pokemons;

    public event Action onUpdated;

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }
        set
        {
            pokemons = value;
            onUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    private void Start()
    {

    }

    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public bool IsInParty(Pokemon pokemon)
    {
        if(pokemons.Where(p => p == pokemon ).Count() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            onUpdated?.Invoke();
        }
        else
        {
            //
        }
    }

    public void PartyUpdated()
    {
        onUpdated?.Invoke();
    }

    public IEnumerator CheckForEvolution()
    {
        foreach(var pokemon in pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if(evolution != null)
            {
                yield return EvolutionManager.Instance.Evolve(pokemon, evolution);
            }
        }
        
       

    }

    public void RemovePokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Remove(newPokemon);
            onUpdated?.Invoke();
        }
        else
        {
            //
        }
    }

    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerMove>().GetComponent<PokemonParty>();
    }


}
