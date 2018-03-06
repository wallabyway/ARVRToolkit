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


namespace Autodesk.Forge.ARKit {

	public static class BoundsExtensions {

		#region Methods
		public static Rect GUIRectWithObject (GameObject go) {
			Bounds bounds = go.GetComponent<Renderer> ().bounds;
			return (GUIRectWithObject (bounds));
		}

		public static Rect GUIRectWithObject (Bounds bounds) {
			Vector3 cen = bounds.center;
			Vector3 ext = bounds.extents;
			Vector2 [] extentPoints = new Vector2 [8] {
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z + ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z + ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z - ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z + ext.z)),
				WorldToGUIPoint (new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z + ext.z))
			};
			Vector2 min = extentPoints [0];
			Vector2 max = extentPoints [0];
			foreach ( Vector2 v in extentPoints ) {
				min = Vector2.Min (min, v);
				max = Vector2.Max (max, v);
			}
			return (new Rect (min.x, min.y, max.x - min.x, max.y - min.y));
		}

		public static Vector2 WorldToGUIPoint (Vector3 world) {
			Vector2 screenPoint = Camera.main.WorldToScreenPoint (world);
			screenPoint.y = (float)Screen.height - screenPoint.y;
			return (screenPoint);
		}

		public static Bounds GameObjectBounds (GameObject obj) {
			Renderer [] renderers = obj.GetComponentsInChildren<Renderer> ();
			Bounds bounds = new Bounds (renderers [0].bounds.center, renderers [0].bounds.size);
			foreach ( Renderer renderer in renderers )
				bounds.Encapsulate (renderer.bounds);
			return (bounds);
		}

		#endregion

	}

}