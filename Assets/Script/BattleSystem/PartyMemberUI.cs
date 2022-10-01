using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HpBar hpbar;
    [SerializeField] Color highlightColor;
    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl" + " " + pokemon.Level;
        hpbar.setHp((float)pokemon.HP / pokemon.MaxHP);

    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = highlightColor;

        }
        else
        {
            nameText.color = Color.black;
        }
    }

}
