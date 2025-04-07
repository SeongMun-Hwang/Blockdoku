using TMPro;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreTmp;
    private void OnEnable()
    {
        scoreTmp.text = GameManager.Instance.scoreManager.ReturnScore().ToString();
    }
}
