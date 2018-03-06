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

	public class ExplodeController : MonoBehaviour {

		#region Fields
		protected ExplodeRuntime _explode = null;
		protected bool _explodeActivated = false;
		protected static float _explodeSpeed = 80f;
		protected static float _explodeTarget = 1f;

		#endregion

		#region Unity APIs
		protected virtual void Awake () {
			InitExplodeEngine (gameObject) ;
			//Explode () ;
		}

		#endregion

		#region Methods
		protected void InitExplodeEngine (GameObject root) {
			_explode =new ExplodeRuntime (root) ;
		}

		public bool isExploded { get { return (_explodeActivated) ; } }

		public void Explode () {
			if ( _explodeActivated )
				return;
			_explodeTarget =0.75f ; // 1f ;
			_explodeActivated =true ;
			_explode.explode (_explodeTarget) ;
			//StartCoroutine (AnimationNumber.Instance.Animate (_explode, _explode._scale, 1f, 5f)) ;
		}

		public void Combine () {
			if ( _explode == null )
				return ;
			_explodeTarget =0f ;
			_explode.explode (_explodeTarget) ; // 0f
			_explodeActivated =false ;
			//StartCoroutine (AnimationNumber.Instance.Animate (_explode, _explode._scale, 0f, 5f)) ;
		}

		public bool ToggleExplode () {
			if ( isExploded )
				Combine () ;
			else
				Explode () ;
			return (isExploded) ;
		}

		public void ToggleExplodeRun () {
			ToggleExplode () ;
		}

		#endregion

	}

}
