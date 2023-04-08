using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {

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
		/*Debug.Log(
			"Load HeightMapSettingsData from: parent: " + Repository.WorldKey +
			", World Name: " + WorldName);*/
        HeightMapSettingsSaveData data = Repository.Load<HeightMapSettingsSaveData>(
			Repository.WorldKey, 
			WorldName, 
			new HeightMapSettingsSaveData()
			);
		if (data.weightedNoiseSettings == null || data.weightedNoiseSettings.Length == 0){
            Debug.LogWarning("Can't find the world: " + Repository.WorldKey + "." + WorldName);
			return;
		}
		int length = data.weightedNoiseSettings.Length;
		//Debug.Log("And it contains " + length + " WeightedNoiseSettings");
		weightedNoiseSettings =  new WeightedNoiseSettings[length];
		System.Array.Copy(data.weightedNoiseSettings, weightedNoiseSettings, length);
		heightCurve = data.heightCurve;
		//Debug.Log("And HeightMapSettings have height: " + height);
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

		data.heightCurve = heightCurve;
		Repository.Save(Repository.WorldKey, WorldName, data);
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
	public AnimationCurve heightCurve;
	//public KeyFrames keys;
}

