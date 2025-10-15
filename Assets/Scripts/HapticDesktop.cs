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

        Renderer desktopRenderer = desktopBg.GetComponent<Renderer>();
        if (desktopRenderer == null)
        {
            Debug.LogError("desktopBg needs a Renderer (e.g. MeshRenderer) to compute size.");
            return;
        }

        //// Get world size of the desktop surface
        //Vector3 worldSize = desktopRenderer.bounds.size;
        

        Vector3 worldSize = new Vector3(25.6f, 10f, 16f);
        Transform dt = desktopBg.transform;

        foreach (var desktop in desktopDumps)
        {
            if (desktop.element == null) continue;

            float imgW = (desktop.img_size != null && desktop.img_size.Count >= 2) ? (float)desktop.img_size[0] : 1f;
            float imgH = (desktop.img_size != null && desktop.img_size.Count >= 2) ? (float)desktop.img_size[1] : 1f;

            foreach (var icon in desktop.element)
            {
                // --- Use "point" instead of bbox ---
                if (icon.point == null || icon.point.Count < 2)
                {
                    Debug.LogWarning("Skipping icon with missing point: " + icon.instruction);
                    continue;
                }

                // Extract normalized coordinates
                float normalizedX = icon.point[0];
                float normalizedY = icon.point[1];

                // Determine if point is normalized (0..1) or pixel coordinates
                bool isNormalized = (normalizedX <= 1f && normalizedY <= 1f);
                if (!isNormalized)
                {
                    normalizedX /= imgW;
                    normalizedY /= imgH;
                }

                normalizedX = Mathf.Clamp01(normalizedX);
                normalizedY = Mathf.Clamp01(normalizedY);

                // Convert normalized [0,1] coords to Unity world offsets
                float offsetX_world = (normalizedX - 0.5f) * worldSize.x;
                float offsetY_world = (0.5f - normalizedY) * worldSize.y;

                // Convert to local position relative to the desktop background
                Vector3 worldPos = dt.position + dt.right * offsetX_world + dt.up * offsetY_world;
                Vector3 localPos = dt.InverseTransformPoint(worldPos);

                // Instantiate and position the icon
                GameObject newIcon = Instantiate(iconPrefab, desktopBg.transform);
                newIcon.name = icon.instruction;
                newIcon.transform.localPosition = new Vector3(localPos.x, 0.05f, localPos.y);

                // Optional: scale icons consistently small so they don’t overlap
                //newIcon.transform.localScale = Vector3.one * 0.05f;

                Debug.Log($"Instantiated '{icon.instruction}' at {localPos} (norm: {normalizedX:F2}, {normalizedY:F2})");
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
    public List<float> point;
}
