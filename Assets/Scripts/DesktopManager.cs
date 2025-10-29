using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.LowLevel;
using Unity.Mathematics;
using System;
public class DesktopManager : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;


    public Transform movingObject;

    public bool enableKeyboardControl = true;
    public bool overrideMouse = true;

    public float moveSpeed = 2f;

    //private float xMin, yMin, xMax, yMax;
    public Renderer virtualDesktopRenderer;
    private Vector3 minBounds, maxBounds;

    //Senmag 9D Stylus Parameters
    public GameObject senmagWorkspace;
    private Transform stylusCursor;
    private bool firstTime = false;


    private Vector2 prevMousePos;
    
    void Start()
    {
        prevMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        minBounds = virtualDesktopRenderer.bounds.min;
        maxBounds = virtualDesktopRenderer.bounds.max;
        Debug.Log("Min bounds, Max bounds : "+minBounds + " " + maxBounds);

    }

    private void LeftClick()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
    }

    private void LeftClickHold()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
    }

    private void RightClick()
    {
        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
    }

    private void RightClickHold()
    {
        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);
    }

    
    void Update()
    {
        if (!overrideMouse)
            return;

        if (senmagWorkspace.transform.childCount == 0 || senmagWorkspace.transform.GetChild(0) == null)
            return;

        stylusCursor = senmagWorkspace.transform.GetChild(0).GetChild(1);

        //if(enableKeyboardControl)
        //{
        //    float moveX = Input.GetAxis("Horizontal");
        //    float movey = Input.GetAxis("Vertical");
        //    Vector3 move = new Vector3(moveX, movey, 0f) * moveSpeed * Time.deltaTime;
        //    movingObject.Translate(move, Space.World);

            //if(Input.GetKey(KeyCode.Space))
            //{
            //    LeftClick();    
            //}

            //if(Input.GetKey(KeyCode.RightShift))
            //{
            //    RightClick();
            //}

        //}




        if (Input.GetKey(KeyCode.LeftShift))
        {
            LeftClick();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            RightClick();
        }



        //float normalizedX = Mathf.InverseLerp(minBounds.x, maxBounds.x, movingObject.position.x);
        //float normalizedY = Mathf.InverseLerp(minBounds.y, maxBounds.y, movingObject.position.y);


        float normalizedX = Mathf.InverseLerp(minBounds.x, maxBounds.x, stylusCursor.position.x);
        float normalizedY = Mathf.InverseLerp(minBounds.y, maxBounds.y, stylusCursor.position.y);


        int screenX = Mathf.RoundToInt(normalizedX * 2560f);
        int screenY = Mathf.RoundToInt((1 - normalizedY) * 1440f);

        SetCursorPos(screenX, screenY);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            LeftClick();
            Debug.Log("Left Click");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RightClick();
            Debug.Log("Right Click");
        }



    }
}
