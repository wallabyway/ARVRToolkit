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

	public static class TransformExtensions {

		public static void FromMatrix (this Transform transform, Matrix4x4 matrix) {
			transform.localScale = matrix.ExtractScale ();
			transform.rotation = matrix.ExtractRotation ();
			transform.position = matrix.ExtractPosition ();
		}

	}

}
