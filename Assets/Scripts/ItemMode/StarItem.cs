using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StarItem : MonoBehaviour
{
    [SerializeField] Image itemImage;
    private Coroutine currentCoroutine;

    private void OnDisable()
    {
        StopBlink();
    }
    public void StartBlink()
    {
        currentCoroutine = StartCoroutine(BlinkCoroutine());
    }
    private IEnumerator BlinkCoroutine()
    {
        Color textColor = itemImage.color;
        textColor.a = 1f;
        while (true)
        {
            for (float f = 0; f < 1f; f += Time.deltaTime)
            {
                textColor.a = Mathf.Lerp(1f, 0f, f / 1f);
                itemImage.color = textColor;
                yield return null;
            }
            for (float f = 1f; f > 0f; f -= Time.deltaTime)
            {
                textColor.a = Mathf.Lerp(0f, 1f, f / 1f);
                itemImage.color = textColor;
                yield return null;
            }
        }
    }
    private void StopBlink()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }
}
