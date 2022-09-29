using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;


	public enum DrawMode {IslandMap, IslandMesh, TerrainMesh, CombinedMesh};
	public DrawMode drawMode;

	public Vector2 sampleCentre;

	[Range(1, 120 + 5)]
	public float mapScale = 10;

	public MeshSettings meshSettings;
	public HeightMapSettings combinedHeightSettings;
	public HeightMapSettings islandHeightSettings;
	public HeightMapSettings terrainHeightSettings;
	public TextureData textureData;

	public Material terrainMaterial;



	[Range(0, MeshSettings.numSupportedLODs-1)]
	public int editorPreviewLOD;
	public bool autoUpdate;


	public void DrawMapInEditor() {
		textureData.ApplyToMaterial (terrainMaterial);
		HeightMap heightMap;

		if (drawMode == DrawMode.IslandMap) {
			// Zoom out by mapScale ...

			// new islandMapSettings (HeightMapSettings):
			HeightMapSettings islandMapSettings = ScriptableObject.CreateInstance("HeightMapSettings") as HeightMapSettings;
			islandMapSettings.heightCurve = islandHeightSettings.heightCurve;
			int countOfWweighted = combinedHeightSettings.weightedNoiseSettings.Length;
			islandMapSettings.weightedNoiseSettings = new WeightedNoiseSettings[countOfWweighted];
			for(int index = 0; index < countOfWweighted; index++){
				// zoomedIslandNoiseSettings (WeightedNoiseSettings):
				WeightedNoiseSettings zoomedIslandNoiseSettings = new WeightedNoiseSettings();
				zoomedIslandNoiseSettings.noiseSettings = combinedHeightSettings.weightedNoiseSettings[index].noiseSettings;
				// and zoom:
				zoomedIslandNoiseSettings.heightMultiplier = combinedHeightSettings.weightedNoiseSettings[index].heightMultiplier/ mapScale;

				islandMapSettings.weightedNoiseSettings[index] = zoomedIslandNoiseSettings;
			}

			// get HeightMap adjusted to map scale:
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, islandMapSettings, sampleCentre, mapScale);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
	        // Debug.LogFormat("IslandMap: Min = {0}, Max = {1}", heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.IslandMesh) {
			// Only show island noise part

			// new islandMapSettings (HeightMapSettings):
			HeightMapSettings islandMapSettings = ScriptableObject.CreateInstance("HeightMapSettings") as HeightMapSettings;
			islandMapSettings.heightCurve = islandHeightSettings.heightCurve;
			islandMapSettings.weightedNoiseSettings = new WeightedNoiseSettings[1];
			islandMapSettings.weightedNoiseSettings[0] = combinedHeightSettings.weightedNoiseSettings[0];

			// get HeightMap adjusted to map scale:
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, islandMapSettings, sampleCentre);
	        // Debug.LogFormat("IslandMesh: Min = {0}, Max = {1}", heightMap.minValue, heightMap.maxValue);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.TerrainMesh) {
			// Only show terrain noise part

			// new islandMapSettings (HeightMapSettings):
			HeightMapSettings islandMapSettings = ScriptableObject.CreateInstance("HeightMapSettings") as HeightMapSettings;
			islandMapSettings.heightCurve = islandHeightSettings.heightCurve;
			islandMapSettings.weightedNoiseSettings = new WeightedNoiseSettings[1];
			islandMapSettings.weightedNoiseSettings[0] = combinedHeightSettings.weightedNoiseSettings[1];

			// get HeightMap adjusted to map scale:
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, islandMapSettings, sampleCentre);
	        // Debug.LogFormat("TerrainMesh: Min = {0}, Max = {1}", heightMap.minValue, heightMap.maxValue);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.CombinedMesh) {
			// show the combined mesh
			
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, combinedHeightSettings, sampleCentre);
	        // Debug.LogFormat("TerrainMesh: Min = {0}, Max = {1}", heightMap.minValue, heightMap.maxValue);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		}
	}


	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height) / 10f;

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
