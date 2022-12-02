using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject PressStart;
    [SerializeField] GameObject mainMenu;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PressStart.gameObject.SetActive(false);
            mainMenu.gameObject.SetActive(true);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadGame()
    {
        var operation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        operation.completed += (AsyncOperation op) =>
        {
            GameController.Instance.OnMenuSelected(3);
        };

        
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

