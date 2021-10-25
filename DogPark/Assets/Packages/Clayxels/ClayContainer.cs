
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Reflection;
#endif


namespace Clayxels{

	/* This class is your main interface to work with Clayxels, it is designed to work in editor and in game.
		Each container nests one or more ClayObject as children in its hierarchy to generate the final clay result.
	*/
	[ExecuteInEditMode]
	public class ClayContainer : MonoBehaviour{

		// private class, containers use chunks to work with big voxel grids and save memory
		class ClayxelChunk{
			public ComputeBuffer pointCloudDataBuffer;
			public ComputeBuffer indirectDrawArgsBuffer;
			public Vector3Int coords = new Vector3Int();
			public Vector3 center = new Vector3();
			public MaterialPropertyBlock materialProperties;
		}

		/* Color of the bounds shown by each container */
		public static Color boundsColor = new Color(0.5f, 0.5f, 1.0f, 0.1f);

		/* Keyboard shortcuts, they can be changed from ClayxelsPrefs.cs .*/
		public static string pickingKey = "p";
		public static string mirrorDuplicateKey = "m";
		
		/* CustomMaterial: specify a material that is not the default one. It will need a special shader as shown in the examples provided.*/
		public Material customMaterial = null;

		/* Enable this when animating or moving clayObjects at runtime. 
			When a container moves, it inhibits any update of the nested ClayObjects. 
			This attribute makes sure that even if this container is moving, the ClayObjects will also be computed for each frame.*/
		public bool forceUpdate = false;

		// private use for a ClayObject to notify its parent container that something got updated
		public bool needsUpdate = true;

		public ClayContainer instanceOf = null;
		
		static ComputeBuffer solidsUpdatedBuffer;
		static ComputeBuffer solidsPerChunkBuffer;
		static ComputeBuffer meshIndicesBuffer = null;
		static ComputeBuffer meshVertsBuffer = null;
		static ComputeBuffer meshColorsBuffer = null;
		static ComputeBuffer pointCloudDataToSolidIdBuffer = null;

		static ComputeShader claycoreCompute;
		static ComputeBuffer gridDataBuffer;
		static ComputeBuffer triangleConnectionTable;
		static ComputeBuffer prefilteredSolidIdsBuffer;
		static ComputeBuffer solidsFilterBuffer;
		static ComputeBuffer numSolidsPerChunkBuffer;

		static ComputeBuffer fieldCache1Buffer = null;
		static ComputeBuffer fieldCache2Buffer = null;
		
		static int maxSolids = 512;
		static int maxSolidsPerVoxel = 128;
		public static int chunkMaxOutPoints = 262144;
		static int inspectorUpdated;
		static int[] tmpChunkData;
		static public bool globalDataNeedsInit = true;
		static List<string> solidsCatalogueLabels = new List<string>();
		static List<List<string[]>> solidsCatalogueParameters = new List<List<string[]>>();
		static List<ComputeBuffer> globalCompBuffers = new List<ComputeBuffer>();
		static int lastUpdatedContainerId = -1;
		static int maxThreads = 8;
		static int[] solidsInSingleChunkArray;
		static int updateFrameSkip = 0;
		static string renderPipe = "";
		static RenderTexture pickingRenderTexture = null;
		static RenderTargetIdentifier pickingRenderTextureId;
		static CommandBuffer pickingCommandBuffer;
		static Texture2D pickingTextureResult;
		static Rect pickingRect;
		static float pickingMousePosX = -1;
		static float pickingMousePosY = -1;
		static int pickedClayObjectId = -1;
		static int pickedContainerId = -1;
		static GameObject pickedObj = null;
		static bool pickingMode = false;
		static bool pickingShiftPressed = false;
		static float[] fieldCacheInitValues = new float[]{};
		static int maxChunkX = 4;
		static int maxChunkY = 4;
		static int maxChunkZ = 4;
		static int totalMaxChunks = 64;
		static int cacheEnabled = 0;
		static int[] indirectArgsData = new int[]{0, 1, 0, 0};
		static Material clayxelPickingMaterial = null;
		static MaterialPropertyBlock pickingMaterialProperties;
		static int totalChunksInScene = 0;
		static bool vramLimitEnabled = false;
		static int numThreadsComputeStartRes;
		static int numThreadsComputeFullRes;

		[SerializeField] int clayxelDetail = 88;
		[SerializeField] int chunksX = 1;
		[SerializeField] int chunksY = 1;
		[SerializeField] int chunksZ = 1;
		[SerializeField] Material material = null;
		[SerializeField] ShadowCastingMode castShadows = ShadowCastingMode.On;
		[SerializeField] bool receiveShadows = true;
		[SerializeField] public string storeAssetPath = "";
		[SerializeField] bool frozen = false;
		[SerializeField] bool clayObjectsOrderLocked = true;
		[SerializeField] bool autoBounds = false;

		int chunkSize = 8;
		bool memoryOptimized = false;
		float globalSmoothing = 0.0f;
		Dictionary<int, int> solidsUpdatedDict = new Dictionary<int, int>();
		List<ClayxelChunk> chunks = new List<ClayxelChunk>();
		List<ComputeBuffer> compBuffers = new List<ComputeBuffer>();
		bool needsInit = true;
		int[] genericIntBufferArray = new int[1]{0};
		List<Vector3> solidsPos;
		List<Quaternion> solidsRot;
		List<Vector3> solidsScale;
		List<float> solidsBlend;
		List<int> solidsType;
		List<Vector3> solidsColor;
		List<Vector4> solidsAttrs;
		List<int> solidsClayObjectId;
		ComputeBuffer solidsPosBuffer = null;
		ComputeBuffer solidsRotBuffer = null;
		ComputeBuffer solidsScaleBuffer = null;
		ComputeBuffer solidsBlendBuffer = null;
		ComputeBuffer solidsTypeBuffer = null;
		ComputeBuffer solidsColorBuffer = null;
		ComputeBuffer solidsAttrsBuffer = null;
		ComputeBuffer solidsClayObjectIdBuffer = null;
		ComputeBuffer genericNumberBuffer;
		ComputeBuffer indirectChunkArgs1Buffer;
		ComputeBuffer indirectChunkArgs2Buffer;
		ComputeBuffer updateChunksBuffer;
		Vector3 boundsScale = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 boundsCenter = new Vector3(0.0f, 0.0f, 0.0f);
		Bounds renderBounds = new Bounds();
		bool solidsHierarchyNeedsScan = false;
		List<ClayObject> clayObjects = new List<ClayObject>();
		List<Solid> solids = new List<Solid>();
		int numChunks = 0;
		float deltaTime = 0.0f;
		float voxelSize = 0.0f;
		int updateFrame = 0;
		float splatRadius = 1.0f;
		bool editingThisContainer = false;
		int autoBoundsChunkSize = 0;
		int autoFrameSkip = 0;
		string userWarning = "";
		
		enum Kernels{
			computeGrid,
			cacheDistField,
			clearCachedDistField,
			generatePointCloud,
			debugDisplayGridPoints,
			computeGridForMesh,
			computeMesh,
			filterSolidsPerChunk
		}

		public bool isAutoBoundsActive(){
			return this.autoBounds;
		}

		public void setAutoBoundsActive(bool state){
			this.autoBounds = state;

			this.needsInit = true;
			this.init();
		}

		/* Max number of points per chunk (number of chunks is set via boundsScale on the inspector)
        	this only affects video memory while sculpting or moving clayObjects at runtime
        	points are also automatically optimized whenever possible. */
		public static void setPointCloudLimit(int num){
			ClayContainer.chunkMaxOutPoints = num;
			ClayContainer.globalDataNeedsInit = true;
		}
		
		/* Skip N frames before updating to reduce stress on GPU and increase FPS count. 
			See ClayxelPrefs.cs */
		public static void setUpdateFrameSkip(int frameSkip){
			ClayContainer.updateFrameSkip = frameSkip;
		}

		/* How many soldis can this container work with in total.
			Valid values: 64, 128, 256, 512, 1024, 4096, 16384
			See ClayxelPrefs.cs */
		public static void setMaxSolids(int num){
			ClayContainer.maxSolids = num;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* Upon creating a container, clayxels will check if there is enough vram available.
        	The check is a rough estimate, disable this check to get past this limit. */
		public static void enableVideoRamSafeLimit(bool state){
			ClayContainer.vramLimitEnabled = state;
		}

		/* How many chunks can this container work with in total.
			Keep these to a minimum when enabling CLAYXELS_CACHEON or video memory might run out.*/
		public static void setMaxChunks(int x, int y, int z){
			ClayContainer.maxChunkX = x;
			ClayContainer.maxChunkY = y;
			ClayContainer.maxChunkZ = z;
			ClayContainer.totalMaxChunks = x * y * z;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* How many solids can stay one next to another while occupying the same voxel.
			Keeping this value low will increase overall performance but will cause disappearing clayxels if the number is exceeded.
			Valid values: 32, 64, 128, 256, 512, 1024, 2048
			See ClayxelPrefs.cs */
		public static void setMaxSolidsPerVoxel(int num){
			ClayContainer.maxSolidsPerVoxel = num;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* Sets how finely detailed are your clayxels, range 0 to 100.*/
		public void setClayxelDetail(int value){
			if(value == this.clayxelDetail || this.frozen || this.needsInit){
				return;
			}

			this.switchComputeData();

			if(value < 0){
				value = 0;
			}
			else if(value > 100){
				value = 100;
			}

			this.clayxelDetail = value;

			this.updateInternalBounds();

			this.forceUpdateAllSolids();
			this.computeClay();
		}

		void updateInternalBounds(){
			this.chunkSize = (int)Mathf.Lerp(40.0f, 4.0f, (float)this.clayxelDetail / 100.0f);
			
			if(this.autoBoundsChunkSize > this.chunkSize){
				this.chunkSize = this.autoBoundsChunkSize;
			}
			
			float voxelSize = (float)this.chunkSize / 256;

			this.voxelSize = voxelSize;
			this.splatRadius = this.voxelSize * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);

			this.globalSmoothing = this.voxelSize;
			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);

			this.boundsScale.x = (float)this.chunkSize * this.chunksX;
			this.boundsScale.y = (float)this.chunkSize * this.chunksY;
			this.boundsScale.z = (float)this.chunkSize * this.chunksZ;
			this.renderBounds.size = this.boundsScale * this.transform.lossyScale.x;

			float gridCenterOffset = (this.chunkSize * 0.5f);
			this.boundsCenter.x = ((this.chunkSize * (this.chunksX - 1)) * 0.5f) - (gridCenterOffset*(this.chunksX-1));
			this.boundsCenter.y = ((this.chunkSize * (this.chunksY - 1)) * 0.5f) - (gridCenterOffset*(this.chunksY-1));
			this.boundsCenter.z = ((this.chunkSize * (this.chunksZ - 1)) * 0.5f) - (gridCenterOffset*(this.chunksZ-1));

			float chunkOffset = this.chunkSize - voxelSize; // removes the seam between chunks

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];
				chunk.center = new Vector3(
					(-((this.chunkSize * this.chunksX) * 0.5f) + gridCenterOffset) + (chunkOffset * chunk.coords.x),
					(-((this.chunkSize * this.chunksY) * 0.5f) + gridCenterOffset) + (chunkOffset * chunk.coords.y),
					(-((this.chunkSize * this.chunksZ) * 0.5f) + gridCenterOffset) + (chunkOffset * chunk.coords.z));
			}

			if(this.autoBounds){
				this.needsUpdate = true;
			}
		}

		/* Get the value specified by setClayxelDetail()*/		
		public int getClayxelDetail(){
			return this.clayxelDetail;
		}

		/* Determines how much work area you have for your sculpt within this container.
			These values are not expressed in scene units, 
			the final size of this container is determined by the value specified with setClayxelDetail().
			Performance tip: The bigger the bounds, the slower this container will be to compute clay in-game.*/
		public void setBoundsScale(int x, int y, int z){
			this.chunksX = x;
			this.chunksY = y;
			this.chunksZ = z;
			this.limitChunkValues();

			this.needsInit = true;

			ClayContainer.totalChunksInScene = 0;
		}

		/* Get the values specified by setBoundsScale()*/		
		public Vector3Int getBoundsScale(){
			return new Vector3Int(this.chunksX, this.chunksY, this.chunksZ);
		}

		/* How many solids can a container work with.*/
		public int getMaxSolids(){
			return ClayContainer.maxSolids;
		}

		/* How many solids are currently used in this container.*/
		public int getNumSolids(){
			return this.solids.Count;
		}

		/* How many ClayObjects currently in this container, each ClayObject will spawn a certain amount of Solids.*/
		public int getNumClayObjects(){
			return  this.clayObjects.Count;
		}

