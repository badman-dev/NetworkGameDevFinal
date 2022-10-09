using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairAim : MonoBehaviour
{
    void Update()
    {
        //manually blocks mouse input when not focused window
        if (!Application.isFocused)
            return;

        //this script just keeps the aim cursor on your mouse, not networked
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);
    }
}