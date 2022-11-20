using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogAction : CutSceneAction
{
    [SerializeField] Dialog dialog;

    public override IEnumerator Play()
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
    }

}
