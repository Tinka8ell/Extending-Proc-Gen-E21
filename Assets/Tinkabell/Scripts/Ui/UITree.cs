using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITree : MonoBehaviour
{
    // public string name = "Unset";
    public UITree parent;
    public UITree[] children;
    public bool disableWhenChildEnabled;
    
    public void enableChild(int index){
        Debug.Log(name + " asked to enable child: " + index);
        for( int i = 0; i < children.Length; i++){
            if (children[i] != null){
                if (i == index) {
                    Debug.Log(name + " enabling child: " + i + " ('" + children[i].name + "')");
                    children[i].enable();
                } else {
                    Debug.Log(name + " disabling other child: " + i + " ('" + children[i].name + "')");
                    children[i].disable();
                }
            }
        }
        if (disableWhenChildEnabled){
            Debug.Log(name + ": set to disable myself, but just making self inactive");
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }

    public void disable(){
        Debug.Log(name + ": asked to be disabled");
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        Debug.Log(name + ": being disabled so disabling my children");
        disableChildren();
    }

    public void enable(){
        Debug.Log(name + ": asked to be enabled");
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        Debug.Log(name + ": being enabled so disabling my children");
        disableChildren();
    }

    public void disableChildren(){
        Debug.Log(name + ": asked to disable " + children.Length + " children");
        for( int i = 0; i < children.Length; i++){
            if (children[i] != null){
                children[i].disable();
            }
        }
    }

}
