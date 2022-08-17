using UnityEngine;
using System.Collections;

public static class Noise {

	public enum NormalizeMode {Local, Global};

	public static float[,] GenerateNoiseMap(
			int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre, 
			bool deepenSea, float deepenRatio) {
		float[,] noiseMap = new float[mapWidth,mapHeight];

		System.Random prng = new System.Random (settings.seed);
		Vector2[] octaveOffsets = new Vector2[settings.octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < settings.octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + settings.offset.x + sampleCentre.x;
			float offsetY = prng.Next (-100000, 100000) - settings.offset.y - sampleCentre.y;
			octaveOffsets [i] = new Vector2 (offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= settings.persistance;
		}

		float seaLevel = 0.5f;

/*         Debug.LogFormat(
			"GenerateNoiseMap: Octaves = {0}, Max Possible = {1}, Sea Level = {2}", 
			settings.octaves, maxPossibleHeight, seaLevel);
 */
		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;


		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < settings.octaves; i++) {
					float sampleX = (x-halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
					float sampleY = (y-halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= settings.persistance;
					frequency *= settings.lacunarity;
				}

				if (noiseHeight > maxLocalNoiseHeight) {
					maxLocalNoiseHeight = noiseHeight;
				} 
				if (noiseHeight < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeight;
				}
				noiseMap [x, y] = noiseHeight;

/* 	        	Debug.LogFormat(
					"GenerateNoiseMap: Noise Map[{0}, {1}]: Noise Height = {2}", 
					x, y, noiseHeight);
 */
				if (settings.normalizeMode == NormalizeMode.Global) {
					// float normalizedHeight = (noiseMap [x, y] + 1) / (maxPossibleHeight / 0.9f);
					float normalizedHeight = 0.5f + noiseMap [x, y] / (maxPossibleHeight * 2f);
					normalizedHeight = Mathf.Clamp (normalizedHeight, 0, 1);
					if (deepenSea && (normalizedHeight < seaLevel)){
						// if deepenSea and below sea level multiply depth by deepenRatio and limit to bottom
						normalizedHeight = Mathf.Clamp ((normalizedHeight - seaLevel) * deepenRatio + seaLevel, 0, int.MaxValue);
					}
					noiseMap [x, y] = normalizedHeight;
/* 		        	Debug.LogFormat(
						"GenerateNoiseMap: Noise Map[{0}, {1}]: Normalized Height = {2}", 
						x, y, normalizedHeight);
 */				}
			}
		}

		if (settings.normalizeMode == NormalizeMode.Global) {
			maxLocalNoiseHeight = maxPossibleHeight;
			minLocalNoiseHeight = 0;
		}
		
		if (settings.normalizeMode == NormalizeMode.Local) {
			for (int y = 0; y < mapHeight; y++) {
				for (int x = 0; x < mapWidth; x++) {
					noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
				}
			}
		}

		return noiseMap;
	}

}

[System.Serializable]
public class NoiseSettings {
	public Noise.NormalizeMode normalizeMode;

	public float scale = 50;

	public int octaves = 6;
	[Range(0,1)]
	public float persistance =.6f;
	public float lacunarity = 2;

	public int seed;
	public Vector2 offset;

	public void ValidateValues() {
		scale = Mathf.Max (scale, 0.01f);
		octaves = Mathf.Max (octaves, 1);
		lacunarity = Mathf.Max (lacunarity, 1);
		persistance = Mathf.Clamp01 (persistance);
	}
}