
#if UNITY_EDITOR // exclude from build

using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

using Clayxels;

namespace Clayxels{
	[CustomEditor(typeof(ClayContainer))]
	public class ClayxelInspector : Editor{
		static bool freezePanel = false;
		
		public override void OnInspectorGUI(){
			Color defaultColor = GUI.backgroundColor;

			ClayContainer clayxel = (ClayContainer)this.target;

			EditorGUILayout.LabelField("Clayxels V1.2");

			if(ClayContainer.isCacheEnabled()){
				GUIStyle s = new GUIStyle();
				s.wordWrap = true;
				s.normal.textColor = Color.yellow;

				EditorGUILayout.LabelField("CLAYXELS_CACHEON is active, this mode uses more video-ram.", s);
			}

			string userWarn = clayxel.getUserWarning();
			if(userWarn != ""){

				GUIStyle s = new GUIStyle();
				s.wordWrap = true;
				s.normal.textColor = Color.yellow;
				EditorGUILayout.LabelField(userWarn, s);
				
			}

			GUI.enabled = !clayxel.isFrozen();

			if(clayxel.instanceOf != null){
				ClayContainer newInstance = (ClayContainer)EditorGUILayout.ObjectField(new GUIContent("instance", "Set this to point at another clayContainer in scene to make this into an instance and avoid having to compute the same thing twice."), clayxel.instanceOf, typeof(ClayContainer), true);
			
				if(newInstance != clayxel.instanceOf && newInstance != clayxel){
					clayxel.instanceOf = newInstance;

					UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
					ClayContainer.getSceneView().Repaint();

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}

				return;
			}

			if(clayxel.getNumSolids() > clayxel.getMaxSolids()){
				EditorGUILayout.LabelField("Max solid count exeeded, see ClayxelsPrefs.cs setMaxSolids.");
			}

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();

			int clayxelDetail = EditorGUILayout.IntField(new GUIContent("clayxel detail", "How coarse or finely detailed is your sculpt. Enable Gizmos in your viewport to see the boundaries."), clayxel.getClayxelDetail());
			
			if(EditorGUI.EndChangeCheck()){
				ClayContainer.inspectorUpdate();

				clayxel.setClayxelDetail(clayxelDetail);
				
				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}

				return;
			}

			GUILayout.BeginHorizontal();

			GUI.backgroundColor = defaultColor;

			if(!clayxel.isAutoBoundsActive()){
				EditorGUI.BeginChangeCheck();
				Vector3Int boundsScale = EditorGUILayout.Vector3IntField(new GUIContent("bounds scale", "How much work area you have for your sculpt within this container. Enable Gizmos in your viewport to see the boundaries."), clayxel.getBoundsScale());
				
				if(EditorGUI.EndChangeCheck()){
					ClayContainer.inspectorUpdate();

					clayxel.setBoundsScale(boundsScale.x, boundsScale.y, boundsScale.z);

					clayxel.init();
					clayxel.needsUpdate = true;
					UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
					ClayContainer.getSceneView().Repaint();

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}

					return;
				}

				if(GUILayout.Button(new GUIContent("-", ""))){
					ClayContainer.inspectorUpdate();

					Vector3Int bounds = clayxel.getBoundsScale();
					clayxel.setBoundsScale(bounds.x - 1, bounds.y - 1, bounds.z - 1);

					clayxel.init();
					clayxel.needsUpdate = true;
					UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
					ClayContainer.getSceneView().Repaint();

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}

					return;
				}

				if(GUILayout.Button(new GUIContent("+", ""))){
					ClayContainer.inspectorUpdate();

					Vector3Int bounds = clayxel.getBoundsScale();
					clayxel.setBoundsScale(bounds.x + 1, bounds.y + 1, bounds.z + 1);

					clayxel.init();
					clayxel.needsUpdate = true;
					UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
					ClayContainer.getSceneView().Repaint();

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}

