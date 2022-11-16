using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    [SerializeField] Dialog yesDialog;
    [SerializeField] Dialog NoDialog;

    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        
        int selectedChoice = 0;

        yield return DialogManager.Instance.ShowDialogText($"Tôi sẽ hồi phục cho Pokemon của bạn !"
            ,choices: new List<string>() { "Có", "Không" }
            ,onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if(selectedChoice == 0)
        {
            //Yes 
            yield return Fader.Instance.FaderIn(0.5f);
            var playerParty = player.GetComponent<PokemonParty>();

            playerParty.Pokemons.ForEach(p => p.Heal());
            playerParty.PartyUpdated();

            yield return Fader.Instance.FaderOut(0.5f);

            yield return DialogManager.Instance.ShowDialog(yesDialog);
        }
        else
        {
            if (selectedChoice == 1)
            {
                //No
                yield return DialogManager.Instance.ShowDialog(NoDialog);
            }
        }


        
    }

}
