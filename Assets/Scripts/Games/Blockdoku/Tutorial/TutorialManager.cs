using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] List<GameObject> tutorialBoards;
    [SerializeField] List<Cube> tutorialCubes;
    [SerializeField] GameObject tutorialEndPanel;
    private int tutorialIndex = 0;
    private void Awake()
    {
        foreach (Cube cube in tutorialCubes)
        {
            cube.onIsFilledChanged += TutorialProgress;
        }
    }
    public void TutorialProgress(bool isFilled)
    {
        if (tutorialIndex == 2)
        {
            tutorialEndPanel.SetActive(true);
            return;
        }
        tutorialBoards[tutorialIndex].gameObject.SetActive(false);
        tutorialCubes[tutorialIndex].onIsFilledChanged -= TutorialProgress;
        tutorialIndex++;
        GameManager.Instance.scoreManager = tutorialBoards[tutorialIndex].GetComponent<ScoreManager>();
        tutorialBoards[tutorialIndex].SetActive(true);
    }
    public void MoveToSingleGame()
    {
        SceneManager.LoadScene("SingleGame");
    }
}