					return;
				}

				if(GUILayout.Button(new GUIContent("auto", ""))){
					clayxel.setAutoBoundsActive(true);

					UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
					ClayContainer.getSceneView().Repaint();

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}
			}
			else{
				GUI.backgroundColor = Color.yellow;

				GUILayout.BeginHorizontal();
				
				EditorGUILayout.LabelField("bounds scale");

				if(GUILayout.Button(new GUIContent("auto", ""))){
					clayxel.setAutoBoundsActive(false);
				}

				GUILayout.EndHorizontal();
			}

			GUI.backgroundColor = defaultColor;

			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			if(GUILayout.Button(new GUIContent("add clay", "lets get this party started"))){
				ClayObject clayObj = ((ClayContainer)this.target).addClayObject();

				Undo.RegisterCreatedObjectUndo(clayObj.gameObject, "added clayxel solid");
				UnityEditor.Selection.objects = new GameObject[]{clayObj.gameObject};

				clayxel.needsUpdate = true;
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				ClayContainer.getSceneView().Repaint();

				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}

				return;
			}

			if(GUILayout.Button(new GUIContent("pick clay ("+ClayContainer.pickingKey+")", "Press p on your keyboard to mouse pick ClayObjects from the viewport. Pressing Shift will add to a previous selection."))){
				ClayContainer.startPicking();
			}

			clayxel.forceUpdate = EditorGUILayout.Toggle(new GUIContent("animate (forceUpdate)", "Enable if you're animating/moving the container as well as the clayObjects inside it."), clayxel.forceUpdate);

			EditorGUILayout.Space();

			GUI.enabled = true;

			ClayxelInspector.freezePanel = EditorGUILayout.Foldout(ClayxelInspector.freezePanel, "freeze options", true);

			if(ClayxelInspector.freezePanel){
				if(clayxel.storeAssetPath == ""){
					clayxel.storeAssetPath = clayxel.gameObject.name;
				}
				clayxel.storeAssetPath = EditorGUILayout.TextField(new GUIContent("frozen asset name", "Specify an asset name to store frozen mesh or claymation on disk. Files are saved relative to this project's Assets folder."), clayxel.storeAssetPath);
				string[] paths = clayxel.storeAssetPath.Split('.');
				if(paths.Length > 0){
					clayxel.storeAssetPath = paths[0];
				}

				EditorGUILayout.Space();

				if(!clayxel.isFrozen()){
					if(GUILayout.Button(new GUIContent("freeze mesh", "Switch between live clayxels and a frozen mesh."))){
						clayxel.freezeContainersHierarchyToMesh();

						#if CLAYXELS_RETOPO
							if(clayxel.shouldRetopoMesh){
								int targetVertCount = RetopoUtils.getRetopoTargetVertsCount(clayxel.gameObject, clayxel.retopoMaxVerts);
								if(targetVertCount == 0){
									return;
								}

								RetopoUtils.retopoMesh(clayxel.gameObject.GetComponent<MeshFilter>().sharedMesh, targetVertCount, -1);
							}
						#endif
						
						clayxel.transferMaterialPropertiesToMesh();

						if(clayxel.storeAssetPath != ""){
							clayxel.storeMesh(clayxel.storeAssetPath);
						}
					}

					#if CLAYXELS_RETOPO
					clayxel.shouldRetopoMesh = EditorGUILayout.Toggle(new GUIContent("retopology", "Use this to generate meshes with a better topology."), clayxel.shouldRetopoMesh);
					if(clayxel.shouldRetopoMesh){
						clayxel.retopoMaxVerts = EditorGUILayout.IntField(new GUIContent("vertex count", "-1 will let the tool decide on the best number of vertices."), clayxel.retopoMaxVerts);
					}
					#endif

					EditorGUILayout.Space();

					if(GUILayout.Button(new GUIContent("freeze claymation", "Freeze this container to a point-cloud file stored on disk and skip heavy computing."))){
						clayxel.freezeClaymation();
					}

					AnimationClip claymationAnimClip = (AnimationClip)EditorGUILayout.ObjectField(new GUIContent("animClip (optional)", ""), clayxel.claymationAnimClip, typeof(AnimationClip), true);
					if(claymationAnimClip != null && claymationAnimClip != clayxel.claymationAnimClip){
						clayxel.claymationStartFrame = 0;
						clayxel.claymationEndFrame = (int)(claymationAnimClip.length * claymationAnimClip.frameRate);
					}

					clayxel.claymationAnimClip = claymationAnimClip;

					if(clayxel.claymationAnimClip != null){
						clayxel.claymationStartFrame = EditorGUILayout.IntField(new GUIContent("start", ""), clayxel.claymationStartFrame);
						clayxel.claymationEndFrame = EditorGUILayout.IntField(new GUIContent("end", ""), clayxel.claymationEndFrame);
					}
				}
				else{
					if(GUILayout.Button(new GUIContent("defrost clayxels", "Back to live clayxels."))){
						clayxel.defrostContainersHierarchy();
						UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
						ClayContainer.getSceneView().Repaint();
					}
				}
			}

			GUI.enabled = !clayxel.isFrozen();

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();

			Material customMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("customMaterial", "Custom materials need to use shaders specifically made for clayxels. Use the provided shaders and examples as reference. "), clayxel.customMaterial, typeof(Material), false);
			
			if(EditorGUI.EndChangeCheck()){
				ClayContainer.inspectorUpdate();
				
				Undo.RecordObject(this.target, "changed clayxel container");

				if(customMaterial != clayxel.customMaterial){
					clayxel.setCustomMaterial(customMaterial);
				}

				clayxel.needsUpdate = true;
				clayxel.forceUpdateAllSolids();
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				ClayContainer.getSceneView().Repaint();

				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}

			if(!clayxel.isFrozenToMesh()){
				this.inspectMaterial(clayxel);
			}

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();

			bool castShadows = EditorGUILayout.Toggle("cast shadows", clayxel.getCastShadows());
			// bool receiveShadows = EditorGUILayout.Toggle("receive shadows", clayxel.getReceiveShadows());

			if(EditorGUI.EndChangeCheck()){
				ClayContainer.inspectorUpdate();

				clayxel.setCastShadows(castShadows);

				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}

			// EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(1)), Color.grey);

			EditorGUILayout.Space();
			
			ClayContainer instance = (ClayContainer)EditorGUILayout.ObjectField(new GUIContent("instance", "Set this to point at another clayContainer in scene to make this into an instance and avoid having to compute the same thing twice."), clayxel.instanceOf, typeof(ClayContainer), true);
			
			if(instance != clayxel.instanceOf && instance != clayxel){
				clayxel.instanceOf = instance;

				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				ClayContainer.getSceneView().Repaint();

				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}

			EditorGUILayout.Space();

			if(GUILayout.Button((new GUIContent("reload all", "This is necessary after you make changes to the shader or to the claySDF file.")))){
				ClayContainer.reloadAll();
			}
		}

		MaterialEditor materialEditor = null;
		void inspectMaterial(ClayContainer clayContainer){
			EditorGUI.BeginChangeCheck();

			if(this.materialEditor != null){
				DestroyImmediate(this.materialEditor);
				this.materialEditor = null;
			}

			Material mat = clayContainer.getMaterial();
			if(mat != null){
				this.materialEditor = (MaterialEditor)CreateEditor(mat);
			}

			if(this.materialEditor != null){
				this.materialEditor.DrawHeader();
				this.materialEditor.OnInspectorGUI();
			}
			
			if(EditorGUI.EndChangeCheck()){
				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}
		}

		void OnDisable (){
			if(this.materialEditor != null) {
				DestroyImmediate(this.materialEditor);
				this.materialEditor = null;
			}
		}
	}

	[CustomEditor(typeof(ClayObject)), CanEditMultipleObjects]
	public class ClayObjectInspector : Editor{
		public override void OnInspectorGUI(){
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			
			ClayObject clayObj = (ClayObject)this.targets[0];
			ClayContainer clayxel = clayObj.getClayContainer();
			if(clayxel == null){
				return;
			}

			EditorGUI.BeginChangeCheck();
			
			string[] solidsLabels = clayxel.getSolidsCatalogueLabels();
	 		int primitiveType = EditorGUILayout.Popup("solidType", clayObj.primitiveType, solidsLabels);

			float blend = EditorGUILayout.Slider("blend", Mathf.Abs(clayObj.blend) * 100.0f, 0.0f, 100.0f);
			if(clayObj.blend < 0.0f){
				if(blend < 0.001f){
					blend = 0.001f;
				}

				blend *= -1.0f;
			}

			blend *= 0.01f;
			if(blend > 1.0f){
				blend = 1.0f;
			}
			else if(blend < -1.0f){
				blend = -1.0f;
			}

			GUILayout.BeginHorizontal();

			Color defaultColor = GUI.backgroundColor;

			if(clayObj.blend >= 0.0f){
				GUI.backgroundColor = Color.yellow;
			}

			if(GUILayout.Button(new GUIContent("add", "Additive blend"))){
				blend = Mathf.Abs(blend);
			}
			
			GUI.backgroundColor = defaultColor;

			if(clayObj.blend < 0.0f){
				GUI.backgroundColor = Color.yellow;
			}

			if(GUILayout.Button(new GUIContent("sub", "Subtractive blend"))){
				if(blend == 0.0f){
					blend = 0.0001f;
				}

				blend = blend * -1.0f;
			}

			GUI.backgroundColor = defaultColor;

			GUILayout.EndHorizontal();

			Color color = EditorGUILayout.ColorField("color", clayObj.color);

	 		Dictionary<string, float> paramValues = new Dictionary<string, float>();
	 		paramValues["x"] = clayObj.attrs.x;
	 		paramValues["y"] = clayObj.attrs.y;
	 		paramValues["z"] = clayObj.attrs.z;
	 		paramValues["w"] = clayObj.attrs.w;

	 		List<string[]> parameters = clayxel.getSolidsCatalogueParameters(primitiveType);
	 		List<string> wMaskLabels = new List<string>();
	 		for(int paramIt = 0; paramIt < parameters.Count; ++paramIt){
	 			string[] parameterValues = parameters[paramIt];
	 			string attr = parameterValues[0];
	 			string label = parameterValues[1];
	 			string defaultValue = parameterValues[2];

	 			if(primitiveType != clayObj.primitiveType){
	 				// reset to default params when changing primitive type
	 				paramValues[attr] = float.Parse(defaultValue, CultureInfo.InvariantCulture);
	 			}
	 			
	 			if(attr.StartsWith("w")){
	 				wMaskLabels.Add(label);
	 			}
	 			else{
	 				paramValues[attr] = EditorGUILayout.FloatField(label, paramValues[attr] * 100.0f) * 0.01f;
	 			}
	 		}

	 		if(wMaskLabels.Count > 0){
	 			paramValues["w"] = (float)EditorGUILayout.MaskField("options", (int)clayObj.attrs.w, wMaskLabels.ToArray());
	 		}
	 		
	 		if(EditorGUI.EndChangeCheck()){
	 			ClayContainer.inspectorUpdate();
	 			ClayContainer._skipHierarchyChanges = true;

	 			Undo.RecordObjects(this.targets, "changed clayobject");

	 			for(int i = 1; i < this.targets.Length; ++i){
	 				bool somethingChanged = false;
	 				ClayObject currentClayObj = (ClayObject)this.targets[i];

	 				bool shouldAutoRename = false;

	 				if(Mathf.Abs(clayObj.blend - blend) > 0.001f || Mathf.Sign(clayObj.blend) != Mathf.Sign(blend)){
	 					currentClayObj.blend = blend;
	 					somethingChanged = true;
	 					shouldAutoRename = true;
	 				}

	 				if(clayObj.color != color){
	 					currentClayObj.color = color;
	 					somethingChanged = true;
	 				}
					
	 				if(clayObj.primitiveType != primitiveType){
	 					currentClayObj.primitiveType = primitiveType;
	 					somethingChanged = true;
	 					shouldAutoRename = true;
	 				}

	 				if(clayObj.attrs.x != paramValues["x"]){
	 					currentClayObj.attrs.x = paramValues["x"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs.y != paramValues["y"]){
	 					currentClayObj.attrs.y = paramValues["y"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs.z != paramValues["z"]){
	 					currentClayObj.attrs.z = paramValues["z"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs.w != paramValues["w"]){
	 					currentClayObj.attrs.w = paramValues["w"];
	 					somethingChanged = true;
	 					shouldAutoRename = true;
	 				}

	 				if(somethingChanged){
	 					currentClayObj.getClayContainer().clayObjectUpdated(currentClayObj);

	 					if(shouldAutoRename){
		 					if(currentClayObj.gameObject.name.StartsWith("clay_")){
		 						clayxel.autoRenameClayObject(currentClayObj);
		 					}
		 				}
	 				}

	 				ClayContainer._skipHierarchyChanges = false;
				}

	 			clayObj.blend = blend;
	 			clayObj.color = color;
	 			clayObj.primitiveType = primitiveType;
	 			clayObj.attrs.x = paramValues["x"];
	 			clayObj.attrs.y = paramValues["y"];
	 			clayObj.attrs.z = paramValues["z"];
	 			clayObj.attrs.w = paramValues["w"];

	 			if(clayObj.gameObject.name.StartsWith("clay_")){
					clayxel.autoRenameClayObject(clayObj);
				}

				clayObj.forceUpdate();
	 			
	 			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
	 			ClayContainer.getSceneView().Repaint();

	 			if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();

			ClayObject.ClayObjectMode mode = (ClayObject.ClayObjectMode)EditorGUILayout.EnumPopup("mode", clayObj.mode);
			
			if(EditorGUI.EndChangeCheck()){
				clayObj.setMode(mode);

				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();

				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}

			if(clayObj.mode == ClayObject.ClayObjectMode.offset){
				this.drawOffsetMode(clayObj);
			}
			else if(clayObj.mode == ClayObject.ClayObjectMode.spline){
				this.drawSplineMode(clayObj);
			}

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUI.enabled = !clayxel.isClayObjectsOrderLocked();
			int clayObjectId = EditorGUILayout.IntField("order", clayObj.clayObjectId);
			GUI.enabled = true;

			if(!clayxel.isClayObjectsOrderLocked()){
				if(clayObjectId != clayObj.clayObjectId){
					int idOffset = clayObjectId - clayObj.clayObjectId; 
					clayxel.reorderClayObject(clayObj.clayObjectId, idOffset);
				}
			}

			if(GUILayout.Button(new GUIContent("↑", ""))){
				clayxel.reorderClayObject(clayObj.clayObjectId, -1);
			}
			if(GUILayout.Button(new GUIContent("↓", ""))){
				clayxel.reorderClayObject(clayObj.clayObjectId, 1);
			}
			if(GUILayout.Button(new GUIContent("⋮", ""))){
				EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), "Component/Clayxels/ClayObject", null);
			}
			GUILayout.EndHorizontal();
		}

		[MenuItem("Component/Clayxels/ClayObject/Mirror Duplicate (m)")]
    	static void MirrorDuplicate(MenuCommand command){
    		ClayContainer.shortcutMirrorDuplicate();
    	}

		[MenuItem("Component/Clayxels/ClayObject/Unlock Order From Hierarchy")]
    	static void OrderFromHierarchyOff(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				clayObj.getClayContainer().setClayObjectsOrderLocked(false);
    			}
    		}
    	}

    	[MenuItem("Component/Clayxels/ClayObject/Lock Order To Hierarchy")]
    	static void OrderFromHierarchyOn(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				clayObj.getClayContainer().setClayObjectsOrderLocked(true);
    			}
    		}
    	}

		[MenuItem("Component/Clayxels/ClayObject/Send Before ClayObject")]
    	static void sendBeforeClayObject(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				clayObj.getClayContainer().selectToReorder(clayObj, 0);
    			}
    		}
    	}

		[MenuItem("Component/Clayxels/ClayObject/Send After ClayObject")]
    	static void sendAfterClayObject(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				clayObj.getClayContainer().selectToReorder(clayObj, 1);
    			}
    		}
    	}

    	[MenuItem("Component/Clayxels/ClayObject/Rename all ClayObjects to Animate")]
    	static void renameToAnimate(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				ClayContainer container = clayObj.getClayContainer();
    				ClayContainer._skipHierarchyChanges = true;// otherwise each rename will trigger onHierarchyChange

    				int numClayObjs = container.getNumClayObjects();

    				for(int i = 0; i < numClayObjs; ++i){
    					ClayObject currentClayObj = container.getClayObject(i);

    					if(currentClayObj.gameObject.name.StartsWith("clay_")){
    						container.autoRenameClayObject(currentClayObj);
    						currentClayObj.name = "(" + i + ")" + currentClayObj.gameObject.name;
    					}
    				}

    				ClayContainer._skipHierarchyChanges = false;
    			}
    		}
    	}

		void drawSplineMode(ClayObject clayObj){
			EditorGUI.BeginChangeCheck();

			int subdivs = EditorGUILayout.IntField("subdivs", clayObj.getSplineSubdiv());

			GUILayout.BeginHorizontal();

			int numPoints = clayObj.splinePoints.Count - 2;
			EditorGUILayout.LabelField("control points: " + numPoints);

			if(GUILayout.Button(new GUIContent("+", ""))){
				clayObj.addSplineControlPoint();
			}

			if(GUILayout.Button(new GUIContent("-", ""))){
				clayObj.removeLastSplineControlPoint();
			}

			GUILayout.EndHorizontal();

			// var list = this.serializedObject.FindProperty("splinePoints");
			// EditorGUILayout.PropertyField(list, new GUIContent("spline points"), true);

			if(EditorGUI.EndChangeCheck()){
				// this.serializedObject.ApplyModifiedProperties();

				clayObj.setSplineSubdiv(subdivs);

				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			}
		}

		void drawOffsetMode(ClayObject clayObj){
			EditorGUI.BeginChangeCheck();
				
			int numSolids = EditorGUILayout.IntField("solids", clayObj.getNumSolids());
			bool allowSceneObjects = true;
			clayObj.offsetter = (GameObject)EditorGUILayout.ObjectField("offsetter", clayObj.offsetter, typeof(GameObject), allowSceneObjects);
			
			if(EditorGUI.EndChangeCheck()){
				if(numSolids < 1){
					numSolids = 1;
				}
				else if(numSolids > 100){
					numSolids = 100;
				}

				clayObj.setOffsetNum(numSolids);
				
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			}
		}
	}

	[CustomEditor(typeof(Claymation))]
	public class ClaymationInspector : Editor{
		public override void OnInspectorGUI(){
			Claymation claymation = (Claymation)this.target;

			Claymation instance = (Claymation)EditorGUILayout.ObjectField(new GUIContent("instance", "Set this to point at another Claymation in scene to make this into an instance and avoid duplicating memory."), claymation.instanceOf, typeof(Claymation), true);
			
			if(instance != claymation.instanceOf && instance != claymation){
				claymation.instanceOf = instance;
			}
			
			if(claymation.instanceOf != null){
				return;
			}

			EditorGUI.BeginChangeCheck();

			TextAsset claymationFile = (TextAsset)EditorGUILayout.ObjectField(new GUIContent("claymation asset", ""), claymation.claymationFile, typeof(TextAsset), true);
			
			if(EditorGUI.EndChangeCheck()){
				if(claymation.claymationFile != claymationFile){
					claymation.claymationFile = claymationFile;
					claymation.init();
				}
			}

			if(GUILayout.Button(new GUIContent("reload file", "click this when your claymation file changes and you need to refresh this player"))){
				claymation.init();
			}

			Material material = (Material)EditorGUILayout.ObjectField(new GUIContent("material", "Use the same materials you use on the clayContainers"), claymation.material, typeof(Material), true);
			claymation.material = material;

			if(claymation.getNumFrames() > 1){
				int frameRate = EditorGUILayout.IntField(new GUIContent("frame rate", "how fast will this claymation play"), claymation.frameRate);

				claymation.frameRate = frameRate;

				claymation.playAnim = EditorGUILayout.Toggle(new GUIContent("play anim", "Always play the anim in loop"), claymation.playAnim);
				
				int currentFrame = EditorGUILayout.IntField(new GUIContent("frame", ""), claymation.getFrame());
				if(currentFrame != claymation.getFrame()){
					claymation.loadFrame(currentFrame);
				}
			}
		}
	}
}

#endif // end if UNITY_EDITOR
