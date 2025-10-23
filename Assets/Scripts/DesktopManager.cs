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

    //private float xMin, yMin, xMax, yMax;
    public Renderer virtualDesktopRenderer;
    private Vector3 minBounds, maxBounds;
   


    private Vector2 prevMousePos;
    
    void Start()
    {
        prevMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        minBounds = virtualDesktopRenderer.bounds.min;
        maxBounds = virtualDesktopRenderer.bounds.max;
        Debug.Log("Min bounds, Max bounds : "+minBounds + " " + maxBounds);

    }

    
    void Update()
    {
        
        if(enableKeyboardControl)
        {
            float moveX = Input.GetAxis("Horizontal");
            float movey = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(moveX, movey, 0f) * moveSpeed * Time.deltaTime;
            movingObject.Translate(move, Space.World);


        }

        float normalizedX = Mathf.InverseLerp(minBounds.x, maxBounds.x, movingObject.position.x);
        float normalizedY = Mathf.InverseLerp(minBounds.y, maxBounds.y, movingObject.position.y);

        int screenX = Mathf.RoundToInt(normalizedX * 2560f);
        int screenY = Mathf.RoundToInt((1 - normalizedY) * 1440f);

        SetCursorPos(screenX, screenY); 

    }
}
