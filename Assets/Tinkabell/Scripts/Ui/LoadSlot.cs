using UnityEngine;
using UnityEngine.EventSystems;
using DevionGames.UIWidgets;
using DevionGames.InventorySystem;
using ContextMenu = DevionGames.UIWidgets.ContextMenu;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class LoadSlot : MonoBehaviour, IPointerUpHandler
{
    public GameObject parent;
    private CharacterCreator characterCreator;
    private TerrainGenerator terrainGenerator;
    public UITree parentInTree;

    void Start(){
        characterCreator = parent.GetComponent<CharacterCreator>();
        terrainGenerator = parent.GetComponent<TerrainGenerator>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        string key = GetComponentInChildren<Text>().text;
        DialogBox dialogBox = InventoryManager.UI.dialogBox;
        dialogBox.Show("Load", "Are you sure you want to load " + key + "? ", null, (int result) => { 
            if (result != 0) return;
            if (characterCreator != null){
                characterCreator.LoadRecipe(key);
                parentInTree.enable();
            } else if (terrainGenerator != null){
                // TODO: what do we do with a new "world"
            }
        }, "Yes", "No");
    }
}