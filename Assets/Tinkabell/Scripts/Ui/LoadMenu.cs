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
        if (selectKey == null || selectKey.Length ==0){
            selectKey = "DefaultKey";
        }
        UpdateStates();
    }

    private void UpdateStates()
    {
        Debug.Log("UpdateStates() - clearing old slots");
        List<LoadSlot> slots = 
            this.slotParent.GetComponentsInChildren<LoadSlot>().ToList();
        slots.Remove(this.slotPrefab);
        for (int i = 0; i < slots.Count; i++)
        {
            DestroyImmediate(slots[i].gameObject);
        }

        Debug.Log("UpdateStates() - getting keys");
        List<string> keys = new List<string>();//Repository.ListKeys(Repository.GameKey, selectKey);
        // keys.Reverse();
        Debug.Log("UpdateStates() - got " + keys.Count + " keys");

        if (keys.Count == 0){
            // temporarily add some defaults for testing
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
        Debug.Log("CreateSlot(" + name + "): " + (this.slotPrefab != null) + ", " + (this.slotParent != null));
        if (this.slotPrefab != null && this.slotParent != null)
        {
            GameObject go = (GameObject)Instantiate(this.slotPrefab.gameObject);
            Text text = go.GetComponentInChildren<Text>();
            Debug.Log("CreateSlot(" + name + "): got text: " + text.text);
            text.text = name;
            go.SetActive(true);
            go.transform.SetParent(this.slotParent, false);
            return go.GetComponent<SelectSlot>();
        }
        return null;
    }
}

