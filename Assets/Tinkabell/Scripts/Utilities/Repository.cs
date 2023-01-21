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
    public static bool DebugRepository = true;
    
    public static string GameKey = "TheLevelling";
	public static string GameState = CombineKeys("GameState");
	public static string WorldKey = CombineKeys("World");
	public static string PlayerKey = CombineKeys("Player");

    // Load and Save String Data - e.g. character data

    public static List<string> ListKeys(string parent){
        return ListSubKeysOfParent(parent);
    }

    public static void SaveString(string parent, string key, string serialized){
        SaveString(CombineKeys(key, parent), serialized);
    }

    private static void SaveString(string key, string serialized){
		DebugRepositoryLog("Saving " + key + ": " + serialized);
		SetJson(key, serialized);
    }

    public static string LoadString(string parent, string key, string backup = ""){
        string newKey = CombineKeys(key, parent);
        DebugRepositoryLog("Loading from key " + newKey);
        return LoadString(newKey, backup);
    }

    private static string LoadString(string key, string backup = ""){
		string value = GetJson(key);
        if(value == null || value.Length == 0){
            DebugRepositoryLog("Can't find the " + key + "!");
            value = backup;
            if (backup != null){
                DebugRepositoryLog("Using backup value for " + key + "!");
                SaveString(key, value); // ensure there is one next time!
            }
		}
        else {
    		DebugRepositoryLog("Retrieved " + key + ": " + value);
        }
        return value;
    }

    // Save and Load data - e.g. HeightMaps, GameData
    // Either keyed - parent.key
    // Or singleton - parent (as the "key")

    public static void Save(string parent, string key, object serializable){
        Save(CombineKeys(key, parent), serializable);
    }

    public static void Save(string key, object serializable){
        string json = JsonUtility.ToJson(serializable, true);
		DebugRepositoryLog("Saving " + key + ": " + json);
		SetJson(key, json);
    }

    public static T Load<T>(string key, string parent, object backup = null){
        string newKey = CombineKeys(key, parent);
        DebugRepositoryLog("Loading from key " + newKey);
        return Load<T>(newKey, backup);
    }

    public static T Load<T>(string key, object backup = null){
        T value;
		string json = GetJson(key);
        if(json == null || json.Length == 0){
            DebugRepositoryLog("Can't find the " + key + "!");
            value = (T) backup;
            if (backup != null){
                DebugRepositoryLog("Using backup value for " + key + "!");
                Save(key, value); // ensure there is one next time!
            }
		}
        else {
    		DebugRepositoryLog("Retrieved " + key + ": " + json);
            value = JsonUtility.FromJson<T>(json);
        }
        return value;
    }

    // keyed data?

    public static string GetJson(string key){
        return GetStringFromPlayerPrefs(key);
    }

    public static void SetJson(string key, string json){
        AddNewKey(key);
        SetStringFromPlayerPrefs(key, json);
    }

    public static void RemoveData(string key, string parent){
        RemoveData(CombineKeys(key, parent));
    }

    public static void RemoveData(string key){
        DeleteStringFromPlayerDefs(key);
        RemoveSubKey(key);
    }

    // utilities

    private static void AddNewKey(string key){
        string parent = GetParent(key);
        if (parent.Length > 0){
            string oldKey = key.Substring(parent.Length + 1);
            List<string> list = ListSubKeysOfParent(parent);
            if (!list.Contains(oldKey)){
                list.Add(oldKey);
                SetSubKeyList(parent, list);
            }
            AddNewKey(parent);
        }
        return;
    }

    private static void RemoveSubKey(string key){
        string parent = GetParent(key);
        if (parent.Length > 0){
            string subKey = key.Substring(parent.Length + 1);
            List<string> list = ListSubKeysOfParent(parent);
            if (list.Contains(subKey)){
                list.Remove(subKey);
                SetSubKeyList(parent, list);
            }
            if (list.Count == 0){
                RemoveKeyFromParent(parent);
            }
        }
        return;
    }

    private static string CombineKeys(string key, string parent = null){
        if (parent == null){
            parent = GameKey;
        }
        return parent + "." + key;
    }

    private static string GetParent(string key){
        int lastDot = key.LastIndexOf('.'); 
        if (lastDot < 0){
            return ""; // no parent
        }
        return key.Substring(0, lastDot);
    }

    private static void RemoveKeyFromParent(string key){
        DeleteStringFromPlayerDefs(key);
        RemoveSubKey(key);
    }

    private static void SetSubKeyList(string parent, List<string> list){
        string keys = string.Join(';', list);
        SetStringFromPlayerPrefs(parent, keys);
        return;
    }

    private static List<string> ListSubKeysOfParent(string parent){
        string keys = GetStringFromPlayerPrefs(parent);
        DebugRepositoryLog("ListSubKeysFromParent(" + parent + "): " + keys);
        List<string> list = new List<string>(keys.Split(';'));
        list.RemoveAll((string item) => item.Length == 0);
        foreach (string item in list){
            DebugRepositoryLog("Item found: '" + item + "'");
        }
        return list;
    }

    private static string GetStringFromPlayerPrefs(string key){
        string value = PlayerPrefs.GetString(key);
        DebugRepositoryLog("GetStringFromPlayerPrefs(" + key + "): '" + value + "'");
        return value;
    }

    private static void SetStringFromPlayerPrefs(string key, string value){
        DebugRepositoryLog("SetStringFromPlayerPrefs(" + key + ", '" + value + "')");
        PlayerPrefs.SetString(key, value);
    }

    private static void DeleteStringFromPlayerDefs(string key){
        PlayerPrefs.DeleteKey(key);
    }

    ////////// dead code //////////
    private static string XGetJson(string key, string parent){
        return GetJson(CombineKeys(key, parent));
    }

    private static void XSetJson(string key, string parent, string json){
        SetJson(CombineKeys(key, parent), json);
    }

    private static void DebugRepositoryLog(string message){
        if (DebugRepository)
            Debug.Log(message);
    }

}
