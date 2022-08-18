using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {

	
	public static HeightMap GenerateCombinedHeightMap(
			int size, HeightMapSettings islandSettings, HeightMapSettings terrainSettings, float ratio, Vector2 sampleCentre) {
		HeightMap islandHeightMap = GenerateHeightMap(size, islandSettings, sampleCentre);
		HeightMap terrainHeightMap = GenerateHeightMap(size, terrainSettings, sampleCentre);
		float terrainRatio = 1 - ratio;
		float[,] values = new float[size, size];
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				values [i, j] = islandHeightMap.values [i, j] * ratio + terrainHeightMap.values [i, j] * terrainRatio;
			}
		}
		float minValue = islandHeightMap.minValue * ratio + terrainHeightMap.minValue * terrainRatio;
		float maxValue = islandHeightMap.maxValue * ratio + terrainHeightMap.maxValue * terrainRatio;
        Debug.LogFormat("GenerateCombinedHeightMap: Min = {0}, Max = {1}", minValue, maxValue);
		return new HeightMap (values, minValue, maxValue);
	}

	public static HeightMap GenerateHeightMap(
			int size, HeightMapSettings settings, Vector2 sampleCentre) {
		float[,] values = Noise.GenerateNoiseMap (size, settings.noiseSettings, sampleCentre);

		AnimationCurve heightCurve_threadsafe = new AnimationCurve (settings.heightCurve.keys);

		float minValue = float.MaxValue;
		float maxValue = float.MinValue;

		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				// never understood this bit!
				//   values [i, j] *= heightCurve_threadsafe.Evaluate (values [i, j]) * settings.heightMultiplier;
				// for now take out the heightCurve ...
				//   values [i, j] *= settings.heightMultiplier;
				float value = values [i, j];
				// move -1 to 1 range to 0 to 1
				value = (value + 1f) / 2f;
				// apply heightCurve
				value = heightCurve_threadsafe.Evaluate(value);
				// move back to -1 to 1 range
				value = value * 2f - 1f;
				// limit sea depth to -1
				value = Mathf.Clamp(value, -1f, 2f); // should be no where near 2, but might be a smidge over 1
				// apply heightMultiplier
				value *= settings.heightMultiplier;

				if (value > maxValue) {
					maxValue = value;
				}
				if (value < minValue) {
					minValue = value;
				}
				values [i, j] = value;
			}
		}

        Debug.LogFormat("GenerateHeightMap: Actual: Min = {0}, Max = {1}", minValue, maxValue);

		maxValue = settings.heightMultiplier;
		minValue = - maxValue;

		return new HeightMap (values, minValue, maxValue);
	}

}

public struct HeightMap {
	public readonly float[,] values;
	public readonly float minValue;
	public readonly float maxValue;

	public HeightMap (float[,] values, float minValue, float maxValue)
	{
		this.values = values;
		this.minValue = minValue;
		this.maxValue = maxValue;
	}
}

