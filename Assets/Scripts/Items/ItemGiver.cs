using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] ItemBase item;
    [SerializeField] int count = 1;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GiveItem(PlayerMove player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
        player.GetComponent<Inventory>().AddItem(item, count);

        used = true;
        string dialogText = $"{player.Name} đã nhận được {item.Name}";
        if (count > 1)
        {
            dialogText = $"{player.Name} đã nhặt được {count} {item.Name} ";
        }
        yield return DialogManager.Instance.ShowDialogText(dialogText);


    }

    public bool CanbeGiven()
    {

        return item != null && !used && count >0;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool) state;
    }
}
