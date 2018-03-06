//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System;
using System.Collections.Generic;
#if !UNITY_WSA
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Diagnostics;
#endif
using System.Runtime.CompilerServices;
using UnityEngine;
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public enum LODLevels {
		Full =0,
		DAQRI =350,
		HoloLens =800,

		e100k =100,
		e200k =200,
		e300k =300,
		e400k =400,
		e500k =500,
		e750k =750,
		e1000k =1000,
		e1500k =1500,
		e2000k =2000,
		e3000k =3000,
		e4000k =4000,
		e5000k =5000
	}

	public interface IForgeLoaderInterface {

		string BEARER { get; set; }
		string URN { get; set; }
		string SCENEID { get; set; }
		bool LoadMetadata { get; set; }
		bool LoadMesh { get; set; }
		bool CreateCollider { get; set; }
		GameObject ROOT { get; set; }
		string PROJECTID { get; set; }

		void Start () ;
		void Awake () ;
		void Update () ;

	}

	public class ForgeLoaderEngine : IForgeLoaderInterface {

		#region Events
		public delegate void ProcessedNodesDelegate (ForgeLoaderEngine sender, float pct) ;
		public event ProcessedNodesDelegate ProcessedNodes ;

		public delegate void ProcessingNodesCompletedDelegate (ForgeLoaderEngine sender, int unprocessedObjects) ;
		public event ProcessingNodesCompletedDelegate ProcessingNodesCompleted ;

		//public delegate void ProcessingNodesErrorDelegate (ForgeLoaderEngine sender) ;
		//public event ProcessingNodesErrorDelegate ProcessingNodesError ;

		#endregion

		#region Properties
		public string BEARER { get; set; }
		public string URN { get; set; }
		public string SCENEID { get; set; }
		private bool _LoadMetadata = true;
		public bool LoadMetadata {
			get {
				return (_LoadMetadata) ;
			}
			set {
				_LoadMetadata =value ;
			}
		}
		private bool _LoadMesh =true ;
		public bool LoadMesh {
			get {
				return (_LoadMesh) ;
			}
			set {
				_LoadMesh =value ;
			}
		}
		private bool _CreateCollider = false;
		public bool CreateCollider {
			get {
				return (_CreateCollider);
			}
			set {
				_CreateCollider = value;
			}
		}
		public GameObject ROOT { get; set; }
		public string PROJECTID { get; set; }
		public bool SaveToDisk { get; set; }
		internal bool Active { get; set; }

		#endregion

		#region Fields
		protected RequestQueueMgr _mgr =new RequestQueueMgr () ;
		public Dictionary<int, Material> _materialLib =new Dictionary<int, Material> () ;
		public Dictionary<int, ForgeProperties> _properties =new Dictionary<int, ForgeProperties> () ;
		private DateTime started ;

		#endregion

		#region Unity APIs
		public virtual void Awake () { // Awake is called before Start()
#if !UNITY_WSA
			//Debug.Log (System.Environment.Version) ; // 2.0
			//ServicePointManager.CertificatePolicy =new NoCheckCertificatePolicy () ;
			SSLValidator.OverrideValidation () ;
#endif
			if ( ProcessedNodes != null )
				ProcessedNodes (this, 0.0f) ;
			started =DateTime.Now ;
			requestScene () ;
			Active =true ;
		}

		public virtual void Start () {
		}

		public virtual void Update () {
			//UnityEngine.Profiling.Profiler.BeginSample ("Forge AR|VR Toolkit");
			if ( !Active )
				return;
			// Do we got more requests to fire?
			int pending =_mgr.Count (SceneLoadingStatus.ePending) ;
			if ( pending < ForgeLoaderConstants.NB_MAX_REQUESTS ) {
				RequestQueueMgrEnumerator news =_mgr.GetTypeEnumerator (SceneLoadingStatus.eNew) ;
				pending =_mgr.Count (SceneLoadingStatus.eNew) ;
				if ( news.MoveNext () ) {
					//UnityEngine.Debug.Log (DateTime.Now.ToString ("HH:mm:ss.f") + " / " + ((IRequestInterface)news.Current).uri.ToString ());
					_mgr.FireRequest (news.Current) ;
				}
			}

			// For each request we got an answer for, we build the corresponding scene object
			RequestQueueMgrEnumerator items =_mgr.GetCompletedEnumerator () ;
			while ( items.MoveNext () ) {
				IRequestInterface item =items.Current ;

				if ( item.resolved == SceneLoadingStatus.eInstanceTree )
					item.fireRequestCallback += requestSceneObjectDetails ;
				else if ( item.resolved == SceneLoadingStatus.eMaterial )
					item.fireRequestCallback += requestTextures ;

				GameObject obj =item.BuildScene (
					item.resolved == SceneLoadingStatus.eInstanceTree ?
						SCENEID : item.GetName (),
					SaveToDisk
				) ;
				if ( obj != null && item.resolved == SceneLoadingStatus.eInstanceTree ) {
					if ( ROOT == null ) {
						ROOT = obj ;
					} else {
						obj.transform.parent =ROOT.transform ;
						obj.transform.localPosition =Vector3.zero ;
					}
				} else if ( item.resolved == SceneLoadingStatus.eMaterial ) { // Safe as we make only 1 request per material
					_materialLib.Add ((item as MaterialRequest).matId, (item as MaterialRequest).material) ;
				} else if ( item.resolved == SceneLoadingStatus.eProperties ) { // Safe as we make only 1 request per propertyset
					_properties.Add ((item as PropertiesRequest).dbId, (item as PropertiesRequest).properties) ;
				} // else if ( item.resolved == SceneLoadingStatus.eTexture )

				break ;
			}

			// Assign Material to Mesh waiting for it
			items =_mgr.GetTypeEnumerator (SceneLoadingStatus.eWaitingMaterial) ;
			while ( items.MoveNext () ) {
				MeshRequest item = items.Current as MeshRequest ;
				if ( !_materialLib.ContainsKey (item.materialId) )
					continue ;
				MeshRenderer renderer =item.gameObject.GetComponent<MeshRenderer> () ;
				renderer.sharedMaterial =_materialLib [item.materialId] ;
				item.state =item.resolved ;
			}

			// Showing progress
			if ( ProcessedNodes != null ) {
				int total =_mgr.Count () ;
				int built =_mgr.Count (new SceneLoadingStatus [] {
					SceneLoadingStatus.eInstanceTree, SceneLoadingStatus.eMesh, SceneLoadingStatus.eMaterial, SceneLoadingStatus.eTexture, SceneLoadingStatus.eProperties,
					SceneLoadingStatus.eWaitingMaterial, SceneLoadingStatus.eWaitingTexture
				}) ;
				int completed =_mgr.Count (SceneLoadingStatus.eReceived) ;

				float val =100.0f * Math.Min (completed + built, total) / total ;
				ProcessedNodes (this, val) ;

				if ( ProcessingNodesCompleted != null ) {
					built =_mgr.Count (new SceneLoadingStatus [] {
						SceneLoadingStatus.eInstanceTree, SceneLoadingStatus.eMesh, SceneLoadingStatus.eMaterial, SceneLoadingStatus.eTexture, SceneLoadingStatus.eProperties,
					}) ;
					int unprocessed =_mgr.Count (new SceneLoadingStatus [] {
						SceneLoadingStatus.eCancelled, SceneLoadingStatus.eError,
					}) ;

					if ( built + unprocessed == total ) {
						ProcessingNodesCompleted (this, unprocessed) ;
						TimeSpan tm =DateTime.Now - started ;
						UnityEngine.Debug.Log (URN + "-" + SCENEID + " loaded in: " + tm.TotalSeconds.ToString ()) ;
						Sleep () ; // Sleep ourself
					}
				}
			}

			//UnityEngine.Profiling.Profiler.EndSample ();
		}

		public virtual void Sleep () {
			Active =false ;
		}

		#endregion

		#region Prepare Requests
		protected void requestNothing (object sender, IRequestInterface args) {
		}

		protected void requestScene () {
			_materialLib.Clear () ;
			_mgr.Add (new InstanceTreeRequest (
				this,
				new System.Uri (ForgeLoaderConstants._endpoint + URN + "/scenes/" + SCENEID),
				BEARER
			)) ;
		}

		protected void requestSceneObjectDetails (object sender, IRequestInterface args) {
			switch ( args.resolved ) {
				case SceneLoadingStatus.eMesh:
					if ( _LoadMesh )
						requestMesh (sender, args) ;
					break;
				case SceneLoadingStatus.eMaterial:
					requestMaterial (sender, args) ;
					break;
				case SceneLoadingStatus.eProperties:
					if ( _LoadMetadata )
						requestProperties (sender, args) ;
					break;
			}
		}

		protected void requestMesh (object sender, IRequestInterface args) {
			// Meshes are safe because they will be called only once
			MeshRequest reqObj = args as MeshRequest;
			reqObj.uri =new System.Uri (ForgeLoaderConstants._endpoint + URN + "/scenes/" + SCENEID + "/mesh/" + reqObj.fragId.Item1 + "/" + reqObj.fragId.Item2) ;
			//UnityEngine.Debug.Log ("URI request: " + reqObj.uri.ToString ());
			_mgr.Register (reqObj) ;
		}

		protected void requestMaterial (object sender, IRequestInterface args) {
			// Material can be shared
			MaterialRequest reqObj =args as MaterialRequest ;
			reqObj.uri =new System.Uri (ForgeLoaderConstants._endpoint + URN + "/material/" + reqObj.matId + "/mat") ;
			//UnityEngine.Debug.Log ("URI request: " + reqObj.uri.ToString ());
			_mgr.Register (reqObj) ;
		}

		protected void requestTextures (object sender, IRequestInterface args) {
			// Textures are safe because they will be called only once per texture/material
			TextureRequest reqObj =args as TextureRequest ;
			reqObj.uri =new System.Uri (ForgeLoaderConstants._endpoint + URN + "/texture/" + reqObj.texture.tex) ;
			//UnityEngine.Debug.Log ("URI request: " + reqObj.uri.ToString ());
			_mgr.Register (reqObj) ;
		}

		protected void requestProperties (object sender, IRequestInterface args) {
			PropertiesRequest reqObj =args as PropertiesRequest ;
			reqObj.uri =new System.Uri (ForgeLoaderConstants._endpoint + URN + "/properties/" + reqObj.dbId.ToString ()) ;
			//UnityEngine.Debug.Log ("URI request: " + reqObj.uri.ToString ());
			_mgr.Register (reqObj) ;
		}

		#endregion

		#region Scene utility
		public static float convertToMeter (string units) {
			if ( units == "centimeter" || units == "cm" )
				return (0.01f) ;
			else if ( units == "millimeter" || units == "mm" )
				return (0.001f) ;
			else if ( units == "foot" || units == "ft" )
				return (0.3048f) ;
			else if ( units == "inch" || units == "in" )
				return (0.0254f) ;
			return (1.0f) ; // "meter" / "m"

// 'decimal-ft'
// 'ft-and-fractional-in'
// 'ft-and-decimal-in'
// 'decimal-in'
// 'fractional-in'
// 'm'
// 'cm'
// 'mm'
// 'm-and-cm'

		}

		public static GameObject SetupForSceneOrientationAndUnits (GameObject sceneRoot, JSONNode metadata) {
			JSONNode pt =metadata ["worldBoundingBox"] ["maxXYZ"] ;
			Vector3 max =new Vector3 (pt [0].AsFloat, pt [1].AsFloat, pt [2].AsFloat) ;
			pt =metadata ["worldBoundingBox"] ["minXYZ"] ;
			Vector3 min =new Vector3 (pt [0].AsFloat, pt [1].AsFloat, pt [2].AsFloat) ;
			Vector3 center =(max + min) / 2f ;
			//Vector3 size =(max - min) / 2f ;
			//Bounds boundsBubble =new Bounds (center, size) ;
			//Bounds bounds =BoundsExtensions.GameObjectBounds (sceneRoot) ;
			pt =metadata ["refPointLMV"] [1] ;
			Vector3 pivot =pt.AsArray.Count == 3 ? new Vector3 (pt [0].AsFloat, pt [1].AsFloat, pt [2].AsFloat) : Vector3.zero ;
			if ( sceneRoot.transform.parent == null ) {
				string name =sceneRoot.name + "Pivot" ;
				GameObject gRoot =new GameObject (name) ;
				sceneRoot.transform.parent =gRoot.transform ;
			}
			Transform root =sceneRoot.transform.parent ;
			JSONNode marker =metadata ["marker"] ;
			if ( marker != null && marker ["point"] != null ) {
				pt =marker ["point"] ;
				Vector3 tr =new Vector3 (pt ["x"].AsFloat, pt ["y"].AsFloat, pt ["z"].AsFloat) ;
				pivot +=center + tr ;
			}
			root.localPosition =pivot ;
			Transform main =sceneRoot.transform ;
			main.localPosition =-pivot ;
			JSONArray upvector =metadata ["worldUpVector"] [1].AsArray ;
			if ( upvector [2].AsFloat != 0f ) { // Z axis
				root.Rotate (Vector3.right, -90) ;
				root.localScale =new Vector3 (-1f, 1f, 1f) ;
			}
			string units =metadata ["units"] ;
			float unitsConvert =ForgeLoaderEngine.convertToMeter (units) ;
			if ( unitsConvert != 1.0f )
				root.localScale =new Vector3 (root.localScale.x * unitsConvert, root.localScale.y * unitsConvert, root.localScale.z * unitsConvert) ;
			// Now move to final position
			root.localPosition =Vector3.zero ;

			return (root.gameObject) ;
		}

		public static Vector3 GetMarkerOrientation (GameObject sceneRoot, JSONNode metadata) {
			Vector3 normal =GetMarkerNormal (sceneRoot, metadata) ;
			Vector4 v4 =new Vector4 (normal.x, normal.y, normal.z, 0f) ;
			Vector4 normal4 =TransformIntoUnity (v4, sceneRoot) ;
			return (normal4) ;
		}

		public static bool IsMarkerDefined (GameObject sceneRoot, JSONNode metadata =null) {
			if ( metadata == null )
				metadata =sceneRoot.GetComponent<ForgeProperties> ().Properties ;
			JSONNode marker =metadata ["marker"] ;
			return (marker != null && marker ["point"] != null) ;
		}

		public static Vector3 GetMarkerPoint (GameObject sceneRoot, JSONNode metadata =null) {
			if ( metadata == null )
				metadata =sceneRoot.GetComponent<ForgeProperties> ().Properties ;
			// Default Scene origin
			Vector3 point =Vector3.zero ;
			JSONNode marker =metadata ["marker"] ;
			if ( marker != null && marker ["point"] != null ) {
				JSONNode pt =marker ["point"] ;
				point =new Vector3 (pt ["x"].AsFloat, pt ["y"].AsFloat, pt ["z"].AsFloat) ;
			}
			return (point) ;
		}

		public static Vector3 GetMarkerNormal (GameObject sceneRoot, JSONNode metadata =null) {
			if ( metadata == null )
				metadata =sceneRoot.GetComponent<ForgeProperties> ().Properties ;
			// Default Scene normal
			Vector3 normal =GetSceneUpVector (sceneRoot, metadata) ;
			JSONNode marker =metadata ["marker"] ;
			if ( marker != null && marker ["face"] != null ) {
				JSONNode pt =marker ["face"] ["normal"] ;
				normal =new Vector3 (pt ["x"].AsFloat, pt ["y"].AsFloat, pt ["z"].AsFloat) ;
			}
			return (normal) ;
		}

		public static Vector3 GetSceneUpVector (GameObject sceneRoot, JSONNode metadata =null) {
			if ( metadata == null )
				metadata =sceneRoot.GetComponent<ForgeProperties> ().Properties ;
			JSONArray worldUpVector =metadata ["worldUpVector"] [1].AsArray ;
			Vector3 upVector =new Vector3 (worldUpVector [0].AsFloat, worldUpVector [1].AsFloat, worldUpVector [2].AsFloat) ;
			return (upVector) ;
		}

		public static string GetSceneUnit (GameObject sceneRoot, JSONNode metadata =null) {
			if ( metadata == null )
				metadata =sceneRoot.GetComponent<ForgeProperties> ().Properties ;
			return (metadata ["units"].Value) ;
		}

		public static Vector3 GetSceneOrigin (GameObject sceneRoot, JSONNode metadata =null) {
			if ( metadata == null )
				metadata =sceneRoot.GetComponent<ForgeProperties> ().Properties ;
			JSONNode pt =metadata ["refPointLMV"] [1] ;
			Vector3 point =pt.AsArray.Count == 3 ? new Vector3 (pt [0].AsFloat, pt [1].AsFloat, pt [2].AsFloat) : Vector3.zero ;
			return (point) ;
		}

		public static Bounds GetSceneBoundingBox (GameObject sceneRoot, JSONNode metadata =null) {
			if ( metadata == null )
				metadata =sceneRoot.GetComponent<ForgeProperties> ().Properties;
			JSONNode pt =metadata ["worldBoundingBox"] ["maxXYZ"];
			Vector3 max =new Vector3 (pt [0].AsFloat, pt [1].AsFloat, pt [2].AsFloat) ;
			pt =metadata ["worldBoundingBox"] ["minXYZ"] ;
			Vector3 min =new Vector3 (pt [0].AsFloat, pt [1].AsFloat, pt [2].AsFloat) ;
			Vector3 center =(max + min) / 2f ;
			Vector3 size =(max - min) / 2f ;
			Bounds boundsBubble =new Bounds (center, size) ;
			return (boundsBubble) ;
		}

		public static Matrix4x4 fromUnityMatrix (GameObject sceneRoot, JSONNode metadata =null) {
			if ( metadata == null )
				metadata =sceneRoot.GetComponent<ForgeProperties> ().Properties ;

			Vector3 center =GetSceneBoundingBox (sceneRoot, metadata).center ;
			Vector3 pivot =GetSceneOrigin (sceneRoot, metadata) ;
			if ( IsMarkerDefined (sceneRoot, metadata) )
				pivot +=/*center +*/ GetMarkerPoint (sceneRoot, metadata) ;
			Vector3 upvector =GetSceneUpVector (sceneRoot, metadata) ;
			string units =GetSceneUnit (sceneRoot, metadata) ;

			Transform tr =new GameObject ().transform ;
			if ( upvector [2] != 0f ) { // Z axis
				tr.Rotate (Vector3.right, -90) ;
				tr.localScale =new Vector3 (-1f, 1f, 1f) ;
			}
			float unitsConvert =ForgeLoaderEngine.convertToMeter (units) ;
			if ( unitsConvert != 1.0f )
				tr.localScale =new Vector3 (tr.localScale.x * unitsConvert, tr.localScale.y * unitsConvert, tr.localScale.z * unitsConvert) ;
			//tr.localPosition =-pivot ;

			Matrix4x4 mat =Matrix4x4.identity ;
			mat =tr.worldToLocalMatrix ;

			Matrix4x4 mTR = Matrix4x4.identity ;
			mTR.m03 =pivot.x ;
			mTR.m13 =pivot.y ;
			mTR.m23 =pivot.z ;
			mat =mTR * mat ;
			GameObject.DestroyImmediate (tr.gameObject) ;
			return (mat) ;
		}

		public static Matrix4x4 toUnityMatrix (GameObject sceneRoot, JSONNode metadata =null) {
			Matrix4x4 mat =fromUnityMatrix (sceneRoot, metadata) ;
			return (mat.inverse) ;
		}

		public static Vector4 TransformFromUnity (Vector4 from, GameObject sceneRoot) {
			Matrix4x4 mat =fromUnityMatrix (sceneRoot) ;
			return (mat * from) ;
		}

		public static Vector4 TransformIntoUnity (Vector4 from, GameObject sceneRoot) {
			Matrix4x4 mat =toUnityMatrix (sceneRoot) ;
			return (mat * from) ;
		}

		#endregion

		#region Default Material
		private static Material _defaultMaterial =null ;
		public static Material GetDefaultMaterial () {
			if ( _defaultMaterial == null ) {
				GameObject temp =GameObject.CreatePrimitive (PrimitiveType.Cube) ;
				MeshRenderer renderer =temp.GetComponent<MeshRenderer> () ;
				_defaultMaterial =renderer.sharedMaterial ;
				GameObject.DestroyImmediate (temp) ;
			}
			return (_defaultMaterial) ;
		}
		#endregion

		#region Certificate passthrough for Debug
		//#if DEBUG && !UNITY_WSA
#if !UNITY_WSA
		//protected class NoCheckCertificatePolicy : ICertificatePolicy {
		//
		//	public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem) {
		//		return (true) ;
		//	}
		//
		//}

		// https://stackoverflow.com/questions/18454292/system-net-certificatepolicy-to-servercertificatevalidationcallback-accept-all-c
		protected static class SSLValidator {

			private static bool OnValidateCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
				return (true) ;
			}

			public static void OverrideValidation () {
				ServicePointManager.ServerCertificateValidationCallback =OnValidateCertificate ;
				ServicePointManager.Expect100Continue =true ;
			}

		}
#endif

		#endregion

		#region Debug utility
		[MethodImpl (MethodImplOptions.NoInlining)]
		public static string GetCurrentMethod () {
#if UNITY_WSA
			return ("") ;
#else
			StackTrace st =new StackTrace () ;
			StackFrame sf =st.GetFrame (1) ;
			return (sf.GetMethod ().Name) ;
#endif
		}

		#endregion

	}

}
