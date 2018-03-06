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
using UnityEngine.Events;


namespace Autodesk.Forge.ARKit {

	[System.Serializable]
	public class QRDecodedEvent : UnityEvent<string, Vector3, Quaternion> {
	}

	public interface IQRCodeDecoderInterface {

		#region Interface Properties
		//bool _searchUntilFound

		//[SerializeField]
		//QRDecodedEvent _qrDecoded ;

		#endregion

		#region Unity APIs
		//void OnEnable () ;
		//void OnDisable () ;
		//void Update () ;

		#endregion

		#region Interface Methods
		//bool InitCamera () ;
		//void StopCamera () ;

		//void ProcessCameraFrame () ;

		void ToggleScan () ;

		#endregion

	}

}