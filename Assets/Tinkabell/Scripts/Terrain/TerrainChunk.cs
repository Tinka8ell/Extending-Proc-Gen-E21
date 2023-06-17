﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk {
	public static bool DebugTerrainChunck = true;
	public event System.Action<TerrainChunk, bool> onVisibilityChanged;
	public Vector2 coord;
	public bool heightMapReceived;
	public HeightMap heightMap;
	public Transform parent; 
	 
	GameObject meshObject; // this chunck
	GameObject seaObject; // if sea is associated with this chunk
	GameObject seaPrefab;
	Vector2 sampleCentre;
	Bounds bounds;

	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
	MeshCollider meshCollider;

	LODInfo[] detailLevels;
	LODMesh[] lodMeshes;
	int colliderLODIndex;

	int previousLODIndex = -1;
	bool hasSetCollider;
	float maxViewDst;

	HeightMapSettings heightMapSettings;
	MeshSettings meshSettings;
	Transform viewer;
	Material material;
	Material biomeMaterial;


    List<Biome> biomes = new List<Biome>();

	/* Create Terrain Chunk
	 * Initialised from TerrainGenerator
	 * Initialised invisible
	 * LODMesh array initialised with callbacks
	 * Contents are added by callbacks after it is Loaded
	 */
	public TerrainChunk(
		Vector2 coord, 
		HeightMapSettings heightMapSettings, 
		MeshSettings meshSettings, 
		LODInfo[] detailLevels, 
		int colliderLODIndex, 
		Transform parent, 
		Transform viewer, 
		Material material,
		Material biomeMaterial, 
		Material redMaterial, 
		Material blueMaterial, 
		Material greenMaterial,
		GameObject seaPrefab) 
		{
		this.coord = coord;
		this.heightMapSettings = heightMapSettings;
		this.meshSettings = meshSettings;
		this.detailLevels = detailLevels;
		this.colliderLODIndex = colliderLODIndex;
		this.parent = parent;
		this.viewer = viewer;
		this.material = material;
		this.biomeMaterial = biomeMaterial;
		this.seaPrefab = seaPrefab;

		sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize ;
		bounds = new Bounds(position,Vector2.one * meshSettings.meshWorldSize );

		DebugTerrainChunckLog("new()");
		meshObject = new GameObject("Terrain Chunk" + coord.ToString());
		meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
		meshCollider = meshObject.AddComponent<MeshCollider>();
		meshRenderer.material = this.material;

		meshObject.transform.position = new Vector3(position.x,0,position.y);
		meshObject.transform.parent = this.parent;
		SetVisible(false);

		lodMeshes = new LODMesh[detailLevels.Length];
		for (int i = 0; i < detailLevels.Length; i++) {
			lodMeshes[i] = new LODMesh(detailLevels[i].lod);
			lodMeshes[i].updateCallback += UpdateTerrainChunk;
			if (i == colliderLODIndex) {
				lodMeshes[i].updateCallback += UpdateCollisionMesh;
			}
		}

		maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;

		Biome damp = new Biome(BiomeType.Damp);
		damp.biomeChunk = new BiomeChunk(coord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, this, viewer, biomeMaterial, redMaterial, blueMaterial, greenMaterial);
		biomes.Add(damp);
	}

	/* Load the terrain Chunck
	 * Actually kick of the callback process to build it in the background
	 */
	public void Load() {
		DebugTerrainChunckLog("Load()");
		// Request the basic HeightMap to be used for this chunck
		ThreadedDataRequester.RequestData(
			() => HeightMapGenerator.GenerateHeightMap (
				meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), 
			OnHeightMapReceived);
	}

	/* Once the HeightMap has been created update it to match
	 */
	void OnHeightMapReceived(object heightMapObject) {
		DebugTerrainChunckLog("OnHeightMapReceived()");
		this.heightMap = (HeightMap)heightMapObject;
		heightMapReceived = true;
		foreach (Biome biome in biomes)
		{
			biome.biomeChunk.OnHeightMapReceived(heightMapObject);
		}
		UpdateTerrainChunk ();
	}

	/* Update a chunk because of changes
	 * Only if we have the height map
	 * If it has become visible:
	 *   Calculate the LOD index
	 *   If index changes (note initialised to -1 so must have changed when first get height map)
	 *     if got the mesh: set it
	 *     else: request it
	 * If visibility changed then react to it
	 */
	public void UpdateTerrainChunk() {
		DebugTerrainChunckLog("UpdateTerrainChunk()");
		if (heightMapReceived) {
			foreach (Biome biome in biomes)
			{
				biome.biomeChunk.UpdateBiomeChunk();
			}
			/*
			if (!hasBiome){
				AddBiome();
			}
			*/
			float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));

			bool wasVisible = IsVisible ();
			bool visible = viewerDstFromNearestEdge <= maxViewDst;

			if (visible) {
				int lodIndex = 0;

				for (int i = 0; i < detailLevels.Length - 1; i++) {
					if (viewerDstFromNearestEdge > detailLevels [i].visibleDstThreshold) {
						lodIndex = i + 1;
					} else {
						break;
					}
				}

				if (lodIndex != previousLODIndex) {
					LODMesh lodMesh = lodMeshes [lodIndex];
					if (lodMesh.hasMesh) {
						previousLODIndex = lodIndex;
						meshFilter.mesh = lodMesh.mesh;
					} else if (!lodMesh.hasRequestedMesh) {
						lodMesh.RequestMesh (heightMap, meshSettings);
					}
				}
			}

			if (wasVisible != visible) {
				SetVisible(visible);
				if (onVisibilityChanged != null) {
					onVisibilityChanged(this, visible);
				}
			}
		}
	}

	/* Update the collision mask
	 * Callback used by TerrainGenerator when viewer has moved and chunk is visible
	 * If within collider LOD index, and not yet requested it: request it
	 * If now dangerously close and have it: apply the collider mesh 
	 */
	public void UpdateCollisionMesh() {
		if (!hasSetCollider) {
			float sqrDstFromViewerToEdge = bounds.SqrDistance (viewerPosition);

			if (sqrDstFromViewerToEdge < detailLevels [colliderLODIndex].sqrVisibleDstThreshold) {
				if (!lodMeshes [colliderLODIndex].hasRequestedMesh) {
					lodMeshes [colliderLODIndex].RequestMesh (heightMap, meshSettings);
				}
			}

			if (sqrDstFromViewerToEdge < TerrainGenerator.sqrColliderGenerationDistanceThreshold) {
				if (lodMeshes[colliderLODIndex].hasMesh) {
					meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
					hasSetCollider = true;
				}
			}
		}
	}

