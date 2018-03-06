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
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public class OauthBearer : OauthProtocol {

		#region Fields
		[Header("Static Access Token (not recommended)", order =1)]
		[LabelOverride ("Access Token")]
		public string _BEARER ="" ;

		[Header ("Forge Loaders object binding", order =2)]
		[LabelOverride ("Forge Loaders")]
		public List<GameObject> _ForgeLoaders =null ;


		[Header ("Events", order =3)]
		[SerializeField]
		public OauthCredentialsReceived _CredentialsReceived =new OauthCredentialsReceived () ;

		#endregion

		#region Properties
		public new string access_token { get { return (_BEARER) ; } }

		#endregion

		#region Unity APIs
		protected void Awake () {
			_BEARER =!String.IsNullOrEmpty(_BEARER) ? _BEARER : ForgeLoaderConstants._BEARER ;
		}

		#endregion

		#region Methods
		public override void Refresh () {
			if ( !string.IsNullOrEmpty (_BEARER) ) {
				_CredentialsReceived.Invoke (_BEARER) ;

				for ( int i =0 ; _ForgeLoaders != null && i < _ForgeLoaders.Count ; i++ ) {
					GameObject loader =_ForgeLoaders [i] ;
					ForgeLoader forgeLoader =loader.GetComponent<ForgeLoader> () ;
					if ( forgeLoader == null )
						continue ;
					forgeLoader._BEARER =_BEARER ;
					if ( string.IsNullOrEmpty (forgeLoader.URN) || string.IsNullOrEmpty (forgeLoader.SCENEID) )
						continue ;
					loader.SetActive (true) ;
				}
			}
		}

		public override void SetCredentials (JSONNode json) {
			base.SetCredentials (json) ;
			_BEARER =json ["access_token"].Value ;
		}

		#endregion

	}

}

