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
using System.ComponentModel;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
#if !UNITY_WSA
using System.Net;
#endif
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	[System.Serializable]
	[System.Obsolete("Deprecated, use oAuthCredentialsReceived instead", true)]
	public class oAuthCompletedEvent : UnityEvent<string> {
	}

	[System.Obsolete ("Deprecated, use OauthKeys instead", true)]
	public class ForgeOAuth2Legged : MonoBehaviour {

		#region Fields
		public string CLIENTID ="" ;
		public string CLIENTSECRET ="" ;
		protected string BEARER ="" ;
		public List<GameObject> LOADERS =null ;

		[SerializeField]
		public oAuthCompletedEvent oAuthCompleted = new oAuthCompletedEvent () ;

		#endregion

		#region Unity APIs
		protected void Awake () {
			//if ( !UnityMainThreadDispatcher.Exists () )
			//	return ;
			oAuth2Legged () ;
		}

		#endregion

		#region Methods
		protected void oAuth2Legged () {
			oAuth2Legged rest =new oAuth2Legged (CLIENTID, CLIENTSECRET) ;
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

					//UploadValuesCompletedEventArgs args2 =args as UploadValuesCompletedEventArgs ;
					//byte[] data =args2.Result ;
					//string textData =System.Text.Encoding.UTF8.GetString (data) ;

					UploadValuesCompletedEventArgs args2 =args as UploadValuesCompletedEventArgs;
					string textData =Encoding.UTF8.GetString (args2.Result) ;

					JSONNode json =JSON.Parse (textData) ;

					BEARER =json ["access_token"] ;
					UnityMainThreadDispatcher.Instance ().Enqueue (() => {
						oAuthCompleted.Invoke (BEARER) ;
					}) ;

					if ( LOADERS != null ) {
						UnityMainThreadDispatcher.Instance ().Enqueue (() => {
							for ( int i =0 ; i < LOADERS.Count ; i++ ) {
								GameObject loader =LOADERS [i] ;
								ForgeLoader forgeLoader =loader.GetComponent<ForgeLoader> () ;
								forgeLoader.BEARER =BEARER ;
								if ( string.IsNullOrEmpty (forgeLoader.URN) || string.IsNullOrEmpty (forgeLoader.SCENEID) )
									continue ;
								loader.SetActive (true) ;
							}
						}) ;
					}
				}
			) ;
		}

		#endregion

	}

}
