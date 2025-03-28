using TMPro;
using UnityEngine;

public class UICanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreTmp;
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
}
