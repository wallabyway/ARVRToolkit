// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
#if !UNITY_WSA
using System.Net;
#endif
using UnityEngine;
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public class OauthURL : OauthProtocol {

		#region Fields
		[Header ("Get Token URL", order =1)]
		[LabelOverride ("URL")]
		public string _URL ="" ;

		[Header ("Forge Loaders object binding", order =2)]
		[LabelOverride ("Forge Loaders")]
		public List<GameObject> _ForgeLoaders =null ;

		[Header ("Events", order =3)]
		[SerializeField]
		public OauthCredentialsReceived _CredentialsReceived =new OauthCredentialsReceived () ;

		#endregion

		#region Unity APIs

		#endregion

		#region Methods
		public override void Refresh () {
			if ( string.IsNullOrEmpty (_URL) )
				return ;

			RestClient rest =new RestClient (new Uri (_URL), null) ;
			rest.FireRequest (
				(object sender, AsyncCompletedEventArgs args) => {
					if ( args == null || args.UserState == null )
						return ;
					if ( args.Error != null ) {
						UnityMainThreadDispatcher.Instance ().Enqueue (() => {
							Debug.Log (ForgeLoader.GetCurrentMethod () + " " + args.Error.Message) ;
						}) ;
						return ;
					}

					DownloadDataCompletedEventArgs args2 =args as DownloadDataCompletedEventArgs ;
					string textData =System.Text.Encoding.UTF8.GetString (args2.Result) ;

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

	}

}
