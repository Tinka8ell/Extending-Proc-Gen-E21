using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;
using UnityEngine.UI;


public class CharacterCreator : MonoBehaviour
{
    public DynamicCharacterAvatar avatar;
    public Slider heightSlider;
    public Slider bellySlider;
    public ImageColorPicker skinTones;
    public ImageColorPicker hairColours;

    public List<string> hairTypes;
    private int hairType;

    private Dictionary<string, DnaSetter> dna;

    private static string MALE = "HumanMaleDCS";
    private static string FEMALE = "HumanFemaleDCS";
    private List<string> maleHairTypes = new List<string>();
    private List<string> femaleHairTypes = new List<string>();

    void Awake(){
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
        avatar.CharacterUpdated.AddListener(Updated);
        heightSlider.onValueChanged.AddListener(ChangeHeight);
        bellySlider.onValueChanged.AddListener(ChangeBelly);
        skinTones.OnColorPicked.AddListener(ChangeSkinColour);
        hairColours.OnColorPicked.AddListener(ChangeHairColour);
    }

    void OnDisable(){
        avatar.CharacterUpdated.RemoveListener(Updated);
        heightSlider.onValueChanged.RemoveListener(ChangeHeight);
        bellySlider.onValueChanged.RemoveListener(ChangeBelly);
        skinTones.OnColorPicked.RemoveListener(ChangeSkinColour);
        hairColours.OnColorPicked.RemoveListener(ChangeHairColour);
    }

    void Updated(UMAData dqata){
        dna = avatar.GetDNA();
        if (avatar.activeRace.name == MALE)
            hairTypes = maleHairTypes;
        else 
            hairTypes = femaleHairTypes;
        heightSlider.value = dna["height"].Get();
        bellySlider.value = dna["belly"].Get();
        string hairString = avatar.GetWardrobeItemName("Hair");
        hairType = hairTypes.FindIndex(s => s == hairString);
        hairType = Mathf.Clamp(hairType, 0, hairTypes.Count - 1);
    }

    public void SwitchGender(bool male){
        if (male && avatar.activeRace.name != MALE)
            avatar.ChangeRace(MALE);
        else if (!male && avatar.activeRace.name != FEMALE)
            avatar.ChangeRace(FEMALE);
    }

    public void ChangeHeight(float value){
        dna["height"].Set(value);
        avatar.BuildCharacter();
    }

    public void ChangeBelly(float value){
        dna["belly"].Set(value);
        avatar.BuildCharacter();
    }

    public void ChangeSkinColour(Color colour){
        avatar.SetColor("Skin", colour);
        avatar.UpdateColors(true);
    }

    public void ChangeHair(bool plus){
        if (plus)
            hairType ++;
        else 
            hairType --;
        hairType = Mathf.Clamp(hairType, 0, hairTypes.Count - 1);
        if (hairType == 0)
            avatar.ClearSlot("Hair");
        else
            avatar.SetSlot("Hair", hairTypes[hairType]);
        avatar.BuildCharacter();
    }

    public void ChangeHairColour(Color colour){
        avatar.SetColor("Hair", colour);
        avatar.UpdateColors(true);
    }

    public void SaveRecipe(){
        string recipe = avatar.GetCurrentRecipe();
        Repository.Save("Player", recipe);
    }

    public void LoadRecipe(){
        string recipe = Repository.Load<string>("Player", "{}");
        avatar.ClearSlots();
        avatar.LoadFromRecipeString(recipe);
    }

}
