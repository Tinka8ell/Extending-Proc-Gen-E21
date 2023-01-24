using DevionGames.UIWidgets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoadMenu : UIWidget
{
    public static bool DebugLoadMenu = false;

    [Header("Reference")]
    [Tooltip("The parent transform of slots.")]
    [SerializeField]
    protected Transform slotParent;

    [Tooltip("The slot prefab. This game object should contain the LoadSlot component.")]
    [SerializeField]
    protected LoadSlot slotPrefab;

    [Tooltip("The select key. This key used to list items in PlayPrefs.")]
    [SerializeField]
    protected string selectKey;

    public UnityEvent<string> PlayerUpdated;
    public UnityEvent<string> WorldUpdated;

    private void Start()
    {
        DebugLoadMenuLog("UpdateStates() - Start");
        if (selectKey == null || selectKey.Length ==0){
            selectKey = "DefaultKey";
        }
        UpdateStates();
    }

    void OnEnable(){
        DebugLoadMenuLog("UpdateStates() - onEnable");
        UpdateStates();
    }

    private void UpdateStates()
    {
        DebugLoadMenuLog("UpdateStates() - clearing old slots");
        List<LoadSlot> slots = 
            this.slotParent.GetComponentsInChildren<LoadSlot>().ToList();
        slots.Remove(this.slotPrefab);
        for (int i = 0; i < slots.Count; i++)
        {
            DestroyImmediate(slots[i].gameObject);
        }

        string parent = Repository.GameKey;
        if (selectKey.Equals("WorldKeys")){
            parent = Repository.WorldKey;
        } else if (selectKey.Equals("PlayerKeys")){
            parent = Repository.PlayerKey;
        }
        DebugLoadMenuLog("UpdateStates() - getting keys for " + parent);
        List<string> keys = Repository.ListKeys(parent);
        DebugLoadMenuLog("UpdateStates() - got " + keys.Count + " keys");

        if (keys.Count == 0){
            // temporarily add some defaults for testing
            Debug.LogWarning("UpdateStates() - got " + keys.Count + " keys, so using defaults");
            if (selectKey.Equals("WorldKeys")){
                keys.Add("Default");
                keys.Add("Small Islands");
                keys.Add("Big Islands");
            } else if (selectKey.Equals("PlayerKeys")){
                keys.Add("Simple Simon");
                keys.Add("Plain Jane");
            }
        }

        for (int i = keys.Count - 1; i >= 0; i--)
        {
            string key = keys[i];
            SelectSlot slot = CreateSlot(key);
        }
    }

    public SelectSlot CreateSlot(string name)
    {
        DebugLoadMenuLog("CreateSlot(" + name + "): " + (this.slotPrefab != null) + ", " + (this.slotParent != null));
        if (this.slotPrefab != null && this.slotParent != null)
        {
            GameObject go = (GameObject)Instantiate(this.slotPrefab.gameObject);
            Text text = go.GetComponentInChildren<Text>();
            DebugLoadMenuLog("CreateSlot(" + name + "): got text: " + text.text);
            text.text = name;
            go.SetActive(true);
            go.transform.SetParent(this.slotParent, false);
            return go.GetComponent<SelectSlot>();
        }
        return null;
    }

    private static void DebugLoadMenuLog(string message){
        if (DebugLoadMenu)
            Debug.Log(message);
    }

}

