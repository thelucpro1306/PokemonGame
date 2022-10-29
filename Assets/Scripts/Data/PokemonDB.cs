using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonDB : MonoBehaviour
{
    static Dictionary<string, PokemonBase> pokemons;

    public static void Init()
    {

        pokemons = new Dictionary<string, PokemonBase>();   
        //Assets/Game/Resources/Pokemons
        var pokemonArray = Resources.LoadAll<PokemonBase>("");
        foreach(var pokemon in pokemonArray)
        {
            if (pokemons.ContainsKey(pokemon.Name))
            {
                Debug.LogError($"There two pokemon with same {pokemon.Name}");
                continue;
            }

            pokemons[pokemon.Name] = pokemon;
        }
    }

    public static PokemonBase GetPokemonByName(string name)
    {
        if (!pokemons.ContainsKey(name))
        {
            Debug.LogError($"Pokemon with name {name} dosen't contain in DB");
        }
        return pokemons[name];
    }

}
