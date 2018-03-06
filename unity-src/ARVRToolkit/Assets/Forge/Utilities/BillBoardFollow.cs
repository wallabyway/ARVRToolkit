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
//using HoloToolkit.Unity;

namespace Autodesk.Forge.ARKit {

	public class BillBoardFollow : MonoBehaviour {

		/// <summary>
		/// The axis about which the object will rotate.
		/// </summary>
		//		[Tooltip("Specifies the axis about which the object will rotate (Free rotates about both X and Y).")]
		//		public PivotAxisExt PivotAxis =PivotAxisExt.Free ;

		/// <summary>
		/// The axis about which the object will rotate.
		/// </summary>
		[Tooltip ("Specifies the distance about which the object will move in front of the camera.")]
		public float PivotDistance = 0.0f;

		/// <summary>
		/// Overrides the cached value of the GameObject's default rotation.
		/// </summary>
		public Quaternion DefaultRotation { get; private set; }
		public float DefaultDistance { get; private set; }

		private void Awake () {
			// Cache the GameObject's default rotation.
			DefaultRotation = gameObject.transform.rotation;
			DefaultDistance = PivotDistance;
			if ( DefaultDistance == 0.0f ) {
				//			RectTransform objectRectTransform =gameObject.GetComponent<RectTransform> () ;
				//			Vector3 size =objectRectTransform.localToWorldMatrix.MultiplyVector (new Vector3 (objectRectTransform.sizeDelta.x, objectRectTransform.sizeDelta.y)) ;
				//			Debug.Log (size) ;
				//			Vector3 size2 =new Vector3 (Screen.width, Screen.height) ;
				//			Debug.Log (size2) ;
				DefaultDistance = (Camera.main.transform.position - gameObject.transform.position).magnitude;
			}
		}

		/// <summary>
		/// Keeps the object facing the camera.
		/// </summary>
		private void LateUpdate () {
			// Get a Vector that points from the Camera to the target.
			Vector3 forward;
			Vector3 up;

			// Adjust the pivot location. Before changing the rotation.
			transform.position = DefaultDistance * Camera.main.transform.forward + Camera.main.transform.position;

			// Adjust for the pivot axis. We need a forward and an up for use with Quaternion.LookRotation
			//			switch ( PivotAxis ) {
			//				// If we're fixing one axis, then we're projecting the camera's forward vector onto
			//				// the plane defined by the fixed axis and using that as the new forward.
			//				case PivotAxisExt.X:
			//					Vector3 right =transform.right ; // Fixed right
			//					forward =Vector3.ProjectOnPlane (Camera.main.transform.forward, right).normalized ;
			//					up =Vector3.Cross (forward, right) ; // Compute the up vector
			//					break ;
			//				case PivotAxisExt.Y:
			//					up =transform.up ; // Fixed up
			//					forward =Vector3.ProjectOnPlane (Camera.main.transform.forward, up).normalized ;
			//					break ;
			//					// If the axes are free then we're simply aligning the forward and up vectors
			//					// of the object with those of the camera. 
			//				case PivotAxisExt.Free:
			//				default:
			forward = Camera.main.transform.forward;
			up = Camera.main.transform.up;
			//					break ;
			//			}

			// Calculate and apply the rotation required to reorient the object
			transform.rotation = Quaternion.LookRotation (forward, up);
		}

	}

}