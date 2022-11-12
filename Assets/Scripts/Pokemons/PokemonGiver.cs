using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonGiver : MonoBehaviour, ISavable
{
    [SerializeField] Pokemon pokemonToGive;
    
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GivePokemon(PlayerMove player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
        pokemonToGive.Init();
        player.GetComponent<PokemonParty>().AddPokemon(pokemonToGive);

        used = true;

        string dialogText = $"{player.Name} đã nhận được Pokemon {pokemonToGive.Base.Name}";
        
        yield return DialogManager.Instance.ShowDialogText(dialogText);


    }

    public bool CanbeGiven()
    {

        return pokemonToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
