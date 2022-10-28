using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScene; 
    public bool isLoaded { get; private set; }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
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
            if(GameController.Instance.PreScene!= null)
            {
                var previoslyLoadedScene = GameController.Instance.PreScene.connectedScene;
                foreach(var scene in previoslyLoadedScene)
                {
                    if(!connectedScene.Contains(scene) && scene != this)
                    {
                        scene.UnLoadScene();
                    }
                }
            }

        }   
    }

    public void LoadScene()
    {
        if (!isLoaded)
        {
            SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);  
            isLoaded = true;
        }
    }

    public void UnLoadScene()
    {
        if (isLoaded)
        {
            SceneManager.UnloadSceneAsync(gameObject.name);
            isLoaded = false;
        }
    }

}
