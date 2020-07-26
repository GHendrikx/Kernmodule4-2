using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    private Vector3 beginPosition;
    private RectTransform rectTransform;
    private void Start()
    {
        beginPosition = transform.position;
        rectTransform = GetComponent<RectTransform>();
    }
    private void Update()
    {
        float yPos = transform.position.y + 1f;
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);

        if (rectTransform.rect.top < 950)
            transform.position = beginPosition;
    }
}
