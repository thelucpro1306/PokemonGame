using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Quest
{
    public QuestBase Base { get; private set; }

    public QuestStatus Status { get; private set; }



    public Quest(QuestBase _quest)
    {
        Base = _quest;
    }

    public Quest(QuestSaveData SaveData)
    {
        Base = QuestDB.GetObjectByName(SaveData.name);
        Status = SaveData.status;
    }

    public QuestSaveData GetSaveData()
    {
        var saveData = new QuestSaveData()
        {
            name = Base.name,
            status = Status
        };
        return saveData;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        yield return DialogManager.Instance.ShowDialog(Base.StartDialog);

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    //hoan thanh quest
    public IEnumerator CompletedQuest(Transform player)
    {
        Status = QuestStatus.Completed;
        yield return DialogManager.Instance.ShowDialog(Base.CompleteDialog);

        var inventory = Inventory.GetInventory();
        var pokemon = PokemonParty.GetPlayerParty();
        if (Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
            pokemon.RemovePokemon(Base.RequiredPokemon);
        }

        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);
            var playerName = player.GetComponent<PlayerMove>().Name;
            
            yield return DialogManager.Instance.ShowDialogText($"{playerName} đã nhận được {Base.RewardItem.Name}");
        }
        if (Base.RewardPokemon.Base != null)
        {
            Base.RewardPokemon.Init();
            pokemon.AddPokemon(Base.RewardPokemon);
        }
        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public bool CanBeCompleted()
    {
        var inventory = Inventory.GetInventory();
        var pokemon = PokemonParty.GetPlayerParty();
        if (Base.RequiredItem != null)
        {
            if (!inventory.HasItem(Base.RequiredItem))
            {
                return false;
            }

        }

        if (Base.RequiredPokemon.Base != null)
        {
            if (!pokemon.IsInParty(Base.RequiredPokemon))
            {
                return false;
            }
        }
        return true;
    }

}

public enum QuestStatus { None, Started, Completed }

[System.Serializable]
public class QuestSaveData
{
    public string name;
    public QuestStatus status;
}