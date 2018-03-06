//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//

//#define UNITY_WSA

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
#if !UNITY_WSA
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
#elif UNITY_WSA
using UnityEngine.Networking;
#endif
using UnityEngine;


namespace Autodesk.Forge.ARKit {

	public class RestClient {

		#region Enums
		public enum RequestStatus {
			eNew,
			ePending,
			eReceived,
			eCancelled,
			eError,
			eCompleted
		}

		#endregion

		#region Properties
		public RequestStatus state { get; set; }
		public Uri uri { get; set; }
		public DateTime emitted { get; set; }
		public Hashtable headers { get; set; }
#if !UNITY_WSA
		public WebClient client { get; set; }
#elif UNITY_WSA
		public UnityWebRequest client { get; set; }
		protected MonoBehaviour mb { get; set; } // The surrogate MonoBehaviour that we'll use to manage this coroutine.
#endif

		#endregion

		#region Constructors
		public RestClient (Uri _uri, Hashtable _headers =null) {
#if !UNITY_WSA
			//Debug.Log (System.Environment.Version) ; // 2.0
			//ServicePointManager.CertificatePolicy =new NoCheckCertificatePolicy () ;
			SSLValidator.OverrideValidation ();
#endif

			state = RequestStatus.eNew;
			uri = _uri;
			headers = (_headers == null ? new Hashtable () : _headers);
#if UNITY_WSA
			mb =GameObject.FindObjectOfType<MonoBehaviour> () ;
#endif
		}

		#endregion

		#region Request workflow
#if !UNITY_WSA
		public virtual void FireRequest (Action<object, AsyncCompletedEventArgs> callback = null) {
			emitted = DateTime.Now;
			try {
				using ( client = new WebClient() ) {
					if ( callback != null )
						client.DownloadDataCompleted += new DownloadDataCompletedEventHandler (callback);
					foreach ( DictionaryEntry entry in headers )
						client.Headers.Add (entry.Key.ToString (), entry.Value.ToString ());
					client.Headers.Add ("Keep-Alive", "timeout=15, max=100");
					state = RequestStatus.ePending;
					client.DownloadDataAsync (uri, this);
				}
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = RequestStatus.eError;
			}
		}
#elif UNITY_WSA
		public virtual void FireRequest (Action<object, AsyncCompletedEventArgs> callback =null) {
			emitted = DateTime.Now;
			mb.StartCoroutine (_FireRequest_ (callback)) ;
		}

		public virtual IEnumerator _FireRequest_ (Action<object, AsyncCompletedEventArgs> callback =null) {
			using ( client =UnityWebRequest.Get (uri.AbsoluteUri) ) {
				foreach ( DictionaryEntry entry in headers )
					client.SetRequestHeader (entry.Key.ToString (), entry.Value.ToString ()) ;
				state =RequestStatus.ePending ;
				//client.DownloadDataAsync (uri, this) ;
				#if UNITY_2017_2_OR_NEWER
				yield return client.SendWebRequest () ;
				#else
				yield return client.Send () ;
				#endif

				if ( client.isNetworkError || client.isHttpError) {
					Debug.Log (ForgeLoader.GetCurrentMethod () + " " + client.error + " - " + client.responseCode) ;
					state =RequestStatus.eError ;
				} else {
					//client.downloadHandler.data
					//client.downloadHandler.text
					if ( callback != null ) {
						DownloadDataCompletedEventArgs args =new DownloadDataCompletedEventArgs (null, false, this) ;
						args.Result =client.downloadHandler.data ;
						callback (this, args) ;
					}
				}
			}
		}
#endif

		public virtual void CancelRequest () {
			state = RequestStatus.eCancelled;
#if !UNITY_WSA
			client.CancelAsync ();
#elif UNITY_WSA
			//client.Abort () ;
#endif
		}

		#endregion

		#region Certificate passthrough for Debug
#if !UNITY_WSA
		//protected class NoCheckCertificatePolicy : ICertificatePolicy {
		//
		//	public bool CheckValidationResult (ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem) {
		//		return (true) ;
		//	}
		//
		//}

		// https://stackoverflow.com/questions/18454292/system-net-certificatepolicy-to-servercertificatevalidationcallback-accept-all-c
		protected static class SSLValidator {

			private static bool OnValidateCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
				return (true);
			}

			public static void OverrideValidation () {
				ServicePointManager.ServerCertificateValidationCallback = OnValidateCertificate;
				ServicePointManager.Expect100Continue = true;
			}

		}
#endif

		#endregion

	}

	#region Async Missing Classes
#if UNITY_WSA //|| DAQRI_SMART_HELMET
	public class UploadStringCompletedEventArgs : AsyncCompletedEventArgs {

		public string Result { get; set; }

		public UploadStringCompletedEventArgs (Exception error, bool cancelled, object userState)
			: base (error, cancelled, userState) {}

	}

	public class DownloadStringCompletedEventArgs : AsyncCompletedEventArgs {

		public string Result { get; set; }

		public DownloadStringCompletedEventArgs (Exception error, bool cancelled, object userState)
			: base (error, cancelled, userState) {}

	}

	public class DownloadDataCompletedEventArgs : AsyncCompletedEventArgs {

		public byte[] Result { get; set; }

		public DownloadDataCompletedEventArgs (Exception error, bool cancelled, object userState)
			: base (error, cancelled, userState) {}

	}

	public class UploadValuesCompletedEventArgs : AsyncCompletedEventArgs {

		public byte[] Result { get; set; }

		public UploadValuesCompletedEventArgs (Exception error, bool cancelled, object userState)
			: base (error, cancelled, userState) {}

	}

#endif

	#endregion

}