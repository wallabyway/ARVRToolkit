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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SimpleJSON;
#if FORGE_HUX
using HUX.Receivers;
#endif


namespace Autodesk.Forge.ARKit {

	#if FORGE_HUX
	using __InteractionReceiver__ =InteractionReceiver ;
	#else
	using __InteractionReceiver__ =MonoBehaviour;
	#endif

	public class ForgePropertiesPanel : __InteractionReceiver__/*, IPointerDownHandler*/ {

		#region Fields
		#if !FORGE_HUX
		[Tooltip("Target Interactible Object to receive events for")]
		public List<GameObject> Interactibles =new List<GameObject> () ;
		#endif
		[LabelOverride ("Title")]
		public GameObject _title =null ;
		[LabelOverride ("Property List")]
		public GameObject _list =null ;
		[LabelOverride ("Properties Template")]
		public GameObject _template =null ;
		[LabelOverride ("Close Button")]
		public GameObject _close =null ;

		//protected Dictionary<string, bool> ShowPosition =new Dictionary<string, bool> () ;

		#endregion

		#region Unity APIs
		protected void Start () {
			if ( _close != null && !Interactibles.Contains (_close) )
				Interactibles.Add (_close) ;
			#if !FORGE_HUX
			if ( _close ) {
				Button button =_close.GetComponent<Button> () ;
				button.enabled =true ;
			}
			#endif
		}
	
		//#if !FORGE_HUX
		//void Update () {
		//	if ( Input.GetMouseButtonDown (0) ) // if left button pressed...
		//		OnTapped () ;
		//}
		//#endif

		#endregion

		#region Methods
		public void LoadProperties (string jsonSt, Vector3 position, Vector3 normal) {
			JSONNode json =JSON.Parse (jsonSt) ;
			LoadProperties (json, position, normal) ;
		}

		public void LoadProperties (ForgeProperties props, Vector3 position, Vector3 normal) {
			JSONNode json =props.Properties ;
			LoadProperties (json, position, normal) ;
		}

		public void LoadProperties (JSONNode j, Vector3 position, Vector3 normal) {
			foreach ( Transform child in _list.transform )
				GameObject.Destroy (child.gameObject) ;
			gameObject.SetActive (true) ;
			_title.GetComponent<Text> ().text ="" ;
			gameObject.transform.localPosition =position ;
			normal.y =0f ;
			gameObject.transform.rotation =Quaternion.LookRotation (-normal) ;

			// Sort properties per category
			Dictionary<string, List<JSONNode>> properties =new Dictionary<string, List<JSONNode>> () ;
			foreach ( JSONNode child in j ["props"].AsArray ) {
				if ( child ["hidden"] == true )
					continue ;
				string category =GetDefaultValueIfUndefined (child, "category", "Misc") ;
				if ( !properties.ContainsKey (category) )
					properties.Add (category, new List<JSONNode> ()) ;
				properties [category].Add (child) ;
			}
			foreach ( KeyValuePair<string, List<JSONNode>> entry in properties ) {
				int nbVisible =0 ;
				foreach ( JSONNode item in entry.Value ) {
					if ( item ["hidden"].AsBool != true )
						nbVisible++ ; 
				}

				//if ( !ShowPosition.ContainsKey (entry.Key) )
				//	ShowPosition.Add (entry.Key, true) ;
				//ShowPosition [entry.Key] =EditorGUILayout.Foldout (ShowPosition [entry.Key], entry.Key) ;
				//if ( ShowPosition [entry.Key] == true ) {
					foreach ( JSONNode item in entry.Value ) {
						string name =GetDefaultValueIfUndefined (item, "displayName", "") ;
						if ( string.IsNullOrEmpty (name) )
							name =GetDefaultValueIfUndefined (item, "name", "") ;

						string value =GetDefaultValueIfUndefined (item, "value", "") ;
						string unit =GetDefaultValueIfUndefined (item, "unit", "") ;
						value +=" " + unit ;
							
						if ( item ["type"].AsInt == 1 )
							value =(item ["value"].AsInt == 1).ToString () ;
						//EditorGUILayout.LabelField (name, value) ;

						GameObject obj =GameObject.Instantiate (_template) ;
						obj.transform.SetParent (_list.transform, false) ;
						//obj.transform.parent =_list.transform ;
						//obj.transform.localPosition =Vector3.zero ;
						//obj.transform.localScale =Vector3.one ;
						//obj.transform.rotation =gameObject.transform.rotation ;
						obj.SetActive (true) ;
						GameObject title =obj.transform.Find ("Title").gameObject ;
						title.GetComponent<Text> ().text =name ;
						GameObject val =obj.transform.Find ("Property").gameObject ;
						val.GetComponent<Text> ().text =value ;

						if ( name == "name" || name == "label" )
							_title.GetComponent<Text> ().text =value ;
					}
				//}
			}
		}

		protected static string GetDefaultValueIfUndefined (JSONNode j, string name, string defaultValue ="") {
			string ret =j [name].Value == "null" || string.IsNullOrEmpty (j [name].Value) ? defaultValue : j [name].Value ;
			return (ret) ;
		}

		#endregion

		#region HUX APIs
		#if FORGE_HUX
		protected override void OnTapped (GameObject obj, HUX.Interaction.InteractionManager.InteractionEventArgs eventArgs) {
			switch ( obj.name ) {
				case "Close":
					gameObject.SetActive (false) ;
					break ;
			}
			base.OnTapped (obj, eventArgs) ;
		}
		#else
		protected void OnTapped () {
			GameObject obj =null ;
			Ray GazeRay =Camera.main.ScreenPointToRay (Input.mousePosition) ;
			RaycastHit info ;
			//int layerMask =LayerMask.NameToLayer ("UI") ;
			if ( !Physics.Raycast (GazeRay, out info/*, 5f, layerMask*/) )
				return ;
			obj =info.transform.gameObject ;
			if ( !Interactibles.Contains (obj) )
				return ;
			gameObject.SetActive (false) ;
		}

		//public void OnPointerDown (PointerEventData eventData) {
		//	gameObject.SetActive (false) ;
		//}
		#endif

		#endregion

	}

}
