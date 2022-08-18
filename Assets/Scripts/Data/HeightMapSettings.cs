using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {

	public NoiseSettings noiseSettings;
	public float heightMultiplier;

	public WeightedNoiseSettings[] weightedNoiseSettings;

	public AnimationCurve heightCurve;

	public float minHeight {
		get {
			return heightMultiplier * 0f;
		}
	}

	public float maxHeight {
		get {
			return heightMultiplier * 1f;
		}
	}

	#if UNITY_EDITOR

	protected override void OnValidate() {
		noiseSettings.ValidateValues ();
		base.OnValidate ();
	}
	#endif

}

[System.Serializable]
public class WeightedNoiseSettings {
	public NoiseSettings noiseSettings;
	public float heightMultiplier;

}

