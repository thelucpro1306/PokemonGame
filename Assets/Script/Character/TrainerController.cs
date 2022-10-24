using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] GameObject exclaimation;
    [SerializeField] Dialog dialog;
    [SerializeField] GameObject fov;
    [SerializeField] Sprite sprite;
    [SerializeField] string name;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();  
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
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

        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
        {
            GameController.Instance.StartTrainerBattle(this);
        }));

    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}
