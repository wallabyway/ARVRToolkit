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
using System.ComponentModel;
using System.Collections.Specialized;
#if !UNITY_WSA
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
#elif UNITY_WSA
using UnityEngine.Networking;
#endif
using UnityEngine;


namespace Autodesk.Forge.ARKit {

	[System.Obsolete ("Deprecated, will be removed in future", true)]
	public class oAuth2Legged {

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
		public string CLIENT_ID { get; set; }
		public string CLIENT_SECRET { get; set; }
		public DateTime emitted { get; set;  }
		public Hashtable headers { get; set; }
#if !UNITY_WSA
		public WebClient client { get; set; }
#elif UNITY_WSA
		public UnityWebRequest client { get; set; }
		protected MonoBehaviour mb { get; set; } // The surrogate MonoBehaviour that we'll use to manage this coroutine.
#endif

		#endregion

		#region Constructors
		public oAuth2Legged (string clientID =null, string clientSecret =null, Hashtable _headers =null) {
#if !UNITY_WSA
			//Debug.Log (System.Environment.Version) ; // 2.0
			//ServicePointManager.CertificatePolicy =new NoCheckCertificatePolicy () ;
			SSLValidator.OverrideValidation ();
#endif
			state =RequestStatus.eNew ;
			uri =new Uri (ForgeLoaderConstants._forgeoAuth2legged) ;
			CLIENT_ID =!String.IsNullOrEmpty (clientID) ? clientID : ForgeLoaderConstants.FORGE_CLIENT_ID ;
			CLIENT_SECRET =!String.IsNullOrEmpty (clientSecret) ? clientSecret : ForgeLoaderConstants.FORGE_CLIENT_SECRET ;

			headers =(_headers == null ? new Hashtable () : _headers) ;
#if UNITY_WSA || COROUTINE
			mb =GameObject.FindObjectOfType<MonoBehaviour> () ;
#endif
		}

		#endregion

		#region Request workflow
#if !UNITY_WSA
		public virtual void FireRequest (Action<object, AsyncCompletedEventArgs> callback =null) {
			emitted =DateTime.Now ;
			try {
				using ( client = new WebClient () ) {
					if ( callback != null )
						//client.UploadValuesCompleted +=new UploadValuesCompletedEventHandler (callback) ;
						client.UploadStringCompleted +=new UploadStringCompletedEventHandler (callback) ;
					foreach ( DictionaryEntry entry in headers )
						client.Headers.Add (entry.Key.ToString (), entry.Value.ToString ()) ;
					client.Headers.Add ("Content-Type", "application/x-www-form-urlencoded") ;

					NameValueCollection form = new NameValueCollection () ;		
					form.Add ("client_id", CLIENT_ID) ;
					form.Add ("client_secret", CLIENT_SECRET) ;
					form.Add ("grant_type", "client_credentials") ;
					form.Add ("scope", "viewables:read%20data:read") ;

					string data ="client_id=" + CLIENT_ID
						+ "&client_secret=" + CLIENT_SECRET
						+ "&grant_type=client_credentials"
						+ "&scope=viewables:read%20data:read" ;

					state =RequestStatus.ePending ;
					//client.UploadValuesAsync (uri, "POST", form, this) ;
					client.UploadStringAsync (uri, "POST", data, this) ;

				}
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message) ;
				state =RequestStatus.eError ;
			}
		}
#elif UNITY_WSA
		public virtual void FireRequest (Action<object, AsyncCompletedEventArgs> callback =null) {
			emitted =DateTime.Now ;
			mb.StartCoroutine (_FireRequest_ (callback)) ;
		}

		public virtual IEnumerator _FireRequest_ (Action<object, AsyncCompletedEventArgs> callback =null) {
			WWWForm form =new WWWForm () ;
			form.AddField ("client_id", CLIENT_ID) ;
			form.AddField ("client_secret", CLIENT_SECRET) ;
			form.AddField ("grant_type", "client_credentials") ;
			form.AddField ("scope", "viewables:read data:read") ;

			using ( client =UnityWebRequest.Post (uri.AbsoluteUri, form) ) {
				foreach ( DictionaryEntry entry in headers )
					client.SetRequestHeader (entry.Key.ToString (), entry.Value.ToString ()) ;
				client.SetRequestHeader ("Content-Type", "application/x-www-form-urlencoded") ;

				state =RequestStatus.ePending ;
				//client.DownloadDataAsync (uri, this) ;
				#if UNITY_2017_2_OR_NEWER
				yield return client.SendWebRequest () ;
				#else
				yield return client.Send () ;
				#endif

				if ( client.isNetworkError || client.isHttpError ) {
					Debug.Log (ForgeLoader.GetCurrentMethod () + " " + client.error + " - " + client.responseCode) ;
					state =RequestStatus.eError ;
				} else {
					if ( callback != null ) {
						UploadValuesCompletedEventArgs args =new UploadValuesCompletedEventArgs (null, false, this) ;
						args.Result =client.downloadHandler.data ;
						callback (this, args) ;
					}
				}
			}
		}
#endif

		public virtual void CancelRequest () {
			state =RequestStatus.eCancelled ;
#if !UNITY_WSA
			client.CancelAsync () ;
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

}