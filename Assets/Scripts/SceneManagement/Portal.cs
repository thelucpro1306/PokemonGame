using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int SceneToLoad = -1;
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
        StartCoroutine(SwitchScene());
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);

        //lam` mo` 
        yield return fader.FaderIn(0.5f);


        //load scene
        yield return SceneManager.LoadSceneAsync(SceneToLoad);
        
        var desPortal = FindObjectsOfType<Portal>().First(x => x != this 
                            && x.destinationIdentifier == this.destinationIdentifier); 
        player.Character.SetPositionAndSnapToTile(desPortal.spawnPoint.position);

        //sau khi nhan vat chuyen canh thi` tat lam` mo`

        yield return fader.FaderOut(0.5f);
        GameController.Instance.PauseGame(false);
        Destroy(gameObject); 
    }

    public Transform SpawnPoint => spawnPoint;

    public bool TriggerRepeatly => false;
}

public enum DestinationIdentifier { A, B ,C ,D , E,ToGym1 }