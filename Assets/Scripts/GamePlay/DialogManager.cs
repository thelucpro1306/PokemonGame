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
    public event Action onDialogFinish;

    public bool isShowing { get; private set; } 
    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowDialogText(string text,bool waitForInput = true, bool autoClose = true)
    {
        OnShowDialog?.Invoke();
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
        onDialogFinish?.Invoke();
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        isShowing = false;
    }

    public IEnumerator ShowDialog(Dialog dialog)
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
