using DevionGames.UIWidgets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SelectMenu : UIWidget
{
    /// <summary>
    /// The parent transform of slots. 
    /// </summary>
    [Header("Reference")]
    [Tooltip("The parent transform of slots.")]
    [SerializeField]
    protected Transform slotParent;

    /// <summary>
    /// The slot prefab. This game object should contain the Slot component or a child class of Slot. 
    /// </summary>
    [Tooltip("The slot prefab. This game object should contain the SelectSlot component.")]
    [SerializeField]
    protected SelectSlot slotPrefab;

    /// <summary>
    /// The key for these PlayerPrefs keys. 
    /// </summary>
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
        List<SelectSlot> slots = 
            this.slotParent.GetComponentsInChildren<SelectSlot>().ToList();
        slots.Remove(this.slotPrefab);
        for (int i = 0; i < slots.Count; i++)
        {
            DestroyImmediate(slots[i].gameObject);
        }

        List<string> keys = PlayerPrefs.GetString(selectKey).Split(';').ToList();
        keys.RemoveAll(x => string.IsNullOrEmpty(x));
        keys.Reverse();

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
        if (this.slotPrefab != null && this.slotParent != null)
        {
            GameObject go = (GameObject)Instantiate(this.slotPrefab.gameObject);
            Text text = go.GetComponentInChildren<Text>();
            text.text = name;
            go.SetActive(true);
            go.transform.SetParent(this.slotParent, false);
            return go.GetComponent<SelectSlot>();
        }
        return null;
    }
}

