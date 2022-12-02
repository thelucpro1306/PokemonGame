using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScene;

    public bool isLoaded { get; private set; }

    List<SavableEntity> savableEntities;

    public static SceneDetails Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log($"Entered {gameObject.name}");
            LoadScene();

            GameController.Instance.SetCurrentScene(this);
            //load cac map co ket noi
            foreach (var item in connectedScene)
            {
                item.LoadScene();
            }

            //Unload scene khong connect

            var preScene = GameController.Instance.PrevScene;
            if (preScene != null)
            {
                var previoslyLoadedScene = preScene.connectedScene;
                foreach (var scene in previoslyLoadedScene)
                {
                    if (!connectedScene.Contains(scene) && scene != this)
                    {
                        scene.UnLoadScene();
                    }
                }
                if (!connectedScene.Contains(preScene))
                {
                    preScene.UnLoadScene();
                }


            }

        }
    }

    public void LoadScene()
    {
        if (!isLoaded)
        {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            isLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };


        }
    }

    public void UnLoadScene()
    {
        if (isLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            isLoaded = false;
        }
    }

    public List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);

        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return savableEntities;
    }

}
