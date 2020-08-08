using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabFunctionality : MonoBehaviour
{
    public InputField[] inputFields;

    public void Start()
    {
    }

    //// Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (inputFields[0].isFocused)
            {
                inputFields[1].Select();
            }
            else
            {
                inputFields[0].Select();
            }

        }
    }
}
