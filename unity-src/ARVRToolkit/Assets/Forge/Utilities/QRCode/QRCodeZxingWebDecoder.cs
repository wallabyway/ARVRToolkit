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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public partial class QRCodeZxingWebDecoder : QRCodeDecoderBase {

		#region Fields
		public AudioSource _photoTaken =null ;
		public AudioSource _decodeSuccessful =null ;
		public AudioSource _decodeFailed =null ;
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

		// This one does not work because of https and badly support on form-data in UnityWebRequest
		protected UnityWebRequest CreateDirectRequest (byte[] photo) {
			//MultipartFormDataSection fn =new MultipartFormDataSection ("f", photo, "image/png") ;
			MultipartFormFileSection fn =new MultipartFormFileSection ("f", photo, "myimage.png", "image/png") ;
			List<IMultipartFormSection> formData =new List<IMultipartFormSection> () ;
			formData.Add (fn) ;

			UnityWebRequest client =UnityWebRequest.Post (
				"http://zxing.org/w/decode",
				formData
			) ;
			return (client) ;
		}

		// This one works only if not https
		protected UnityWebRequest CreateRequest (byte[] photo) {
			UnityWebRequest client =UnityWebRequest.Put (
				//"http://192.168.1.10:3000/qrcode",
				"http://qr-code.autodesk.io/qrcode",
				photo
			) ;
			client.SetRequestHeader ("Content-Type", "application/octet-stream") ;
			return (client) ;
		}

		protected IEnumerator UploadAndHandlePhoto (byte[] photo, Matrix4x4 cameraToWorldMatrix) {
			if ( _photoTaken != null ) {
				UnityMainThreadDispatcher.Instance ().Enqueue (() => {
					_photoTaken.Play () ;
				}) ;
			}
			using ( UnityWebRequest client =CreateRequest (photo) ) {
				#if UNITY_2017_2_OR_NEWER
				yield return client.SendWebRequest () ;
				#else
				yield return client.Send () ;
				#endif

				if ( client.isNetworkError || client.isHttpError ) {
					Debug.Log (ForgeLoader.GetCurrentMethod () + " " + client.error + " - " + client.responseCode) ;
					ReturnOrLoop (null, Vector3.zero, Quaternion.identity) ;
				} else {
					string jsonString =client.downloadHandler.text ;
					// System.Text.Encoding.UTF8.GetString(client.downloadHandler.data)

					// Position the camera for raycasting
					Vector3 position =cameraToWorldMatrix.MultiplyPoint (Vector3.zero) ;
					Quaternion rotation =Quaternion.LookRotation (-cameraToWorldMatrix.GetColumn (2), cameraToWorldMatrix.GetColumn (1)) ;
					ReturnOrLoop (jsonString, position, rotation) ;
				}
			}
		}

		//protected override bool ReturnOrLoop (string json, Vector3 position, Quaternion rotation) {
		//	if ( json != null || (json == null && _searchUntilFound == false) ) {
		//		UnityMainThreadDispatcher.Instance ().Enqueue (() => {
		//			if ( _decodeSuccessful != null && json != null )
		//				_decodeSuccessful.Play () ;
		//			else if ( _decodeFailed != null && json == null )
		//				_decodeFailed.Play () ;
		//			gameObject.SetActive (false) ;
		//			_qrDecoded.Invoke (json, position, rotation) ;
		//		}) ;
		//		return (true) ;
		//	}
		//	return (false) ;
		//}

		#endregion

	}

	#if UNITY_EDITOR || !UNITY_WSA
	public partial class QRCodeZxingWebDecoder : QRCodeDecoderBase {

		#region Fields
		protected WebCamTexture _wct =null ;
		protected bool _processingImage =false ; // If true, an image is processed on the WEB

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
			_processingImage =false ;
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
			_processingImage =true ;
			if ( _wct != null )
				_wct.Stop () ;
			_wct =null ;
		}

		protected override void ProcessCameraFrame () {
			if ( _wct == null || !_wct.isPlaying || _processingImage == true )
				return ;
			Texture2D snap =new Texture2D (_wct.width, _wct.height) ;
			snap.SetPixels32 (_wct.GetPixels32 ()) ;
			snap.Apply () ;

			byte[] buffer =snap.EncodeToJPG () ;
			Matrix4x4 cameraToWorldMatrix =Matrix4x4.identity ;

			// For debug
			//System.IO.File.WriteAllBytes ("C:\\Users\\cyrille\\Documents\\photo.jpg", buffer) ;
			//buffer =System.IO.File.ReadAllBytes ("C:\\Users\\cyrille\\Documents\\photo.jpg") ;

			// Start a coroutine to handle the server request
			_processingImage =true ;
			StartCoroutine (UploadAndHandlePhoto (buffer, cameraToWorldMatrix)) ;
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
				_processingImage =false ;
				return (false) ;
			}
		}

		#endregion

	}

	#endif

	// For WSA, see QRCodeZxinWedDecoderWSA.cs

}