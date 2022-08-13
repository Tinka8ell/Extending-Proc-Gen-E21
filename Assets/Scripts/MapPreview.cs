﻿using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;


	public enum DrawMode {IslandMap, TerrainMap, IslandMesh, TerrainMesh, CombinedMesh, FalloffMap};
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
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, islandHeightSettings, Vector2.zero);
			DrawTexture (TextureGenerator.TextureFromHeightMap (heightMap));
		} else if (drawMode == DrawMode.TerrainMap) {
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, islandHeightSettings, Vector2.zero);
			DrawTexture (TextureGenerator.TextureFromHeightMap (heightMap));
		} else if (drawMode == DrawMode.IslandMesh) {
			textureData.UpdateMeshHeights (terrainMaterial, islandHeightSettings.minHeight, islandHeightSettings.maxHeight);
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, islandHeightSettings, Vector2.zero);
			DrawMesh (MeshGenerator.GenerateTerrainMesh (heightMap.values,meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.TerrainMesh) {
			textureData.UpdateMeshHeights (terrainMaterial, terrainHeightSettings.minHeight, terrainHeightSettings.maxHeight);
			heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, terrainHeightSettings, Vector2.zero);
			DrawMesh (MeshGenerator.GenerateTerrainMesh (heightMap.values,meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.CombinedMesh) {
			textureData.UpdateMeshHeights (
				terrainMaterial, 
				islandHeightSettings.minHeight * ratio + terrainHeightSettings.minHeight * (1-ratio), 
				islandHeightSettings.maxHeight * ratio + terrainHeightSettings.maxHeight * (1-ratio));
			heightMap = HeightMapGenerator.GenerateCombinedHeightMap(
				meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, islandHeightSettings, terrainHeightSettings, ratio, Vector2.zero);
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
