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
using UnityEngine;
using UnityEngine.UI;
#if !UNITY_WSA
using System.Net;
#endif
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public class Commands : MonoBehaviour {

		#region Fields
		protected string _bearer ="" ;
		public GameObject loader =null ;
		public GameObject combobox =null ;

		protected JSONNode scenesList ;

		#endregion

		#region Unity APIs
		protected void Awake () {
		}

		#endregion

		#region Commands
		public void ListScenes (string bearer) {
			_bearer =bearer ;
			ForgeLoader forgeLoader =loader.GetComponent<ForgeLoader> () ;
			string url =ForgeLoaderConstants._endpoint1 + forgeLoader.URN + "/scenes" ;
			Hashtable headers =new Hashtable () ;
			headers.Add ("Authorization", "Bearer " + _bearer) ;
			RestClient rest =new RestClient (new System.Uri (url), headers) ;
			rest.FireRequest (
				(object sender, AsyncCompletedEventArgs args) => {
					if ( args == null || args.UserState == null )
						return ;
					if ( args.Error != null ) {
						UnityMainThreadDispatcher.Instance ().Enqueue (() => {
							Debug.Log (Autodesk.Forge.ARKit.ForgeLoader.GetCurrentMethod () + " " + args.Error.Message) ;
						}) ;
						return ;
					}

					DownloadDataCompletedEventArgs args2 =args as DownloadDataCompletedEventArgs ;
					string textData =System.Text.Encoding.UTF8.GetString (args2.Result) ;

					scenesList =JSON.Parse (textData) ;

					if ( scenesList.AsArray.Count > 0 ) {
						UnityMainThreadDispatcher.Instance ().Enqueue (() => {
							Dropdown dd =combobox.GetComponent<Dropdown> () ;
							foreach ( JSONNode child in scenesList.AsArray ) {
								dd.options.Add (new Dropdown.OptionData (child.Value)) ;
							}
							combobox.SetActive (true) ;
						}) ;
					}
				}
			) ;
		}

		public void LoadScene (int scene) {
			if ( scene <= 0 || scene > scenesList.AsArray.Count )
				return ;
			scene-- ;
			ForgeLoader forgeLoader = loader.GetComponent<ForgeLoader> () ;

			GameObject obj =new GameObject () ;
			obj.SetActive (false) ;
			ForgeLoader objLoader =obj.AddComponent<ForgeLoader> () ;
			objLoader.URN = forgeLoader.URN ;
			objLoader.BEARER =_bearer ;
			objLoader.SCENEID =scenesList.AsArray [scene].Value ;
			objLoader.ProcessedNodes =forgeLoader.ProcessedNodes ;
			objLoader.ProcessingNodesCompleted =forgeLoader.ProcessingNodesCompleted ;
			obj.SetActive (true) ;
		}

		#endregion

	}

}
