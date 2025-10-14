using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;


public class HapticDesktop : MonoBehaviour
{
    public TextAsset[] jsonDumps;

    public GameObject iconPrefab;

    public GameObject desktopBg;

    public int jsonDumpNumber;

    void Start()
    {
        if(jsonDumps == null || jsonDumps.Length == 0)
        {
            Debug.LogWarning("No JSON Dumps assigned in the Inspector");


            return;
        }

        string jsonText = jsonDumps[0].text;

        List<DesktopDump> desktopDump = JsonConvert.DeserializeObject<List<DesktopDump>>(jsonText);
        
        GenerateDesktop(desktopDump);

    }


    //public void GenerateDesktop(List<DesktopDump> desktopDump)
    //{
    //    foreach (var desktop in desktopDump)
    //    {
    //        float planeWidth = desktopBg.transform.localScale.x * 10f;
    //        float planeHeight = desktopBg.transform.localScale.z * 10f;

    //        foreach (var icon in desktop.element)
    //        {
    //            GameObject newIcon = Instantiate(iconPrefab, desktopBg.transform);

    //            float normalizedX = icon.bbox[0] / desktop.img_size[0];
    //            float normalizedY = icon.bbox[1] / desktop.img_size[1];

    //            float worldX = (normalizedX - 0.5f) * planeWidth;
    //            float worldZ = (0.5f - normalizedY) * planeHeight;

    //            Vector3 position = new Vector3(worldX, worldZ, 0f);
    //            newIcon.transform.localPosition = position;

    //            Debug.Log("Instantiated icon : " + icon.instruction);
    //        }
    //    }
        
    //}



    public void GenerateDesktop(List<DesktopDump> desktopDumps)
{
    if (desktopBg == null)
    {
        Debug.LogError("desktopBg is not assigned!");
        return;
    }
    if (iconPrefab == null)
    {
        Debug.LogError("iconPrefab is not assigned!");
        return;
    }

    // Get Renderer to compute world size reliably
    Renderer desktopRenderer = desktopBg.GetComponent<Renderer>();
    if (desktopRenderer == null)
    {
        Debug.LogError("desktopBg needs a Renderer (e.g. MeshRenderer) to compute size.");
        return;
    }

    // world bounds of the desktop surface (accounts for scale and rotation)
    Vector3 worldSize = desktopRenderer.bounds.size;
    // We'll map normalized X -> desktop.right * worldSize.x
    // and normalized Y -> desktop.up * worldSize.y
    Transform dt = desktopBg.transform;

    foreach (var desktop in desktopDumps)
    {
        if (desktop.element == null) continue;

        // img_size in your JSON is like [2560,1600]; keep as floats
        float imgW = (desktop.img_size != null && desktop.img_size.Count >= 2) ? (float)desktop.img_size[0] : 1f;
        float imgH = (desktop.img_size != null && desktop.img_size.Count >= 2) ? (float)desktop.img_size[1] : 1f;

        foreach (var icon in desktop.element)
        {
            if (icon.bbox == null || icon.bbox.Count < 4)
            {
                Debug.LogWarning("Skipping icon with invalid bbox: " + icon.instruction);
                continue;
            }

            // bbox format in your dumps: [xmin, ymin, xmax, ymax]
            float xmin = icon.bbox[0];
            float ymin = icon.bbox[1];
            float xmax = icon.bbox[2];
            float ymax = icon.bbox[3];

            // Compute center of bbox (in source coordinate space)
            float centerX = (xmin + xmax) * 0.5f;
            float centerY = (ymin + ymax) * 0.5f;

            // Determine whether bbox is normalized (0..1) or pixel coordinates
            bool isNormalized = (xmin <= 1f && ymin <= 1f && xmax <= 1f && ymax <= 1f);

            float normalizedX = isNormalized ? centerX : (centerX / imgW);
            float normalizedY = isNormalized ? centerY : (centerY / imgH);

            // Clamp just in case
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);

            // Map normalized coords (0..1, top-left origin in JSON) to world offsets
            // JSON origin = top-left, so y must be inverted when mapping to up direction
            float offsetX_world = (normalizedX - 0.5f) * worldSize.x;
            float offsetY_world = (0.5f - normalizedY) * worldSize.y;

            // Build world position from desktop center
            Vector3 worldPos = dt.position + dt.right * offsetX_world + dt.up * offsetY_world;

            // Convert world position to desktop local position (so it's correct as a child)
            Vector3 localPos = dt.InverseTransformPoint(worldPos);

            // Instantiate and place
            GameObject newIcon = Instantiate(iconPrefab, desktopBg.transform);
            newIcon.transform.localPosition = localPos;

            // Optionally scale icon to bbox size (visual)
            float normalizedWidth = isNormalized ? (xmax - xmin) : ((xmax - xmin) / imgW);
            float normalizedHeight = isNormalized ? (ymax - ymin) : ((ymax - ymin) / imgH);
            float worldWidth = normalizedWidth * worldSize.x;
            float worldHeight = normalizedHeight * worldSize.y;

            // Set a reasonable thickness on the z axis; adjust factor as needed
            Vector3 iconLocalScale = newIcon.transform.localScale;
            // If prefab default scale is meaningful, you might want to multiply, else set size relative:
            // We'll set localScale so the icon visually matches bounding box: compute local scale using inverse of parent's lossyScale
            // Quick approach: directly set localScale to small scale proportional to world size
            newIcon.transform.localScale = new Vector3(worldWidth * (1f / dt.lossyScale.x), worldHeight * (1f / dt.lossyScale.y), iconLocalScale.z);

            Debug.Log($"Instantiated '{icon.instruction}' at local {localPos} (norm {normalizedX:F3},{normalizedY:F3})");
        }
    }
}
    void Update()
    {
        
    }
}

[System.Serializable]
public class DesktopDump
{
    public string img_url;
    public List<int> img_size;
    public List<IconData> element;

}


[System.Serializable]
public class IconData
{
    public string instruction;
    public List<float> bbox;
}
