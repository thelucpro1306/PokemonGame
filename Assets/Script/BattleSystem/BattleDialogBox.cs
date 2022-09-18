using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject MoveSelector;
    [SerializeField] GameObject MoveDetails;
    [SerializeField] int lettersPerSecond;
    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Color hightlightedColor;

    [SerializeField] Text PPTexts;
    [SerializeField] Text typeTexts;



    public void setDialog(string dialog)
    {
        dialogText.text = dialog;    
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";

        foreach (var item in dialog.ToCharArray())
        {
            dialogText.text += item;
            yield return new WaitForSeconds(1f/lettersPerSecond);  
        }    
    }

    public void enableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void enableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);  
    }

    public void enableMoveSelector(bool enabled)
    {
        MoveSelector.SetActive(enabled);
        MoveDetails.SetActive(enabled); 
    }

    public void updateActionSelection(int selected)
    {
        for(int i = 0; i < actionTexts.Count; ++i)
        {
            if(i == selected)
            {
                actionTexts[i].color = hightlightedColor;
            }
            else
            {
                actionTexts[i].color = Color.black;
            }
        }   
    }

}
