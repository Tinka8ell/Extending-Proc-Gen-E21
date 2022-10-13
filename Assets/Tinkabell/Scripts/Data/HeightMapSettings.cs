using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {

	public static string WorldKey = ".World";

	public string WorldName = "Default";

	public WeightedNoiseSettings[] weightedNoiseSettings;

	public AnimationCurve heightCurve;

	public float height {
		get {
			float total = 0f;
			for (int index = 0; index < weightedNoiseSettings.Length; index ++){
				if (weightedNoiseSettings[index].noiseSettings != null){
					total += weightedNoiseSettings[index].heightMultiplier;
				}
			}
			return total;
		}
	}

	public float minHeight {
		get {
			return - height;
		}
	}

	public float maxHeight {
		get {
			return height;
		}
	}

	public void Load(string name){
		WorldName = name;
		Load();
	}

	public void Load(){
		string key = WorldName + WorldKey;
        if(!PlayerPrefs.HasKey(key)){
            Debug.LogError("Can't find the world: " + key);
			return;
		}
        HeightMapSettingsSaveData data = JsonUtility.FromJson<HeightMapSettingsSaveData>(PlayerPrefs.GetString(key));
		
		int length = data.weightedNoiseSettings.Length;
		weightedNoiseSettings =  new WeightedNoiseSettings[length];
		System.Array.Copy(data.weightedNoiseSettings, weightedNoiseSettings, length);

		length = data.keys.Length;
		Keyframe[] keys = new Keyframe[length];
		for (int i = 0; i < length; i++){
			keys[i] = new Keyframe(data.keys[i][0], data.keys[i][1], data.keys[i][2], data.keys[i][3], data.keys[i][4], data.keys[i][5]);
		}
		heightCurve = new AnimationCurve(keys);
	}

	public void SaveAs(string name){
		WorldName = name;
		Save();
	}

	public void Save(){
		string key = WorldName + WorldKey;
        HeightMapSettingsSaveData data = new HeightMapSettingsSaveData();
		int length = weightedNoiseSettings.Length;
		data.weightedNoiseSettings =  new WeightedNoiseSettings[length];
		System.Array.Copy(weightedNoiseSettings, data.weightedNoiseSettings, length);

		Keyframe[] keys = heightCurve.keys;
		length = keys.Length;
		for (int i = 0; i < length; i++){
			data.keys[i] = new float[]{keys[i].time, keys[i].value, keys[i].inTangent, keys[i].outTangent, keys[i].inWeight, keys[i].outWeight};
		}

        // convert the save data object to a string
        string rawData = JsonUtility.ToJson(data);

        // save it to our PlayerPrefs
        PlayerPrefs.SetString(key, rawData);
	}

	#if UNITY_EDITOR

	protected override void OnValidate() {
			for (int index = 0; index < weightedNoiseSettings.Length; index ++){
				if (weightedNoiseSettings[index].noiseSettings != null){
					weightedNoiseSettings[index].noiseSettings.ValidateValues ();
				}
			}
		base.OnValidate ();
	}
	#endif

}

[System.Serializable]
public class WeightedNoiseSettings {
	public NoiseSettings noiseSettings;
	public float heightMultiplier;

}

[System.Serializable]
public class HeightMapSettingsSaveData {
	public WeightedNoiseSettings[] weightedNoiseSettings;

	// Animation Curve
	public float[][] keys;
}

