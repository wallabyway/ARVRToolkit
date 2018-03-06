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

	public class MakeChildrenInteractible : EditorWindow {

		#region Fields

		#endregion

		#region Unity APIs
		protected void OnGUI () {
		}

		#endregion

		#region Methods
		public static void MakeAllInteractible () {
			MakeAllInteractible (Selection.gameObjects) ;
		}

		public static void MakeAllInteractible (GameObject[] roots) {
			foreach ( GameObject root in roots )
				MakeAllInteractible (root) ;
		}

		public static void MakeAllInteractible (GameObject root) {
			MeshFilter[] filters =root.GetComponentsInChildren<MeshFilter> () ;
			foreach ( MeshFilter filter in filters ) {
				Collider collider =filter.gameObject.GetComponent<Collider> () ;
				if ( collider == null ) {
					//MeshCollider meshCollider =new MeshCollider () ;
					MeshCollider meshCollider =filter.gameObject.AddComponent<MeshCollider> () ;
					meshCollider.sharedMesh =filter.sharedMesh ;
				}
			}
		}

		#endregion

		#region Menu
		[MenuItem("Forge/Make Children Interactible", false, ForgeConstants.MAKE_INTERACTIBLE_MENU)]
		public static void _MakeChildrenInteractible () {
			MakeAllInteractible () ;
		}

		[MenuItem("Forge/Make Children Interactible", true)]
		public static bool ObjectSelectedValidation () {
			return (
				   Selection.activeGameObject != null
				//&& Selection.activeGameObject.layer == LayerMask.NameToLayer (ForgeConstants.INTERACTIBLE)
				&& Selection.activeGameObject.transform.childCount > 0
			) ;
		}

		#endregion

	}

}
