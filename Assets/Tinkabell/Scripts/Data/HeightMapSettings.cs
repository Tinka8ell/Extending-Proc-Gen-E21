using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {

	public static string WorldKey = "World";

	public string WorldName = "Default";

	public WeightedNoiseSettings[] weightedNoiseSettings;

	public AnimationCurve heightCurve;

	public float height {
		get {
			float total = 0f;
			if (weightedNoiseSettings != null){
				for (int index = 0; index < weightedNoiseSettings.Length; index ++){
					if (weightedNoiseSettings[index].noiseSettings != null){
						total += weightedNoiseSettings[index].heightMultiplier;
					}
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
		string json = Repository.GetJson(WorldKey, WorldName);
        if(json == null || json.Length == 0){
            Debug.Log("Can't find the world: " + WorldKey + "." + WorldName);
			return;
		}
		Debug.Log("Retrieved json: " + json);
        HeightMapSettingsSaveData data = JsonUtility.FromJson<HeightMapSettingsSaveData>(json);
		int length = data.weightedNoiseSettings.Length;
		Debug.Log("contains " + length + " WeightedNoiseSettings");
		weightedNoiseSettings =  new WeightedNoiseSettings[length];
		System.Array.Copy(data.weightedNoiseSettings, weightedNoiseSettings, length);
		if (data.keys == null || data.keys.keyFrames == null){ // AnimationCurve is missing
			Debug.Log("no AnimationCurve, so failing!");
			weightedNoiseSettings = null;
			Repository.Remove(WorldKey, WorldName); // clear out bad key
		} else { // get the AnimationCurve
			length = data.keys.keyFrames.Length;
			Debug.Log("contains " + length + " Keyframe keys");
			Keyframe[] keys = new Keyframe[length];
			for (int i = 0; i < length; i++){
				keys[i] = new Keyframe(
					data.keys.keyFrames[i].keys[0], 
					data.keys.keyFrames[i].keys[1], 
					data.keys.keyFrames[i].keys[2], 
					data.keys.keyFrames[i].keys[3], 
					data.keys.keyFrames[i].keys[4], 
					data.keys.keyFrames[i].keys[5]);
			}
			heightCurve = new AnimationCurve(keys);
		}
	}

	public void SaveAs(string name){
		WorldName = name;
		Save();
	}

	public void Save(){
        HeightMapSettingsSaveData data = new HeightMapSettingsSaveData();
		int length = weightedNoiseSettings.Length;
		data.weightedNoiseSettings =  new WeightedNoiseSettings[length];
		System.Array.Copy(weightedNoiseSettings, data.weightedNoiseSettings, length);

		Keyframe[] keys = heightCurve.keys;
		length = keys.Length;
		data.keys = new KeyFrames(length);
		for (int i = 0; i < length; i++){
			data.keys.keyFrames[i] = new Keys(new float[]{
			   keys[i].time,
			   keys[i].value,
			   keys[i].inTangent,
			   keys[i].outTangent,
			   keys[i].inWeight,
			   keys[i].outWeight
			});
		}

        // convert the save data object to a string
        string json = JsonUtility.ToJson(data, true);
		Debug.Log("Saving json: " + json);

        // save it to our PlayerPrefs
		Repository.SetJson(WorldKey, WorldName, json);
	}

	#if UNITY_EDITOR

	protected override void OnValidate() {
		if(weightedNoiseSettings == null){
			Debug.LogWarning("Validating HeightMapSettings, but weightedNoiseSettings is null");
		} else {
			for (int index = 0; index < weightedNoiseSettings.Length; index ++){
				if (weightedNoiseSettings[index].noiseSettings != null){
					weightedNoiseSettings[index].noiseSettings.ValidateValues ();
				}
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
public class Keys {
	public float[] keys;
	public Keys(int length){
		keys = new float[length];
	}
	public Keys(float[] keys){
		this.keys = keys;
	}
}

[System.Serializable]
public class KeyFrames {
	public Keys[] keyFrames;
	public KeyFrames(int length){
		keyFrames = new Keys[length];
	}
}

[System.Serializable]
public class HeightMapSettingsSaveData {
	public WeightedNoiseSettings[] weightedNoiseSettings;

	// Animation Curve
	public KeyFrames keys;
}

