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

    public void onPlayerTriggered(PlayerMove player)
    {
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);
        GameController.Instance.PauseGame(true);
        yield return SceneManager.LoadSceneAsync(SceneToLoad);
        Debug.Log("Logggggggggg");
        var desPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationIdentifier == this.destinationIdentifier); 
        player.Character.SetPositionAndSnapToTile(desPortal.spawnPoint.position);
        GameController.Instance.PauseGame(false);
        Destroy(gameObject); 
    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationIdentifier { A, B ,C ,D , E }