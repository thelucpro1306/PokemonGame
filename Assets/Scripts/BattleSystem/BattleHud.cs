using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HpBar hpbar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;
    [SerializeField] Color cfnColor;

    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColors;
    public void SetData(Pokemon pokemon)
    {
        if(_pokemon != null)
        {
            _pokemon.OnHpChanged -= UpdateHP;
            _pokemon.OnStatusChanged -= SetStatusText;
        }

        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        SetLevel();
        hpbar.setHp((float)pokemon.HP / pokemon.MaxHP);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.slp, slpColor },
            {ConditionID.par, parColor },
            {ConditionID.frz, frzColor },
            {ConditionID.cfn, cfnColor },
        };

        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
        _pokemon.OnHpChanged += UpdateHP;
    }

    void SetStatusText()
    {
        if(_pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokemon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.Id];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lv. " + _pokemon.Level;
    }

    public void SetExp()
    {
        if(expBar == null)
        {
            return;
        }
        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset=false)
    {
        if (expBar == null)
        {
            yield break;
        }

        if(reset)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);

        float normalizedExp = (float)(_pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpbar.isUpdating == false);
    }

    public IEnumerator UpdateHPAsync()
    {
        
        yield return hpbar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHP);
            
    }

    public void ClearData()
    {
        if (_pokemon != null)
        {
            _pokemon.OnHpChanged -= UpdateHP;
            _pokemon.OnStatusChanged -= SetStatusText;
        }
    }

}
