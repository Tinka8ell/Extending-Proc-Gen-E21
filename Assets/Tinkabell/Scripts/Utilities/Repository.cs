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
	public static string GameState = CombineKeys("GameState");
	public static string WorldKey = CombineKeys("World");

    private static string CombineKeys(string key, string parent = null){
        if (parent == null){
            parent = GameKey;
        }
        return parent + "." + key;
    }

    public static string GetJson(string key, string parent){
        return GetJson(CombineKeys(key, parent));
    }

    public static void SetJson(string key, string parent, string json){
        SetJson(CombineKeys(key, parent), json);
    }

    public static void Remove(string key, string parent){
        Remove(CombineKeys(key, parent));
    }

    public static string GetJson(string key){
        return PlayerPrefs.GetString(key);
    }

    public static void SetJson(string key, string json){
        PlayerPrefs.SetString(key, json);
    }

    public static void Remove(string key){
        PlayerPrefs.DeleteKey(key);
    }

    public static void SaveString(string key, string parent, string serialized){
        SaveString(CombineKeys(key, parent), serialized);
    }

    public static void SaveString(string key, string serialized){
		Debug.Log("Saving " + key + ": " + serialized);

        // save it to our PlayerPrefs
		SetJson(key, serialized);
    }

    public static void Save(string key, string parent, object serializable){
        Save(CombineKeys(key, parent), serializable);
    }

    public static void Save(string key, object serializable){
        string json = JsonUtility.ToJson(serializable, true);
		Debug.Log("Saving " + key + ": " + json);

        // save it to our PlayerPrefs
		SetJson(key, json);
    }

    public static string LoadString(string key, string parent, string backup = ""){
        string newKey = CombineKeys(key, parent);
        Debug.Log("Loading from key " + newKey);
        return LoadString(newKey, backup);
    }

    public static string LoadString(string key, string backup = ""){
		string value = GetJson(key);
        if(value == null || value.Length == 0){
            Debug.Log("Can't find the " + key + "!");
            value = backup;
            if (backup != null){
                Debug.Log("Using backup value for " + key + "!");
                SaveString(key, value); // ensure there is one next time!
            }
		}
        else {
    		Debug.Log("Retrieved " + key + ": " + value);
        }
        return value;
    }

    public static T Load<T>(string key, string parent, object backup = null){
        string newKey = CombineKeys(key, parent);
        Debug.Log("Loading from key " + newKey);
        return Load<T>(newKey, backup);
    }

    public static T Load<T>(string key, object backup = null){
        T value;
		string json = GetJson(key);
        if(json == null || json.Length == 0){
            Debug.Log("Can't find the " + key + "!");
            value = (T) backup;
            if (backup != null){
                Debug.Log("Using backup value for " + key + "!");
                Save(key, value); // ensure there is one next time!
            }
		}
        else {
    		Debug.Log("Retrieved " + key + ": " + json);
            value = JsonUtility.FromJson<T>(json);
        }
        return value;
    }
}
