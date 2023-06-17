using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

	public static float viewerMoveThresholdForChunkUpdate = 25f;
	public static float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
	public static float colliderGenerationDistanceThreshold = 5;
	public static float sqrColliderGenerationDistanceThreshold = colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold;


	public int colliderLODIndex;
	public LODInfo[] detailLevels;

	public string WorldName = "Default";

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;

	public Transform viewer; 
	public Material mapMaterial;
	public Material biomeMaterial;
	public Material redMaterial;
	public Material blueMaterial;
	public Material greenMaterial;

	public GameObject seaPrefab;
	
	Vector2 viewerPosition;
	Vector2 viewerPositionOld;

	float meshWorldSize;
	int chunksVisibleInViewDst;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

	/* Awake
	 * Load the HeightMapSettings, for this world
	 */
	void Awake(){
		GameManager gameManager = GameManager.Instance;
		if (heightMapSettings == null){ // starting a new world
			WorldName = gameManager.WorldName;
			//Debug.Log("Starting terrain from new, so loading world '" + WorldName + "' heightMapSettings");
			LoadNewWorld();
		} else { // starting game, so prep world system if not there
			//Debug.Log("Awakening terrain, so checking world '" + WorldName + "' exists");
			gameManager.WorldName = WorldName;
			HeightMapSettings defaultWorld = (HeightMapSettings) ScriptableObject.CreateInstance("HeightMapSettings");
			defaultWorld.Load(WorldName);
			if (defaultWorld == null || defaultWorld.height == 0){ // no settings out there
				//Debug.Log("It is missing so creating from ourselves");
				heightMapSettings.SaveAs(WorldName);  // create the default
			}
		}
		// make sure our viewer is our player ...
		viewer = gameManager.Player.transform;

	}

	/* On Start of the Terrain Generator:
	 * Set up mapMaterial using textureSettings and heightMapSettings min and max Height
	 * Work out the meshWorldSize chunksVisibleInViewDst using detailLevels, meshSettings and maxViewDst
	 * Initialise the visible chunks
	 */
	void Start() {
		//Debug.Log("TerrainGenerator is starting up");
		if (heightMapSettings == null || heightMapSettings.height == 0){ 
			// no settings out there!
			Debug.LogError(WorldName + " is missing so aborting!");
			Application.Quit();
		}
	 	// Set up mapMaterial using textureSettings and heightMapSettings min and max Height
		textureSettings.ApplyToMaterial (mapMaterial);
		textureSettings.UpdateMeshHeights (mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

		// Work out the meshWorldSize chunksVisibleInViewDst using detailLevels  and meshSettings
		float maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold; // furthest visibleDstThreshold
		meshWorldSize = meshSettings.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize); 

		// Initialise the visible chunks
		UpdateVisibleChunks();
	}

	/* On each frame:
	 * Work out where the "viewer" is
	 * If "viewer" has moved: update the collision mesh for the visible chuncks
	 * If the "viewer" has moved significantly, then change their old position and update the visible chunks
	 */
	void Update() {
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);

		// ensure the "viewer" does not fall through the terrain!
		if (viewerPosition != viewerPositionOld) {
			foreach (TerrainChunk chunk in visibleTerrainChunks) {
				chunk.UpdateCollisionMesh ();
			}
		}

		// moved enough that we change what is visible
		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks ();
		}
	}
		
	/* When we need to check or change what is visible
	 * For all currently visible chunks update them
	 * Get "viewer" chunk coordinate
	 * For all viewable chunk coordinates:
	 *   If not already updated (above): 
	 *      If exists: update it
	 *      Else: start creating it
	 */
	void UpdateVisibleChunks() {
		// For all currently visible, update them
		HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2> ();
		for (int i = visibleTerrainChunks.Count-1; i >= 0; i--) {
			alreadyUpdatedChunkCoords.Add (visibleTerrainChunks [i].coord);
			visibleTerrainChunks [i].UpdateTerrainChunk ();
		}

		// where is the "viewer"
		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / meshWorldSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / meshWorldSize);

		// for all visible coords update or create chunck
		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!alreadyUpdatedChunkCoords.Contains (viewedChunkCoord)) {
					if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
						terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
					} else {
						TerrainChunk newChunk = new TerrainChunk(
							viewedChunkCoord,
							heightMapSettings,
							meshSettings, 
							detailLevels, 
							colliderLODIndex, 
							transform, 
							viewer, 
							mapMaterial,
							biomeMaterial,
							redMaterial, 
							blueMaterial, 
							greenMaterial,
							seaPrefab);
						terrainChunkDictionary.Add (viewedChunkCoord, newChunk);
						newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
						newChunk.Load ();
					}
				}

			}
		}
	}

	/* Callback when a chunk's visibility changes
	 *   add or remove from the list of visible chunks
	 */
	void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible) {
		if (isVisible) {
			visibleTerrainChunks.Add (chunk);
		} else {
			visibleTerrainChunks.Remove (chunk);
		}
	}

	public void SetViewer(Transform transform){
		viewer = transform;
	}

	private void LoadNewWorld(){
		LoadNewWorld(GameManager.Instance.WorldName, false);
	}

	public void LoadNewWorld(string name, bool updateVisisbleChunks=true){
		string oldName = GameManager.Instance.WorldName;
		GameManager.Instance.WorldName = name;
		//Debug.Log("Loading new world '" + name + "' heightMapSettings");
		HeightMapSettings newHeightMapSettings = (HeightMapSettings) ScriptableObject.CreateInstance("HeightMapSettings");
		newHeightMapSettings.Load(name);
		if (newHeightMapSettings.height == 0){ // no settings out there
			Debug.LogWarning("HeightMapSettings: '" + name + "' is missing so resetting");
			GameManager.Instance.WorldName = oldName;
		} else {
			heightMapSettings = newHeightMapSettings;
			// Reset any existing chunks
			//Debug.Log("Destroying " + terrainChunkDictionary.Count + " exisitng chunks");
			foreach (var item in terrainChunkDictionary)
			{
				item.Value.DestroyChunk();
			}
			terrainChunkDictionary.Clear();
			visibleTerrainChunks = new List<TerrainChunk>();
			if(updateVisisbleChunks){
        		// Initialise the visible chunks
				//Debug.Log("Updating visible chunks");
		        UpdateVisibleChunks();
			}
		}
	}

}

