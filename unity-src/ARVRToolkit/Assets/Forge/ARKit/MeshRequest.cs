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
using System.ComponentModel;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if !UNITY_WSA
using System.Net;
#endif
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public class MeshRequest : RequestObjectInterface {

		#region Properties
		public Eppy.Tuple<int, int> fragId { get; set; }
		public int materialId { get; set; }
		private bool createCollider = false;
		protected Vector3 [] _vertices = null;
		protected int [] _triangles = null;
		protected Vector2 [] _uvs = null;

		#endregion

		#region Constructors
		public MeshRequest (IForgeLoaderInterface _loader, Uri _uri, string _bearer, Eppy.Tuple<int, int> _fragId, int _materialId, JSONNode node) : base (_loader, _uri, _bearer) {
			resolved = SceneLoadingStatus.eMesh;
			fragId = _fragId;
			materialId = _materialId;
			lmvtkDef = node;
			compression = true;
			createCollider =_loader.CreateCollider;
		}

		#endregion

		#region Forge Request Object Interface

		//public override void FireRequest () ;

		//public override void CancelRequest () ;

		public override void ProcessResponse (AsyncCompletedEventArgs e) {
			//TimeSpan tm = DateTime.Now - emitted;
			//UnityEngine.Debug.Log ("Received: " + tm.TotalSeconds.ToString () + " / " + uri.ToString ());
			DownloadDataCompletedEventArgs args = e as DownloadDataCompletedEventArgs;
			try {
				byte [] bytes = args.Result;
				if ( compression )
					bytes =RequestObjectInterface.Decompress (bytes) ;

				int nbCoords = getInt (bytes, 0);
				int index = sizeof (Int32);
				float [] coords = getFloats (bytes, nbCoords, sizeof (int));
				index += nbCoords * sizeof (float);
				_vertices = new Vector3 [nbCoords / 3];
				for ( int i = 0, ii = 0 ; i < nbCoords ; i += 3, ii++ )
					_vertices [ii] = new Vector3 (coords [i], coords [i + 1], coords [i + 2]);

				int nbTriangles = getInt (bytes, index);
				index += sizeof (Int32);
				_triangles = getInts (bytes, nbTriangles, index);
				index += nbTriangles * sizeof (Int32);

				int nbUVs = getInt (bytes, index);
				index += sizeof (Int32);
				float [] uv_a = nbUVs != 0 ? getFloats (bytes, nbUVs, index) : null;
				_uvs = nbUVs != 0 ? new Vector2 [nbUVs / 2] : null;
				for ( int i = 0, ii = 0 ; i < nbUVs ; i += 2, ii++ )
					_uvs [ii] = new Vector2 (uv_a [i], uv_a [i + 1]);

				state = SceneLoadingStatus.eReceived;
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			} finally {
			}
		}

		public override string GetName () {
			return ("mesh-" + fragId.Item1.ToString () + "-" + fragId.Item2.ToString ());
		}

		public override GameObject BuildScene (string name, bool saveToDisk = false) {
			try {
				if ( _vertices.Length == 0 || _triangles.Length == 0 ) {
					state = SceneLoadingStatus.eCancelled;
					return (gameObject);
				}
				Mesh mesh = new Mesh ();
				mesh.vertices = _vertices;
				mesh.triangles = _triangles;
				if ( _uvs != null && _uvs.Length != 0 )
					mesh.uv = _uvs;
				mesh.RecalculateNormals ();
				mesh.RecalculateBounds ();

				MeshFilter filter = gameObject.AddComponent<MeshFilter> ();
				filter.sharedMesh = mesh;
				MeshRenderer renderer = gameObject.AddComponent<MeshRenderer> ();
				renderer.sharedMaterial = ForgeLoaderEngine.GetDefaultMaterial ();
				if ( createCollider ) {
					MeshCollider collider = gameObject.AddComponent<MeshCollider>();
					collider.sharedMesh = mesh;
				}
#if UNITY_EDITOR
				if ( saveToDisk ) {
					AssetDatabase.CreateAsset (mesh, ForgeConstants._resourcesPath + this.loader.PROJECTID + "/" + name + ".asset");
					//AssetDatabase.SaveAssets () ;
					//AssetDatabase.Refresh () ;
					//mesh =AssetDatabase.LoadAssetAtPath<Mesh> (ForgeConstants._resourcesPath + this.loader.PROJECTID + "/" + name + ".asset") ;
				}
#endif

				base.BuildScene (name, saveToDisk);
				state = SceneLoadingStatus.eWaitingMaterial;
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			}
			return (gameObject);
		}

		#endregion

		#region Methods
		protected static int getInt (byte [] b, int index = 0) {
			//const int len =sizeof (Int32) ;
			//if ( BitConverter.IsLittleEndian )
			//	Array.Reverse (b, index, len) ;
			int i = BitConverter.ToInt32 (b, index);
			return (i);
		}

		protected static int [] getInts (byte [] b, int nb, int index = 0) {
			const int len = sizeof (Int32);
			int [] intArr = new int [nb];
			for ( int i = 0, pos = index ; i < nb ; i++, pos += len ) {
				//if ( BitConverter.IsLittleEndian )
				//	Array.Reverse (b, i * len, len) ;
				intArr [i] = BitConverter.ToInt32 (b, pos);
			}
			return (intArr);
		}

		protected static float [] getFloats (byte [] b, int nb, int index = 0) {
			const int len = sizeof (float);
			float [] floatArr = new float [nb];
			for ( int i = 0, pos = index ; i < nb ; i++, pos += len ) {
				//if ( BitConverter.IsLittleEndian )
				//	Array.Reverse (b, i * len, len) ;
				floatArr [i] = BitConverter.ToSingle (b, pos);
			}
			return (floatArr);
		}

		#endregion

	}

}