using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {

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

