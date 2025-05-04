using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject singleBtn;
    private bool isSingleBtnClicked = false;
    //싱글게임 버튼
    public void SingleBtnOnClicked()
    {
        if (!isSingleBtnClicked)
        {
            singleBtn.GetComponent<Animator>().SetTrigger("OnClicked");
            isSingleBtnClicked = true;
        }
        else
        {
            singleBtn.GetComponent<Animator>().SetTrigger("OffClicked");
            isSingleBtnClicked = false;
        }
    }
    //싱글게임 -> 새게임 버튼
    public void SingleNewBtnOnclicked()
    {
        SceneManager.LoadScene("SingleGame");
    }
    //싱글게임 -> 새게임 버튼
    public void SingleLoadBtnOnClicked()
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
