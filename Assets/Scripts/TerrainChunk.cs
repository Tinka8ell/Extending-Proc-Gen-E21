using UnityEngine;

public class TerrainChunk {
	
	const float colliderGenerationDistanceThreshold = 5;
	public event System.Action<TerrainChunk, bool> onVisibilityChanged;
	public Vector2 coord;
	 
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

	HeightMap heightMap;
	bool heightMapReceived;
	int previousLODIndex = -1;
	bool hasSetCollider;
	float maxViewDst;

	HeightMapSettings heightMapSettings;
	MeshSettings meshSettings;
	Transform viewer;

	bool hasBiome;
	Biome biome;

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
		GameObject seaPrefab) 
		{
		this.coord = coord;
		this.detailLevels = detailLevels;
		this.colliderLODIndex = colliderLODIndex;
		this.heightMapSettings = heightMapSettings;
		this.meshSettings = meshSettings;
		this.viewer = viewer;
		this.seaPrefab = seaPrefab;

		sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize ;
		bounds = new Bounds(position,Vector2.one * meshSettings.meshWorldSize );


		meshObject = new GameObject("Terrain Chunk");
		meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
		meshCollider = meshObject.AddComponent<MeshCollider>();
		meshRenderer.material = material;

		meshObject.transform.position = new Vector3(position.x,0,position.y);
		meshObject.transform.parent = parent;
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
	}

	/* Load the terrain Chunck
	 * Actually kick of the callback process to build it in the background
	 */
	public void Load() {
		// Request the basic HeightMap to be used for this chunck
		ThreadedDataRequester.RequestData(
			() => HeightMapGenerator.GenerateHeightMap (
				meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), 
			OnHeightMapReceived);
	}

	/* Once the HeightMap has been created update it to match
	 */
	void OnHeightMapReceived(object heightMapObject) {
		this.heightMap = (HeightMap)heightMapObject;
		heightMapReceived = true;
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
		if (heightMapReceived) {
			if (!hasBiome){
				AddBiome();
			}
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

			if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold) {
				if (lodMeshes[colliderLODIndex].hasMesh) {
					meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
					hasSetCollider = true;
				}
			}
		}
	}

	public void AddBiome(){
		biome = new Biome();
		hasBiome = true;
		if (biome.seaType == SeaType.HasSea){
			seaObject = GameObject.Instantiate<GameObject>(seaPrefab, meshObject.transform.position, Quaternion.identity, meshObject.transform);
		}
	}


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

}

class LODMesh {

	public Mesh mesh;
	public bool hasRequestedMesh;
	public bool hasMesh;
	int lod;

	// Once created initialised to UpdateTerrainChunck
	// and if collision LOD also UpdateCollisionMask
	public event System.Action updateCallback;

	public LODMesh(int lod) {
		this.lod = lod;
	}

	void OnMeshDataReceived(object meshDataObject) {
		mesh = ((MeshData)meshDataObject).CreateMesh ();
		hasMesh = true;
		updateCallback(); // so the relevant callbacks get called
	}

	public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
		hasRequestedMesh = true;
		ThreadedDataRequester.RequestData (
			() => MeshGenerator.GenerateTerrainMesh (heightMap.values, meshSettings, lod), 
			OnMeshDataReceived);
	}

}

enum SeaType {Unknown, HasSea, NotSea, LandLocked};

class Biome{
	public SeaType seaType = SeaType.Unknown;
}