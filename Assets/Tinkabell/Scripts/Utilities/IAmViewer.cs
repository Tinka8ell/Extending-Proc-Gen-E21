using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAmViewer : MonoBehaviour
{
    private GameObject mapGenerator;
    private TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = GameObject.Find("Map Generator");
        terrainGenerator = mapGenerator.GetComponent(typeof(TerrainGenerator)) as TerrainGenerator;
        terrainGenerator.SetViewer(this.transform);
    }
}
