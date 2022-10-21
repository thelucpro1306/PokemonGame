using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour,Interactable 
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Sprite> sprites;

    SpriteAnimator spriteAnimator;

    //private void Start()
    //{
    //    spriteAnimator = new SpriteAnimator(sprites, GetComponent<SpriteRenderer>());
    //    spriteAnimator.Start();
    //}

    //private void Update()
    //{
    //    spriteAnimator.HandleUpdate();
    //}

    public void Interact()
    {
       StartCoroutine(DialogManager.Instance.ShowDialog(dialog));

    }



}
