using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestList : MonoBehaviour
{
    List<Quest> quests = new List<Quest>();

    public event Action OnUpdated;

    public void AddQuest (Quest quest)
    {
        if (!quests.Contains(quest))
        {
            quests.Add(quest);
        }


        OnUpdated?.Invoke();
    }
    public bool IsStarted(string questName)
    {
        var questStatus = quests.FirstOrDefault(q => q.Base.QuestName.Equals(questName))?.Status; 
        return questStatus == QuestStatus.Started || questStatus == QuestStatus.Completed;
    }
    public bool IsCompleted(string questName)
    {
        var questStatus = quests.FirstOrDefault(q => q.Base.QuestName.Equals(questName))?.Status;
        return questStatus == QuestStatus.Completed;
    }

    public static QuestList GetQuestList()
    {
        return FindObjectOfType<PlayerMove>().GetComponent<QuestList>();

    }
}
