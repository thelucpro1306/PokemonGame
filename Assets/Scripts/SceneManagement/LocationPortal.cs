using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocationPortal : MonoBehaviour,IPlayerTriggerable
{
   
    [SerializeField] Transform spawnPoint;
    [SerializeField] DestinationIdentifier destinationIdentifier;

    PlayerMove player;
    Fader fader;


    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    public void onPlayerTriggered(PlayerMove player)
    {
        player.Character.Animator.isMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    IEnumerator Teleport()
    {
       
        GameController.Instance.PauseGame(true);

        //lam` mo` 
        yield return fader.FaderIn(0.5f);
        var desPortal = FindObjectsOfType<LocationPortal>().First(x => x != this
                            && x.destinationIdentifier == this.destinationIdentifier);
        player.Character.SetPositionAndSnapToTile(desPortal.spawnPoint.position);

        //sau khi nhan vat chuyen canh thi` tat lam` mo`

        yield return fader.FaderOut(0.5f);
        GameController.Instance.PauseGame(false);
        
    }

    public Transform SpawnPoint => spawnPoint;

    public bool TriggerRepeatly => false;
}
