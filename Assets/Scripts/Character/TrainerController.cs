using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] GameObject exclaimation;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject fov;
    [SerializeField] Sprite sprite;
    [SerializeField] string name;
    [SerializeField] float money = 100f;
    bool battleLost = false;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();  
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();       
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            yield return DialogManager.Instance.ShowDialog(dialog);
            GameController.Instance.StartTrainerBattle(this);           
        }
        else
        {

            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
        }

    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;

        if (dir == FacingDirection.Right)
        {
            angle = 90f;
        }
        else
        {
            if(dir == FacingDirection.Up)
            {
                angle = 180f;
            }
            else
            {
                if(dir == FacingDirection.Left)
                {
                    angle = 270f;
                } 
            }
        }

        fov.transform.eulerAngles = new Vector3(0f,0f,angle);    

    }

    public IEnumerator triggerTrainerBattle(PlayerMove player)
    {
        // Hien thi dau cham thang tren dau NPC
        exclaimation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclaimation.SetActive(false);

        // NPC di chuyen toi cho nguoi choi

        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;

        moveVec = new Vector2(Mathf.Round(moveVec.x)
            , Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        // NPC noi chuyen voi nguoi choi

        yield return DialogManager.Instance.ShowDialog(dialog);

        GameController.Instance.StartTrainerBattle(this);

       

    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool) state;
        if (battleLost)
        {
            fov.gameObject.SetActive(false);
        }
        else
        {
            fov.gameObject.SetActive(true);
        }
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public float Money
    {
        get => money;
    }

}
