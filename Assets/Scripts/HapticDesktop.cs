using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public class HapticDesktop : MonoBehaviour
{
    //public TextAsset jsonDump; // changed to a single file for clarity
    public GameObject iconPrefab;
    public GameObject desktopBg;

    private Dictionary<string, GameObject> activeElements = new Dictionary<string, GameObject>();
    private HashSet<string> seenThisFrame = new HashSet<string>();

    private Dictionary<int, List<float>> skippedElements = new Dictionary<int, List<float>>(); //List of skipped elements displayed at the end
    private int i = 1; // Serial number to list out skipped elements
    private int iconCounter = 0;
    void Start()
    {
        //if (jsonDump == null)
        //{
        //    Debug.LogWarning("No JSON Dump assigned in the Inspector");
        //    return;
        //}

        // Deserialize the JSON


        StartCoroutine(RealtimeJSONReceiver());
        
    }

    public IEnumerator RealtimeJSONReceiver()
    {
        string jsonPath = "F:/Internship - KyleKeane/ui_realtime.json";
        float refreshRate = 1f;
        string lastJson = "";

        while(true)
        {
            if(System.IO.File.Exists(jsonPath))
            {
                string currentJson = System.IO.File.ReadAllText(jsonPath);

                if(currentJson != lastJson)
                {
                    lastJson = currentJson;

                    DesktopDump desktopDump = JsonConvert.DeserializeObject<DesktopDump>(currentJson);

                    //foreach(Transform child in desktopBg.transform)
                    //{
                    //    Destroy(child.gameObject);
                    //}
                    
                    GenerateDesktop(desktopDump);
                }

            }
            yield return new WaitForSeconds(refreshRate);   
        }


    }

    public void GenerateDesktop(DesktopDump desktop)
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

        //Vector3 worldSize = desktopRenderer.bounds.size;
        //Debug.Log("World Size: " + worldSize.x + ", " + worldSize.z);
        Vector3 worldSize = new Vector3 (25.6f, 10f, 14.40f);
        
        Transform dt = desktopBg.transform;

        if (desktop.elements == null)
        {
            Debug.LogWarning("No elements found in JSON.");
            return;
        }

        float imgW = (desktop.img_size != null && desktop.img_size.Count >= 2) ? (float)desktop.img_size[0] : 1f;
        float imgH = (desktop.img_size != null && desktop.img_size.Count >= 2) ? (float)desktop.img_size[1] : 1f;

        //float imgW = 2560;
        //float imgH = 1440;

        seenThisFrame.Clear();



        foreach (var icon in desktop.elements)
        {
            iconCounter++;
            if (icon.bbox == null || icon.bbox.Count < 4)
                continue;

            if(string.IsNullOrEmpty(icon.name))
            {
                skippedElements.Add(i, icon.bbox);
                i++;
                continue;
            }

            string elementName = icon.name;
            seenThisFrame.Add(elementName);

            // --- Compute point from bbox ---
            float bboxLeft = icon.bbox[0];
            float bboxTop = icon.bbox[1];
            float bboxWidth = icon.bbox[2];
            float bboxHeight = icon.bbox[3];

            

            float pointX = bboxLeft + bboxWidth / 2f;
            float pointY = bboxTop + bboxHeight / 2f;

            // Normalize point coordinates
            float normalizedX = pointX / imgW;
            float normalizedY = pointY / imgH;

            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);

            // Map normalized coords to world space on desktop
            float offsetX_world = (normalizedX - 0.5f) * worldSize.x;
            float offsetY_world = (0.5f - normalizedY) * worldSize.y;

            Vector3 worldPos = dt.position + dt.right * offsetX_world + dt.up * offsetY_world;
            Vector3 localPos = dt.InverseTransformPoint(worldPos);

            // Mapping LocalScale of icons to Unity World Scale
            float normalizedWidth = icon.bbox[2] / imgW;
            float normalizedHeight = icon.bbox[3] / imgH;

            float worldWidth = normalizedWidth * worldSize.x;
            float worldHeight = normalizedHeight * worldSize.y;


            float zBase = 0.05f;
            float zSpacing = 0.4f;
            float localY = zBase + zSpacing * icon.z_index;

            GameObject obj;
            if(!activeElements.TryGetValue(elementName, out obj))
            {
                obj = Instantiate(iconPrefab, desktopBg.transform);
                obj.name = icon.name + " " + icon.z_index ?? "UIElement";
                activeElements[elementName] = obj;
                Debug.Log($"Instantiated element : {elementName}");
            }

            
            //update position and scale
            obj.transform.localPosition = new Vector3(localPos.x, localY, localPos.y);
            obj.transform.localScale = new Vector3(worldWidth / dt.lossyScale.x, 0.3f, worldHeight / dt.lossyScale.z);

            // Instantiate icon prefab
            //GameObject newIcon = Instantiate(iconPrefab, desktopBg.transform);
            //newIcon.name = icon.name + " "+ icon.z_index ?? "UIElement";
            //newIcon.transform.localPosition = new Vector3(localPos.x, localY, localPos.y);

            //newIcon.transform.localScale = new Vector3(worldWidth / dt.lossyScale.x, 0.3f, worldHeight / dt.lossyScale.z);

            //Debug.Log($"Instantiated '{newIcon.name}' at {localPos} (norm: {normalizedX:F2}, {normalizedY:F2})");


        }

        var toRemove = activeElements.Keys.Except(seenThisFrame).ToList();
        foreach(var key in toRemove)
        {
            Destroy(activeElements[key]);
            activeElements.Remove(key);
            Debug.Log($"Removed element : {key}");

        }


        Debug.Log("Total Number of Icons : " + iconCounter);
        Debug.Log("Number of Skipped Elements : "+ skippedElements.Count);
        


    }
}

[System.Serializable]
public class DesktopDump
{
    public string timestamp;
    public List<int> img_size; // you can manually set this for now
    public List<ElementData> elements;
}

[System.Serializable]
public class ElementData
{
    public string name;
    public string control_type;
    public List<float> bbox;
    public int z_index;
}
