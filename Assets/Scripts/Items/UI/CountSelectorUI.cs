using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] Text countText;
    [SerializeField] Text priceText;

    bool selected;
    int currentCount;
    int maxCount;
    float pricePerUnit;

    public IEnumerator ShowSelector(int maxCount, float pricePerUnit, Action<int> onCountSelected)
    {
        this.maxCount = maxCount;
        this.pricePerUnit = pricePerUnit;
        selected = false;
        currentCount = 1;
        gameObject.SetActive(true);
        SetValues();

        yield return new WaitUntil(() => selected == true);

        onCountSelected?.Invoke(currentCount);
        gameObject.SetActive(false);

    }

    private void Update()
    {
        int prevCount = currentCount;

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            ++currentCount;
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                --currentCount;
            }
        }



        currentCount = Mathf.Clamp(currentCount, 1, maxCount);

        if (currentCount != prevCount)
        {
            SetValues();
        }
        if (Input.GetKeyUp(KeyCode.Return))
        {
            selected = true;
        }
    }

    void SetValues()
    {
        countText.text = "x " + currentCount;
        priceText.text = "G " + pricePerUnit * currentCount;
    }


}
