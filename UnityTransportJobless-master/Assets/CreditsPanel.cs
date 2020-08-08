using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsPanel : MonoBehaviour
{
    float BeginY;
    private void Start()
    {
        BeginY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        float y = transform.position.y + .1f;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);

        if(y > 110)
            transform.position = new Vector3(transform.position.x, BeginY, transform.position.z);
    }
}
