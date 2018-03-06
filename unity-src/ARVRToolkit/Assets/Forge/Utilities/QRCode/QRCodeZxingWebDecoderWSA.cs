//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_WSA
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR.WSA.WebCam;
#else
using UnityEngine.VR.WSA.WebCam;
#endif
#endif


namespace Autodesk.Forge.ARKit {

#if !UNITY_EDITOR && UNITY_WSA
	public partial class QRCodeZxingWebDecoder : QRCodeDecoderBase {

	#region Fields
		protected PhotoCapture _photoCapture =null ;
		private Resolution _cameraResolution ;

	#endregion

		#region Unity APIs
		//protected virtual void OnEnable () 
		//protected virtual void OnDisable ()

		protected override void Update () {
		}

	#endregion

	#region Methods
		protected override bool InitCamera () {
			_cameraResolution =PhotoCapture.SupportedResolutions.OrderByDescending ((res) => res.width * res.height).First () ;
			PhotoCapture.CreateAsync (false, delegate (PhotoCapture captureObject) {
				_photoCapture =captureObject ;
	
				CameraParameters cameraParameters =new CameraParameters () ;
				cameraParameters.hologramOpacity =1.0f ;
				cameraParameters.cameraResolutionWidth =_cameraResolution.width ;
				cameraParameters.cameraResolutionHeight =_cameraResolution.height ;
				cameraParameters.pixelFormat =CapturePixelFormat.BGRA32 ; // CapturePixelFormat.JPEG/CapturePixelFormat.PNG;

				// Start the photo mode and start taking pictures
				_photoCapture.StartPhotoModeAsync (cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
					//InvokeRepeating ("ProcessCameraFrame", 0, _pictureInterval) ;
					Invoke ("ProcessCameraFrame", 0) ;
				}) ;
			}) ;
			return (true) ;
		}

		protected override void StopCamera () {
			if ( _photoCapture == null )
				return ;
			_photoCapture.StopPhotoModeAsync (delegate (PhotoCapture.PhotoCaptureResult res) {
				_photoCapture.Dispose () ;
				_photoCapture =null ;
			}) ;
		}

		protected override void ProcessCameraFrame () {
			if ( _photoCapture != null ) {
				_photoCapture.TakePhotoAsync (delegate (PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame) {
					if ( _photoTaken != null ) {
						//UnityEngine.WSA.Application.InvokeOnAppThread (() => {
							_photoTaken.Play () ;
						//}, false) ;
					}
					List<byte> buffer =new List<byte> () ;
					photoCaptureFrame.CopyRawImageDataIntoBuffer (buffer) ;

					Texture2D tex =new Texture2D (_cameraResolution.width, _cameraResolution.height) ;
					photoCaptureFrame.UploadImageDataToTexture (tex) ; // not supported by JPEG
					tex.Apply () ;
					byte[] jpeg =ImageConversion.EncodeToJPG (tex) ;

					if ( _image != null ) {
						//UnityEngine.WSA.Application.InvokeOnAppThread (() => {
							_image.texture =tex ;
						//	photoCaptureFrame.UploadImageDataToTexture (_image.texture as Texture2D) ;
						//}, false) ;
					}

					//static byte[] EncodeToJPG(Texture2D tex);

					// Check if we can receive the position where the photo was taken
					Matrix4x4 cameraToWorldMatrix ;
					if ( !photoCaptureFrame.TryGetCameraToWorldMatrix (out cameraToWorldMatrix) )
						cameraToWorldMatrix =Matrix4x4.identity ;
					// Start a coroutine to handle the server request
					StartCoroutine (UploadAndHandlePhoto (jpeg, cameraToWorldMatrix)) ;
				}) ;
			}
		}

		protected override bool ReturnOrLoop (string json, Vector3 position, Quaternion rotation) {
			if ( json != null || (json == null && _searchUntilFound == false) ) {
				//UnityEngine.WSA.Application.InvokeOnAppThread (() => {
					if ( _decodeSuccessful != null && json != null )
						_decodeSuccessful.Play () ;
					else if ( _decodeFailed != null && json == null )
						_decodeFailed.Play () ;
					gameObject.SetActive (false) ;
					_qrDecoded.Invoke (json, position, rotation) ;
				//}, false) ;
				return (true) ;
			} else {
				Invoke ("ProcessCameraFrame", 0) ;
				return (false) ;
			}
		}

	#endregion

	}

#endif

}