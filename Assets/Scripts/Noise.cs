using UnityEngine;
using System.Collections;

public static class Noise {

	/**
	 * Generate a noise map from data passed.
	 * Noise maps a square, and nominally between -1 and 1.
	 * Practically perlin noise can generate values greater than 1 (by a bit).
	 * Sea Level will skew the data and return values that a much less then -1
	 * with the expectation that these will be truncated by height map generation.
	 */
	public static float[,] GenerateNoiseMap(
			int size, NoiseSettings settings, Vector2 sampleCentre, float zoom=1f) {
		float seaGradient = settings.seaGradient;
		float seaLevel = settings.seaLevel;
		// Debug.LogFormat("GenerateNoiseMap: Centre = {2}, Sea: Gradient = {0}, Level = {1}", seaGradient, seaLevel, settings.offset + sampleCentre);
		// Debug.LogFormat("GenerateNoiseMap: Scale = {0}, Octaves = {1}, Persistance = {2}, Lacunarity = {3}, Seed = {4}", settings.scale, settings.octaves, settings.persistance, settings.lacunarity, settings.seed);

		float[,] noiseMap = new float[size, size];

		// reset random number generation so get consistant random numbers
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

		float halfWidth = size / 2f;
		float halfHeight = size / 2f;

		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < settings.octaves; i++) {
					float sampleX = ((x - halfWidth) * zoom + octaveOffsets[i].x) / settings.scale * frequency;
					float sampleY = ((y - halfHeight) * zoom + octaveOffsets[i].y) / settings.scale * frequency;

					// PerlinNoise generates values from 0f to 1f (approximately!)
					// So must convert to range -1f to 1f
					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;  
					noiseHeight += perlinValue * amplitude;

					amplitude *= settings.persistance;  // slightly smaller each time
					frequency *= settings.lacunarity;   // at a higher frequency each time
				}

				noiseHeight /= maxPossibleHeight; // move from a range +/- noiseHeight to +/- 1

				float range = 1 - seaLevel;
				noiseHeight -= seaLevel; // move sea level to 0
				noiseHeight /= range; // move from a range +/- range to +/- 1 (ignore massively negative numbers out of range)

				if (noiseHeight < 0) {
					noiseHeight *= seaGradient; // enhance sea depth further if required
				}

				noiseMap [x, y] = noiseHeight;
			}
		}

		return noiseMap;
	}

}

[System.Serializable]
public class NoiseSettings {
	public float scale = 50;
	[Range(-1,1)]
	public float seaLevel = 0f; // 0 is default => don't move
	public float seaGradient = 1; // 1 is default => don't enhance

	public int octaves = 6;
	[Range(0,1)]
	public float persistance = 0.498f; // was 0.6f;
	public float lacunarity = 2;

	public int seed; // a way to fix generation of random noise repeatedly
	public Vector2 offset; // so can skew the origin of the perlin noise

	public void ValidateValues() {
		scale = Mathf.Max (scale, 0.01f); // protect from division by zero or a negative number!
		seaGradient = Mathf.Max (seaGradient, 1f); // protect from minimising sea gradient
		octaves = Mathf.Max (octaves, 1); // so we have at least 1 (i.e. at least base perlin noise)
		lacunarity = Mathf.Max (lacunarity, 1); // so frequency increases each octave
		persistance = Mathf.Clamp01 (persistance); // so amplitude does not increase each octave
	}
}