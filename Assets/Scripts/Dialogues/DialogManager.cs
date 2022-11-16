using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;
    [SerializeField] ChoiceBox choiceBox;


    public static DialogManager Instance { get; private set; }

    public event Action OnShowDialog;
    public event Action onDialogFinish;

    public bool isShowing { get; private set; } 
    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowDialogText(string text,bool waitForInput = true, bool autoClose = true
                , List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        OnShowDialog?.Invoke();
        isShowing = true;
        dialogBox.SetActive(true);
        yield return TypeDialog(text);
        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        }

        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        if (autoClose)
        {
            CloseDialog();
        }
        onDialogFinish?.Invoke();
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        isShowing = false;
    }

    public IEnumerator ShowDialog(Dialog dialog, List<string> choices = null,Action<int> onChoiceSelected = null)
    {
        yield return new WaitForEndOfFrame();

        isShowing = true;
        OnShowDialog?.Invoke();
        dialogBox.SetActive(true);
        foreach(var line in dialog.Lines)
        {
            yield return TypeDialog(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        }

        if(choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        dialogBox.SetActive(false);
        isShowing = false;
        onDialogFinish?.Invoke();
    }




    public void HandleUpdate()
    {
        
    }

    public IEnumerator TypeDialog(string line)
    {
        dialogText.text = "";

        foreach (var item in line.ToCharArray())
        {
            dialogText.text += item;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

    }

}
