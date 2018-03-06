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
using UnityEditor;
using SimpleJSON;

namespace Autodesk.Forge.ARKit {

	[CustomEditor (typeof(ForgeProperties))]
	public class ForgePropertiesEditor : Editor {

		#region Fields
		protected Dictionary<string, bool> ShowPosition =new Dictionary<string, bool> () ;

		#endregion

		#region Unity APIs
		public void OnSceneGUI () {
			Repaint () ;
		}

		public override void OnInspectorGUI () {
			ForgeProperties myTarget =(ForgeProperties)target ;

			if ( myTarget.Properties == null )
				return ;
			JSONNode j =myTarget.Properties ;
			if ( j ["props"].Count != 0 )
				PropertiesUI (j) ;
			else
				MetadataUI (j) ;
		}

		#endregion

		#region Methods
		protected static string GetDefaultValueIfUndefined (JSONNode j, string name, string defaultValue ="") {
			string ret =j [name].Value == "null" || string.IsNullOrEmpty (j [name].Value) ? defaultValue : j [name].Value ;
			return (ret) ;
		}

		public void PropertiesUI (JSONNode j) {
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
				if ( !ShowPosition.ContainsKey (entry.Key) )
					ShowPosition.Add (entry.Key, true) ;
				ShowPosition [entry.Key] =EditorGUILayout.Foldout (ShowPosition [entry.Key], entry.Key) ;
				if ( ShowPosition [entry.Key] == true ) {
					foreach ( JSONNode item in entry.Value ) {
						string name =GetDefaultValueIfUndefined (item, "displayName", "") ;
						if ( string.IsNullOrEmpty (name) )
							name =GetDefaultValueIfUndefined (item, "name", "") ;

						string value =GetDefaultValueIfUndefined (item, "value", "") ;
						string unit =GetDefaultValueIfUndefined (item, "unit", "") ;
						value +=" " + unit;
							
						if ( item ["type"].AsInt == 1 )
							value =(item ["value"].AsInt == 1).ToString () ;
						EditorGUILayout.LabelField (name, value) ;
					}
				}
			}
		}

		public void MetadataUI (JSONNode j) {
			MetadataObjectUI (j.AsObject) ;
		}

		public void MetadataObjectUI (JSONObject j) {
			JSONNode.Enumerator iter =j.GetEnumerator () ;
			while ( iter.MoveNext () ) {
				KeyValuePair<string, JSONNode> N =(KeyValuePair<string, JSONNode>)iter.Current ;
				if ( N.Value.AsArray != null && N.Value.AsArray.Count != 0 ) {
					if ( N.Value.AsArray.Count == 3 && N.Value.AsArray [0].Value != "XYZ" ) {
						MetadataVector3UI (N.Value.AsArray, N.Key) ;
						continue ;
					}
					if ( !ShowPosition.ContainsKey (N.Key) )
						ShowPosition.Add (N.Key, true) ;
					ShowPosition [N.Key] =EditorGUILayout.Foldout (ShowPosition [N.Key], N.Key) ;
					if ( ShowPosition [N.Key] == true ) {
						EditorGUI.indentLevel++ ;
						MetadataArrayUI (N.Value.AsArray, name) ;
						EditorGUI.indentLevel-- ;
					}
					continue ;
				}
				if ( N.Value.AsObject != null && N.Value.AsObject.Count != 0 ) {
					if ( N.Value.AsObject.Count == 3 ) {
						JSONNode.Enumerator iter2 =N.Value.AsObject.GetEnumerator () ;
						int nb =0 ;
						while ( iter2.MoveNext () ) {
							KeyValuePair<string, JSONNode> K =(KeyValuePair<string, JSONNode>)iter2.Current ;
							if ( K.Key == "x" || K.Key == "y" || K.Key == "z" )
								nb++ ;
						}
						if ( nb == 3 ) {
							MetadataVector3UI (N.Value.AsObject, N.Key) ;
							continue ;
						}
					}
					if ( !ShowPosition.ContainsKey (N.Key) )
						ShowPosition.Add (N.Key, true) ;
					ShowPosition [N.Key] =EditorGUILayout.Foldout (ShowPosition [N.Key], N.Key) ;
					if ( ShowPosition [N.Key] == true ) {
						EditorGUI.indentLevel++ ;
						MetadataObjectUI (N.Value.AsObject) ;
						EditorGUI.indentLevel-- ;
					}
					continue ;
				}
				EditorGUILayout.LabelField (N.Key, N.Value.ToString ()) ;
			}
		}

		public void MetadataArrayUI (JSONArray j, string key) {
			int n =0 ;
			JSONNode.Enumerator iter =j.GetEnumerator () ;
			while ( iter.MoveNext () ) {
				JSONNode N =(JSONNode)iter.Current ;
				if ( N.AsArray != null && N.AsArray.Count != 0 ) {
					if ( N.AsArray.Count == 3 && N.AsArray [0].Value != "XYZ" ) {
						MetadataVector3UI (N.AsArray, n.ToString ()) ;
						n++ ;
						continue ;
					}
					string name =key + n.ToString () ;
					if ( !ShowPosition.ContainsKey (name) )
						ShowPosition.Add (name, true) ;
					ShowPosition [name] =EditorGUILayout.Foldout (ShowPosition [name], name) ;
					if ( ShowPosition [name] == true ) {
						EditorGUI.indentLevel++ ;
						MetadataArrayUI (N.AsArray, name) ;
						EditorGUI.indentLevel-- ;
					}
					n++ ;
					continue ;
				}
				if ( N.AsObject != null && N.AsObject.Count != 0 ) {
					string name =key + n.ToString () ;
					if ( !ShowPosition.ContainsKey (name) )
						ShowPosition.Add (name, true) ;
					ShowPosition [name] =EditorGUILayout.Foldout (ShowPosition [name], name) ;
					if ( ShowPosition [name] == true ) {
						EditorGUI.indentLevel++ ;
						MetadataObjectUI (N.AsObject) ;
						EditorGUI.indentLevel-- ;
					}
					n++ ;
					continue ;
				}
				EditorGUILayout.LabelField (n.ToString (), N.Value.ToString ()) ;
				n++ ;
			}
		}

		public void MetadataVector3UI (JSONArray j, string label) {
			EditorGUILayout.Vector3Field (
				label,
				new Vector3 (j [0].AsFloat, j [1].AsFloat, j [2].AsFloat)
			) ;
		}

		public void MetadataVector3UI (JSONObject j, string label) {
			EditorGUILayout.Vector3Field (
				label,
				new Vector3 (j ["x"].AsFloat, j ["y"].AsFloat, j ["z"].AsFloat)
			) ;
		}

		#endregion

	}

}