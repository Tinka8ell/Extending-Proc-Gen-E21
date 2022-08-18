using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;


	public enum DrawMode {IslandMap, IslandMesh, TerrainMesh, CombinedMesh, FalloffMap};
	public DrawMode drawMode;

	public MeshSettings meshSettings;
	public HeightMapSettings islandHeightSettings;
	[Range(0,1)]
	public float ratio = 1;
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
			NoiseSettings islandNoiseSettings = new NoiseSettings();
			islandNoiseSettings.scale = islandHeightSettings.noiseSettings.scale / meshSettings.numVertsPerLine;
			islandNoiseSettings.seaGradient = islandHeightSettings.noiseSettings.seaGradient;
			islandNoiseSettings.octaves = islandHeightSettings.noiseSettings.octaves;
			islandNoiseSettings.persistance = islandHeightSettings.noiseSettings.persistance;
			islandNoiseSettings.lacunarity = islandHeightSettings.noiseSettings.lacunarity;
			islandNoiseSettings.seed = islandHeightSettings.noiseSettings.seed;
			islandNoiseSettings.offset = islandHeightSettings.noiseSettings.offset - new Vector2(meshSettings.numVertsPerLine / 2, meshSettings.numVertsPerLine / 2);

			HeightMapSettings islandMapSettings = ScriptableObject.CreateInstance("HeightMapSettings") as HeightMapSettings;
			islandMapSettings.noiseSettings = islandNoiseSettings;
			islandMapSettings.heightMultiplier = islandHeightSettings.heightMultiplier;
			islandMapSettings.heightCurve = islandHeightSettings.heightCurve;

			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, islandMapSettings, Vector2.zero);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.IslandMesh) {
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, islandHeightSettings, Vector2.zero);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.TerrainMesh) {
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, terrainHeightSettings, Vector2.zero);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.CombinedMesh) {
			heightMap = HeightMapGenerator.GenerateCombinedHeightMap(
				meshSettings.numVertsPerLine, islandHeightSettings, terrainHeightSettings, ratio, Vector2.zero);
			textureData.UpdateMeshHeights (terrainMaterial, heightMap.minValue, heightMap.maxValue);
			DrawMesh (MeshGenerator.GenerateTerrainMesh (heightMap.values,meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.FalloffMap) {
			heightMap = new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine),0,1);
			DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
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
