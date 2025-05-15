using TMPro;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreTmp;
    private void OnEnable()
    {
        if (scoreTmp != null)
        {
            scoreTmp.text = GameManager.Instance.scoreManager.ReturnScore().ToString();
        }
    }
}
