using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FloatingScore : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private Vector3 originalLocalPos;
    private Color originalColor;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float moveDistance = 100.0f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    private Queue<string> messageQueue = new Queue<string>();
    private bool isShowing = false;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        originalLocalPos = transform.localPosition;
        originalColor = textMesh.color;

        // Ensure it's hidden initially
        gameObject.SetActive(false);
    }

    public void Show(int score, int combo, string specialMessage = "")
    {
        string msg = "";
        if (!string.IsNullOrEmpty(specialMessage))
        {
            msg = $"{specialMessage}!\n+{score}";
        }
        else
        {
            msg = combo > 0 ? $"{combo} combo!\n+{score}" : $"+{score}";
        }
        
        messageQueue.Enqueue(msg);
        
        if (!isShowing)
        {
            // 코루틴을 시작하기 전에 반드시 오브젝트를 활성화해야 합니다.
            gameObject.SetActive(true);
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        isShowing = true;

        while (messageQueue.Count > 0)
        {
            textMesh.text = messageQueue.Dequeue();
            yield return StartCoroutine(AnimateScore());
        }

        gameObject.SetActive(false);
        isShowing = false;
    }

    private IEnumerator AnimateScore()
    {
        float elapsed = 0f;
        transform.localPosition = originalLocalPos;
        Color color = originalColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Move up
            transform.localPosition = originalLocalPos + new Vector3(0, progress * moveDistance, 0);

            // Handle Alpha (Fade In and Fade Out)
            if (elapsed < fadeInDuration)
            {
                color.a = Mathf.Lerp(0f, originalColor.a, elapsed / fadeInDuration);
            }
            else if (elapsed > duration - fadeOutDuration)
            {
                float fadeOutElapsed = elapsed - (duration - fadeOutDuration);
                color.a = Mathf.Lerp(originalColor.a, 0f, fadeOutElapsed / fadeOutDuration);
            }
            else
            {
                color.a = originalColor.a;
            }

            textMesh.color = color;
            yield return null;
        }
        
        // Reset position for next message if any
        transform.localPosition = originalLocalPos;
    }
}
