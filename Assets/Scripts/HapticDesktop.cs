using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticDesktop : MonoBehaviour
{
    public TextAsset[] jsonDumps;

    public GameObject iconPrefab;

    public GameObject desktopBg;

    public int jsonDumpNumber;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}

[System.Serializable]
public class DesktopDump
{
    public string img_url;
    public Vector2 img_size;
    public List<IconData> element;

}

[System.Serializable]
public class IconData
{
    public string instruction;
    public float[] bbox;
}
