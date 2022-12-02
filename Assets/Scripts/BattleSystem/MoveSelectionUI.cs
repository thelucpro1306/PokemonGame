using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveTexts;
    [SerializeField] Color hightlineColor;


    int currentSelection = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for(int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].name;   
        }

        moveTexts[currentMoves.Count].text = newMove.name;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentSelection;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentSelection;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentSelection += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentSelection -= 2;

        currentSelection = Mathf.Clamp(currentSelection, 0,3);

        UpdateOldSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            onSelected?.Invoke(currentSelection );
        }

    }

    public void UpdateOldSelection(int selection)
    {
        for(int i = 0; i < PokemonBase.MaxNumOfMoves +1; i++)
        {
            if(i == selection)
            {
                moveTexts[i].color = hightlineColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }
    }

}
