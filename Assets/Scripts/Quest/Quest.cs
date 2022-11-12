using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest 
{

    

    public QuestBase Base { get; private set; }

    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _quest)
    {
        Base = _quest;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        yield return DialogManager.Instance.ShowDialog(Base.StartDialog);
    }

    //hoan thanh quest
    public IEnumerator CompletedQuest(Transform player)
    {
        Status = QuestStatus.Completed;
        yield return DialogManager.Instance.ShowDialog(Base.CompleteDialog);

        var inventory = Inventory.GetInventory();

        if(Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }

        if(Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);
            var playerName = player.GetComponent<PlayerMove>().Name;
            yield return DialogManager.Instance.ShowDialogText($"{playerName} đã nhận được {Base.RewardItem.Name}");
        }

    }

    public bool CanBeCompleted()
    {
        var inventory = Inventory.GetInventory();
        if(Base.RequiredItem != null)
        {
            if (!inventory.HasItem(Base.RequiredItem))
            {
                return false;
            }
            
        }
        return true;
    }

}

public enum QuestStatus { None, Started,Completed}