[System.Serializable]
public struct LODInfo {
	[Range(0, MeshSettings.numSupportedLODs - 1)]
	// Used to create LODMeshs in TerrainChuncks
	public int lod;
	public float visibleDstThreshold;


	public float sqrVisibleDstThreshold {
		get {
			return visibleDstThreshold * visibleDstThreshold;
		}
	}
}

class LODMesh {

	public Mesh mesh;
	public bool hasRequestedMesh;
	public bool hasMesh;
	int lod;
	bool isBiome;

	// Once created initialised to UpdateTerrainChunck
	// and if collision LOD also UpdateCollisionMask
	public event System.Action updateCallback;

	public LODMesh(int lod, bool isBiome = false) {
		this.lod = lod;
		this.isBiome = isBiome;
	}

	void OnMeshDataReceived(object meshDataObject) {
		mesh = ((MeshData)meshDataObject).CreateMesh();
		hasMesh = true;
		updateCallback(); // so the relevant callbacks get called
	}

	public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings, bool isBiome = false) {
		hasRequestedMesh = true;
		ThreadedDataRequester.RequestData (
			() => MeshGenerator.GenerateTerrainMesh (heightMap.values, meshSettings, lod, isBiome), 
			OnMeshDataReceived);
	}

}

enum SeaType {Unknown, HasSea, NotSea, LandLocked};
enum BiomeType {Damp};

class Biome{
	public BiomeType biomeType;
	public SeaType seaType = SeaType.Unknown;
	public BiomeChunk biomeChunk;

	public Biome(BiomeType type){
		biomeType = type;
	}
}
