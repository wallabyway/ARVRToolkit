// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections.Specialized;
#if !UNITY_WSA
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
#else
using UnityEngine.Networking;
#endif
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public class OauthKeys : OauthProtocol {

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
		public DateTime emitted { get; set;  }
		#if !UNITY_WSA
		public WebClient client { get; set; }
		#else
		public UnityWebRequest client { get; set; }
		protected MonoBehaviour mb { get; set; } // The surrogate MonoBehaviour that we'll use to manage this coroutine.
		#endif

		#endregion

		#region Fields
		[Header ("Forge Developer Keys", order =1)]
		[LabelOverride ("Client ID")]
		public string _CLIENT_ID ="" ;
		[LabelOverride ("Client Secret")]
		public string _CLIENT_SECRET ="" ;

		[Header("Forge Loaders object binding", order =2)]
		[LabelOverride ("Forge Loaders")]
		public List<GameObject> _ForgeLoaders =null ;

		[Header ("Events", order =3)]
		[SerializeField]
		public OauthCredentialsReceived _CredentialsReceived =new OauthCredentialsReceived () ;

		#endregion

		#region Unity APIs
		protected void Awake () {
			_CLIENT_ID =!String.IsNullOrEmpty (_CLIENT_ID) ? _CLIENT_ID : ForgeLoaderConstants.FORGE_CLIENT_ID ;
			_CLIENT_SECRET =!String.IsNullOrEmpty (_CLIENT_SECRET) ? _CLIENT_SECRET : ForgeLoaderConstants.FORGE_CLIENT_SECRET ;
			#if !UNITY_WSA
			//Debug.Log (System.Environment.Version) ; // 2.0
			//ServicePointManager.CertificatePolicy =new NoCheckCertificatePolicy () ;
			SSLValidator.OverrideValidation ();
			#else
			mb =GameObject.FindObjectOfType<MonoBehaviour> () ;
			#endif
		}

		protected override void OnEnable () {
			state =RequestStatus.eNew ;
			base.OnEnable () ;
		}

		#endregion

		#region Methods
		public override void Refresh () {
			if ( string.IsNullOrEmpty (_CLIENT_ID) || string.IsNullOrEmpty (_CLIENT_SECRET) )
				return ;

			FireRequest (
				(object sender, AsyncCompletedEventArgs args) => {
					if ( args == null || args.UserState == null )
						return ;
					if ( args.Error != null ) {
						UnityMainThreadDispatcher.Instance ().Enqueue (() => {
							Debug.Log (ForgeLoader.GetCurrentMethod () + " " + args.Error.Message) ;
						}) ;
						return ;
					}

					#if !UNITY_WSA
					UploadStringCompletedEventArgs args2 =args as UploadStringCompletedEventArgs ;
					string textData =args2.Result ;
					#else
					UploadValuesCompletedEventArgs args2 =args as UploadValuesCompletedEventArgs ;
					string textData =System.Text.Encoding.UTF8.GetString (args2.Result) ;
					#endif
					
					try {
						credentials =JSON.Parse (textData) ;
						expiresAt =DateTime.Now + TimeSpan.FromSeconds (credentials ["expires_in"].AsDouble - 120 /*2 minutes*/) ;
						if ( isActive ) {
							_timer =new System.Threading.Timer ((obj) => {
									UnityMainThreadDispatcher.Instance().Enqueue(() => {
										Refresh () ;
									}) ;
								},
								null,
								Math.Max (2500, (int)(TimeSpan.FromSeconds (credentials ["expires_in"].AsDouble - 120 /*2 minutes*/).TotalMilliseconds)),
								System.Threading.Timeout.Infinite
							) ;
						}

						UnityMainThreadDispatcher.Instance().Enqueue(() => {
							_CredentialsReceived.Invoke (credentials ["access_token"].Value) ;

							for ( int i =0 ; _ForgeLoaders != null && i < _ForgeLoaders.Count ; i++ ) {
								GameObject loader =_ForgeLoaders [i] ;
								ForgeLoader forgeLoader =loader.GetComponent<ForgeLoader> () ;
								if ( forgeLoader == null )
									continue ;
								forgeLoader._BEARER =credentials ["access_token"].Value ;
								if ( string.IsNullOrEmpty (forgeLoader.URN) || string.IsNullOrEmpty (forgeLoader.SCENEID) )
									continue ;
								loader.SetActive (true) ;
							}
						}) ;
					} catch ( Exception ex ) {
						UnityMainThreadDispatcher.Instance ().Enqueue (() => {
							Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message) ;
						}) ;
					}
				}
			) ;
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
					client.Headers.Add ("Content-Type", "application/x-www-form-urlencoded") ;

					NameValueCollection form = new NameValueCollection () ;		
					form.Add ("client_id", _CLIENT_ID) ;
					form.Add ("client_secret", _CLIENT_SECRET) ;
					form.Add ("grant_type", "client_credentials") ;
					form.Add ("scope", "viewables:read%20data:read") ;

					string data ="client_id=" + _CLIENT_ID
						+ "&client_secret=" + _CLIENT_SECRET
						+ "&grant_type=client_credentials"
						+ "&scope=viewables:read%20data:read" ;

					state =RequestStatus.ePending ;
					//client.UploadValuesAsync (new Uri (ForgeLoaderConstants._forgeoAuth2legged), "POST", form, this) ;
					client.UploadStringAsync (new Uri (ForgeLoaderConstants._forgeoAuth2legged), "POST", data, this) ;
				}
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message) ;
				state =RequestStatus.eError ;
			}
		}

		#else
		public virtual void FireRequest (Action<object, AsyncCompletedEventArgs> callback =null) {
			emitted =DateTime.Now ;
			mb.StartCoroutine (_FireRequest_ (callback)) ;
		}

		public virtual IEnumerator _FireRequest_ (Action<object, AsyncCompletedEventArgs> callback =null) {
			WWWForm form =new WWWForm () ;
			form.AddField ("client_id", _CLIENT_ID) ;
			form.AddField ("client_secret", _CLIENT_SECRET) ;
			form.AddField ("grant_type", "client_credentials") ;
			form.AddField ("scope", "viewables:read data:read") ;

			using ( client =UnityWebRequest.Post (ForgeLoaderConstants._forgeoAuth2legged, form) ) {
				client.SetRequestHeader ("Content-Type", "application/x-www-form-urlencoded") ;

				state =RequestStatus.ePending ;
				//client.DownloadDataAsync (new Uri (ForgeLoaderConstants._forgeoAuth2legged), this) ;
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
			#else
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