		/* Invoke this after adding a new ClayObject in scene to have the container notified instantly.*/
		public void scanClayObjectsHierarchy(){
			this.clayObjects.Clear();
			this.solidsUpdatedDict.Clear();
			this.solids.Clear();
			
			List<ClayObject> collectedClayObjs = new List<ClayObject>();
			this.scanRecursive(this.transform, collectedClayObjs);

			for(int i = 0; i < collectedClayObjs.Count; ++i){
				this.collectClayObject(collectedClayObjs[i]);
			}

			this.solidsHierarchyNeedsScan = false;

			if(this.numChunks == 1){
				this.genericIntBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.genericIntBufferArray);
			}
		}

		/* Get and own the list of solids in this container. 
			Useful when you don't want a heavy hierarchy of ClayObject in scene (ex. working with particles). */
		public List<Solid> getSolids(){
			return this.solids;
		}

		/* If you work directly with the list of solids in this container, invoke this to notify when a solid has changed.*/
		public void solidUpdated(int id){
			if(id < ClayContainer.maxSolids){
				this.solidsUpdatedDict[id] = 1;

				this.needsUpdate = true;
			}
		}

		/* If you are manipulating the internal list of solids, use this after you add or remove solids in the list.*/
		public void updatedSolidCount(){
			if(this.numChunks == 1){
				this.genericIntBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.genericIntBufferArray);
			}
			
			for(int i = 0; i < this.solids.Count; ++i){
				Solid solid = this.solids[i];
				solid.id = i;
				
				if(solid.id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[solid.id] = 1;
				}
				else{
					break;
				}
			}
		}

		/* Set a material with a clayxels-compatible shader or set it to null to return to the standard clayxels shader.*/
		public void setCustomMaterial(Material material){
			this.customMaterial = material;
			this.material = material;

			this.initMaterialProperties();
		}

		/* Automatically invoked once when the game starts, 
			you only need to invoke this yourself if you change what's declared in ClayXelsPrefs.cs at runtime.*/
		static public void initGlobalData(){
			if(!ClayContainer.globalDataNeedsInit){
				return;
			}

			ClayxelsPrefs.apply();

			ClayContainer.totalChunksInScene = 0;

			ClayContainer.numThreadsComputeStartRes = 64 / ClayContainer.maxThreads;
			ClayContainer.numThreadsComputeFullRes = 256 / ClayContainer.maxThreads;

			string renderPipeAsset = "";
			if(GraphicsSettings.renderPipelineAsset != null){
				renderPipeAsset = GraphicsSettings.renderPipelineAsset.GetType().Name;
			}
			
			if(renderPipeAsset == "HDRenderPipelineAsset"){
				ClayContainer.renderPipe = "hdrp";
			}
			else if(renderPipeAsset == "UniversalRenderPipelineAsset"){
				ClayContainer.renderPipe = "urp";
			}
			else{
				ClayContainer.renderPipe = "builtin";
			}

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					ClayContainer.setupScenePicking();
					ClayContainer.pickingMode = false;
					ClayContainer.pickedObj = null;
				}
			#endif

			ClayContainer.reloadSolidsCatalogue();

			ClayContainer.globalDataNeedsInit = false;

			ClayContainer.lastUpdatedContainerId = -1;

			ClayContainer.releaseGlobalBuffers();

			UnityEngine.Object clayCore = Resources.Load("clayCoreLock");
			if(clayCore == null){
				clayCore = Resources.Load("clayCore");
			}

			ClayContainer.claycoreCompute = (ComputeShader)Instantiate(clayCore);
			
			ClayContainer.gridDataBuffer = new ComputeBuffer(256 * 256 * 256, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.gridDataBuffer);

			ClayContainer.prefilteredSolidIdsBuffer = new ComputeBuffer((64 * 64 * 64) * ClayContainer.maxSolidsPerVoxel, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);

			int maxSolidsPerVoxelMask = ClayContainer.maxSolidsPerVoxel / 32;
			ClayContainer.solidsFilterBuffer = new ComputeBuffer((64 * 64 * 64) * maxSolidsPerVoxelMask, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsFilterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "solidsFilter", ClayContainer.solidsFilterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "solidsFilter", ClayContainer.solidsFilterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "solidsFilter", ClayContainer.solidsFilterBuffer);

			ClayContainer.claycoreCompute.SetInt("maxSolidsPerVoxel", maxSolidsPerVoxel);
			ClayContainer.claycoreCompute.SetInt("maxSolidsPerVoxelMask", maxSolidsPerVoxelMask);
			
			ClayContainer.triangleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.triangleConnectionTable);

			ClayContainer.triangleConnectionTable.SetData(MeshUtils.TriangleConnectionTable);
			
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "triangleConnectionTable", ClayContainer.triangleConnectionTable);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "triangleConnectionTable", ClayContainer.triangleConnectionTable);

			ClayContainer.claycoreCompute.SetInt("maxSolids", ClayContainer.maxSolids);

			int numKernels = Enum.GetNames(typeof(Kernels)).Length;
			for(int i = 0; i < numKernels; ++i){
				ClayContainer.claycoreCompute.SetBuffer(i, "gridData", ClayContainer.gridDataBuffer);
			}

			ClayContainer.numSolidsPerChunkBuffer = new ComputeBuffer(64, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);

			ClayContainer.solidsUpdatedBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsUpdatedBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "solidsUpdated", ClayContainer.solidsUpdatedBuffer);

			int maxChunks = 64;
			ClayContainer.solidsPerChunkBuffer = new ComputeBuffer(ClayContainer.maxSolids * maxChunks, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);

			ClayContainer.solidsInSingleChunkArray = new int[ClayContainer.maxSolids];
			for(int i = 0; i < ClayContainer.maxSolids; ++i){
				ClayContainer.solidsInSingleChunkArray[i] = i;
			}

			ClayContainer.tmpChunkData = new int[ClayContainer.chunkMaxOutPoints * 2];

			if(ClayContainer.cacheEnabled == 1){
				ClayContainer.setupDistFieldCache();
			}

			ClayContainer.meshIndicesBuffer = null;
			ClayContainer.meshVertsBuffer = null;
			ClayContainer.meshColorsBuffer = null;

			ClayContainer.claycoreCompute.SetInt("chunkMaxOutPoints", ClayContainer.chunkMaxOutPoints);
			ClayContainer.pointCloudDataToSolidIdBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints * ClayContainer.totalMaxChunks, sizeof(int));
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "pointCloudDataToSolidId", ClayContainer.pointCloudDataToSolidIdBuffer);
			ClayContainer.globalCompBuffers.Add(ClayContainer.pointCloudDataToSolidIdBuffer);

			ClayContainer.claycoreCompute.SetInt("storeSolidId", 0);
		}

		/* Automatically invoked once when the game starts, 
			you only need to invoke this yourself if you change chunkSize or chunksX,Y,Z attributes.*/
		public void init(){
			#if UNITY_EDITOR
				if(!Application.isPlaying){
					this.reinstallEditorEvents();
				}
			#endif

			if(ClayContainer.globalDataNeedsInit){
				ClayContainer.initGlobalData();
			}

			this.needsInit = false;

			this.userWarning = "";

			if(this.instanceOf != null){
				return;
			}
			
			if(this.frozen){
				this.releaseBuffers();
				return;
			}

			bool vramAvailable = this.checkVRam();
			if(!vramAvailable){
				this.enabled = false;
				Debug.Log("Clayxels: you have reached the maximum amount of containers for your available video ram.\nTo increase this limit, open ClayxelsPrefs.cs and lower the maximum amount of chunks from ClayContainer.setMaxChunks().");
				return;
			}

			this.memoryOptimized = false;

			this.chunkSize = (int)Mathf.Lerp(40.0f, 4.0f, (float)this.clayxelDetail / 100.0f);
			this.limitChunkValues();

			this.clayObjects.Clear();
			this.solidsUpdatedDict.Clear();

			this.releaseBuffers();

			this.solidsHierarchyNeedsScan = true;
			this.scanClayObjectsHierarchy();

			this.voxelSize = (float)this.chunkSize / 256;
			this.splatRadius = this.voxelSize * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);

			this.initChunks();

			this.autoFrameSkip = 0;
			this.autoBoundsChunkSize = 0;
			if(this.autoBounds){
				this.initAutoBounds();
			}

			this.globalSmoothing = this.voxelSize;
			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);
			
			this.genericNumberBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
			this.compBuffers.Add(this.genericNumberBuffer);

			this.needsUpdate = true;
			ClayContainer.lastUpdatedContainerId = -1;

			this.initMaterialProperties();

			this.initSolidsData();

			this.computeClay();

			this.updateFrame = 0;

			if(Application.isPlaying){
				this.editingThisContainer = false;
			}

			if(this.clayObjects.Count > 0 && !this.forceUpdate && !this.editingThisContainer){
				this.optimizeMemory();

				if(Application.isPlaying){
					this.enableAllClayObjects(false);
				}
			}
		}

		/* Spawn a new ClayObject in scene under this container.*/
		public ClayObject addClayObject(){
			GameObject clayObj = new GameObject("clay_cube+");
			clayObj.transform.parent = this.transform;
			clayObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

			ClayObject clayObjComp = clayObj.AddComponent<ClayObject>();
			clayObjComp.clayxelContainerRef = new WeakReference(this);
			clayObjComp.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

			this.collectClayObject(clayObjComp);

			this.needsUpdate = true;

			return clayObjComp;
		}

		/* Get a ClayObject inside this container by id.*/
		public ClayObject getClayObject(int id){
			return this.clayObjects[id];
		}

		/* Scan for ClayObjects in this container at the next update.*/
		public void scheduleClayObjectsScan(){
			this.solidsHierarchyNeedsScan = true;
			this.needsUpdate = true;
		}

		/* Invoke this when you need all solids in a container to be updated, ex. if you change the material attributes.*/
		public void forceUpdateAllSolids(){
			for(int i = 0; i < this.solids.Count; ++i){
				int id = this.solids[i].id;
				if(id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[id] = 1;
				}
				else{
					break;
				}
			}

			this.needsUpdate = true;
		}

		/* Notify this container that one of the nested ClayObject has changed.*/
		public void clayObjectUpdated(ClayObject clayObj){
			if(!this.transform.hasChanged || this.forceUpdate){
				for(int i = 0; i < clayObj.getNumSolids(); ++i){
					int id = clayObj.getSolid(i).id;
					if(id < ClayContainer.maxSolids){
						this.solidsUpdatedDict[id] = 1;
					}
				}

				this.needsUpdate = true;
			}
		}
		
		/* Get the material currently in use by this container. */
		public Material getMaterial(){
			return this.material;
		}

		/* Automatically invoked when the game starts.
			If this container will not receive any more editing for a while,
			then use this method to resize the memory used by this container to make it weight just as a frozen mesh.
			Memory will automatically be expanded again if you tweak one of the ClayObjects in this container.*/
		public void optimizeMemory(){
			if(this.memoryOptimized || this.forceUpdate){
				return;
			}
			
			this.memoryOptimized = true;

			List<ClayxelChunk> newChunks = new List<ClayxelChunk>();

			this.userWarning = "";

			for(int i = 0; i < this.chunks.Count; ++i){
				ClayxelChunk chunk = this.chunks[i];
				chunk.indirectDrawArgsBuffer.GetData(ClayContainer.indirectArgsData);
				
				int pointCount = ClayContainer.indirectArgsData[0] / 3;

				if(pointCount > ClayContainer.chunkMaxOutPoints){
					pointCount = ClayContainer.chunkMaxOutPoints;
					ClayContainer.chunkMaxOutPoints = pointCount;

					this.userWarning = "max point count exceeded, increase limit from file ClayxelsPrefs.cs setPointCloudLimit";

					Debug.Log("Clayxels: container " + this.gameObject.name + " has exceeded the limit of points allowed, to increase the limit use ClayContainer.setPointCloudLimit() inside ClayxelsPrefs.cs");
				}
				
				chunk.pointCloudDataBuffer.GetData(ClayContainer.tmpChunkData, 0, 0, pointCount * 2);
				
				chunk.pointCloudDataBuffer.Release();
				this.compBuffers.Remove(chunk.pointCloudDataBuffer);
				
				if(pointCount > 0){
					chunk.pointCloudDataBuffer = new ComputeBuffer(pointCount, sizeof(int) * 2);
					this.compBuffers.Add(chunk.pointCloudDataBuffer);

					chunk.pointCloudDataBuffer.SetData(ClayContainer.tmpChunkData, 0, 0, pointCount * 2);

					chunk.materialProperties.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
					chunk.materialProperties.SetInt("solidHighlightId", -1);

					newChunks.Add(chunk);
				}
				else{
					chunk.indirectDrawArgsBuffer.Release();
					this.compBuffers.Remove(chunk.indirectDrawArgsBuffer);
				}
			}

			this.numChunks = newChunks.Count;
			this.chunks = newChunks;

			this.updateFrame = 0;
		}

		/* Force this container to compute the final clay result now.
			Useful if you have set frame skips or limited the chunks to update per frame.*/
		public void computeClay(){
			if(this.needsInit){
				return;
			}

			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}
			
			if(this.solidsHierarchyNeedsScan){
				this.scanClayObjectsHierarchy();
			}

			if(this.memoryOptimized){
				this.expandMemory();
			}

			if(this.autoBounds){
				float boundsSize = this.computeAutoBounds();
				this.updateAutoBounds(boundsSize);
			}
			
			this.needsUpdate = false;
			this.updateFrame = 0;

			this.updateSolids();
			
			if(this.numChunks == 1){
				this.computeChunk(0);
			}
			else{
				for(int i = 0; i < this.numChunks; ++i){
					this.computeChunk(i);
				}
			}
		}

		/* */
		public void setCastShadows(bool state){
			if(state){
				this.castShadows = ShadowCastingMode.On;
			}
			else{
				this.castShadows = ShadowCastingMode.Off;		
			}
		}

		/* */
		public bool getCastShadows(){
			if(this.castShadows == ShadowCastingMode.On){
				return true;
			}

			return false;
		}

		/* */
		public void setReceiveShadows(bool state){
			this.receiveShadows = state;
		}

		/* */
		public bool getReceiveShadows(){
			return this.receiveShadows;
		}

		/* Schedule a draw call, 
			this is only useful if you disable this container's Update and want to manually draw its content.
			Instance can be set to "this" if you want to draw this container, or to another container entirely if
			you want to draw an instance of that other container.*/
		public void drawClayxels(ClayContainer instance){
			instance.renderBounds.center = instance.transform.position;
			
			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				this.drawChunk(chunkIt, instance);
			}
		}

		/*  If CLAYXELS_CACHEON is set to 1 inside claySDF.compute,
			 this bool will force containers to 1,1,1 boundsScale to prevent big memory usage.
			 When disabling this, try to not exceed ClayContainer.setMaxChunks(3,3,3).*/
		public static bool safeCacheMemoryLimit = true;

		/*  Is CLAYXELS_CACHEON set to 1 inside claySDF.compute?
			 Caching will enable the use of ClayContainer.cacheDistField() .*/
		public static bool isCacheEnabled(){
			if(ClayContainer.cacheEnabled > 0){
				return true;
			}

			return false;
		}

		/* If CLAYXELS_CACHEON is set to 1 in claySDF.compute, use this method to cache whatever clay you have visible at this moment in time.
			After caching you can disable all the ClayObjects that contributed to your results.
			Introducing new ClayObjects after caching will result in your sculpt to keep updating as if all your ClayObjects were 
			still live in the scene. */
		public void cacheClay(){
			this.updateFrame = 0;
			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];

				chunk.materialProperties.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);

				this.computeChunkCache(i);
			}
		}

		/* If CLAYXELS_CACHEON is set to 1 in claySDF.compute, use this method to clear the cahced clay.*/
		public void clearCachedClay(){
			int threads = 64 / ClayContainer.maxThreads;

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];

				uint indirectChunkId = sizeof(int) * ((uint)i * 3);

				ClayContainer.claycoreCompute.SetInt("chunkId", i);
				ClayContainer.claycoreCompute.SetVector("chunkCenter", chunk.center);

				ClayContainer.claycoreCompute.Dispatch((int)Kernels.clearCachedDistField, threads, threads, threads);
			}
		}

		/* Returns a mesh at the specified level of detail, clayxelDetail will range from 0 to 100.
			Useful to generate mesh colliders, to improve performance leave colorizeMesh and generateNormals to false.*/
		public Mesh generateMesh(int detail, bool colorizeMesh = false, bool computeNormals = false){
			this.switchComputeData();
			this.bindSolidsBuffers((int)Kernels.computeGridForMesh);

			int prevDetail = this.clayxelDetail;

			if(detail != this.clayxelDetail){
				this.setClayxelDetail(detail);
			}

			if(computeNormals){
				colorizeMesh = true;
			}
			
			if(ClayContainer.meshIndicesBuffer == null){
				ClayContainer.meshIndicesBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints*6, sizeof(int) * 3, ComputeBufferType.Counter);
				ClayContainer.globalCompBuffers.Add(ClayContainer.meshIndicesBuffer);
				
				ClayContainer.meshVertsBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints*6, sizeof(float) * 3);
				ClayContainer.globalCompBuffers.Add(ClayContainer.meshVertsBuffer);

				ClayContainer.meshColorsBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints*6, sizeof(float) * 4);
				ClayContainer.globalCompBuffers.Add(ClayContainer.meshColorsBuffer);
			}

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "meshOutIndices", ClayContainer.meshIndicesBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "meshOutPoints", ClayContainer.meshVertsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "meshOutColors", ClayContainer.meshColorsBuffer);

			List<Vector3> totalVertices = null;
			List<int> totalIndices = null;
			List<Color> totalColors = null;

			if(this.numChunks > 1){
				totalVertices = new List<Vector3>();
				totalIndices = new List<int>();

				if(colorizeMesh){
					totalColors = new List<Color>();
				}
			}

			int totalNumVerts = 0;

			Mesh mesh = new Mesh();
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			ClayContainer.claycoreCompute.SetInt("numSolids", this.solids.Count);
			ClayContainer.claycoreCompute.SetFloat("chunkSize", (float)this.chunkSize);

			this.forceUpdateAllSolids();
			this.computeClay();

			// special case for seems in meshed chunks
			float seamOffset = (this.chunkSize / 256.0f) * 3.0f;
			float chunkOffset = this.chunkSize - seamOffset;
			float gridCenterOffset = (this.chunkSize * 0.5f);

			this.userWarning = "";

			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				ClayxelChunk chunk = this.chunks[chunkIt];
				
				Vector3 chunkCenter = new Vector3(
					(-((this.chunkSize * this.chunksX) * 0.5f) + gridCenterOffset) + (chunkOffset * chunk.coords.x),
					(-((this.chunkSize * this.chunksY) * 0.5f) + gridCenterOffset) + (chunkOffset * chunk.coords.y),
					(-((this.chunkSize * this.chunksZ) * 0.5f) + gridCenterOffset) + (chunkOffset * chunk.coords.z));

				ClayContainer.meshIndicesBuffer.SetCounterValue(0);

				ClayContainer.claycoreCompute.SetInt("chunkId", chunkIt);

				ClayContainer.claycoreCompute.SetVector("chunkCenter", chunkCenter);

				ClayContainer.claycoreCompute.Dispatch((int)Kernels.computeGridForMesh, ClayContainer.numThreadsComputeStartRes, ClayContainer.numThreadsComputeStartRes, ClayContainer.numThreadsComputeStartRes);
				
				ClayContainer.claycoreCompute.SetInt("outMeshIndexOffset", totalNumVerts);
				ClayContainer.claycoreCompute.Dispatch((int)Kernels.computeMesh, ClayContainer.numThreadsComputeFullRes, ClayContainer.numThreadsComputeFullRes, ClayContainer.numThreadsComputeFullRes);

				int numTris = this.getBufferCount(ClayContainer.meshIndicesBuffer);
				int numVerts = numTris * 3;

				if(numVerts > ClayContainer.chunkMaxOutPoints * 6){
					this.userWarning = "max point count exceeded, increase limit from file ClayxelsPrefs.cs setPointCloudLimit";
					Debug.Log("Clayxels: container " + this.gameObject.name + " has exceeded the limit of points allowed, to increase the limit use ClayContainer.setPointCloudLimit() inside ClayxelsPrefs.cs");
					mesh = null;

					break;
				}

				totalNumVerts += numVerts;
				
				if(mesh != null){
					if(this.numChunks > 1){
						Vector3[] vertices = new Vector3[numVerts];
						ClayContainer.meshVertsBuffer.GetData(vertices);

						int[] indices = new int[numVerts];
						ClayContainer.meshIndicesBuffer.GetData(indices);

						totalVertices.AddRange(vertices);
						totalIndices.AddRange(indices);

						if(colorizeMesh){
							Color[] colors = new Color[numVerts];
							ClayContainer.meshColorsBuffer.GetData(colors);

							totalColors.AddRange(colors);
						}
					}
				}
			}

			if(mesh != null){
				if(this.numChunks > 1){
					mesh.vertices = totalVertices.ToArray();
					mesh.triangles = totalIndices.ToArray();

					if(colorizeMesh){
						mesh.colors = totalColors.ToArray();
					}
				}
				else{
					Vector3[] vertices = new Vector3[totalNumVerts];
					ClayContainer.meshVertsBuffer.GetData(vertices);

					mesh.vertices = vertices;

					int[] indices = new int[totalNumVerts];
					ClayContainer.meshIndicesBuffer.GetData(indices);

					mesh.triangles = indices;

					if(colorizeMesh){
						Color[] colors = new Color[totalNumVerts];
						meshColorsBuffer.GetData(colors);

						mesh.colors = colors;
					}
				}
			}

			if(prevDetail != this.clayxelDetail){
				this.setClayxelDetail(prevDetail);
			}

			return mesh;
		}

		/* Freeze this container to a mesh. Specify meshDetail from 0 to 100.*/
		public void freezeToMesh(int meshDetail){
			if(this.needsInit){
				this.init();
			}

			if(this.gameObject.GetComponent<MeshFilter>() == null){
				this.gameObject.AddComponent<MeshFilter>();
			}
			
			MeshRenderer render = this.gameObject.GetComponent<MeshRenderer>();
			if(render == null){
				render = this.gameObject.AddComponent<MeshRenderer>();
			}

			Material mat = render.sharedMaterial;
			if(mat == null){
				if(ClayContainer.renderPipe == "hdrp"){
					render.sharedMaterial = new Material(Shader.Find("Clayxels/ClayxelHDRPMeshShader"));
				}
				else if(ClayContainer.renderPipe == "urp"){
					render.sharedMaterial = new Material(Shader.Find("Clayxels/ClayxelURPMeshShader"));
				}
				else{
					render.sharedMaterial = new Material(Shader.Find("Clayxels/ClayxelBuiltInMeshShader"));
				}
			}
			
			if(meshDetail < 0){
				meshDetail = 0;
			}
			else if(meshDetail > 100){
				meshDetail = 100;
			}

			bool vertexColors = true;
			Mesh mesh = this.generateMesh(meshDetail, vertexColors);
			if(mesh == null){
				return;
			}

			this.frozen = true;
			this.enabled = false;

			MeshUtils.weldVertices(mesh);
			mesh.Optimize();
			mesh.RecalculateNormals();
			
			this.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;

			this.releaseBuffers();
		}

		/* Transfer every material attribue found with the same name from this container's material, to the generated mesh material. */
		public void transferMaterialPropertiesToMesh(){
			MeshRenderer render = this.gameObject.GetComponent<MeshRenderer>();
			if(render == null){
				return;
			}
			
			for(int propertyId = 0; propertyId < this.material.shader.GetPropertyCount(); ++propertyId){
				ShaderPropertyType type = this.material.shader.GetPropertyType(propertyId);
				string name = this.material.shader.GetPropertyName(propertyId);
				
				if(render.sharedMaterial.shader.FindPropertyIndex(name) != -1){
					if(type == ShaderPropertyType.Color || type == ShaderPropertyType.Vector){
						render.sharedMaterial.SetVector(name, this.material.GetVector(name));
					}
					else if(type == ShaderPropertyType.Float || type == ShaderPropertyType.Range){
						render.sharedMaterial.SetFloat(name, this.material.GetFloat(name));
					}
					else if(type == ShaderPropertyType.Texture){
						render.sharedMaterial.SetTexture(name, this.material.GetTexture(name));
					}
				}
			}
		}

		/* Is this container using a mesh filter to display a mesh? */
		public bool isFrozenToMesh(){
			if(this.gameObject.GetComponent<MeshFilter>() != null){
				return true;
			}

			return false;
		}

		public bool isFrozen(){
			return this.frozen;
		}

		/* Disable the frozen state and get back to live clayxels. */
		public void defrostToLiveClayxels(){
			this.frozen = false;
			this.needsInit = true;
			this.enabled = true;

			if(this.gameObject.GetComponent<MeshFilter>() != null){
				DestroyImmediate(this.gameObject.GetComponent<MeshFilter>());
			}

			Claymation claymation = this.gameObject.GetComponent<Claymation>();
			if(claymation != null){
				claymation.enabled = false;
			}
		}

		/* ClayContainers that have forceUpdate set to false will disable all their clayObjects when the game starts.
			Use this method to re-enable all clayObjects in a container while the game is running. */
		public void enableAllClayObjects(bool state){
			for(int i = 0; i < this.clayObjects.Count; ++i){
				this.clayObjects[i].gameObject.SetActive(state);
			}
		}

		/* Freeze this container plus all the containers that are part of this hierarchy. */
		public void freezeContainersHierarchyToMesh(){
			ClayContainer[] containers = this.GetComponentsInChildren<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				containers[i].freezeToMesh(this.clayxelDetail);
			}
		}

		/* Defrost this container plus all the containers that are part of this hierarchy. */
		public void defrostContainersHierarchy(){
			ClayContainer[] containers = this.GetComponentsInChildren<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				containers[i].defrostToLiveClayxels();
			}
		}

		/* Access the point cloud buffer that is about to be drawn.
			To correctly access the point cloud data you should refer to the function 
			clayxelVertNormalBlend inside clayxelSRPUtils.cginc .*/
		public List<ComputeBuffer> getPointCloudBuffers(){
			List<ComputeBuffer> pointCloudBuffers = new List<ComputeBuffer>();

			for(int i = 0; i < this.numChunks; ++i){
				ComputeBuffer buff = this.chunks[i].pointCloudDataBuffer;
				pointCloudBuffers.Add(buff);
			}

			return pointCloudBuffers;
		}

		// public members for internal use

		static public void reloadSolidsCatalogue(){
			ClayContainer.solidsCatalogueLabels.Clear();
			ClayContainer.solidsCatalogueParameters.Clear();

			int lastParsed = -1;
			try{
				string claySDF = ((TextAsset)Resources.Load("claySDF", typeof(TextAsset))).text;
				ClayContainer.parseSolidsAttrs(claySDF, ref lastParsed);

				string numThreadsDef = "MAXTHREADS";
				ClayContainer.maxThreads = (int)char.GetNumericValue(claySDF[claySDF.IndexOf(numThreadsDef) + numThreadsDef.Length + 1]);

				string cacheDef = "CLAYXELS_CACHEON";
				ClayContainer.cacheEnabled = (int)char.GetNumericValue(claySDF[claySDF.IndexOf(cacheDef) + cacheDef.Length + 1]);
			}
			catch{
				Debug.Log("error trying to parse parameters in claySDF.compute, solid #" + lastParsed);
			}
		}

		public string[] getSolidsCatalogueLabels(){
			return ClayContainer.solidsCatalogueLabels.ToArray();
		}

		
		public List<string[]> getSolidsCatalogueParameters(int solidId){
			return ClayContainer.solidsCatalogueParameters[solidId];
		}

		public bool isClayObjectsOrderLocked(){
			return this.clayObjectsOrderLocked;
		}

		public void setClayObjectsOrderLocked(bool state){
			this.clayObjectsOrderLocked = state;
		}

		public void reorderClayObject(int clayObjOrderId, int offset){
			List<ClayObject> tmpList = new List<ClayObject>(this.clayObjects.Count);
			for(int i = 0; i < this.clayObjects.Count; ++i){
				tmpList.Add(this.clayObjects[i]);
			}
			
			int newOrderId = clayObjOrderId + offset;
			if(newOrderId < 0){
				newOrderId = 0;
			}
			else if(newOrderId > this.clayObjects.Count - 1){
				newOrderId = this.clayObjects.Count - 1;
			}
			
			ClayObject clayObj1 = tmpList[clayObjOrderId];
			ClayObject clayObj2 = tmpList[newOrderId];

			tmpList.Remove(clayObj1);
			tmpList.Insert(newOrderId, clayObj1);

			clayObj1.clayObjectId = tmpList.IndexOf(clayObj1);
			clayObj2.clayObjectId = tmpList.IndexOf(clayObj2);

			if(this.clayObjectsOrderLocked){
				clayObj1.transform.SetSiblingIndex(tmpList.IndexOf(clayObj1));
			}
			
			this.scanClayObjectsHierarchy();
		}

		public bool checkVRam(){
			if(!ClayContainer.vramLimitEnabled){
				return true;
			}

			if(ClayContainer.totalChunksInScene <= 0){
				ClayContainer.totalChunksInScene = 0;
				ClayContainer[] clayxelObjs = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
				for(int i = 0; i < clayxelObjs.Length; ++i){
					ClayContainer container = clayxelObjs[i];
					if(container.enabled && container.instanceOf == null){
						ClayContainer.totalChunksInScene += container.numChunks;
					}
				}
			}

			int mbPerContainer = (64 * ClayContainer.chunkMaxOutPoints) / 8000000;// bit to mb
			int sceneContainersMb = mbPerContainer * ClayContainer.totalChunksInScene;
			int memoryLimit = SystemInfo.graphicsMemorySize - mbPerContainer;

			bool vramOk = true;
			if(sceneContainersMb > memoryLimit){
				vramOk = false;
			}

			return vramOk;
		}

		public string getUserWarning(){
			return this.userWarning;
		}

		// end of public interface //

		static void parseSolidsAttrs(string content, ref int lastParsed){
			string[] lines = content.Split(new[]{ "\r\n", "\r", "\n" }, StringSplitOptions.None);
			for(int i = 0; i < lines.Length; ++i){
				string line = lines[i];
				if(line.Contains("label: ")){
					if(line.Split('/').Length == 3){// if too many comment slashes, it's a commented out solid,
						lastParsed += 1;

						string[] parameters = line.Split(new[]{"label:"}, StringSplitOptions.None)[1].Split(',');
						string label = parameters[0].Trim();
						
						ClayContainer.solidsCatalogueLabels.Add(label);

						List<string[]> paramList = new List<string[]>();

						for(int paramIt = 1; paramIt < parameters.Length; ++paramIt){
							string param = parameters[paramIt];
							string[] attrs = param.Split(':');
							string paramId = attrs[0];
							string[] paramLabelValue = attrs[1].Split(' ');
							string paramLabel = paramLabelValue[1];
							string paramValue = paramLabelValue[2];

							paramList.Add(new string[]{paramId.Trim(), paramLabel.Trim(), paramValue.Trim()});
						}

						ClayContainer.solidsCatalogueParameters.Add(paramList);
					}
				}
			}
		}

		void initSolidsData(){
			this.solidsPosBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			this.compBuffers.Add(this.solidsPosBuffer);
			this.solidsRotBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 4);
			this.compBuffers.Add(this.solidsRotBuffer);
			this.solidsScaleBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			this.compBuffers.Add(this.solidsScaleBuffer);
			this.solidsBlendBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float));
			this.compBuffers.Add(this.solidsBlendBuffer);
			this.solidsTypeBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			this.compBuffers.Add(this.solidsTypeBuffer);
			this.solidsColorBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			this.compBuffers.Add(this.solidsColorBuffer);
			this.solidsAttrsBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 4);
			this.compBuffers.Add(this.solidsAttrsBuffer);
			this.solidsClayObjectIdBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			this.compBuffers.Add(this.solidsClayObjectIdBuffer);

			this.solidsPos = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			this.solidsRot = new List<Quaternion>(new Quaternion[ClayContainer.maxSolids]);
			this.solidsScale = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			this.solidsBlend = new List<float>(new float[ClayContainer.maxSolids]);
			this.solidsType = new List<int>(new int[ClayContainer.maxSolids]);
			this.solidsColor = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			this.solidsAttrs = new List<Vector4>(new Vector4[ClayContainer.maxSolids]);
			this.solidsClayObjectId = new List<int>(new int[ClayContainer.maxSolids]);
		}

		void OnDestroy(){
			ClayContainer.totalChunksInScene = 0;

			this.releaseBuffers();

			if(UnityEngine.Object.FindObjectsOfType<ClayContainer>().Length == 0){
				ClayContainer.releaseGlobalBuffers();
			}

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					this.removeEditorEvents();
				}
			#endif
		}

		void releaseBuffers(){
			for(int i = 0; i < this.compBuffers.Count; ++i){
				this.compBuffers[i].Release();
			}

			this.compBuffers.Clear();
		}

		static void releaseGlobalBuffers(){
			for(int i = 0; i < ClayContainer.globalCompBuffers.Count; ++i){
				ClayContainer.globalCompBuffers[i].Release();
			}

			ClayContainer.globalCompBuffers.Clear();

			ClayContainer.globalDataNeedsInit = true;
		}

		void limitChunkValues(){
			if(this.chunksX > ClayContainer.maxChunkX){
				this.chunksX = ClayContainer.maxChunkX;
			}
			if(this.chunksY > ClayContainer.maxChunkY){
				this.chunksY = ClayContainer.maxChunkY;
			}
			if(this.chunksZ > ClayContainer.maxChunkZ){
				this.chunksZ = ClayContainer.maxChunkZ;
			}
			if(this.chunksX < 1){
				this.chunksX = 1;
			}
			if(this.chunksY < 1){
				this.chunksY = 1;
			}
			if(this.chunksZ < 1){
				this.chunksZ = 1;
			}

			if(this.chunkSize < 4){
				this.chunkSize = 4;
			}
			else if(this.chunkSize > 255){
				this.chunkSize = 255;
			}
		}

		void initChunks(){
			this.numChunks = 0;
			this.chunks.Clear();

			if(this.autoBounds){
				this.chunksX = ClayContainer.maxChunkX;
				this.chunksY = ClayContainer.maxChunkY;
				this.chunksZ = ClayContainer.maxChunkZ;
			}

			this.boundsScale.x = (float)this.chunkSize * this.chunksX;
			this.boundsScale.y = (float)this.chunkSize * this.chunksY;
			this.boundsScale.z = (float)this.chunkSize * this.chunksZ;
			this.renderBounds.size = this.boundsScale * this.transform.lossyScale.x;

			float gridCenterOffset = (this.chunkSize * 0.5f);
			this.boundsCenter.x = ((this.chunkSize * (this.chunksX - 1)) * 0.5f) - (gridCenterOffset*(this.chunksX-1));
			this.boundsCenter.y = ((this.chunkSize * (this.chunksY - 1)) * 0.5f) - (gridCenterOffset*(this.chunksY-1));
			this.boundsCenter.z = ((this.chunkSize * (this.chunksZ - 1)) * 0.5f) - (gridCenterOffset*(this.chunksZ-1));

			for(int z = 0; z < this.chunksZ; ++z){
				for(int y = 0; y < this.chunksY; ++y){
					for(int x = 0; x < this.chunksX; ++x){
						this.initNewChunk(x, y, z);
						this.numChunks += 1;
					}
				}
			}

			this.updateChunksBuffer = new ComputeBuffer(this.numChunks, sizeof(int));
			this.compBuffers.Add(this.updateChunksBuffer);

			this.indirectChunkArgs1Buffer = new ComputeBuffer(this.numChunks * 3, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectChunkArgs1Buffer);

			this.indirectChunkArgs2Buffer = new ComputeBuffer(this.numChunks * 3, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectChunkArgs2Buffer);

			int[] indirectChunk1 = new int[this.numChunks * 3];
			int[] indirectChunk2 = new int[this.numChunks * 3];

			int indirectChunkSize1 = 64 / ClayContainer.maxThreads;
			int indirectChunkSize2 = 256 / ClayContainer.maxThreads;
			
			int[] updateChunks = new int[this.numChunks];

			for(int i = 0; i < this.numChunks; ++i){
				int indirectChunkId = i * 3;
				indirectChunk1[indirectChunkId] = indirectChunkSize1;
				indirectChunk1[indirectChunkId + 1] = indirectChunkSize1;
				indirectChunk1[indirectChunkId + 2] = indirectChunkSize1;

				indirectChunk2[indirectChunkId] = indirectChunkSize2;
				indirectChunk2[indirectChunkId + 1] = indirectChunkSize2;
				indirectChunk2[indirectChunkId + 2] = indirectChunkSize2;

				updateChunks[i] = 1;
			}

			this.updateChunksBuffer.SetData(updateChunks);
			this.indirectChunkArgs1Buffer.SetData(indirectChunk1);
			this.indirectChunkArgs2Buffer.SetData(indirectChunk2);
		}

		void initNewChunk(int x, int y, int z){
			ClayxelChunk chunk = new ClayxelChunk();
			this.chunks.Add(chunk);

			float seamOffset = (this.chunkSize / 256.0f); // removes the seam between chunks
			float chunkOffset = this.chunkSize - seamOffset;
			float gridCenterOffset = (this.chunkSize * 0.5f);
			chunk.coords = new Vector3Int(x, y, z);
			chunk.center = new Vector3(
				(-((this.chunkSize * this.chunksX) * 0.5f) + gridCenterOffset) + (chunkOffset * x),
				(-((this.chunkSize * this.chunksY) * 0.5f) + gridCenterOffset) + (chunkOffset * y),
				(-((this.chunkSize * this.chunksZ) * 0.5f) + gridCenterOffset) + (chunkOffset * z));

			chunk.pointCloudDataBuffer = new ComputeBuffer(ClayContainer.chunkMaxOutPoints, sizeof(int) * 2);
			this.compBuffers.Add(chunk.pointCloudDataBuffer);

			chunk.indirectDrawArgsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(chunk.indirectDrawArgsBuffer);

			chunk.indirectDrawArgsBuffer.SetData(new int[]{0, 2, 0, 0});

			chunk.materialProperties = new MaterialPropertyBlock();

			chunk.materialProperties.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
			chunk.materialProperties.SetVector("chunkCenter",  chunk.center);
		}

		void initMaterialProperties(){
			bool isPrefab = false;

			#if UNITY_EDITOR
				isPrefab = PrefabUtility.IsPartOfAnyPrefab(this.gameObject);
			#endif
			
			if(this.customMaterial != null){
				if(isPrefab){// use shared material
					this.material = this.customMaterial;
				}
				else{
					bool createNewMaterialInstance = true;
					if(this.material != null){
						if(this.material.shader.name == this.customMaterial.shader.name){
							// if a modified version of this customMaterial was already in use, don't destroy it
							createNewMaterialInstance = false;
						}
					}

					if(createNewMaterialInstance){
						this.material = new Material(this.customMaterial);
					}
				}
			}
			else{
				if(isPrefab){
					Debug.Log("Clayxels prefab " + this.gameObject.name + " needs a shared customMaterial or prefab will revert to a default material.");
				}

				if(this.material != null){// validate pipeline shader
					if(this.material.shader.name.StartsWith("Clayxels/Clayxel")){
						if(ClayContainer.renderPipe == "hdrp" && this.material.shader.name != "Clayxels/ClayxelHDRPShader"){
							this.material = null;
						}
						else if(ClayContainer.renderPipe == "urp" && this.material.shader.name != "Clayxels/ClayxelURPShader"){
							this.material = null;
						}
						else if(ClayContainer.renderPipe == "builtin" && this.material.shader.name != "Clayxels/ClayxelBuiltInShader"){
							this.material = null;
						}
					}
				}

				if(this.material != null){
					// if material is still not null, means it's a valid shader,
					// probably this container got duplicated in scene
					this.material = new Material(this.material);
				}
				else{
					// brand new container, lets create a new material
					if(ClayContainer.renderPipe == "hdrp"){
						this.material = new Material(Shader.Find("Clayxels/ClayxelHDRPShader"));
					}
					else if(ClayContainer.renderPipe == "urp"){
						this.material = new Material(Shader.Find("Clayxels/ClayxelURPShader"));
					}
					else{
						this.material = new Material(Shader.Find("Clayxels/ClayxelBuiltInShader"));
					}
				}
			}

			if(this.customMaterial == null){
				// set the default clayxel texture to a dot on the standard material
				Texture texture = this.material.GetTexture("_MainTex");
				if(texture == null){
					this.material.SetTexture("_MainTex", (Texture)Resources.Load("clayxelDot"));
				}
			}
			
			this.material.SetFloat("chunkSize", (float)this.chunkSize);

			for(int i = 0; i < this.numChunks; ++i){
				ClayxelChunk chunk = this.chunks[i];
				
				chunk.materialProperties.SetInt("solidHighlightId", -1);
				chunk.materialProperties.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
				chunk.materialProperties.SetVector("chunkCenter",  chunk.center);
			}
		}

		void scanRecursive(Transform trn, List<ClayObject> collectedClayObjs){
			ClayObject clayObj = trn.gameObject.GetComponent<ClayObject>();
			if(clayObj != null){
				if(clayObj.isValid() && trn.gameObject.activeSelf){
					if(this.clayObjectsOrderLocked){
						clayObj.clayObjectId = collectedClayObjs.Count;
						collectedClayObjs.Add(clayObj);
					}
					else{
						int id = clayObj.clayObjectId;
						if(id < 0){
							id = 0;
						}

						if(id > collectedClayObjs.Count - 1){
							collectedClayObjs.Add(clayObj);
						}
						else{
							collectedClayObjs.Insert(id, clayObj);
						}
					}
				}
			}

			for(int i = 0; i < trn.childCount; ++i){
				GameObject childObj = trn.GetChild(i).gameObject;
				if(childObj.activeSelf && childObj.GetComponent<ClayContainer>() == null){
					this.scanRecursive(childObj.transform, collectedClayObjs);
				}
			}
		}

		void collectClayObject(ClayObject clayObj){
			if(clayObj.getNumSolids() == 0){
				clayObj.init();
			}

			clayObj.clayObjectId = this.clayObjects.Count;
			this.clayObjects.Add(clayObj);

			for(int i = 0; i < clayObj.getNumSolids(); ++i){
				Solid solid = clayObj.getSolid(i);
				solid.id = this.solids.Count;
				solid.clayObjectId = clayObj.clayObjectId;
				this.solids.Add(solid);

				if(solid.id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[solid.id] = 1;
				}
				else{
					break;
				}
			}

			clayObj.transform.hasChanged = true;
			clayObj.setClayxelContainer(this);
		}

		int getBufferCount(ComputeBuffer buffer){
			ComputeBuffer.CopyCount(buffer, this.genericNumberBuffer, 0);
			this.genericNumberBuffer.GetData(this.genericIntBufferArray);
			int count = this.genericIntBufferArray[0];

			return count;
		}

		void computeChunk(int chunkId){
			ClayxelChunk chunk = this.chunks[chunkId];

			uint indirectChunkId = sizeof(int) * ((uint)chunkId * 3);

			ClayContainer.claycoreCompute.SetInt("chunkId", chunkId);
			ClayContainer.claycoreCompute.SetVector("chunkCenter", chunk.center);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "indirectDrawArgs", chunk.indirectDrawArgsBuffer);
			ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.computeGrid, this.indirectChunkArgs1Buffer, indirectChunkId);

			// generate point cloud
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "indirectDrawArgs", chunk.indirectDrawArgsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "pointCloudData", chunk.pointCloudDataBuffer);
			ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.generatePointCloud, this.indirectChunkArgs2Buffer, indirectChunkId);
		}

		void computeChunkCache(int chunkId){
			ClayxelChunk chunk = this.chunks[chunkId];

			uint indirectChunkId = sizeof(int) * ((uint)chunkId * 3);

			ClayContainer.claycoreCompute.SetInt("chunkId", chunkId);
			ClayContainer.claycoreCompute.SetVector("chunkCenter", chunk.center);

			ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.cacheDistField, this.indirectChunkArgs1Buffer, indirectChunkId);
		}

		void updateSolids(){
			int solidCount = this.solids.Count;
			if(solidCount > ClayContainer.maxSolids){
				solidCount = ClayContainer.maxSolids;
			}

			foreach(int i in this.solidsUpdatedDict.Keys){
				Solid solid = this.solids[i];

				int clayObjId = solid.clayObjectId;
				if(solid.clayObjectId > -1){
					ClayObject clayObj = this.clayObjects[solid.clayObjectId];
					clayObj.pullUpdate();
				}
				else{
					clayObjId = 0;
				}

				this.solidsPos[i] = solid.position;
				this.solidsRot[i] = solid.rotation;
				this.solidsScale[i] = solid.scale;
				this.solidsBlend[i] = solid.blend;
				this.solidsType[i] = solid.primitiveType;
				this.solidsColor[i] = solid.color;
				this.solidsAttrs[i] = solid.attrs;
				this.solidsClayObjectId[i] = clayObjId;
			}

			if(this.solids.Count > 0){
				this.solidsPosBuffer.SetData(this.solidsPos);
				this.solidsRotBuffer.SetData(this.solidsRot);
				this.solidsScaleBuffer.SetData(this.solidsScale);
				this.solidsBlendBuffer.SetData(this.solidsBlend);
				this.solidsTypeBuffer.SetData(this.solidsType);
				this.solidsColorBuffer.SetData(this.solidsColor);
				this.solidsAttrsBuffer.SetData(this.solidsAttrs);
				this.solidsClayObjectIdBuffer.SetData(this.solidsClayObjectId);
			}

			ClayContainer.claycoreCompute.SetInt("numSolids", this.solids.Count);
			ClayContainer.claycoreCompute.SetFloat("chunkSize", (float)this.chunkSize);

			if(this.numChunks > 1 || this.autoBounds){
				ClayContainer.claycoreCompute.SetInt("numSolidsUpdated", this.solidsUpdatedDict.Count);
				ClayContainer.solidsUpdatedBuffer.SetData(this.solidsUpdatedDict.Keys.ToArray());
				
				ClayContainer.claycoreCompute.Dispatch((int)Kernels.filterSolidsPerChunk, this.chunksX, this.chunksY, this.chunksZ);
			}

			this.solidsUpdatedDict.Clear();
		}

		float _debugAutoBounds = 0.0f;
		void updateAutoBounds(float boundsSize){
			this._debugAutoBounds = boundsSize;

			int newChunkSize = Mathf.CeilToInt(boundsSize / ClayContainer.maxChunkX);

			int detailChunkSize = (int)Mathf.Lerp(40.0f, 4.0f, (float)this.clayxelDetail / 100.0f);

			int estimatedNumChunks = Mathf.CeilToInt(boundsSize / detailChunkSize);

			if(estimatedNumChunks < 1){
				estimatedNumChunks = 1;
			}
			else if(estimatedNumChunks > ClayContainer.maxChunkX){
				estimatedNumChunks = ClayContainer.maxChunkX;
			}

			this.autoFrameSkip = estimatedNumChunks;
			
			if(estimatedNumChunks != this.chunksX){
				this.autoBoundsChunkSize = newChunkSize;

				this.resizeChunks(estimatedNumChunks);
				this.updateInternalBounds();
				this.forceUpdateAllSolids();
			}
			else if(newChunkSize != this.autoBoundsChunkSize){
				this.autoBoundsChunkSize = newChunkSize;

				if(this.autoBoundsChunkSize > detailChunkSize){
					this.updateInternalBounds();
					this.forceUpdateAllSolids();
				}
			}
		}

		void resizeChunks(int estimatedNumChunks){
			this.chunksX = estimatedNumChunks;
			this.chunksY = estimatedNumChunks;
			this.chunksZ = estimatedNumChunks;
			
			if(this.autoBoundsChunkSize > this.chunkSize){
				this.chunkSize = this.autoBoundsChunkSize;
			}

			this.boundsScale.x = (float)this.chunkSize * this.chunksX;
			this.boundsScale.y = (float)this.chunkSize * this.chunksY;
			this.boundsScale.z = (float)this.chunkSize * this.chunksZ;
			this.renderBounds.size = this.boundsScale * this.transform.lossyScale.x;

			float gridCenterOffset = (this.chunkSize * 0.5f);
			this.boundsCenter.x = ((this.chunkSize * (this.chunksX - 1)) * 0.5f) - (gridCenterOffset*(this.chunksX-1));
			this.boundsCenter.y = ((this.chunkSize * (this.chunksY - 1)) * 0.5f) - (gridCenterOffset*(this.chunksY-1));
			this.boundsCenter.z = ((this.chunkSize * (this.chunksZ - 1)) * 0.5f) - (gridCenterOffset*(this.chunksZ-1));

			float seamOffset = (this.chunkSize / 256.0f);
			float chunkOffset = this.chunkSize - seamOffset;

			ClayContainer.indirectArgsData = new int[]{0, 2, 0, 0};
			
			int chunkIt = 0;
			for(int z = 0; z < this.chunksZ; ++z){
				for(int y = 0; y < this.chunksY; ++y){
					for(int x = 0; x < this.chunksX; ++x){
						ClayxelChunk chunk = this.chunks[chunkIt];

						chunk.indirectDrawArgsBuffer.SetData(ClayContainer.indirectArgsData);
						
						chunk.coords = new Vector3Int(x, y, z);
						chunk.center = new Vector3(
							(-((this.chunkSize * this.chunksX) * 0.5f) + gridCenterOffset) + (chunkOffset * x),
							(-((this.chunkSize * this.chunksY) * 0.5f) + gridCenterOffset) + (chunkOffset * y),
							(-((this.chunkSize * this.chunksZ) * 0.5f) + gridCenterOffset) + (chunkOffset * z));

						chunkIt += 1;
					}
				}
			}

			this.numChunks = chunkIt;

			ClayContainer.claycoreCompute.SetInt("numChunksX", this.chunksX);
			ClayContainer.claycoreCompute.SetInt("numChunksY", this.chunksY);
			ClayContainer.claycoreCompute.SetInt("numChunksZ", this.chunksZ);
		}

		void initAutoBounds(){
			float boundsSize = this.computeAutoBounds();
			this.updateAutoBounds(boundsSize);
			this.updateInternalBounds();
		}

		float computeAutoBounds(){
			Vector3 autoBoundsScale = Vector3.zero;

			int solidCount = this.solids.Count;
			if(solidCount > ClayContainer.maxSolids){
				solidCount = ClayContainer.maxSolids;
			}

			for(int i = 0; i < solidCount; ++i){
				Solid solid = this.solids[i];

				if(solid.clayObjectId > -1){
					ClayObject clayObj = this.clayObjects[solid.clayObjectId];
					clayObj.pullUpdate();
				}
				
				float boundingSphere = Mathf.Max(solid.scale.x, Mathf.Max(solid.scale.y, solid.scale.z)) * 1.732f;
				float autoBoundsPosX = Mathf.Abs(solid.position.x * 2.0f) + boundingSphere;
				float autoBoundsPosY = Mathf.Abs(solid.position.y * 2.0f) + boundingSphere;
				float autoBoundsPosZ = Mathf.Abs(solid.position.z * 2.0f) + boundingSphere;

				if(autoBoundsPosX > autoBoundsScale.x){
					autoBoundsScale.x = autoBoundsPosX;
				}
				if(autoBoundsPosY > autoBoundsScale.y){
					autoBoundsScale.y = autoBoundsPosY;
				}
				if(autoBoundsPosZ > autoBoundsScale.z){
					autoBoundsScale.z = autoBoundsPosZ;
				}
			}

			float autoBoundsChunkSize = Mathf.Max(autoBoundsScale.x, Mathf.Max(autoBoundsScale.y, autoBoundsScale.z));

			return autoBoundsChunkSize;
		}

		void logFPS(){
			this.deltaTime += (Time.unscaledDeltaTime - this.deltaTime) * 0.1f;
			float fps = 1.0f / this.deltaTime;
			Debug.Log(fps);
		}

		void switchComputeData(){
			int id = this.GetInstanceID();
			if(ClayContainer.lastUpdatedContainerId == id){
				return;
			}

			ClayContainer.lastUpdatedContainerId = id;
			
			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);

			ClayContainer.claycoreCompute.SetInt("numChunksX", this.chunksX);
			ClayContainer.claycoreCompute.SetInt("numChunksY", this.chunksY);
			ClayContainer.claycoreCompute.SetInt("numChunksZ", this.chunksZ);

			this.bindSolidsBuffers((int)Kernels.computeGrid);
			this.bindSolidsBuffers((int)Kernels.generatePointCloud);

			if(ClayContainer.cacheEnabled == 1){
				this.bindSolidsBuffers((int)Kernels.cacheDistField);
			}

			if(this.numChunks == 1 && !this.autoBounds){
				this.genericIntBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.genericIntBufferArray);

				ClayContainer.solidsPerChunkBuffer.SetData(ClayContainer.solidsInSingleChunkArray);
			}
			else{
				this.bindSolidsBuffers((int)Kernels.filterSolidsPerChunk);

				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "updateChunks", this.updateChunksBuffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "indirectChunkArgs1", this.indirectChunkArgs1Buffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "indirectChunkArgs2", this.indirectChunkArgs2Buffer);
			}
		}

		void bindSolidsBuffers(int kernId){
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsPos", this.solidsPosBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsRot", this.solidsRotBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsScale", this.solidsScaleBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsBlend", this.solidsBlendBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsType", this.solidsTypeBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsColor", this.solidsColorBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsAttrs", this.solidsAttrsBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsClayObjectId", this.solidsClayObjectIdBuffer);
		}

		void drawChunk(int chunkId, ClayContainer instance){
			ClayxelChunk chunk = this.chunks[chunkId];

			chunk.materialProperties.SetMatrix("objectMatrix", instance.transform.localToWorldMatrix);
			chunk.materialProperties.SetFloat("splatRadius", instance.splatRadius);
			chunk.materialProperties.SetFloat("chunkSize", (float)this.chunkSize);

			if(this.autoBounds){
				chunk.materialProperties.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
				chunk.materialProperties.SetVector("chunkCenter",  chunk.center);
			}

			#if UNITY_EDITOR
				// update some properties of the material only while in editor to avoid disappearing clayxels on certain editor events
				if(!Application.isPlaying){
					this.updateMaterialInEditor(chunkId, chunk, instance);
				}
			#endif

			Graphics.DrawProceduralIndirect(this.material, 
				instance.renderBounds,
				MeshTopology.Triangles, chunk.indirectDrawArgsBuffer, 0,
				null, chunk.materialProperties,
				this.castShadows, this.receiveShadows, this.gameObject.layer);
		}

		void Start(){
			if(this.needsInit){
				this.init();
			}
		}

		bool checkNeedsInit(){
			// we need to perform these checks because prefabs will reset some of these attributes upon instancing
			if(this.needsInit || this.numChunks == 0 || this.material == null){
				return true;
			}

			return false;
		}

		void Update(){
			if(this.instanceOf != null){
				if(this.instanceOf.frozen){
					this.enabled = false;
					
					return;
				}

				this.renderBounds = instanceOf.renderBounds;
				this.splatRadius = this.instanceOf.voxelSize * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);
				this.instanceOf.drawClayxels(this);

				return;
			}

			if(this.checkNeedsInit()){
				this.init();
				this.updateFrame = 0;
			}
			else{
				// inhibit updates if this transform is the trigger
				if(this.transform.hasChanged){
					this.needsUpdate = false;
					this.transform.hasChanged = false;

					// if this transform moved and also one of the solids moved, then we still need to update
					if(this.forceUpdate){
						this.needsUpdate = true;
					}
				}
			}
			
			if(this.needsUpdate && this.updateFrame == 0){
				this.computeClay();
			}
			
			if(ClayContainer.updateFrameSkip > 0){
				this.updateFrame = (this.updateFrame + 1) % (ClayContainer.updateFrameSkip + this.autoFrameSkip);
			}
			
			this.splatRadius = this.voxelSize * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);
			
			this.drawClayxels(this);
		}

		static void setupDistFieldCache(){
			uint encodeR6G6B6A14(float r, float g, float b, float a){
				uint ri = (uint)(r * 63.0f);
				uint gi = (uint)(g * 63.0f);
				uint bi = (uint)(b * 63.0f);
				uint ai = (uint)(((a + 1.0f) * 0.5f) * 16383.0f);
				
			 	uint rgba = (((ri<<6|gi)<<6|bi)<<14)|ai;

			 	return rgba;
			}

			if(ClayContainer.safeCacheMemoryLimit){
				if(ClayContainer.totalMaxChunks > 1){
					// prevent video memory from becoming huge 
					ClayContainer.maxChunkX = 1;
					ClayContainer.maxChunkY = 1;
					ClayContainer.maxChunkZ = 1;
					ClayContainer.totalMaxChunks = 1;
				}
			}

			int cacheSize1 = 64 * 64 * 64;
			int cacheSize2 = 256 * 256 * 256;
			
			ClayContainer.fieldCache1Buffer = new ComputeBuffer(cacheSize1 * ClayContainer.totalMaxChunks, sizeof(float));
			ClayContainer.globalCompBuffers.Add(ClayContainer.fieldCache1Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "fieldCache1", ClayContainer.fieldCache1Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "fieldCache1", ClayContainer.fieldCache1Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.clearCachedDistField, "fieldCache1", ClayContainer.fieldCache1Buffer);

			ClayContainer.fieldCache2Buffer = new ComputeBuffer(cacheSize2 * ClayContainer.totalMaxChunks, sizeof(uint));
			ClayContainer.globalCompBuffers.Add(ClayContainer.fieldCache2Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "fieldCache2", ClayContainer.fieldCache2Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.cacheDistField, "fieldCache2", ClayContainer.fieldCache2Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.clearCachedDistField, "fieldCache2", ClayContainer.fieldCache2Buffer);

			float[] cache1 = new float[cacheSize1 * ClayContainer.totalMaxChunks];
			uint[] cache2 = new uint[cacheSize2 * ClayContainer.totalMaxChunks];
			for(int i = 0; i < cacheSize1 * ClayContainer.totalMaxChunks; ++i){
				cache1[i] = 1.0f;
			}

			uint defaultValue = encodeR6G6B6A14(1.0f, 1.0f, 1.0f, 1.0f);
			for(int i = 0; i < cacheSize2 * ClayContainer.totalMaxChunks; ++i){
				cache2[i] = defaultValue;
			}

			ClayContainer.fieldCache1Buffer.SetData(cache1);
			ClayContainer.fieldCache2Buffer.SetData(cache2);

			ClayContainer.claycoreCompute.SetInt("fieldCacheSize1", cacheSize1);
			ClayContainer.claycoreCompute.SetInt("fieldCacheSize2", cacheSize2);
		}

		void expandMemory(){
			if(!this.memoryOptimized){
				return;
			}

			this.memoryOptimized = false;

			for(int i = 0; i < this.chunks.Count; ++i){
				ClayxelChunk chunk = this.chunks[i];

				chunk.pointCloudDataBuffer.Release();
				this.compBuffers.Remove(chunk.pointCloudDataBuffer);

				chunk.indirectDrawArgsBuffer.Release();
				this.compBuffers.Remove(chunk.indirectDrawArgsBuffer);
			}

			this.chunks.Clear();
			this.numChunks = 0;

			if(this.autoBounds){
				this.chunksX = ClayContainer.maxChunkX;
				this.chunksY = this.chunksX;
				this.chunksZ = this.chunksX;
			}

			for(int z = 0; z < this.chunksZ; ++z){
				for(int y = 0; y < this.chunksY; ++y){
					for(int x = 0; x < this.chunksX; ++x){
						this.initNewChunk(x, y, z);
						this.numChunks += 1;
					}
				}
			}
			
			this.updateFrame = 0;
			
			this.autoFrameSkip = 0;
			this.autoBoundsChunkSize = 0;
			if(this.autoBounds){
				this.initAutoBounds();
			}
		}

		// All functions past this point are used only in editor
		#if UNITY_EDITOR
		void Awake(){
			if(!Application.isPlaying){
				// this is needed to trigger a re-init after playing in editor
				ClayContainer.globalDataNeedsInit = true;
				this.needsInit = true;
			}
		}

		public void autoRenameClayObject(ClayObject clayObj){
			 List<string> solidsLabels = ClayContainer.solidsCatalogueLabels;

			string blendSign = "+";
			if(clayObj.blend < 0.0f){
				blendSign = "-";
			}

			string isColoring = "";
			if(clayObj.attrs.w == 1.0f){
				blendSign = "";
				isColoring = "[paint]";
			}

			clayObj.gameObject.name = "clay_" + solidsLabels[clayObj.primitiveType] + blendSign + isColoring;
		}

		public static void shortcutMirrorDuplicate(){
			for(int i = 0; i < UnityEditor.Selection.gameObjects.Length; ++i){
				GameObject gameObj = UnityEditor.Selection.gameObjects[i];
				if(gameObj.GetComponent<ClayObject>()){
					gameObj.GetComponent<ClayObject>().mirrorDuplicate();
				}
			}
		}

		static void shortcutAddClay(){
			if(UnityEditor.Selection.gameObjects.Length > 0){
				if(UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>()){
					ClayContainer container = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>().getClayContainer();
					ClayObject clayObj = container.addClayObject();
					UnityEditor.Selection.objects = new GameObject[]{clayObj.gameObject};
				}
				else if(UnityEditor.Selection.gameObjects[0].GetComponent<ClayContainer>() != null){
					ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayContainer>().addClayObject();
					UnityEditor.Selection.objects = new GameObject[]{clayObj.gameObject};
				}
			}
		}

		public static float getEditorUIScale(){
			PropertyInfo p =
				typeof(GUIUtility).GetProperty("pixelsPerPoint", BindingFlags.Static | BindingFlags.NonPublic);

			float editorUiScaling = 1.0f;
			if(p != null){
				editorUiScaling = (float)p.GetValue(null, null);
			}

			return editorUiScaling;
		}

		void updateMaterialInEditor(int chunkId, ClayxelChunk chunk, ClayContainer instance){
			if(instance.pickingThis && ClayContainer.pickedObj == null){
				chunk.materialProperties.SetBuffer("pointCloudDataToSolidId", ClayContainer.pointCloudDataToSolidIdBuffer);
				chunk.materialProperties.SetInt("chunkId", chunkId);
				chunk.materialProperties.SetInt("chunkMaxOutPoints", ClayContainer.chunkMaxOutPoints);

				if(!instance.editingThisContainer){
					chunk.materialProperties.SetInt("solidHighlightId", -2);
				}
				else{
					chunk.materialProperties.SetInt("solidHighlightId", ClayContainer.pickedClayObjectId);
				}
			}
			else{
				chunk.materialProperties.SetInt("solidHighlightId", -1);
			}
			
			if(!this.autoBounds){
				chunk.materialProperties.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
				chunk.materialProperties.SetVector("chunkCenter",  chunk.center);
			}
		}

		[MenuItem("GameObject/3D Object/Clayxel Container" )]
		public static ClayContainer createNewContainer(){
			 GameObject newObj = new GameObject("ClayxelContainer");
			 ClayContainer newClayContainer = newObj.AddComponent<ClayContainer>();

			 UnityEditor.Selection.objects = new GameObject[]{newObj};

			 return newClayContainer;
		}

		void OnValidate(){
			// called on a few (annoying) occasions, like every time any script recompiles
			this.needsInit = true;
			this.numChunks = 0;
		}

		void removeEditorEvents(){
			AssemblyReloadEvents.beforeAssemblyReload -= this.onBeforeAssemblyReload;

			EditorApplication.hierarchyChanged -= this.onHierarchyChanged;

			UnityEditor.Selection.selectionChanged -= this.onSelectionChanged;

			Undo.undoRedoPerformed -= this.onUndoPerformed;
		}

		void reinstallEditorEvents(){
			this.removeEditorEvents();

			AssemblyReloadEvents.beforeAssemblyReload += this.onBeforeAssemblyReload;

			EditorApplication.hierarchyChanged += this.onHierarchyChanged;

			UnityEditor.Selection.selectionChanged += this.onSelectionChanged;

			Undo.undoRedoPerformed += this.onUndoPerformed;
		}

		void onBeforeAssemblyReload(){
			// called when this script recompiles

			if(Application.isPlaying){
				return;
			}

			this.releaseBuffers();
			ClayContainer.releaseGlobalBuffers();

			ClayContainer.globalDataNeedsInit = true;
			this.needsInit = true;
		}

		void onUndoPerformed(){
			this.updateFrame = 0;

			if(Undo.GetCurrentGroupName() == "changed clayobject" ||
				Undo.GetCurrentGroupName() == "changed clayxel container"){
				this.needsUpdate = true;
			}
			else if(Undo.GetCurrentGroupName() == "changed clayxel grid"){
				this.init();
			}
			else if(Undo.GetCurrentGroupName() == "added clayxel solid"){
				this.needsUpdate = true;
			}
			else if(Undo.GetCurrentGroupName() == "Selection Change"){
				if(!UnityEditor.Selection.Contains(this.gameObject)){
					if(UnityEditor.Selection.gameObjects.Length > 0){
						ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
						if(clayObj != null){
							if(clayObj.getClayContainer() == this){
								this.needsUpdate = true;
							}
						}
					}
				}
			}

			// if(this.editingThisContainer){
			// 	if(this.autoBounds){
			// 		this.forceUpdateAllSolids();
			// 		this.computeClay();
			// 		this.needsUpdate = true;
			// 	}
			// }
			
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			ClayContainer.getSceneView().Repaint();
		}

		public static bool _skipHierarchyChanges = false;
		void onHierarchyChanged(){
			if(this.frozen){
				return;
			}

			if(!this.enabled){
				return;
			}

			if(ClayContainer._skipHierarchyChanges){
				return;
			}

			// the order of operations here is important to successfully move clayOjects from one container to another
			if(this.editingThisContainer){
				this.solidsHierarchyNeedsScan = true;
			}

			this.onSelectionChanged();
			
			if(this.editingThisContainer){
				this.forceUpdateAllSolids();
				this.computeClay();
			}
		}

		public static void inspectorUpdate(){
			ClayContainer.inspectorUpdated = UnityEngine.Object.FindObjectsOfType<ClayContainer>().Length;
		}

		static ClayContainer selectionReoderContainer = null;
		static int selectionReorderId = -1;
		static int selectionReorderIdOffset = 0;

		public void selectToReorder(ClayObject clayObjToReorder, int reorderOffset){
			ClayContainer.selectionReoderContainer = this;
			ClayContainer.selectionReorderId = clayObjToReorder.clayObjectId;
			ClayContainer.selectionReorderIdOffset = reorderOffset;
		}

		void reorderSelected(){
			if(ClayContainer.selectionReoderContainer != this){
				ClayContainer.selectionReoderContainer = null;
				return;
			}

			if(UnityEditor.Selection.gameObjects.Length == 0){
				return;
			}

			ClayObject selectedClayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
			if(selectedClayObj == null){
				return;
			}

			if(selectedClayObj.getClayContainer() != ClayContainer.selectionReoderContainer){
				return;
			}

			ClayObject reoderedClayObj = this.clayObjects[ClayContainer.selectionReorderId];

			int idOffset = selectedClayObj.clayObjectId - ClayContainer.selectionReorderId; 
			this.reorderClayObject(ClayContainer.selectionReorderId, idOffset + ClayContainer.selectionReorderIdOffset);

			ClayContainer.pickedObj = reoderedClayObj.gameObject;
			ClayContainer.pickingMode = true;

			ClayContainer.selectionReoderContainer = null;
		}

		void onSelectionChanged(){
			// for some reason this callback is also triggered by the inspector
			// so we first have to check if this is really a selection change or an inspector update. wtf. 
			if(ClayContainer.inspectorUpdated > 0){
				ClayContainer.inspectorUpdated -= 1;
				return;
			}

			if(this.needsInit){
				return;
			}

			if(this.frozen){
				return;
			}

			if(this.instanceOf != null){
				return;
			}

			if(ClayContainer.selectionReoderContainer != null){
				this.reorderSelected();
			}

			if(!this.enabled){
				return;
			}
			
			bool wasEditingThis = this.editingThisContainer;
			this.editingThisContainer = false;
			if(UnityEditor.Selection.Contains(this.gameObject)){
				// check if this container got selected
				this.editingThisContainer = true;
			}

			if(!this.editingThisContainer){
				for(int i = 0; i < UnityEditor.Selection.gameObjects.Length; ++i){
					GameObject sel = UnityEditor.Selection.gameObjects[i];
					ClayObject clayObj = sel.GetComponent<ClayObject>();
					if(clayObj != null){
						if(clayObj.getClayContainer() == this){
							this.editingThisContainer = true;

							return;
						}
					}
				}

				if(wasEditingThis){// if we're changing selection, optimize the buffers of this container
					this.forceUpdateAllSolids();
					this.computeClay();
					this.optimizeMemory();
					this.drawClayxels(this);
					
					UnityEditor.EditorApplication.QueuePlayerLoopUpdate();// fix instances disappearing
				}
			}
		}

		static void onSceneGUI(SceneView sceneView){
			if(Application.isPlaying){
				return;
			}

			if(!UnityEditorInternal.InternalEditorUtility.isApplicationActive){
				// this callback keeps running even in the background
				return;
			}

			Event ev = Event.current;
			
			if(ev.isKey){
				if(ev.keyCode.ToString().ToLower() == ClayContainer.pickingKey){
					ClayContainer.startPicking();
				}
				else if(ev.keyCode.ToString().ToLower() == ClayContainer.mirrorDuplicateKey){
					ClayContainer.shortcutMirrorDuplicate();
				}
				else if(ev.keyCode.ToString().ToLower() == "f"){
					ClayContainer.frameSelected();
				}
				// else if(ev.keyCode.ToString().ToLower() == ClayContainer.addClayKey){
				// 	ClayContainer.shortcutAddClay();
				// }

				// test cache
				// else if(ev.keyCode == KeyCode.C){
				// 	UnityEngine.Object.FindObjectsOfType<ClayContainer>()[0].cacheClay();
				// 	UnityEngine.Object.FindObjectsOfType<ClayContainer>()[0].computeClay();
				// }
				// else if(ev.keyCode == KeyCode.X){
				// 	UnityEngine.Object.FindObjectsOfType<ClayContainer>()[0].clearCachedClay();
				// }

				return;
			}

			if(!ClayContainer.pickingMode){
				return;
			}
			
			if(ClayContainer.pickedObj != null){
				if(ClayContainer.pickingShiftPressed){
					List<UnityEngine.Object> sel = new List<UnityEngine.Object>();
		   			for(int i = 0; i < UnityEditor.Selection.objects.Length; ++i){
		   				sel.Add(UnityEditor.Selection.objects[i]);
		   			}
		   			sel.Add(ClayContainer.pickedObj);
		   			UnityEditor.Selection.objects = sel.ToArray();
	   			}
	   			else{
					UnityEditor.Selection.objects = new GameObject[]{ClayContainer.pickedObj};
				}
			}
			
			if(ev.type == EventType.MouseMove){
				float uiScale = ClayContainer.getEditorUIScale();
				ClayContainer.pickingMousePosX = (int)ev.mousePosition.x * uiScale;
				ClayContainer.pickingMousePosY = (int)ev.mousePosition.y * uiScale;
				
				if(ClayContainer.pickedObj != null){
					ClayContainer.clearPicking();
				}
			}
			else if(ev.type == EventType.MouseDown && !ev.alt){
				if(ClayContainer.pickingMousePosX < 0 || ClayContainer.pickingMousePosX >= sceneView.camera.pixelWidth || 
					ClayContainer.pickingMousePosY < 0 || ClayContainer.pickingMousePosY >= sceneView.camera.pixelHeight){
					ClayContainer.clearPicking();
					return;
				}

				ev.Use();

				ClayContainer.finalizePicking(sceneView);
			}
			else if((int)ev.type == 7){ // on repaint
				ClayContainer.performPicking(sceneView);
			}

			ClayContainer.getSceneView().Repaint();
		}

		static void setupScenePicking(){
			SceneView sceneView = (SceneView)SceneView.sceneViews[0];
			SceneView.duringSceneGui -= ClayContainer.onSceneGUI;
			SceneView.duringSceneGui += ClayContainer.onSceneGUI;

			ClayContainer.pickingCommandBuffer = new CommandBuffer();
			
			ClayContainer.pickingTextureResult = new Texture2D(1, 1, TextureFormat.ARGB32, false);

			ClayContainer.pickingRect = new Rect(0, 0, 1, 1);

			if(ClayContainer.pickingRenderTexture != null){
				ClayContainer.pickingRenderTexture.Release();
				ClayContainer.pickingRenderTexture = null;
			}

			ClayContainer.pickingRenderTexture = new RenderTexture(1024, 768, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			ClayContainer.pickingRenderTexture.Create();
			ClayContainer.pickingRenderTextureId = new RenderTargetIdentifier(ClayContainer.pickingRenderTexture);

			if(ClayContainer.clayxelPickingMaterial ==  null){
				Shader pickingShader = Shader.Find("Clayxels/ClayxelPickingShader");
				ClayContainer.clayxelPickingMaterial = new Material(pickingShader);
				ClayContainer.pickingMaterialProperties = new MaterialPropertyBlock();
			}
		}

		public static void startPicking(){
			ClayContainer.pickingMode = true;
			ClayContainer.pickedObj = null;

			ClayContainer.getSceneView().Repaint();
		}

		static void clearPicking(){
			if(ClayContainer.pickedContainerId > -1){
				ClayContainer[] containers = GameObject.FindObjectsOfType<ClayContainer>();
				containers[ClayContainer.pickedContainerId].pickingThis = false;
			}

			if(ClayContainer.lastPickedContainerId > -1){
				ClayContainer[] containers = GameObject.FindObjectsOfType<ClayContainer>();
				containers[ClayContainer.lastPickedContainerId].pickingThis = false;
			}

			bool continuePicking = false;
			if(ClayContainer.pickedObj != null){
				if(ClayContainer.pickedObj.GetComponent<ClayContainer>() != null){
					ClayContainer.pickedObj = null;
					ClayContainer.pickingMode = true;

					continuePicking = true;
				}
			}
			
			if(!continuePicking){
				ClayContainer.pickingMode = false;
				ClayContainer.pickedObj = null;
				ClayContainer.pickedContainerId = -1;
				ClayContainer.pickedClayObjectId = -1;
				ClayContainer.lastPickedContainerId = -1;
			}

			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
		}

		static int lastPickedContainerId = -1;
		bool pickingThis = false;

		static void finalizePicking(SceneView sceneView){
	  		if(ClayContainer.pickedContainerId > -1){
	  			ClayContainer[] containers = GameObject.FindObjectsOfType<ClayContainer>();
	  			ClayContainer container = containers[ClayContainer.pickedContainerId];

	  			GameObject newSel = null;
	  			if(ClayContainer.pickedClayObjectId > -1 && container.editingThisContainer){
		  			newSel = container.getClayObject(ClayContainer.pickedClayObjectId).gameObject;
		  		}
		  		else{
		  			newSel = container.gameObject;
		  		}

	  			UnityEditor.Selection.objects = new GameObject[]{newSel};
	  			ClayContainer.pickedObj = newSel;
	  			ClayContainer.pickingShiftPressed = Event.current.shift;
	  			
	  			return;
	  		}
			
			ClayContainer.clearPicking();
		}

		static void performPicking(SceneView sceneView){
			if(ClayContainer.pickingMousePosX < 0 || ClayContainer.pickingMousePosX >= sceneView.camera.pixelWidth || 
				ClayContainer.pickingMousePosY < 0 || ClayContainer.pickingMousePosY >= sceneView.camera.pixelHeight){
				return;
			}

			ClayContainer[] containers = GameObject.FindObjectsOfType<ClayContainer>();

			if(ClayContainer.lastPickedContainerId > -1 && ClayContainer.pickedContainerId != ClayContainer.lastPickedContainerId){
				ClayContainer lastContainer = containers[ClayContainer.lastPickedContainerId];
				lastContainer.pickingThis = false;
				ClayContainer.lastPickedContainerId = -1;
			}
				
			if(ClayContainer.pickedContainerId > -1){
				ClayContainer container = containers[ClayContainer.pickedContainerId];
				ClayContainer.lastPickedContainerId = ClayContainer.pickedContainerId;
				
				if(container.editingThisContainer){
					ClayContainer.claycoreCompute.SetInt("storeSolidId", 1);
		  			container.computeClay();
		  			ClayContainer.claycoreCompute.SetInt("storeSolidId", 0);
		  		}
				
				container.pickingThis = true;
			}
			
			ClayContainer.pickedClayObjectId = -1;
	  		ClayContainer.pickedContainerId = -1;

			ClayContainer.pickingCommandBuffer.Clear();
			ClayContainer.pickingCommandBuffer.SetRenderTarget(ClayContainer.pickingRenderTextureId);
			ClayContainer.pickingCommandBuffer.ClearRenderTarget(true, true, Color.black, 1.0f);

			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];
				if(container.enabled){
					container.drawClayxelPicking(i, ClayContainer.pickingCommandBuffer);
				}
			}
			
			Graphics.ExecuteCommandBuffer(ClayContainer.pickingCommandBuffer);
			
			int rectWidth = (int)(1024.0f * ((float)ClayContainer.pickingMousePosX / (float)sceneView.camera.pixelWidth ));
			int rectHeight = (int)(768.0f * ((float)ClayContainer.pickingMousePosY / (float)sceneView.camera.pixelHeight));
			#if UNITY_EDITOR_OSX
				rectHeight = 768 - rectHeight;
			#endif

			ClayContainer.pickingRect.Set(
				rectWidth, 
				rectHeight, 
				1, 1);

			RenderTexture oldRT = RenderTexture.active;
			RenderTexture.active = ClayContainer.pickingRenderTexture;
			ClayContainer.pickingTextureResult.ReadPixels(ClayContainer.pickingRect, 0, 0);
			ClayContainer.pickingTextureResult.Apply();
			RenderTexture.active = oldRT;
			
			Color pickCol = ClayContainer.pickingTextureResult.GetPixel(0, 0);
			int pickId = (int)((pickCol.r + pickCol.g * 255.0f + pickCol.b * 255.0f) * 255.0f);
	  		ClayContainer.pickedClayObjectId = pickId - 1;
	  		ClayContainer.pickedContainerId = (int)(pickCol.a * 256.0f);
	  		
	  		if(ClayContainer.pickedContainerId >= 255){
	  			ClayContainer.pickedContainerId = -1;
	  		}
		}

		void drawClayxelPicking(int containerId, CommandBuffer pickingCommandBuffer){
			if(this.needsInit && this.instanceOf == null){
				return;
			}

			ClayContainer container = this;
			if(this.instanceOf != null){
				container = this.instanceOf;
			}
			
			ClayContainer.pickingMaterialProperties.SetBuffer("pointCloudDataToSolidId", ClayContainer.pointCloudDataToSolidIdBuffer);
			ClayContainer.pickingMaterialProperties.SetInt("chunkMaxOutPoints", ClayContainer.chunkMaxOutPoints);
			ClayContainer.pickingMaterialProperties.SetMatrix("objectMatrix", this.transform.localToWorldMatrix);
			ClayContainer.pickingMaterialProperties.SetFloat("splatRadius",  this.splatRadius);
			ClayContainer.pickingMaterialProperties.SetFloat("chunkSize", (float)container.chunkSize);

			for(int chunkIt = 0; chunkIt < container.numChunks; ++chunkIt){
				ClayxelChunk chunk = container.chunks[chunkIt];
				
				ClayContainer.pickingMaterialProperties.SetBuffer("chunkPoints", chunk.pointCloudDataBuffer);
				ClayContainer.pickingMaterialProperties.SetVector("chunkCenter",  chunk.center);
				ClayContainer.pickingMaterialProperties.SetInt("chunkId", chunkIt);
				ClayContainer.pickingMaterialProperties.SetInt("containerId", containerId);

				if(this.pickingThis){
					ClayContainer.pickingMaterialProperties.SetInt("selectMode", 1);
				}
				else{
					ClayContainer.pickingMaterialProperties.SetInt("selectMode", 0);
				}

				pickingCommandBuffer.DrawProceduralIndirect(Matrix4x4.identity, ClayContainer.clayxelPickingMaterial, -1, 
					MeshTopology.Triangles, chunk.indirectDrawArgsBuffer, 0, ClayContainer.pickingMaterialProperties);
			}
		}

		void OnDrawGizmos(){
			if(Application.isPlaying){
				return;
			}

			if(!this.editingThisContainer){
				return;
			}

			// debug auto bounds
			// if(this.autoBounds){
				// Gizmos.color = Color.red;
				// Gizmos.matrix = this.transform.localToWorldMatrix;
				// Gizmos.DrawWireCube(Vector3.zero, new Vector3(this._debugAutoBounds, this._debugAutoBounds, this._debugAutoBounds));
			// }

			Gizmos.color = ClayContainer.boundsColor;
			Gizmos.matrix = this.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(this.boundsCenter, this.boundsScale);
		}

		static public void reloadAll(){
			ClayContainer.globalDataNeedsInit = true;

			ClayContainer[] clayxelObjs = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
			for(int i = 0; i < clayxelObjs.Length; ++i){
				clayxelObjs[i].init();
			}
						
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			((SceneView)SceneView.sceneViews[0]).Repaint();
		}

		public static SceneView getSceneView(){
			return (SceneView)SceneView.sceneViews[0];
		}

		public bool shouldRetopoMesh = false;
		public int retopoMaxVerts = -1;
		
		public void storeMesh(string assetName){
			if(this.gameObject.GetComponent<MeshFilter>().sharedMesh == null){
				return;
			}

			string assetNameUnique = this.storeAssetPath + "_" + this.GetInstanceID();

			Material storedMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/" + assetNameUnique + ".mat", typeof(Material));
			if(storedMat == null){
				if(this.gameObject.GetComponent<MeshRenderer>() != null){
					if(this.gameObject.GetComponent<MeshRenderer>().sharedMaterial != null){
						AssetDatabase.CreateAsset(this.gameObject.GetComponent<MeshRenderer>().sharedMaterial, "Assets/" + assetNameUnique + ".mat");
					}
				}
			}

			AssetDatabase.CreateAsset(this.gameObject.GetComponent<MeshFilter>().sharedMesh, "Assets/" + assetNameUnique + ".mesh");
			AssetDatabase.SaveAssets();
			
			UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath("Assets/" + assetNameUnique + ".mesh");
			for(int i = 0; i < data.Length; ++i){
				if(data[i].GetType() == typeof(Mesh)){
					if(this.gameObject.GetComponent<MeshFilter>() != null){
						this.gameObject.GetComponent<MeshFilter>().sharedMesh = (Mesh)data[i];
					}
					
					if(this.gameObject.GetComponent<MeshRenderer>() != null && storedMat != null){
						this.gameObject.GetComponent<MeshRenderer>().sharedMaterial = storedMat;
					}

					break;
				}
			}
		}

		public AnimationClip claymationAnimClip = null;
		public int claymationStartFrame = 0;
		public int claymationEndFrame = 0;

		public delegate void AnimUpdateCallback(int frame);

		/* Freeze this container to a claymation, a compact data format that retains the same shader as live clayxels. */
		public void freezeClaymation(){
			if(this.needsInit){
				this.init();
			}

			bool success = true;
			string assetNameUnique = this.storeAssetPath + "_" + this.GetInstanceID();
			if(this.claymationAnimClip == null){
				success = this.bakeClaymationFile(assetNameUnique, 0, 0);
			}
			else{
				success = this.bakeClaymationFile(assetNameUnique, this.claymationStartFrame, this.claymationEndFrame, this.animClipCallback);
			}

			if(!success){
				return;
			}
			
			Claymation claymation = this.gameObject.GetComponent<Claymation>();
			if(claymation == null){
				claymation = this.gameObject.AddComponent<Claymation>();
			}
			
			claymation.claymationFile = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/" + assetNameUnique + ".clay.bytes", typeof(TextAsset));

			claymation.material = this.material;

			if(this.claymationAnimClip != null){
				claymation.frameRate = (int)this.claymationAnimClip.frameRate;
			}

			claymation.enabled = true;
			claymation.init();

			this.frozen = true;
			this.enabled = false;

			this.releaseBuffers();
		}

		public void freezeContainersHierarchyToClaymation(){
			ClayContainer[] containers = this.GetComponentsInChildren<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				containers[i].freezeClaymation();
			}
		}

		/* Use this method when you want to provide a custom animCallback in order to bake procedural motion coming from your code.
		To simply bake models and animationClips use ClayContainer.freezeClaymation().*/
		public bool bakeClaymationFile(string assetName, int startFrame = 0, int endFrame = 0, AnimUpdateCallback animCallback = null){
			int numFrames = (endFrame - startFrame) + 1;

			// claymation file format starts here
			BinaryWriter writer = new BinaryWriter(File.Open(Application.dataPath + "/" + assetName + ".clay.bytes", FileMode.Create));

			int fileFormat = 0;
			writer.Write(fileFormat);
			writer.Write(this.chunkSize);
			writer.Write(this.chunksX);
			writer.Write(this.chunksY);
			writer.Write(this.chunksZ);
			writer.Write(numFrames);

			this.switchComputeData();

			if(this.memoryOptimized){
				this.expandMemory();
			}

			this.userWarning = "";
			
			for(int frameIt = startFrame; frameIt < (endFrame + 1); ++frameIt){
				if(animCallback != null){
					animCallback(frameIt);
				}
				
				this.forceUpdateAllSolids();
				this.updateSolids();

				for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
					this.computeChunk(chunkIt);

					int numPoints = 0;
					bool success = this.bakeClaymationFrame(chunkIt, ref numPoints, ref ClayContainer.tmpChunkData);
					if(!success){
						writer.Close();

						return false;
					}

					writer.Write(numPoints);
					
					for(int pointIt = 0; pointIt < numPoints; ++pointIt){
						int dataIt = pointIt * 2;
						writer.Write(ClayContainer.tmpChunkData[dataIt]);
						writer.Write(ClayContainer.tmpChunkData[dataIt + 1]);
					}
				}
			}
			
			writer.Close();
			
			AssetDatabase.Refresh();

			return true;
		}

		void animClipCallback(int frame){
			if(this.claymationAnimClip != null){
				this.claymationAnimClip.SampleAnimation(this.gameObject, (float)frame / this.claymationAnimClip.frameRate);
			}
		}

		bool bakeClaymationFrame(int chunkId, ref int numPoints, ref int[] pointsData){
			ClayxelChunk chunk = this.chunks[chunkId];
			chunk.indirectDrawArgsBuffer.GetData(ClayContainer.indirectArgsData);

			numPoints = ClayContainer.indirectArgsData[0] / 3;

			if(numPoints > ClayContainer.chunkMaxOutPoints){
				this.userWarning = "max point count exceeded, increase limit from file ClayxelsPrefs.cs setPointCloudLimit";
				Debug.Log("Clayxels: container " + this.gameObject.name + " has exceeded the limit of points allowed, to increase the limit use ClayContainer.setPointCloudLimit() inside ClayxelsPrefs.cs");
				
				return false;
			}
			
			chunk.pointCloudDataBuffer.GetData(pointsData, 0, 0, numPoints * 2);

			return true;
		}

		static void frameSelected(){
			int numClayxelsObjs = 0;
			Bounds bounds = new Bounds();

			for(int i = 0; i < UnityEditor.Selection.gameObjects.Length; ++i){
				GameObject selObj = UnityEditor.Selection.gameObjects[i];

				ClayContainer container = selObj.GetComponent<ClayContainer>();
				if(container != null){
					bounds.Encapsulate(container.renderBounds);
					numClayxelsObjs += 1;
				}
				else{
					ClayObject clayObj = selObj.GetComponent<ClayObject>();
					if(clayObj != null){
						bounds.Encapsulate(new Bounds(clayObj.transform.position, clayObj.transform.lossyScale));
						numClayxelsObjs += 1;
					}
				}
			}

			if(numClayxelsObjs > 0){
				ClayContainer.getSceneView().Frame(bounds, false);
			}
		}

		#endif// end if UNITY_EDITOR
	}
}