/*
	public void AddBiome(){
		DebugTerrainChunckLogFormat("AddBiome called for {0}", meshObject.transform.position);
		biome = new Biome();
		hasBiome = true;
		// temp for now
		biome.seaType = SeaType.HasSea;
		if (biome.seaType == SeaType.HasSea){
			DebugTerrainChunckLogFormat("AddBiome creating sea for {0}", meshObject.transform.position);
			seaObject = GameObject.Instantiate<GameObject>(seaPrefab, meshObject.transform.position, Quaternion.identity, meshObject.transform);
		}
	}
*/

	// utility methods

	// Convert "viewer" to a 2D position
	Vector2 viewerPosition {
		get {
			return new Vector2(viewer.position.x, viewer.position.z);
		}
	}

	public void SetVisible(bool visible) {
		meshObject.SetActive(visible);
		if (seaObject != null) { // if we have an associated sea object then keep it in sync
			seaObject.SetActive(visible);
		}
	}

	public bool IsVisible() {
		return meshObject.activeSelf;
	}

    internal void DestroyChunk()
    {
		// DebugTerrainChunckLog("Destroying game objects for this chunk");
        if(seaObject){
			GameObject.Destroy(seaObject);
		}
        if(meshObject){
			GameObject.Destroy(meshObject);
		}
    }

    private void DebugTerrainChunckLog(string message){
        if (DebugTerrainChunck && coord.x == 0 && coord.y == 0)
            Debug.Log("TerrainChunck(" + coord.ToString() + "): " + message);
    }

}

