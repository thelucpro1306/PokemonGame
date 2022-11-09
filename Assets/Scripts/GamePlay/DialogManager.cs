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

    public static DialogManager Instance { get; private set; }

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    public bool isShowing { get; private set; } 

    int currentLine = 0;
    bool isTyping;

    Dialog dialog;
    Action onDialogFinished;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowDialogText(string text,bool waitForInput = true, bool autoClose = true)
    {
        isShowing = true;
        dialogBox.SetActive(true);
        yield return TypeDialog(text);
        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        }
        if (autoClose)
        {
            CloseDialog();
        }
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        isShowing = false;
    }

    public IEnumerator ShowDialog(Dialog dialog, Action onFinished = null)
    {
        yield return new WaitForEndOfFrame();

        isShowing = true;
        OnShowDialog?.Invoke();
        
        this.dialog = dialog;
        onDialogFinished = onFinished;

        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));

    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !isTyping)
        {
            ++currentLine;
            if (currentLine < dialog.Lines.Count)
            {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
            else
            {

                currentLine = 0;
                isShowing = false;
                dialogBox.SetActive(false);
                onDialogFinished?.Invoke(); 
                OnCloseDialog?.Invoke();
            }
        }
    }

    public IEnumerator TypeDialog(string line)
    {
        dialogText.text = "";
        isTyping = true;
        foreach (var item in line.ToCharArray())
        {
            dialogText.text += item;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }

}
