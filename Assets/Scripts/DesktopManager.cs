using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.LowLevel;
public class DesktopManager : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    public Transform movingObject;

    public bool enableKeyboardControl = true;

    public float moveSpeed = 2f;


    private Vector2 prevMousePos;
    
    void Start()
    {
        prevMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);


    }

    
    void Update()
    {
        
        if(enableKeyboardControl)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;
            movingObject.Translate(move, Space.World);


        }

    }
}
