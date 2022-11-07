using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    [SerializeField] GameObject heath;

    public bool isUpdating { get; private set; }


    public void setHp(float hpNormalize)
    {
        heath.transform.localScale = new Vector3(hpNormalize, 1f);
    }

    public IEnumerator SetHPSmooth(float newHp)
    {
        isUpdating = true;
        float curHp = heath.transform.localScale.x;
        float changeAmt = curHp - newHp;
        while(curHp - newHp > Mathf.Epsilon)
        {
            curHp -= changeAmt * Time.deltaTime;
            setHp(curHp);
            yield return null;  
        }
        setHp(newHp);
        isUpdating = false;
    }

}
