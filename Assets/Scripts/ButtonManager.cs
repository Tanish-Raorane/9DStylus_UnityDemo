using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class ButtonManager : MonoBehaviour
{
    public DesktopManager desktopManager;

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern System.IntPtr GetActiveWindow();

    private const int SW_MINIMIZE = 6;

    void Start()
    {
        
    }

    

    public void EnableOverride()
    {
        desktopManager.overrideMouse = true;
    }

    public void DisableOverride()
    {
        desktopManager.overrideMouse = false;
    }

    public void Minimize()
    {
        System.IntPtr hWnd = GetActiveWindow();
        ShowWindow(hWnd, SW_MINIMIZE);

    }

    public void Quit()
    {
        Application.Quit();
    }

    void Update()
    {
        
    }
}
