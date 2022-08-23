using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {

	
	public static HeightMap GenerateHeightMap(
			int size, HeightMapSettings settings, Vector2 sampleCentre) {
		HeightMap heightMap = new HeightMap(new float[size, size], 0, 0);
		for(int index = 0; index < settings.weightedNoiseSettings.Length; index++){
			if (settings.weightedNoiseSettings[index].noiseSettings != null){
				if (settings.weightedNoiseSettings[index].heightMultiplier > 0){
					HeightMap partialHeightMap = GeneratePartialHeightMap(size, settings, sampleCentre, index);

					for (int i = 0; i < size; i++) {
						for (int j = 0; j < size; j++) {
							heightMap.values [i, j] += partialHeightMap.values [i, j];
						}
					}
					heightMap.minValue += partialHeightMap.minValue;
					heightMap.maxValue += partialHeightMap.maxValue;
				}
			}
		}

        // Debug.LogFormat("GenerateHeightMap: Min = {0}, Max = {1}", heightMap.minValue, heightMap.maxValue);
		return heightMap;
	}

	public static HeightMap GeneratePartialHeightMap(
			int size, HeightMapSettings settings, Vector2 sampleCentre, int index=0) {
		float[,] values = Noise.GenerateNoiseMap (size, settings.weightedNoiseSettings[index].noiseSettings, sampleCentre);

		AnimationCurve heightCurve_threadsafe = new AnimationCurve (settings.heightCurve.keys);

		float minValue = float.MaxValue;
		float maxValue = float.MinValue;

		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				float value = values [i, j];
				if (value < 0) {
					// for sea apply curve over twice depth
					// move -2 to 0 range to 0 to 1
					value = 1f + value / 2f;
					// apply heightCurve
					value = heightCurve_threadsafe.Evaluate(value);
					// move back to -2 to 0 range
					value = (value - 1f) * 2f;
				} else {
					// for land apply curve over full height
					// apply heightCurve
					value = heightCurve_threadsafe.Evaluate(value);
				}
				// limit sea depth to -1
				value = Mathf.Clamp(value, -1f, 2f); // should be no where near 2, but might be a smidge over 1
				// apply heightMultiplier
				value *= settings.weightedNoiseSettings[index].heightMultiplier;

				if (value > maxValue) {
					maxValue = value;
				}
				if (value < minValue) {
					minValue = value;
				}
				values [i, j] = value;
			}
		}

        // Debug.LogFormat("GeneratePartialHeightMap: Actual: Min = {0}, Max = {1}", minValue, maxValue);

		maxValue = settings.weightedNoiseSettings[index].heightMultiplier;
		minValue = - maxValue;

		return new HeightMap (values, minValue, maxValue);
	}

}

public struct HeightMap {
	public float[,] values;
	public float minValue;
	public float maxValue;

	public HeightMap (float[,] values, float minValue, float maxValue)
	{
		this.values = values;
		this.minValue = minValue;
		this.maxValue = maxValue;
	}
}

