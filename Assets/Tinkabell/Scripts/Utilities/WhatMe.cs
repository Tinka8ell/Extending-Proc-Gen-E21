using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhatMe : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       Debug.Log("What Me Start");
    }

    void OnEnable(){
       Debug.Log("What Me OnEnable");
    }

    void OnDisable(){
       Debug.Log("What Me OnDisable");
    }

}
