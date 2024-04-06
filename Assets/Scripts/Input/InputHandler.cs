using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField, ReadOnly] private Vector2 mouseDir = Vector2.zero;

    private void Update()
    {
        if (LobbyUI.Instance.inputMode == InputMode.Mouse)
        {
            Vector3 mouseWorldPos = Utils.Camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            mouseDir = mouseWorldPos - transform.position;
            mouseDir.Normalize();
        } else if (LobbyUI.Instance.inputMode == InputMode.Keyboard)
        {
            mouseDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            mouseDir.Normalize();
        }
        //TODO: Add Joystick
        // else if (LobbyUI.Instance.inputMode == InputMode.Joystick)
        // {
        // }
    }

    public NetworkInputData GetNetworkInput() => new(mouseDir);
}

public enum InputMode
{
    Mouse = 0,
    Keyboard = 1,
    // Joystick = 2
}