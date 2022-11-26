using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * This is a place for the saving and laoding data.
 * For now these will be stored in PlayDefs, but later other locations.
 * Structure:
 * Levelling
 *    World
 *       World-Name+Of+World - for each named world ('+' for ' ')
 *    Player
 *       Player-Name+Of+Player - for each named world ('+' for ' ')
 *    Game
 *       Game-World-Player-Instance - defines which, who and allows for parallel games
 *          - used as "name" for Inventory / Game saves
 *    Chunk
 *       Chunk-Game-X+Y-Time - defines the state of that cunk of that game (world/player/instance) at that time
 */
[System.Serializable]
public class Repository 
{
    public static string GameKey = "TheLevelling";

    private string CombineKeys(string parent, string key){
        return parent + "." + key;
    }

    public string GetJson(string parent, string key){
        return GetJson(CombineKeys(parent, key));
    }

    public void SetJson(string parent, string key, string json){
        SetJson(CombineKeys(parent, key), json);
    }

    public void Remove(string parent, string key){
        Remove(CombineKeys(parent, key));
    }

    public string GetJson(string key){
        return PlayerPrefs.GetString(key);
    }

    public void SetJson(string key, string json){
        PlayerPrefs.SetString(key, json);
    }

    public void Remove(string key){
        PlayerPrefs.DeleteKey(key);
    }

}
