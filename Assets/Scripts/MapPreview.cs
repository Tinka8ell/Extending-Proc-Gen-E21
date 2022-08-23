using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;


	public enum DrawMode {IslandMap, IslandMesh, TerrainMesh, CombinedMesh};
	public DrawMode drawMode;

	public MeshSettings meshSettings;
	public HeightMapSettings combinedHeightSettings;
	public HeightMapSettings islandHeightSettings;
	public HeightMapSettings terrainHeightSettings;
	public TextureData textureData;

	public Material terrainMaterial;



	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int editorPreviewLOD;
	public bool autoUpdate;


	public void DrawMapInEditor() {
		textureData.ApplyToMaterial (terrainMaterial);
		HeightMap heightMap;

		if (drawMode == DrawMode.IslandMap) {
			int scale = meshSettings.numVertsPerLine;
			NoiseSettings islandNoiseSettings = new NoiseSettings();
			islandNoiseSettings.scale = islandHeightSettings.weightedNoiseSettings[0].noiseSettings.scale / scale;
			islandNoiseSettings.seaGradient = islandHeightSettings.weightedNoiseSettings[0].noiseSettings.seaGradient;
			islandNoiseSettings.octaves = islandHeightSettings.weightedNoiseSettings[0].noiseSettings.octaves;
			islandNoiseSettings.persistance = islandHeightSettings.weightedNoiseSettings[0].noiseSettings.persistance;
			islandNoiseSettings.lacunarity = islandHeightSettings.weightedNoiseSettings[0].noiseSettings.lacunarity;
			islandNoiseSettings.seed = islandHeightSettings.weightedNoiseSettings[0].noiseSettings.seed;
			islandNoiseSettings.offset = islandHeightSettings.weightedNoiseSettings[0].noiseSettings.offset - new Vector2(meshSettings.numVertsPerLine / 2, meshSettings.numVertsPerLine / 2);

			WeightedNoiseSettings weightedIslandNoiseSettings = new WeightedNoiseSettings();
			weightedIslandNoiseSettings.noiseSettings = islandNoiseSettings;
			weightedIslandNoiseSettings.heightMultiplier = islandHeightSettings.weightedNoiseSettings[0].heightMultiplier/ scale;

			HeightMapSettings islandMapSettings = ScriptableObject.CreateInstance("HeightMapSettings") as HeightMapSettings;
			islandMapSettings.heightCurve = islandHeightSettings.heightCurve;
			islandMapSettings.weightedNoiseSettings = new WeightedNoiseSettings[1];
			islandMapSettings.weightedNoiseSettings[0] = weightedIslandNoiseSettings;

			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, islandMapSettings, Vector2.zero);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
	        // Debug.LogFormat("IslandMap: Min = {0}, Max = {1}", heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.IslandMesh) {
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, islandHeightSettings, Vector2.zero);
	        // Debug.LogFormat("IslandMesh: Min = {0}, Max = {1}", heightMap.minValue, heightMap.maxValue);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.TerrainMesh) {
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, terrainHeightSettings, Vector2.zero);
	        // Debug.LogFormat("TerrainMesh: Min = {0}, Max = {1}", heightMap.minValue, heightMap.maxValue);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.CombinedMesh) {
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, combinedHeightSettings, Vector2.zero);
	        // Debug.LogFormat("TerrainMesh: Min = {0}, Max = {1}", heightMap.minValue, heightMap.maxValue);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		}
	}


	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height) /10f;

		textureRender.gameObject.SetActive (true);
		meshFilter.gameObject.SetActive (false);
	}

	public void DrawMesh(MeshData meshData) {
		meshFilter.sharedMesh = meshData.CreateMesh ();

		textureRender.gameObject.SetActive (false);
		meshFilter.gameObject.SetActive (true);
	}



	void OnValuesUpdated() {
		if (!Application.isPlaying) {
			DrawMapInEditor ();
		}
	}

	void OnTextureValuesUpdated() {
		textureData.ApplyToMaterial (terrainMaterial);
	}

	void OnValidate() {

		if (meshSettings != null) {
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (islandHeightSettings != null) {
			islandHeightSettings.OnValuesUpdated -= OnValuesUpdated;
			islandHeightSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (terrainHeightSettings != null) {
			terrainHeightSettings.OnValuesUpdated -= OnValuesUpdated;
			terrainHeightSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (textureData != null) {
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}

	}

}
