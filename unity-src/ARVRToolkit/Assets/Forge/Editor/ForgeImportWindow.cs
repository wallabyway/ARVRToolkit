//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Autodesk.Forge.ARKit {

	#if UNITY_EDITOR
	class ImportBubbleWindow : EditorWindow {

		#region Fields
		public string _projectid =ForgeLoaderConstants._PROJECTID ;
		public LODLevels _lodLevel =LODLevels.Full ;
		//public bool _autoScale =false ;
		public bool _loadMetadata =true ;
		public bool _loadMesh =true ;
		public bool _createColliders =false ;
		public bool _saveToDisk =true ;

		protected string _scene =ForgeLoaderConstants._SCENEID ;
		protected string _urn =ForgeLoaderConstants._URN ;
		protected string _bearer =ForgeLoaderConstants._BEARER ;

		protected ForgeLoaderEngine _loader =new ForgeLoaderEngine () ;
		protected bool _loaderActive =false ;

		#endregion

		#region Menu
		[MenuItem ("Forge/Import Scene", false, ForgeConstants.IMPORT_SCENE_MENU)]
		public static void Init () {
			ImportBubbleWindow window =EditorWindow.GetWindowWithRect (
				typeof (ImportBubbleWindow),
				new Rect (0, 0, 640, 190)
			) as ImportBubbleWindow ;
			window.Show () ;
		}

		#endregion

		#region Unity APIs
		protected void OnGUI () {
			EditorGUILayout.Space () ;
			_projectid =EditorGUILayout.TextField ("Project Name", _projectid) ;

			EditorGUILayout.Space () ;
			_scene =EditorGUILayout.TextField ("Scene Name", _scene) ;

			EditorGUILayout.Space () ;
			_urn =EditorGUILayout.TextField ("Project URN", _urn) ;

			EditorGUILayout.Space () ;
			_bearer =EditorGUILayout.TextField ("Access Token", _bearer) ;

			//EditorGUILayout.Space () ;
			//_lodLevel =(LODLevels)EditorGUILayout.EnumPopup ("Target", _lodLevel) ;

			//EditorGUILayout.Space () ;
			//_autoScale =EditorGUILayout.ToggleLeft ("Auto Scale", _autoScale) ;

			EditorGUILayout.Space () ;
			EditorGUILayout.BeginHorizontal () ;
			_loadMetadata = EditorGUILayout.ToggleLeft ("Load Metadata", _loadMetadata) ;
			//EditorGUILayout.Space ();
			_loadMesh = EditorGUILayout.ToggleLeft ("Load Mesh & Materials", _loadMesh) ;
			//EditorGUILayout.Space ();
			_createColliders = EditorGUILayout.ToggleLeft ("Create Colliders", _createColliders) ;
			EditorGUILayout.EndHorizontal () ;

			EditorGUILayout.Space () ;
			_saveToDisk =EditorGUILayout.ToggleLeft ("Save Asset to disk", _saveToDisk) ;

			EditorGUILayout.Space () ;
			/*Rect r =*/EditorGUILayout.BeginHorizontal () ;
			if ( GUILayout.Button ("Import") )
				ImportBubble () ;
			if ( GUILayout.Button ("Cancel") )
				CloseDialog () ;
			EditorGUILayout.EndHorizontal () ;

			if ( _loaderActive )
				_loader.Update () ;
		}

		#endregion

		#region Forge Import
		protected void ImportBubble () {
			_projectid =_projectid.Trim () ;
			if ( string.IsNullOrEmpty (_projectid) ) {
				EditorUtility.DisplayDialog ("Unable to import", "Please specify a valid project name.", "Close") ;
				return ;
			}
			_scene =_scene.Trim () ;
			if ( string.IsNullOrEmpty (_scene) ) {
				EditorUtility.DisplayDialog ("Unable to import", "Please specify a valid project name.", "Close") ;
				return ;
			}
			_urn =_urn.Trim () ;
			if ( string.IsNullOrEmpty (_urn) ) {
				EditorUtility.DisplayDialog ("Unable to import", "Please specify a valid project URN.", "Close") ;
				return ;
			}
			_bearer =_bearer.Trim () ;
			if ( string.IsNullOrEmpty (_bearer) ) {
				EditorUtility.DisplayDialog ("Unable to import", "Please specify a valid access token.", "Close") ;
				return ;
			}
			
			if ( this._saveToDisk ) {
				string resources =ForgeConstants._resourcesPath + this._projectid ;
				FileUtil.DeleteFileOrDirectory (resources) ;
				System.IO.Directory.CreateDirectory (resources) ;
				AssetDatabase.Refresh () ;
			}

			_loader.PROJECTID =this._projectid ;
			_loader.URN =this._urn ;
			_loader.BEARER =this._bearer ;
			_loader.SCENEID =this._scene ;
			_loader.LoadMesh = this._loadMesh;
			_loader.CreateCollider =this._createColliders;
			_loader.LoadMetadata = this._loadMetadata;
			_loader.SaveToDisk =this._saveToDisk ;
			_loader.ProcessedNodes +=new ForgeLoaderEngine.ProcessedNodesDelegate (ProcessedNodes) ;
			_loader.ProcessingNodesCompleted +=new ForgeLoaderEngine.ProcessingNodesCompletedDelegate (ProcessingNodesCompleted) ;
			_loaderActive =true ;
			_loader.Awake () ;
		}

		#endregion

		#region Events
		public void ProcessedNodes (ForgeLoaderEngine sender, float pct) {
			//EditorUtility.DisplayProgressBar
			if ( EditorUtility.DisplayCancelableProgressBar (
				"Importing Scene, please wait...",
				pct.ToString ("0.00") + " %",
				pct / 100
			)
			) {
				// User pressed cancel
				_loaderActive =false ;
				this.Repaint () ;
				CloseDialog () ;
			}
		}

		public void ProcessingNodesCompleted (ForgeLoaderEngine sender, int unprocessedObjects) {
			_loaderActive =false ;
			this.Repaint () ;
			CloseDialog () ;

			if ( this._saveToDisk ) {
				AssetDatabase.SaveAssets () ;
				AssetDatabase.Refresh () ;
			}
		}

		protected void CloseDialog () {
			EditorUtility.ClearProgressBar () ;
			Close () ;
			GUIUtility.ExitGUI () ;
		}

		protected void OnInspectorUpdate () {
			this.Repaint () ;
		}

		#endregion

	}

	#endif

}
