using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        
        yield return DialogManager.Instance.ShowDialog(dialog);
        yield return Fader.Instance.FaderIn(0.5f);
        var playerParty = player.GetComponent<PokemonParty>();

        playerParty.Pokemons.ForEach(p => p.Heal());
        playerParty.PartyUpdated();

        yield return Fader.Instance.FaderOut(0.5f);

        yield return DialogManager.Instance.ShowDialogText($"Pokemon của bạn đã hoàn toàn khoẻ mạnh");
    }

}
