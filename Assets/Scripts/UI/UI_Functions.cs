using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class UI_Functions : MonoBehaviour
{
    public static UI_Functions Instance { get; private set; }

    public static event Action OnGameRestart;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackInput();
        }
    }

    private void HandleBackInput()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Title")
        {
            Debug.Log("Quitting application...");
            Application.Quit();
        }
        else
        {
            Debug.Log("Returning to Title scene...");
            BacktoTitleOnClicked();
        }
    }

    public void BacktoTitleOnClicked()
    {
        SceneManager.LoadScene("Title");
    }

    public void TriggerGameRestart()
    {
        OnGameRestart?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
