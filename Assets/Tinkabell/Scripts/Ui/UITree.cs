using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITree : MonoBehaviour
{
    public static bool DebugUITree = false;
    
    public UITree parent;
    public UITree[] children;
    public bool disableWhenChildEnabled;

    public void enableChild(int index){
        DebugUITreeLog(name + " asked to enable child: " + index);
        for( int i = 0; i < children.Length; i++){
            if (children[i] != null){
                if (i == index) {
                    DebugUITreeLog(name + " enabling child: " + i + " ('" + children[i].name + "')");
                    children[i].enable();
                } else {
                    DebugUITreeLog(name + " disabling other child: " + i + " ('" + children[i].name + "')");
                    children[i].disable();
                }
            }
        }
        if (disableWhenChildEnabled){
            DebugUITreeLog(name + ": set to disable myself, but just making self inactive");
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }

    public void Alert(string message){
        Debug.LogWarning(name + ": " + message);
    }

    public void disable(){
        DebugUITreeLog(name + ": asked to be disabled");
        if (gameObject.activeSelf){
            gameObject.SetActive(false);
        }
        DebugUITreeLog(name + ": being disabled so disabling my children");
        disableChildren();
    }

    public void enable(){
        DebugUITreeLog(name + ": asked to be enabled");
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        DebugUITreeLog(name + ": being enabled so disabling my children");
        disableChildren();
    }

    public void disableChildren(){
        DebugUITreeLog(name + ": asked to disable " + children.Length + " children");
        for( int i = 0; i < children.Length; i++){
            if (children[i] != null){
                children[i].disable();
            }
        }
    }

    private static void DebugUITreeLog(string message){
        if (DebugUITree)
            Debug.Log(message);
    }

}
