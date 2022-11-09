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
    
    Pokemon _pokemon;

    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();
        _pokemon.OnHpChanged += UpdateData;

    }

    void UpdateData()
    {
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl" + " " + _pokemon.Level;

        //if(_pokemon.MaxHP > 0)
        //{
        //    hpbar.setHp((float)_pokemon.HP / _pokemon.MaxHP);
        //}
        hpbar.setHp((float)_pokemon.HP / _pokemon.MaxHP);
    }


    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = GlobalSetting.i.HighlightedColor;

        }
        else
        {
            nameText.color = Color.black;
        }
    }

}
