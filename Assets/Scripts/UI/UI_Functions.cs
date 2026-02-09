using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Functions : MonoBehaviour
{
    private UI_Functions instance;
    public UI_Functions Instance { get { return instance; } }
    public void BacktoTitleOnClicked()
    {
        SceneManager.LoadScene("Title");
    }
    public void RestartGameOnClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
