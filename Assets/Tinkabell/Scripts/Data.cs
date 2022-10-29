using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * This is a place for the saving and laoding data.
 * For now these will be stored in PlayDefs, but later other locations.
 * Structure:
 * GameKeys
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
public class Data: MonoBehaviour
{
    public static string GameKey = "GameKeys";

        // Basic Data is a list of key words
    public List<string> KeyWords;

    public Data(string json){
        // to be completed
    }


    public static Data GetGameKeys(){
        return GetDataKeys(GameKey);
    }

    public static Data GetDataKeys(string key){
        string keys = PlayerPrefs.GetString(key);
        // keys.RemoveAll(x => string.IsNullOrEmpty(x));
        return new Data(keys);
    }

    public static void SetGameKeys(List<string> list){
        SetDataKeys(GameKey, list);
    }

    public static void SetDataKeys(string key, List<string> list){
        if (!list.Contains(key)) {
            list.Add(key);
            PlayerPrefs.SetString(key, string.Join(";", list));
        }
    }

    public static void SetDataKeys(string parent, string key){
        List<string> list = GetDataKeys(parent).KeyWords;
        if (!list.Contains(key)) {
            list.Add(key);
            PlayerPrefs.SetString(key, string.Join(";", list));
        }
    }

    public static void UpdateKeys(string key){
        UpdateKeys(GameKey, key);
    }

    public static void UpdateKeys(string parent, string key){
        var pos = key.IndexOf("-");
        string rest = "";
        if (pos > 1){
            rest = key.Substring(pos + 1);
            key = key.Substring(0, pos);
        }
        SetDataKeys(parent, key);
        if (rest != ""){
            UpdateKeys(parent + "-" + key, rest);
        }
    }

    public static string NameToKey(string name){
        return name.Replace(" ", "+");
    }

    public static string KeyToName(string key){
        return key.Replace("+", " ");
    }

    public static string GetData(string key){
        return PlayerPrefs.GetString(key);
    }

    public static string GetData(string parent, string key){
        return PlayerPrefs.GetString(key);
    }

}
