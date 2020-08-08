using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    Vector2 originPosition;

    private void Start()
    {
        StartCoroutine(Shake(.15f,4f));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        originPosition = panel.GetComponent<RectTransform>().position;
        float elapse = 0f;

        while (elapse < duration)
        {
            float objX = originPosition.x;
            float objY = originPosition.y;
            float x = Random.Range(objX - 1, objX + 1) * magnitude;
            float y = Random.Range(objY-1, objY + 1) * magnitude;

            panel.GetComponent<RectTransform>().position = new Vector2(objX, objY);
            elapse = Time.deltaTime;
            yield return null;

        }

        panel.GetComponent<RectTransform>().position = originPosition;

    }
}