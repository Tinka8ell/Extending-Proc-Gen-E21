using UnityEngine;
using UnityEditor;
using System.IO;

public class ShowMe
{

    private static string dirAssets = "Assets";
    private static string subTinkabell = "Tinkabell";
    private static string dirTinkabell = dirAssets + "/" + subTinkabell;
    private static string subLogs = "Logs";
    private static string dirLogs = dirTinkabell + "/" + subLogs;
    private static string LogFile = dirLogs + "/List.txt";
    private static string subRepository = "UMARecipes";
    private static string dirRepository = dirTinkabell + "/" + subRepository;
    private static string subFemale = "female";
    private static string dirFemale = dirRepository + "/" + subFemale;
    private static string subMale = "male";
    private static string dirMale = dirRepository + "/" + subMale;

    [MenuItem("Tools/Tinkabell/Show Me")]
    public static void ShowMeScript()
    {
        CreateLogfile();
        UmaExample();
    }

    static void CreateLogfile()
    {
        if (!Directory.Exists(dirTinkabell))
        {
            AssetDatabase.CreateFolder(dirAssets, subTinkabell);
        }

        if (!Directory.Exists(dirLogs))
        {
            AssetDatabase.CreateFolder(dirTinkabell, subLogs);
        }

        if (!File.Exists(LogFile)){
            File.CreateText(LogFile);
        }

        if (!Directory.Exists(dirRepository))
        {
            AssetDatabase.CreateFolder(dirTinkabell, subRepository);
        }

        if (!Directory.Exists(dirFemale))
        {
            AssetDatabase.CreateFolder(dirRepository, subFemale);
        }

        if (!Directory.Exists(dirMale))
        {
            AssetDatabase.CreateFolder(dirRepository, subMale);
        }
    }

    static void UmaExample()
    {
        Log("path, type, name, display, slot, race");

        string[] guids;

        guids = AssetDatabase.FindAssets("t:UMAWardrobeRecipe");
        
        bool female = false;
        bool male = false;
        string assetPath;
        string assetName;
        string[] label = new string[1];
        foreach (string guid in guids)
        {
            string name = AssetDatabase.GUIDToAssetPath(guid);
            if (!name.StartsWith(dirRepository)){ // don't list those we have copied
                UMA.CharacterSystem.UMAWardrobeRecipe UMAWardrobeRecipe = AssetDatabase.LoadAssetAtPath<UMA.CharacterSystem.UMAWardrobeRecipe>(name);
                var type = UMAWardrobeRecipe.GetType();
                var races = UMAWardrobeRecipe.compatibleRaces;
                female = false;
                male = false;
                foreach(var race in races){
                    Log(name + ", " + type  + ", " + UMAWardrobeRecipe.name + ", " + UMAWardrobeRecipe.DisplayValue + ", " + UMAWardrobeRecipe.wardrobeSlot + ", " + race); 
                    // check if male or femail
                    female = female || race.StartsWith("HumanFemale");
                    male = male || race.StartsWith("HumanMale");
                }
                // move to local copies
                if (female){
                    assetName = AssetDatabase.LoadMainAssetAtPath(name).name;
                    assetPath = dirFemale + $"/{assetName}.asset";
                    Debug.Log(AssetDatabase.MoveAsset(name, assetPath));
                    label[0] = subFemale;
                    AssetDatabase.SetLabels(UMAWardrobeRecipe, label);
                }
                if (male){
                    assetName = AssetDatabase.LoadMainAssetAtPath(name).name;
                    assetPath = dirMale + $"/{assetName}.asset";
                    Debug.Log(AssetDatabase.MoveAsset(name, assetPath));
                    label[0] = subMale;
                    AssetDatabase.SetLabels(UMAWardrobeRecipe, label);
                }
            }
        }
    }

    static void Log(string message){
        Debug.Log(message);
        File.AppendAllText(LogFile, message + "\n");
    }
}
