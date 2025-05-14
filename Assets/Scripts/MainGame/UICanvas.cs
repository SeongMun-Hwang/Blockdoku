using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UICanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreTmp;
    [SerializeField] TextMeshProUGUI comboTmp;
    [SerializeField] GameObject gameOverPanel;
    private static UICanvas instance;
    public static UICanvas Instance
    {
        get { return instance; }
    }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void SetScore(int score)
    {
        scoreTmp.text = score.ToString();
    }
    public void ShowCombo(string str)
    {
        StartCoroutine(ShowComboCoroutine(str));
    }
    private IEnumerator ShowComboCoroutine(string str)
    {
        comboTmp.text = str;

        Color textColor = comboTmp.color;
        textColor.a = 1f;

        for (float f = 0; f < 1f; f += Time.deltaTime)
        {
            textColor.a = Mathf.Lerp(1f, 0f, f / 1f);
            comboTmp.color = textColor;
            yield return null;
        }
    }
    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        GameManager.Instance.RemoveGameData();
    }
    public void RetryBtnOnclicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void TitleBtnOnClicked()
    {
        SceneManager.LoadScene("Title");
    }
    public void BackBtnOnClicked()
    {
        GameManager.Instance.SaveGameData();
        SceneManager.LoadScene("Title");
    }
    public void ResetBtnOnClicked()
    {
        GameManager.Instance.RemoveGameData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
