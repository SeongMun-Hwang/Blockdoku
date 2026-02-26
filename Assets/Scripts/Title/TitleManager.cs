using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    //싱글게임 -> 새게임 버튼
    public void BlockdokuBtnOnClicked()
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
    public void TenSumBtnOnClicked()
    {
        SceneManager.LoadScene("10SUM");
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
