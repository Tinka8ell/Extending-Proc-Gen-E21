using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class CharacterCreator : MonoBehaviour
{
    private DynamicCharacterAvatar avatar;
    public DynamicCharacterAvatar Avatar {
        get
        {
            if (avatar == null){
                avatar = Player.GetComponent<DynamicCharacterAvatar>();
                if (avatar == null){
                    Debug.LogError("Avata is not set able to be set from Player!");
                }
            }
            return avatar;
        }
    }
    private GameObject player;
    public GameObject Player {
        get
        {
            if (player == null){
                player = GameManager.Instance.player;
                if (player == null){
                    Debug.LogError("Player is not set in GameManager!");
                }
            }
            return player;
        }
    }
    public Slider heightSlider;
    public Slider bellySlider;
    public ImageColorPicker skinTones;
    public ImageColorPicker hairColours;
    public GameObject keyInputField;

    private List<string> hairTypes;
    private int hairType;

    private Dictionary<string, DnaSetter> dna;

    private static string MALE = "HumanMaleDCS";
    private static string FEMALE = "HumanFemaleDCS";
    private List<string> maleHairTypes = new List<string>();
    private List<string> femaleHairTypes = new List<string>();

    public UnityEvent<string> NameChangedEvent;


    void Awake(){
        Debug.Log("Avatar Awake");
        maleHairTypes.Add("None");
        maleHairTypes.Add("MaleHair1");
        maleHairTypes.Add("MaleHair2");
        maleHairTypes.Add("MaleHair3");
        maleHairTypes.Add("MaleHairSlick01_Recipe");
        femaleHairTypes.Add("None");
        femaleHairTypes.Add("FemaleHair1");
        femaleHairTypes.Add("FemaleHair2");
        femaleHairTypes.Add("FemaleHair3");
    }

    void OnEnable(){
        Debug.Log("CharacterCreator OnEnable");
        if (Avatar == null){
            Debug.LogWarning("Avatar was not set when CharacterCreator enabled");
        } else {
            Avatar.CharacterUpdated.AddListener(Updated);
        }
        heightSlider.onValueChanged.AddListener(ChangeHeight);
        // bellySlider.onValueChanged.AddListener(ChangeBelly);
        skinTones.OnColorPicked.AddListener(ChangeSkinColour);
        hairColours.OnColorPicked.AddListener(ChangeHairColour);
    }

    void OnDisable(){
        Debug.Log("CharacterCreator OnDisable");
        if (Avatar == null){
            Debug.LogWarning("Avatar was not set when CharacterCreator disabled");
        } else {
            Avatar.CharacterUpdated.RemoveListener(Updated);
        }
        heightSlider.onValueChanged.RemoveListener(ChangeHeight);
        // bellySlider.onValueChanged.RemoveListener(ChangeBelly);
        skinTones.OnColorPicked.RemoveListener(ChangeSkinColour);
        hairColours.OnColorPicked.RemoveListener(ChangeHairColour);
    }

    void Updated(UMAData dqata){
        dna = Avatar.GetDNA();
        if (Avatar.activeRace.name == MALE)
            hairTypes = maleHairTypes;
        else 
            hairTypes = femaleHairTypes;
        heightSlider.value = dna["height"].Get();
        bellySlider.value = dna["belly"].Get();
        string hairString = Avatar.GetWardrobeItemName("Hair");
        hairType = hairTypes.FindIndex(s => s == hairString);
        hairType = Mathf.Clamp(hairType, 0, hairTypes.Count - 1);
    }

    public void SwitchGender(bool male){
        if (male && Avatar.activeRace.name != MALE)
            Avatar.ChangeRace(MALE);
        else if (!male && Avatar.activeRace.name != FEMALE)
            Avatar.ChangeRace(FEMALE);
        Player.SetActive(true);
    }

    public void ChangeHeight(float value){
        dna["height"].Set(value);
        Avatar.BuildCharacter();
    }

    public void ChangeBelly(float value){
        dna["belly"].Set(value);
        Avatar.BuildCharacter();
    }

    public void ChangeSkinColour(Color colour){
        Debug.Log("Avatar ChangeSkinColour: " + colour);
        Avatar.SetColor("Skin", colour);
        Avatar.UpdateColors(true);
    }

    public void ChangeHair(bool plus){
        if (plus)
            hairType ++;
        else 
            hairType --;
        hairType = Mathf.Clamp(hairType, 0, hairTypes.Count - 1);
        if (hairType == 0)
            Avatar.ClearSlot("Hair");
        else
            Avatar.SetSlot("Hair", hairTypes[hairType]);
        Avatar.BuildCharacter();
    }

    public void ChangeHairColour(Color colour){
        Debug.Log("Avatar ChangeHairColour: " + colour);
        Avatar.SetColor("Hair", colour);
        Avatar.UpdateColors(true);
    }

    public void SaveRecipe(){
        string key = GameManager.Instance.PlayerName;
        Debug.Log("SaveRecipe() using: " + key);
        if (key.Length == 0){
            key = "Player";
            Debug.Log("SaveRecipe() overriding to: " + key);
        }
        SaveRecipe(key);
    }

    public void SaveInputNamedRecipe(){
        string key = keyInputField.GetComponent<TMP_InputField>().text;
        Debug.Log("SaveInputNamedRecipe() using: " + key);
        SaveRecipe(key);
    }

    public void SaveRecipe(string key){
        Debug.LogWarning("SaveRecipe(" + key + ") <<<<<<<=======");
        UpdatePlayerName(key);
        string recipe = Avatar.GetCurrentRecipe();
        Debug.Log("Saving receipe(" + key + "): " + recipe);
        Repository.SaveString(Repository.PlayerKey, key, recipe);
    }

    public void LoadRecipe(){
        string key = GameManager.Instance.PlayerName;
        Debug.Log("LoadRecipe() using: " + key);
        if (key.Length == 0){
            key = "Player!";
            Debug.Log("LoadRecipe() overriding to: " + key);
        }
        LoadRecipe(key);
    }

    public void LoadRecipe(string key){
        Debug.LogWarning("LoadRecipe(" + key + ") <<<<<<<=======");
        UpdatePlayerName(key);
        string recipe = Repository.LoadString(Repository.PlayerKey, key, "{}");
        Avatar.ClearSlots();
        Debug.Log("Loading receipe(" + key + "): " + recipe);
        Avatar.LoadFromRecipeString(recipe);
        Player.SetActive(true);
    }

    private void UpdatePlayerName(string name){
        Debug.Log("CharacterCreator: UpdatePlayerName(" + name + ")");
        GameManager.Instance.PlayerName=name;
        keyInputField.GetComponent<TMP_InputField>().SetTextWithoutNotify(GameManager.Instance.PlayerName);
    }



}
