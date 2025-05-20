using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject multiAnnounce;
    private bool isMultiAnnounceActive = false;
    //싱글게임 -> 새게임 버튼
    public void SingleNewBtnOnclicked()
    {
        if (File.Exists(SavePaths.BoardDataPath))
        {
            File.Delete(SavePaths.BoardDataPath);
            Debug.Log("Board Save file deleted");
        }
        if (File.Exists(SavePaths.BlockDataPath))
        {
            File.Delete(SavePaths.BlockDataPath);
            Debug.Log("Block Save file deleted");
        }
        SceneManager.LoadScene("SingleGame");
    }
    //싱글게임 -> 새게임 버튼
    public void SingleLoadBtnOnClicked()
    {
        SceneManager.LoadScene("SingleGame");
    }
    public void MultiBtnOnClicked()
    {
        multiAnnounce.SetActive(true);
    }
    public void MultiBtnOffClicked()
    {
        multiAnnounce.SetActive(false);
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
