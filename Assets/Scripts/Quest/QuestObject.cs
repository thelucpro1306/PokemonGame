using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] QuestBase questToCheck;
    [SerializeField] ObjectAction onStart;
    [SerializeField] ObjectAction onComplete;

    QuestList questList;

    private void Start()
    {
        questList = QuestList.GetQuestList();
        questList.OnUpdated += UpdateObjectStatus;
        UpdateObjectStatus();
    }

    private void OnDestroy()
    {
        questList.OnUpdated -= UpdateObjectStatus;
    }

    public void UpdateObjectStatus()
    {
        if (onStart != ObjectAction.DoNotThing && questList.IsStarted(questToCheck.QuestName))
        {
            foreach(Transform child in transform)
            {
                if(onStart == ObjectAction.Enable)
                {
                    child.gameObject.SetActive(true);
                }
                else
                {
                    if (onStart == ObjectAction.Disable)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

        if (onComplete != ObjectAction.DoNotThing && questList.IsCompleted(questToCheck.QuestName))
        {
            foreach (Transform child in transform)
            {
                if (onComplete == ObjectAction.Enable)
                {
                    child.gameObject.SetActive(true);
                }
                else
                {
                    if (onComplete == ObjectAction.Disable)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

    }




}

public enum ObjectAction { DoNotThing, Enable, Disable }