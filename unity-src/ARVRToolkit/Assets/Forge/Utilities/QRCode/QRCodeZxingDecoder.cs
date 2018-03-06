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
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR || !UNITY_WSA
using ZXing; // https://github.com/micjahn/ZXing.Net
using ZXing.Common;
using ZXing.QrCode;
#endif

namespace Autodesk.Forge.ARKit {

	public partial class QRCodeZxingDecoder :  QRCodeDecoderBase {

		#region Fields
		public AudioSource _photoTaken = null;
		public AudioSource _decodeSuccessful = null;
		public AudioSource _decodeFailed = null;
		public RawImage _image =null ;

		#endregion

		#region Unity APIs
		//protected virtual void OnEnable () 
		//protected virtual void OnDisable ()
		//protected abstract void Update ()

		#endregion

		#region Methods
		//protected virtual bool InitCamera ()
		//protected virtual void StopCamera ()
		//protected virtual void ProcessCameraFrame ()

		#endregion

	}

#if UNITY_EDITOR || !UNITY_WSA
	public partial class QRCodeZxingDecoder : QRCodeDecoderBase {

	#region Fields
		protected WebCamTexture _wct =null ;

	#endregion

	#region Unity APIs
		//protected virtual void OnEnable () 
		//protected virtual void OnDisable ()

		protected override void Update () {
			ProcessCameraFrame () ;
		}

	#endregion

	#region Methods
		protected override bool InitCamera () {
			bool ret =false ;
			WebCamDevice[] devices =WebCamTexture.devices ;
			if ( Application.platform == RuntimePlatform.Android ) {
				foreach ( WebCamDevice cam in devices ) {
					if ( cam.isFrontFacing ) {
						_wct =new WebCamTexture (cam.name, Screen.height, Screen.width, 12) ;
						if ( _wct != null ) {
							_wct.deviceName =cam.name ;
							_wct.Play () ;
							ret =true ;
							break ;
						}
					}
				}
			} else {
				string deviceName =devices [0].name ;
				//_wct =new WebCamTexture (deviceName, Screen.width, Screen.height, 12) ;
				_wct =new WebCamTexture (deviceName, 2048, 1152, 12) ;
				if ( _wct != null ) {
					//_image.texture =_wct ;
					_wct.Play () ;
					ret =true ;
				}
			}
			if ( _image != null && _wct != null ) {
				_image.texture =_wct ;
				_image.material.mainTexture =_wct ;
			}
			return (ret) ;
		}

		protected override void StopCamera () {
			if ( _wct != null )
				_wct.Stop () ;
			_wct =null ;
		}

		protected override void ProcessCameraFrame () {
			if ( _wct == null || !_wct.isPlaying )
				return ;

			Color32[] pixels =_wct.GetPixels32 () ;
			Matrix4x4 cameraToWorldMatrix =Matrix4x4.identity ;
			Vector3 position =cameraToWorldMatrix.MultiplyPoint (Vector3.zero) ;
			Quaternion rotation =Quaternion.LookRotation (-cameraToWorldMatrix.GetColumn (2), cameraToWorldMatrix.GetColumn (1)) ;

			// For debug
			//System.IO.File.WriteAllBytes ("C:\\Users\\cyrille\\Documents\\photo.jpg", buffer) ;
			//byte[] buffer =System.IO.File.ReadAllBytes ("C:\\Users\\cyrille\\Documents\\photo.jpg") ;
			//Texture2D tex =new Texture2D (_wct.width, _wct.height, TextureFormat.BGRA32, false) ;
			//tex.LoadRawTextureData (buffer) ;
			//pixels =tex.GetPixels32 () ;

			//List<BarcodeFormat> codeFormats =new List<BarcodeFormat> () ;
			//codeFormats.Add (BarcodeFormat.QR_CODE) ;
			IBarcodeReader reader =new BarcodeReader () /*{
				AutoRotate =true,
				TryInverted =true,
				Options ={
					PossibleFormats =codeFormats,
					TryHarder =true,
					ReturnCodabarStartEnd =true,
					PureBarcode =false
				}
			}*/ ;
			var result =reader.Decode (pixels, _wct.width, _wct.height) ;
			if ( result != null && _photoTaken != null ) {
				UnityMainThreadDispatcher.Instance ().Enqueue (() => {
					_photoTaken.Play () ;
				}) ;
			}
			Debug.Log (result == null ? "not decoded" : result.Text) ;
			ReturnOrLoop (result == null ? null : result.Text, position, rotation) ;
		}

		protected override bool ReturnOrLoop (string json, Vector3 position, Quaternion rotation) {
			if ( !string.IsNullOrEmpty (json) || (string.IsNullOrEmpty (json) && _searchUntilFound == false) ) {
				UnityMainThreadDispatcher.Instance ().Enqueue (() => {
					if ( _decodeSuccessful != null && !string.IsNullOrEmpty (json) )
						_decodeSuccessful.Play () ;
					else if ( _decodeFailed != null && string.IsNullOrEmpty (json) )
						_decodeFailed.Play () ;
					gameObject.SetActive (false) ;
					_qrDecoded.Invoke (json, position, rotation) ;
				}) ;
				return (true) ;
			} else {
				return (false) ;
			}
		}

	#endregion

	}

#endif

}
