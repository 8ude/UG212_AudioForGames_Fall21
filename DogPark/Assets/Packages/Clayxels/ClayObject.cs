using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Clayxels;

namespace Clayxels{

	/* ClayObject are nested inside a ClayContainer to form sculptures.
		A ClayObject can spawn one or multiple Clayxels.Solid objects to form more complex structures 
		like splines or duplicated offsetted patterns.
	*/
	[ExecuteInEditMode]
	public class ClayObject : MonoBehaviour{
		public enum ClayObjectMode{
			single,
			offset,
			spline
		}

		public float blend = 0.0f;
		public Color color;
		public Vector4 attrs = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
		public int primitiveType = 0;
		public ClayObjectMode mode = ClayObjectMode.single;
		public GameObject offsetter = null;
		public List<GameObject> splinePoints = new List<GameObject>();

		public int clayObjectId = 0;

		public WeakReference clayxelContainerRef = null;

		[SerializeField] int numSolids = 1;
		[SerializeField]int splineSubdiv = 3;
		List<Solid> solids = new List<Solid>();
		bool invalidated = false;
		Color gizmoColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);

		void Awake(){
			ClayContainer container = this.getClayContainer();
			if(container != null){
				container.scheduleClayObjectsScan();
			}
		}

		void Update(){
			this.updateSolids(true);

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					ClayContainer parentContainer = this.GetComponentInParent<ClayContainer>();
					if(parentContainer != this.getClayContainer()){
						this.init();
					}
				}
			#endif
		}

		public void forceUpdate(){
			this.transform.hasChanged = true;

			this.updateSolids(true);
		}

		public void pullUpdate(){
			this.updateSolids(false);
		}

		public Solid getSolid(int id){
			return this.solids[id];
		}

		public int getNumSolids(){
			return this.solids.Count;
		}

		public void setOffsetNum(int num){
			this.numSolids = num;
			this.init();
			this.forceUpdate();

			this.getClayContainer().scheduleClayObjectsScan();
		}

		public int getSplineSubdiv(){
			return this.splineSubdiv;
		}

		public void setSplineSubdiv(int num){
			this.splineSubdiv = num;

			this.updateSplineSubdiv();

			this.init();
			this.forceUpdate();

			this.getClayContainer().scheduleClayObjectsScan();
		}

		public void init(){
			this.solids.Clear();
			this.changeNumSolids(this.numSolids);
			this.transform.hasChanged = true;
			
			ClayContainer oldContainer = null;
			if(this.clayxelContainerRef != null){
				oldContainer = (ClayContainer)this.clayxelContainerRef.Target;
			}

			this.clayxelContainerRef = null;
			if(this.transform.parent == null){
				return;
			}

			GameObject parent = this.transform.parent.gameObject;

			ClayContainer clayxel = null;
			for(int i = 0; i < 100; ++i){
				clayxel = parent.GetComponent<ClayContainer>();
				if(clayxel != null){
					break;
				}
				else{
					parent = parent.transform.parent.gameObject;
				}
			}

			if(clayxel == null){
				Debug.Log("failed to find parent clayxel container");
			}
			else{
				this.clayxelContainerRef = new WeakReference(clayxel);

				if(oldContainer != null && oldContainer != clayxel){
					clayxel.scheduleClayObjectsScan();
				}
			}
		}

		public void mirrorDuplicate(){
			ClayObject mirrorClayObj = null;
			
			ClayObject[] clayObjs = this.getClayContainer().GetComponentsInChildren<ClayObject>();
			for(int i = 0; i < clayObjs.Length; ++i){
				ClayObject clayObj = clayObjs[i];
				if(clayObj.name == "mirror_" + this.name){
					if(clayObj.blend == this.blend && clayObj.attrs == this.attrs){
						mirrorClayObj = clayObj;
						break;
					}
				}
			}

			if(mirrorClayObj == null){
				mirrorClayObj = Instantiate(this.gameObject, this.getClayContainer().transform).GetComponent<ClayObject>();
				mirrorClayObj.name = "mirror_" + this.name;
			}

			Matrix4x4 mirrorMat = Matrix4x4.Scale(new Vector3(-1.0f, 1.0f, 1.0f));
			mirrorMat = mirrorMat * this.transform.localToWorldMatrix;
			mirrorClayObj.transform.localScale = mirrorMat.lossyScale;
			mirrorClayObj.transform.position = new Vector3(mirrorMat[0,3], mirrorMat[1,3], mirrorMat[2,3]);
			mirrorClayObj.transform.rotation = mirrorMat.rotation;

			this.getClayContainer().clayObjectUpdated(mirrorClayObj);
		}

		public bool isValid(){
			return !this.invalidated;
		}	

		public void setMode(ClayObjectMode mode){
			if(mode == this.mode){
				return;
			}

			this.mode = mode;

			if(this.mode == ClayObjectMode.offset){
				if(this.offsetter == null){
					GameObject offsetObj = new GameObject("offsetter");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 30.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					
					this.offsetter = offsetObj;
				}

				this.numSolids = 3;
			}
			else if(this.mode == ClayObjectMode.spline){
				if(this.splinePoints.Count == 0){
					GameObject offsetObj = new GameObject("splinePnt1");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					this.splinePoints.Add(offsetObj);
					this.splinePoints.Add(offsetObj);

					offsetObj = new GameObject("splinePnt2");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(2.0f, 1.0f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					this.splinePoints.Add(offsetObj);

					offsetObj = new GameObject("splinePnt3");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(3.0f, 0.0f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					this.splinePoints.Add(offsetObj);

					offsetObj = new GameObject("splinePnt4");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(4.0f, 0.0f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					this.splinePoints.Add(offsetObj);
					this.splinePoints.Add(offsetObj);
				}

				this.updateSplineSubdiv();
			}
			else{
				this.numSolids = 1;
			}

			this.init();

			this.forceUpdate();

			this.getClayContainer().scheduleClayObjectsScan();
		}

		public GameObject addSplineControlPoint(){
			GameObject prevLastPoint = this.splinePoints[this.splinePoints.Count - 3];
			GameObject lastPoint = this.splinePoints[this.splinePoints.Count - 1];

			Vector3 delta = (lastPoint.transform.position - prevLastPoint.transform.position);
			delta.Normalize();

			GameObject offsetObj = new GameObject("splinePnt" + (this.splinePoints.Count-1));
			offsetObj.transform.parent = this.transform;
			offsetObj.transform.position = lastPoint.transform.position + delta;
			offsetObj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
			offsetObj.transform.localScale = lastPoint.transform.localScale;

			this.splinePoints[this.splinePoints.Count - 1] = offsetObj;

			this.splinePoints.Add(offsetObj);

			this.updateSplineSubdiv();

			this.init();

			this.forceUpdate();

			this.getClayContainer().scheduleClayObjectsScan();

			return offsetObj;
		}

		public void removeLastSplineControlPoint(){
			GameObject controlPoint = this.splinePoints[this.splinePoints.Count - 1];
			DestroyImmediate(controlPoint);

			this.splinePoints.RemoveAt(this.splinePoints.Count - 1);

			this.splinePoints[this.splinePoints.Count - 1] = this.splinePoints[this.splinePoints.Count - 2];

			this.updateSplineSubdiv();

			this.init();

			this.forceUpdate();

			this.getClayContainer().scheduleClayObjectsScan();
		}

		public ClayContainer getClayContainer(){
			if(this.clayxelContainerRef == null){
				if(this.invalidated){
					return null;
				}

				this.init();

				if(this.clayxelContainerRef == null){
					return null;
				}
			}

			return (ClayContainer)this.clayxelContainerRef.Target;
		}

		public void setClayxelContainer(ClayContainer container){
			this.clayxelContainerRef = new WeakReference(container);
		}

		public void setPrimitiveType(int primType){
			this.primitiveType = primType;
		}

		public Color getColor(){
			return this.color;
		}

		// end of public interface //

		void updateSolids(bool notifyContainer){
			if(this.mode == ClayObjectMode.single){
				if(this.transform.hasChanged){
					this.transform.hasChanged = false;

					this.updateSingle();

					if(notifyContainer){
						this.getClayContainer().clayObjectUpdated(this);
					}
				}
			}
			else if(this.mode == ClayObjectMode.offset){
				if(this.transform.hasChanged || this.offsetter.transform.hasChanged){
					this.transform.hasChanged = false;
					this.offsetter.transform.hasChanged = false;

					this.updateOffset();

					if(notifyContainer){
						this.getClayContainer().clayObjectUpdated(this);
					}
				}
			}
			else if(this.mode == ClayObjectMode.spline){
				bool changed = false;

				if(this.transform.hasChanged){
					changed = true;
				}
				else{
					for(int i = 0; i < this.splinePoints.Count; ++i){
						try{
							if(this.splinePoints[i].transform.hasChanged){
								changed = true;
								break;
							}
						}
						catch{
							this.repairSplineControlPoints();

							return;
						}
					}
				}

				if(changed){
					this.transform.hasChanged = false;

					try{
						this.updateSpline();
					}
					catch{
						this.repairSplineControlPoints();

						return;
					}

					if(notifyContainer){
						this.getClayContainer().clayObjectUpdated(this);
					}
				}
			}
		}

		void changeNumSolids(int num){
			this.solids = new List<Solid>(num);

			for(int i = 0; i < num; ++i){
				this.solids.Add(new Solid());
			}
		}

		void repairSplineControlPoints(){
			this.splinePoints.Clear();

			for(int i = 0; i < this.transform.childCount; ++i){
				GameObject controlPnt = this.transform.GetChild(i).gameObject;
				if(controlPnt.name.StartsWith("splinePnt")){
					this.splinePoints.Add(controlPnt);

					if(this.splinePoints.Count == 1){
						this.splinePoints.Add(controlPnt);				
					}

					controlPnt.name = "splinePnt" + (this.splinePoints.Count - 1);
				}
			}

			this.splinePoints.Add(this.splinePoints[this.splinePoints.Count - 1]);

			this.updateSplineSubdiv();

			this.init();

			this.forceUpdate();

			this.getClayContainer().scheduleClayObjectsScan();
		}
		
		void OnDestroy(){
			this.invalidated = true;
			
			ClayContainer clayxel = this.getClayContainer();
			if(clayxel != null){
				clayxel.scheduleClayObjectsScan();

				#if UNITY_EDITOR
					if(!Application.isPlaying){
						UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
					}
				#endif
			}
		}

		void updateSplineSubdiv(){
			if(this.splineSubdiv < 0){
				this.splineSubdiv = 0;
			}

			if(this.splineSubdiv > 20){
				this.splineSubdiv = 20;
			}

			this.numSolids = this.splineSubdiv * (this.splinePoints.Count - 3);
			if(this.numSolids < 0){
				this.numSolids = 0;
			}
		}

		void updateSingle(){
			if(this.solids.Count == 0){
				return;
			}

			Solid solid = this.solids[0];

			ClayContainer container = this.getClayContainer();
			Matrix4x4 invMat = container.transform.worldToLocalMatrix * this.transform.localToWorldMatrix;

			solid.position.x = invMat[0, 3];
			solid.position.y = invMat[1, 3];
			solid.position.z = invMat[2, 3];

			solid.rotation = invMat.rotation;

			solid.scale.x = Mathf.Abs((this.transform.lossyScale.x / container.transform.lossyScale.x) * 0.5f);
			solid.scale.y = Mathf.Abs((this.transform.lossyScale.y / container.transform.lossyScale.y) * 0.5f);
			solid.scale.z = Mathf.Abs((this.transform.lossyScale.z / container.transform.lossyScale.z) * 0.5f);
			
			solid.blend = this.blend;
			
			solid.color.x = this.color.r;
			solid.color.y = this.color.g;
			solid.color.z = this.color.b;

			solid.attrs = this.attrs;

			solid.primitiveType = this.primitiveType;
		}

		void updateOffset(){
			ClayContainer container = this.getClayContainer();
			Matrix4x4 invMat = container.transform.worldToLocalMatrix * this.transform.localToWorldMatrix;

			Vector3 offsetPos = new Vector3(invMat[0, 3], invMat[1, 3], invMat[2, 3]);
			Quaternion offsetRot = invMat.rotation;
			Vector3 offsetScale;
			offsetScale.x = (this.transform.lossyScale.x / container.transform.lossyScale.x) * 0.5f;
			offsetScale.y = (this.transform.lossyScale.y / container.transform.lossyScale.y) * 0.5f;
			offsetScale.z = (this.transform.lossyScale.z / container.transform.lossyScale.z) * 0.5f;

			Vector3 offsetterPos;
			for(int i = 0; i < this.solids.Count; ++i){
				Solid solid = this.solids[i];

				solid.position = offsetPos;

				offsetterPos = this.offsetter.transform.localPosition;
				offsetterPos.x *= this.offsetter.transform.lossyScale.x / container.transform.lossyScale.x;
				offsetterPos.y *= this.offsetter.transform.lossyScale.y / container.transform.lossyScale.y;
				offsetterPos.z *= this.offsetter.transform.lossyScale.z / container.transform.lossyScale.z;

				offsetPos += offsetRot * offsetterPos;

				solid.rotation = offsetRot;

				offsetRot = offsetRot * this.offsetter.transform.localRotation;

				solid.scale = offsetScale;

				offsetScale.x *= this.offsetter.transform.localScale.x;
				offsetScale.y *= this.offsetter.transform.localScale.y;
				offsetScale.z *= this.offsetter.transform.localScale.z;

				solid.blend = this.blend;
				
				solid.color.x = this.color.r;
				solid.color.y = this.color.g;
				solid.color.z = this.color.b;

				solid.attrs = this.attrs;

				solid.primitiveType = this.primitiveType;
			}
		}

		void updateSpline(){
			if(this.splinePoints.Count > 3){
				float incrT = 1.0f / this.splineSubdiv;

				int solidIt = 0;

				ClayContainer container = this.getClayContainer();
				Matrix4x4 parentInvMat = container.transform.worldToLocalMatrix;
				
				for(int i = 0; i < this.splinePoints.Count - 3; ++i){
					GameObject splinePoint0 = this.splinePoints[i];
					GameObject splinePoint1 = this.splinePoints[i + 1];
					GameObject splinePoint2 = this.splinePoints[i + 2];
					GameObject splinePoint3 = this.splinePoints[i + 3];

					splinePoint0.transform.hasChanged = false;
					splinePoint1.transform.hasChanged = false;
					splinePoint2.transform.hasChanged = false;
					splinePoint3.transform.hasChanged = false;

					Vector3 s0;
					s0.x = splinePoint0.transform.lossyScale.x / container.transform.lossyScale.x;
					s0.y = splinePoint0.transform.lossyScale.y / container.transform.lossyScale.y;
					s0.z = splinePoint0.transform.lossyScale.z / container.transform.lossyScale.z;

					Vector3 s1;
					s1.x = splinePoint1.transform.lossyScale.x / container.transform.lossyScale.x;
					s1.y = splinePoint1.transform.lossyScale.y / container.transform.lossyScale.y;
					s1.z = splinePoint1.transform.lossyScale.z / container.transform.lossyScale.z;

					Vector3 s2;
					s2.x = splinePoint2.transform.lossyScale.x / container.transform.lossyScale.x;
					s2.y = splinePoint2.transform.lossyScale.y / container.transform.lossyScale.y;
					s2.z = splinePoint2.transform.lossyScale.z / container.transform.lossyScale.z;

					Vector3 s3;
					s3.x = splinePoint3.transform.lossyScale.x / container.transform.lossyScale.x;
					s3.y = splinePoint3.transform.lossyScale.y / container.transform.lossyScale.y;
					s3.z = splinePoint3.transform.lossyScale.z / container.transform.lossyScale.z;

					Matrix4x4 pointMat0 = parentInvMat * splinePoint0.transform.localToWorldMatrix;
					Matrix4x4 pointMat1 = parentInvMat * splinePoint1.transform.localToWorldMatrix;
					Matrix4x4 pointMat2 = parentInvMat * splinePoint2.transform.localToWorldMatrix;
					Matrix4x4 pointMat3 = parentInvMat * splinePoint3.transform.localToWorldMatrix;

					Vector3 point0 = new Vector3(pointMat0[0, 3], pointMat0[1, 3], pointMat0[2, 3]);
					Vector3 point1 = new Vector3(pointMat1[0, 3], pointMat1[1, 3], pointMat1[2, 3]);
					Vector3 point2 = new Vector3(pointMat2[0, 3], pointMat2[1, 3], pointMat2[2, 3]);
					Vector3 point3 = new Vector3(pointMat3[0, 3], pointMat3[1, 3], pointMat3[2, 3]);

					for(int j = 0; j < this.splineSubdiv; ++j){
						float t = incrT * j;

						Solid solid = this.solids[solidIt];
						solid.position = this.getCatmullRomVec3(point0, point1, point2, point3, t);
						solid.rotation = this.getCatmullRomQuat(pointMat0.rotation, pointMat1.rotation, pointMat2.rotation, pointMat3.rotation, t);
						solid.scale = this.getCatmullRomVec3(s0, s1, s2, s3, t) * 0.5f;

						solid.blend = this.blend;
						solid.attrs = this.attrs;
						solid.primitiveType = this.primitiveType;

						solid.color.x = this.color.r;
						solid.color.y = this.color.g;
						solid.color.z = this.color.b;

						solidIt += 1;
					}
				}
			}
		}

		Vector3 getCatmullRomVec3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t){
			//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
			Vector3 a = 2f * p1;
			Vector3 b = p2 - p0;
			Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
			Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

			//The cubic polynomial: a + b * t + c * t^2 + d * t^3
			Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

			return pos;
		}

		Quaternion getCatmullRomQuat(Quaternion p0, Quaternion p1, Quaternion p2, Quaternion p3, float t){
			Quaternion a;
			a.x = p1.x * 2.0f;
			a.y = p1.y * 2.0f;
			a.z = p1.z * 2.0f;
			a.w = p1.w * 2.0f;

			Quaternion b;
			b.x = p2.x - p0.x;
			b.y = p2.y - p0.y;
			b.z = p2.z - p0.z;
			b.w = p2.w - p0.w;

			Quaternion c;
			c.x = 2.0f * p0.x - 5.0f * p1.x + 4.0f * p2.x - p3.x;
			c.y = 2.0f * p0.y - 5.0f * p1.y + 4.0f * p2.y - p3.y;
			c.z = 2.0f * p0.z - 5.0f * p1.z + 4.0f * p2.z - p3.z;
			c.w = 2.0f * p0.w - 5.0f * p1.w + 4.0f * p2.w - p3.w;

			Quaternion d;
			d.x = -p0.x + 3.0f * p1.x - 3.0f * p2.x + p3.x;
			d.y = -p0.y + 3.0f * p1.y - 3.0f * p2.y + p3.y;
			d.z = -p0.z + 3.0f * p1.z - 3.0f * p2.z + p3.z;
			d.w = -p0.w + 3.0f * p1.w - 3.0f * p2.w + p3.w;

			Quaternion rot;
			float pow2 = t * t;
			float pow3 = t * t * t;
			rot.x = 0.5f * (a.x + (b.x * t) + (c.x * pow2) + (d.x * pow3));
			rot.y = 0.5f * (a.y + (b.y * t) + (c.y * pow2) + (d.y * pow3));
			rot.z = 0.5f * (a.z + (b.z * t) + (c.z * pow2) + (d.z * pow3));
			rot.w = 0.5f * (a.w + (b.w * t) + (c.w * pow2) + (d.w * pow3));

			return rot.normalized;
		}

		#if UNITY_EDITOR

		public virtual void onInspectorGUI(){
			Debug.Log("insp clayObj");
		}

		void OnDrawGizmos(){
			if(this.blend < 0.0f || // negative shape?
				(((int)this.attrs.w >> 0)&1) == 1){// painter?

				if(UnityEditor.Selection.Contains(this.gameObject)){// if selected draw wire cage
					Gizmos.color = this.gizmoColor;
					if(this.primitiveType == 0){
						Gizmos.matrix = this.transform.localToWorldMatrix;
						Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
					}
					else if(this.primitiveType == 1){
						Gizmos.matrix = this.transform.localToWorldMatrix;
						Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
					}
					else if(this.primitiveType == 2){
						this.drawCylinder();
					}
					else if(this.primitiveType == 3){
						this.drawTorus();
					}
					else if(this.primitiveType == 4){
						this.drawCurve();
					}
					else if(this.primitiveType == 5){
						Gizmos.matrix = this.transform.localToWorldMatrix;
						Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
					}
					else if(this.primitiveType == 6){
						Gizmos.matrix = this.transform.localToWorldMatrix;
						Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
					}
				}
			}
		}

		void drawCurve(){
			Handles.color = Color.white;
			
			float radius = this.attrs.z * 0.5f;
			Vector3 heightVec = (this.transform.up * (this.transform.lossyScale.y - this.attrs.z)) * 0.5f;
			Vector3 sideVec = this.transform.right * ((this.transform.lossyScale.x*0.5f) - radius);
			Vector3 startPnt = this.transform.position - sideVec - heightVec;
			Vector3 endPnt = this.transform.position + sideVec - heightVec;
			Vector3 tanOffset = this.transform.right * - (this.transform.lossyScale.x * 0.2f);
			Vector3 tanOffset2 = this.transform.up * (radius * 0.5f);
			Vector3 tanSlide = this.transform.right * ((this.attrs.x - 0.5f) * (this.transform.lossyScale.x * 0.5f));
			Vector3 startTan = this.transform.position + heightVec + tanOffset + tanOffset2 + tanSlide;
			Vector3 endTan = this.transform.position + heightVec - tanOffset + tanOffset2 + tanSlide;
			Vector3 elongVec =  this.transform.forward * ((this.transform.lossyScale.z * 0.5f) - radius);
			Vector3 elongVec2 =  this.transform.forward * ((this.transform.lossyScale.z * 0.5f) - (radius*2.0f));

			float w0 = (1.0f - this.attrs.y) * 2.0f;
			float w1 = this.attrs.y * 2.0f;

			Handles.DrawBezier(startPnt - (elongVec*w0), endPnt - (elongVec*w1), startTan - elongVec, endTan - elongVec, Color.white, null, 2.0f);
			Handles.DrawBezier(startPnt + (elongVec*w0), endPnt + (elongVec*w1), startTan + elongVec, endTan + elongVec, Color.white, null, 2.0f);

			Gizmos.DrawWireSphere(startPnt - elongVec2, radius * w0);
			Gizmos.DrawWireSphere(endPnt - elongVec2, radius * w1);

			if(this.transform.lossyScale.z > 1.0f){
				Gizmos.DrawWireSphere(startPnt + elongVec2, radius);
				Gizmos.DrawWireSphere(endPnt + elongVec2, radius);

				Handles.DrawLine(
					startPnt + elongVec2 - (this.transform.right * radius), 
					startPnt - elongVec2 - (this.transform.right * radius));

				Handles.DrawLine(
					endPnt + elongVec2 + (this.transform.right * radius), 
					endPnt - elongVec2 + (this.transform.right * radius));
			}
		}

		void drawTorus(){
			Handles.color = Color.white;

			float radius = this.attrs.x;

			Vector3 elongationVec = this.transform.forward * ((this.transform.lossyScale.z * 0.5f) - radius);
			Vector3 sideVec = this.transform.right * ((this.transform.lossyScale.x * 0.5f) - radius);
			Vector3 radiusSideOffsetVec = this.transform.right * radius;
			Vector3 heightVec = this.transform.up * ((this.transform.lossyScale.y * 0.5f) - radius);
			Vector3 radiusUpOffsetVec = this.transform.up * radius;
			Vector3 sideCrossSecVec = this.transform.right * (this.transform.lossyScale.x * 0.5f);

			float crossSecRadius = this.transform.lossyScale.x * 0.5f;
			Vector3 radiusCrossSecVec = this.transform.up * crossSecRadius;
			Vector3 heightCrossSecVec = this.transform.up * ((this.transform.lossyScale.y *0.5f) - crossSecRadius);

			float crossSecRadiusIn = (this.transform.lossyScale.x * 0.5f) - (radius*2.0f);
			Vector3 sideCrossSecVecIn = this.transform.right * ((this.transform.lossyScale.x * 0.5f) - (radius * 2.0f));

			if(this.transform.lossyScale.y >= this.transform.lossyScale.x){
				// cross out section
				Handles.DrawWireArc(this.transform.position + heightCrossSecVec, 
					this.transform.forward, this.transform.right, 180.0f, crossSecRadius);

				Handles.DrawWireArc(this.transform.position - heightCrossSecVec, 
					this.transform.forward, this.transform.right, -180.0f, crossSecRadius);

				Handles.DrawLine(
					this.transform.position + heightCrossSecVec + sideCrossSecVec, 
					this.transform.position - heightCrossSecVec + sideCrossSecVec);

				Handles.DrawLine(
					this.transform.position + heightCrossSecVec - sideCrossSecVec, 
					this.transform.position - heightCrossSecVec - sideCrossSecVec);

				// cross in section
				Handles.DrawWireArc(this.transform.position + heightCrossSecVec, 
					this.transform.forward, this.transform.right, 180.0f, crossSecRadiusIn);

				Handles.DrawWireArc(this.transform.position - heightCrossSecVec, 
					this.transform.forward, this.transform.right, -180.0f, crossSecRadiusIn);

				Handles.DrawLine(
					this.transform.position + heightCrossSecVec + sideCrossSecVecIn, 
					this.transform.position - heightCrossSecVec + sideCrossSecVecIn);

				Handles.DrawLine(
					this.transform.position + heightCrossSecVec - sideCrossSecVecIn, 
					this.transform.position - heightCrossSecVec - sideCrossSecVecIn);
			}

			if(this.transform.lossyScale.z >= radius * 2.0f){
				// top section
				Handles.DrawWireArc(this.transform.position - elongationVec + heightVec, 
					this.transform.right, this.transform.up, -180.0f, radius);

				Handles.DrawWireArc(this.transform.position + elongationVec + heightVec, 
					this.transform.right, this.transform.up, 180.0f, radius);

				Handles.DrawLine(
					this.transform.position + elongationVec + heightVec + radiusUpOffsetVec , 
					this.transform.position - elongationVec + heightVec + radiusUpOffsetVec);

				Handles.DrawLine(
					this.transform.position + elongationVec + heightVec - radiusUpOffsetVec , 
					this.transform.position - elongationVec + heightVec - radiusUpOffsetVec);

				// bottom section
				Handles.DrawWireArc(this.transform.position - elongationVec - heightVec, 
					this.transform.right, this.transform.up, -180.0f, radius);

				Handles.DrawWireArc(this.transform.position + elongationVec - heightVec, 
					this.transform.right, this.transform.up, 180.0f, radius);

				Handles.DrawLine(
					this.transform.position + elongationVec - heightVec + radiusUpOffsetVec , 
					this.transform.position - elongationVec - heightVec + radiusUpOffsetVec);

				Handles.DrawLine(
					this.transform.position + elongationVec - heightVec - radiusUpOffsetVec , 
					this.transform.position - elongationVec - heightVec - radiusUpOffsetVec);

				// left section
				Handles.DrawWireArc(this.transform.position - elongationVec - sideVec, 
					this.transform.up, this.transform.right, 180.0f, radius);

				Handles.DrawWireArc(this.transform.position + elongationVec - sideVec, 
					this.transform.up, this.transform.right, -180.0f, radius);

				Handles.DrawLine(
					this.transform.position + elongationVec - sideVec + radiusSideOffsetVec , 
					this.transform.position - elongationVec - sideVec + radiusSideOffsetVec);

				Handles.DrawLine(
					this.transform.position + elongationVec - sideVec - radiusSideOffsetVec, 
					this.transform.position - elongationVec - sideVec - radiusSideOffsetVec);

				// right section
				Handles.DrawWireArc(this.transform.position - elongationVec + sideVec, 
					this.transform.up, this.transform.right, 180.0f, radius);

				Handles.DrawWireArc(this.transform.position + elongationVec + sideVec, 
					this.transform.up, this.transform.right, -180.0f, radius);

				Handles.DrawLine(
					this.transform.position + elongationVec + sideVec + radiusSideOffsetVec , 
					this.transform.position - elongationVec + sideVec + radiusSideOffsetVec);

				Handles.DrawLine(
					this.transform.position + elongationVec + sideVec - radiusSideOffsetVec, 
					this.transform.position - elongationVec + sideVec - radiusSideOffsetVec);
			}
		}

		void drawCylinder(){
			Handles.color = Color.white;
			
			float radius = this.transform.lossyScale.x;
			if(this.transform.lossyScale.z < radius){
				radius = this.transform.lossyScale.z;
			}

			radius *= 0.5f;

			Vector3 arcDir = this.transform.right;
			Vector3 extVec = - (this.transform.forward * ((this.transform.lossyScale.z * 0.5f) - radius));
			if(this.transform.lossyScale.z < this.transform.lossyScale.x){
				arcDir = this.transform.forward;
				extVec = (this.transform.right * ((this.transform.lossyScale.x*0.5f) - radius));
			}

			Vector3 heightVec = this.transform.up * (this.transform.lossyScale.y * 0.5f);

			// draw top
			Handles.DrawWireArc(this.transform.position + extVec + heightVec, this.transform.up, arcDir, 180.0f, radius);
			Handles.DrawWireArc(this.transform.position - extVec + heightVec, this.transform.up, arcDir, -180.0f, radius);

			Handles.DrawLine(
				this.transform.position + extVec + heightVec + (arcDir*radius), 
				this.transform.position - extVec + heightVec + (arcDir*radius));

			Handles.DrawLine(
				this.transform.position + extVec + heightVec - (arcDir*radius), 
				this.transform.position - extVec + heightVec - (arcDir*radius));

			// draw bottom
			Handles.DrawWireArc(this.transform.position + extVec - heightVec, this.transform.up, arcDir, 180.0f, radius+this.attrs.z);
			Handles.DrawWireArc(this.transform.position - extVec - heightVec, this.transform.up, arcDir, -180.0f, radius+this.attrs.z);
			
			Handles.DrawLine(
				this.transform.position + extVec - heightVec - (arcDir*(radius+this.attrs.z)), 
				this.transform.position - extVec - heightVec - (arcDir*(radius+this.attrs.z)));

			Handles.DrawLine(
				this.transform.position + extVec - heightVec + (arcDir*(radius+this.attrs.z)), 
				this.transform.position - extVec - heightVec + (arcDir*(radius+this.attrs.z)));

			// draw side lines
			Handles.DrawLine(
				this.transform.position + heightVec + (arcDir*radius), 
				this.transform.position - heightVec + (arcDir*(radius+this.attrs.z)));

			Handles.DrawLine(
				this.transform.position + heightVec - (arcDir*radius), 
				this.transform.position - heightVec - (arcDir*(radius+this.attrs.z)));
		}
		#endif // end if UNITY_EDITOR 
	}
}
