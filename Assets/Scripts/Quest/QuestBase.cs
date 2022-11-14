using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Quest/Creat a new quest")]

public class QuestBase : ScriptableObject
{
    [SerializeField] string questName;
    [SerializeField] string description;

    [SerializeField] Dialog startDialog;
    [SerializeField] Dialog inProgressDialog;
    [SerializeField] Dialog completeDialog;

    [SerializeField] ItemBase requiredItem;
    [SerializeField] ItemBase rewardItem;
    [SerializeField] Pokemon requiredPokemon;
    [SerializeField] Pokemon rewardPokemon;

    public string QuestName => questName;
    public string Description => description;
    public Dialog StartDialog => startDialog;
    public Dialog InProgressDialog => inProgressDialog?.Lines.Count > 0 ? inProgressDialog : startDialog;
    public Dialog CompleteDialog => completeDialog;
    
    public ItemBase RequiredItem => requiredItem;
    public ItemBase RewardItem => rewardItem;
    public Pokemon RequiredPokemon => requiredPokemon;
    public Pokemon RewardPokemon => rewardPokemon;

    
        


}
