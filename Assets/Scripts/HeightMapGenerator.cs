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
				values [i, j] *= heightCurve_threadsafe.Evaluate (values [i, j]) * settings.heightMultiplier;

				if (values [i, j] > maxValue) {
					maxValue = values [i, j];
				}
				if (values [i, j] < minValue) {
					minValue = values [i, j];
				}
			}
		}

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

