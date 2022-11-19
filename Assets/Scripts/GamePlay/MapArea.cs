using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Tổng tỉ lệ phải bằng 100")]
    [SerializeField] List<PokemonEncouterRecord> wildPokemons;

    private void Start()
    {
        int totalChance = 0;
        foreach(PokemonEncouterRecord record in wildPokemons)
        {
            record.chanceLower = totalChance;
            record.chanceUpper = totalChance + record.chancePercentage;
            totalChance += record.chancePercentage; 
        }
    }

    public Pokemon GetRandomWildPokemon()
    {
        int randVal = Random.Range(1, 101);
        var pokemonRecord = wildPokemons.First(p => randVal >= p.chanceLower && randVal <= p.chanceUpper);
        var levelRange = pokemonRecord.levelRange;
        
        var level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y+1);
        var wildPokemon = new Pokemon(pokemonRecord.pokemon, level);

        wildPokemon.Init();
        return wildPokemon;

    }
}

[System.Serializable]
public class PokemonEncouterRecord
{
    public PokemonBase pokemon;
    public Vector2Int levelRange;
    public int chancePercentage;

    public int chanceLower { get; set; }
    public int chanceUpper { get; set; }

}