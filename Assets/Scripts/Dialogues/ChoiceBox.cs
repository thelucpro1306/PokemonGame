using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] ChoiceText choiceTextPrefab;
    
    bool choiceSelected = false;
    List<ChoiceText> choiceText;
    int currentChoice;


    public IEnumerator ShowChoices(List<string> choices, Action<int> onChoiceSelected )
    {
        choiceSelected = false;
        currentChoice = 0;

        gameObject.SetActive(true);

        //Xoa cac doi tuong con mac dinh
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        choiceText = new List<ChoiceText>();

        // them choice tu` prefab
        foreach(var choice in choices)
        {
            var choiceTextObj = Instantiate(choiceTextPrefab,transform);           
            choiceTextObj.Textfied.text = choice;
            choiceText.Add(choiceTextObj);
        }  

        yield return new WaitUntil( () => choiceSelected == true);

        onChoiceSelected?.Invoke(currentChoice);
        gameObject.SetActive(false);


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++currentChoice;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --currentChoice;
            }
        }

        currentChoice = Mathf.Clamp(currentChoice, 0, choiceText.Count - 1);

        for(int i = 0; i < choiceText.Count; ++i)
        {
            choiceText[i].SetSelected(i == currentChoice);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            choiceSelected = true;
        }


    }

}
