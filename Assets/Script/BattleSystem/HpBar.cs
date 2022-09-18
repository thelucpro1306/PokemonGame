using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    [SerializeField] GameObject heath;

    public void setHp(float hpNormalize)
    {
        heath.transform.localScale = new Vector3(hpNormalize, 1f);
    }

}
