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

	public class BuildPrefabs : EditorWindow {

		#region Fields
		public string _name ="" ;
		public bool _flattern =false ;
		public bool _addColliders =false ;
		protected static string _resourcesPath =ForgeConstants._resourcesPath ;
		protected static string _bundlePath =ForgeConstants._bundlePath ;

		#endregion

		#region Properties
		public string _resources { get { return (_resourcesPath + _name) ; } }
		public string _prefab { get { return (_resourcesPath + _name + ".prefab") ; } }
		public string _bundle { get { return (_bundlePath + _name + ".unity3d") ; } }

		#endregion

		#region Unity APIs
		protected void OnGUI () {
			EditorGUILayout.Space();
			_name =EditorGUILayout.TextField ("Prefab Name", _name) ;
			EditorGUILayout.Space();
			_flattern =EditorGUILayout.ToggleLeft ("Flattern Hierarchy", _flattern) ;
			_addColliders =EditorGUILayout.ToggleLeft ("Add Colliders", _addColliders) ;
			EditorGUILayout.Space () ;
			/*Rect r =*/EditorGUILayout.BeginHorizontal () ;
			if ( GUILayout.Button ("Save Prefab") )
				OnClickSavePrefab () ;
			if ( GUILayout.Button ("Cancel") )
				CloseDialog () ;
			EditorGUILayout.EndHorizontal () ;
		}

		#endregion

		#region Methods
		void OnClickSavePrefab () {
			_name =_name.Trim () ;
			if ( string.IsNullOrEmpty (_name) ) {
				EditorUtility.DisplayDialog ("Unable to save prefab", "Please specify a valid prefab name.", "Close") ;
				return ;
			}

			GameObject root =Selection.activeGameObject ;
			if ( _addColliders ) {
				MakeChildrenInteractible.MakeAllInteractible (root) ;
			}

			if ( !_flattern ) {
				/*GameObject prefab =*/PrefabUtility.CreatePrefab (
					_prefab,
					Selection.activeGameObject,
					ReplacePrefabOptions.ConnectToPrefab
				) ;
			} else {
				GameObject clone =Instantiate<GameObject> (root) ;
				Transform [] transforms =clone.GetComponentsInChildren<Transform> () ;
				foreach ( Transform tr in transforms ) {
					if ( tr.gameObject == clone )
						continue ;
					tr.parent =clone.transform ;
				}
				/*GameObject prefab =*/PrefabUtility.CreatePrefab (
					_prefab,
					clone,
					ReplacePrefabOptions.ConnectToPrefab
				) ;
				DestroyImmediate (clone) ;
			}
			CloseDialog () ;
		}

		protected void CloseDialog () {
			Close () ;
			GUIUtility.ExitGUI () ;
		}

		#endregion

		#region Menu
		[MenuItem("Forge/Build Prefab", false, ForgeConstants.BUILD_PREFAB_MENU)]
		public static void BuildPrefab () {
			BuildPrefabs window =EditorWindow.GetWindowWithRect (
				typeof (BuildPrefabs),
				new Rect(0, 0, 640, 100)
			) as BuildPrefabs ;
			window._name =Selection.activeGameObject.name ;
			window._flattern =false ;
			window._addColliders =false ;
			window.Show () ;
		}

		[MenuItem("Forge/Build Prefab", true)]
		public static bool ObjectSelectedValidation () {
			return (
				   Selection.activeGameObject != null
				//&& Selection.activeGameObject.layer == LayerMask.NameToLayer (ForgeConstants.INTERACTIBLE)
				//&& Selection.activeGameObject.transform.childCount > 0
			) ;
		}

		#endregion

	}

}
