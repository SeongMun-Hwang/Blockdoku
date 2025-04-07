using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{ 
    public void SingleBtnOnClicked()
    {
        SceneManager.LoadScene("SingleGame");
    }
    public void MultiBtnOnClicked()
    {
        Debug.Log("Multi game btn clicked");
    }
    public void QuitBtnOnClicked()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
