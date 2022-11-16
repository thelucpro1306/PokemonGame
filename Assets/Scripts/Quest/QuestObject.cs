using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] QuestBase questToCheck;
    [SerializeField] ObjectAction onStart;
    [SerializeField] ObjectAction onComplete;
    [Header("Đối tượng phụ muốn BẬT sau khi hoàn thành quest")]
    [SerializeField] List<GameObject> enableConnectObjects = new();
    [Header("Đối tượng phụ muốn TẮT sau khi hoàn thành quest")]
    [SerializeField] List<GameObject> disableConnectObjects = new();
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
            foreach (Transform child in transform)
            {
                if (onStart == ObjectAction.Enable)
                {
                    child.gameObject.SetActive(true);

                    var saveable = child.GetComponent<SavableEntity>();
                    if (saveable != null)
                    {
                        SavingSystem.i.RestoreEntity(saveable);
                    }
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
                    var saveable = child.GetComponent<SavableEntity>();
                    if (saveable != null)
                    {
                        SavingSystem.i.RestoreEntity(saveable);
                    }
                }
                else
                {
                    if (onComplete == ObjectAction.Disable)
                    {
                        child.gameObject.SetActive(false);
                        if (enableConnectObjects != null)
                        {
                            foreach (var item in enableConnectObjects)
                            {
                                item.SetActive(true);
                            }
                        }
                    }
                }
            }
        }

    }




}

public enum ObjectAction { DoNotThing, Enable, Disable }
