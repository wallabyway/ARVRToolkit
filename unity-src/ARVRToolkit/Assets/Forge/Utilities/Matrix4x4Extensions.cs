//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autodesk.Forge.ARKit {

	public static class Matrix4x4Extensions {

		public static Quaternion ExtractRotation (this Matrix4x4 matrix) {
			Vector3 forward = matrix.GetColumn (2);
			//forward.x =matrix.m02 ;
			//forward.y =matrix.m12 ;
			//forward.z =matrix.m22 ;

			Vector3 upwards = matrix.GetColumn (1);
			//upwards.x =matrix.m01 ;
			//upwards.y =matrix.m11 ;
			//upwards.z =matrix.m21 ;

			return (Quaternion.LookRotation (forward, upwards));
		}

		public static Vector3 ExtractPosition (this Matrix4x4 matrix) {
			Vector3 position = matrix.GetColumn (3);
			//position.x =matrix.m03 ;
			//position.y =matrix.m13 ;
			//position.z =matrix.m23 ;
			return (position);
		}

		public static Vector3 ExtractScale (this Matrix4x4 matrix) {
			Vector3 scale = new Vector3 (
				matrix.GetColumn (0).magnitude,
				matrix.GetColumn (1).magnitude,
				matrix.GetColumn (2).magnitude
			);
			//scale.x =new Vector4 (matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude ;
			//scale.y =new Vector4 (matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude ;
			//scale.z =new Vector4 (matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude ;
			return (scale);
		}

	}

}
