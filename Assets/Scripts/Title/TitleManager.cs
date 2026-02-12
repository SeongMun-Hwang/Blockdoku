using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject multiAnnounce;
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
        SceneManager.LoadScene("Blockdoku_2D");
    }
    //싱글게임 -> 새게임 버튼
    public void SingleLoadBtnOnClicked()
    {
//#if UNITY_EDITOR
//        PlayerPrefs.SetInt("NotFirstTime", 0);
//#endif
//        if (PlayerPrefs.GetInt("NotFirstTime") == 1)
//        {
            SceneManager.LoadScene("Blockdoku_2D");
//        }
//        else
//        {
//            PlayerPrefs.SetInt("NotFirstTime", 1);
//            SceneManager.LoadScene("Tutorial");
//        }
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
    public void OnQuestionMarkClicked()
    {
        SceneManager.LoadScene("Tutorial");
    }
}
