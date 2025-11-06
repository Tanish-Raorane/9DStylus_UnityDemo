using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class constantUpdate : MonoBehaviour
{
    private GameObject cube;

    void Start()
    {
        cube = this.gameObject;
    }

    
    void Update()
    {
        cube.transform.position = new Vector3(3.95f, 10.7318f, -2.25f);
    }
}
