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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
#if UNITY_EDITOR || !UNITY_WSA
//using ZXing; // https://github.com/micjahn/ZXing.Net
//using ZXing.Common;
//using ZXing.QrCode;
#else
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR.WSA.WebCam;
#else
using UnityEngine.VR.WSA.WebCam;
#endif
#endif
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public abstract class QRCodeDecoderBase : MonoBehaviour, IQRCodeDecoderInterface	{

#region Interface Enums
		public enum CaptureMode {
			Photo,
			Camera
		}

#endregion

#region Fields
		public CaptureMode _mode =CaptureMode.Camera ;
		public bool _searchUntilFound =false ;		

		[SerializeField]
		public QRDecodedEvent _qrDecoded =new QRDecodedEvent () ;

#endregion

#region Unity APIs
		protected virtual void OnEnable () {
			InitCamera () ;
		}

		protected virtual void OnDisable () {
			StopCamera () ;
		}

		protected abstract void Update () ;

#endregion

#region Methods
		protected abstract bool InitCamera () ;
		protected abstract void StopCamera () ;
		protected abstract void ProcessCameraFrame () ;
		protected abstract bool ReturnOrLoop (string json, Vector3 position, Quaternion rotation) ;


		public void ToggleScan () {
			gameObject.SetActive (!gameObject.activeSelf) ;
		}

#endregion

	}

}