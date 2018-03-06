// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	[System.Serializable]
	public class OauthCredentialsReceived : UnityEvent<string> {
	}

	internal interface OauthProtocolInterface {

		#region Properties
		bool isActive { get; }
		bool isValid { get; }
		bool is2legged { get; }
		bool is3legged { get; }
		string access_token { get; }
		string refresh_token { get; }
		//JSONNode credentials { get; set; }
		//DateTime expiresAt { get; set; }

		#endregion

		#region Unity APIs
		//void OnEnable () ;
		//void OnDisable () ;

		#endregion

		#region Methods
		void Refresh () ;

		#endregion

	}

	public abstract class OauthProtocol : MonoBehaviour, OauthProtocolInterface {

		#region Properties
		public bool isActive { get; internal set; }
		public bool isValid { get { return (credentials != null && DateTime.Now < expiresAt) ; } }
		public bool is2legged { get { return (isValid && credentials ["refresh_token"] == null) ; } }
		public bool is3legged { get { return (isValid && credentials ["refresh_token"] != null) ; } }
		public string access_token { get { return (credentials == null ? "" : credentials ["access_token"].Value) ; } }
		public string refresh_token { get { return (credentials == null ? "" : credentials ["refresh_token"].Value) ; } }
		protected JSONNode credentials { get; set; }
		protected DateTime expiresAt { get; set; }

		#endregion

		#region Properties
		internal System.Threading.Timer _timer { get; set; }

		#endregion

		#region Fields
		//[Header ("Forge Loaders object binding", order =2)]
		//[LabelOverride ("Forge Loaders")]
		//public List<GameObject> _ForgeLoaders =null ;

		//[Header ("Events", order =3)]
		//[SerializeField]
		//public OauthCredentialsReceived _CredentialsReceived =new OauthCredentialsReceived () ;

		#endregion

		#region Unity APIs
		protected virtual void OnEnable () {
			isActive =true ;
			Refresh () ;
		}

		protected virtual void OnDisable () {
			isActive =false ;
			if ( _timer != null ) {
				_timer.Dispose () ;
				_timer =null ;
			}
		}

		#endregion

		#region Methods
		public abstract void Refresh () ;
		
		public virtual void SetCredentials (string json) {
			try {
				JSONNode token =JSON.Parse (json) ;
				SetCredentials (token) ;
			} catch ( Exception /*ex*/ ) {
			}
		}

		public virtual void SetCredentials (JSONNode json) {
			credentials =json ;
		}

		#endregion

	}

	public class LabelOverride : PropertyAttribute {

		#region Fields
		public string _label ;

		#endregion

		#region Constructors
		public LabelOverride (string label) {
			this._label =label ;
		}

		#endregion

		#region Drawer
		#if UNITY_EDITOR
		[CustomPropertyDrawer (typeof(LabelOverride))]
		public class ThisPropertyDrawer : PropertyDrawer {
			public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
				var propertyAttribute =this.attribute as LabelOverride ;
				label.text =propertyAttribute._label ;
				EditorGUI.PropertyField (position, property, label) ;
			}
		}
		#endif

		#endregion

	}

}

