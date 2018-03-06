//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif


// http://heliosinteractive.com/scaling-ui-hololens/

namespace Autodesk.Forge.ARKit {

	public class HuD : MonoBehaviour {

		#region Fields
		public GameObject LeftPanel = null;
		public GameObject RightPanel = null;
		public GameObject TopPanel = null;
		public GameObject BottomPanel = null;
		public GameObject CenterPanel = null;

		public Color TextColor = new Color (1, 1, 1, 1);
		public Color BackgroundColor = new Color (0, 0, 0, 0.3f);

		#endregion

		#region Unity APIs
		protected virtual void Start () {
			SetLeftPanelColor (TextColor);
			SetRightPanelColor (TextColor);
			SetLeftPanelBckColor (BackgroundColor);
			SetRightPanelBckColor (BackgroundColor);
		}

		protected virtual void Update () {
			//			Canvas canvas =GetComponent<Canvas> () ;
			//			if ( canvas.worldCamera == null ) {
			//				canvas.worldCamera =Camera.main ;
			//				canvas.planeDistance =1.0f ;
			//			}
			//			if ( canvas.GetComponent<Camera> () == null ) {
			//				
			//	
			//			}
		}

		#endregion

		#region HuD Controllers
		public bool hasActiveHuD {
			get {
				bool flag = false;
				if ( LeftPanel ) flag |= true;
				if ( RightPanel ) flag |= true;
				if ( TopPanel ) flag |= true;
				if ( BottomPanel ) flag |= true;
				if ( CenterPanel ) flag |= true;
				return (flag);
			}
		}

		public void ActivatePanel (GameObject panel) {
			if ( panel == LeftPanel ) ShowLeftPanel ();
			if ( panel == RightPanel ) ShowRightPanel ();
			if ( panel == TopPanel ) ShowTopPanel ();
			if ( panel == BottomPanel ) ShowBottomPanel ();
			if ( panel == CenterPanel ) ShowCenterPanel ();
		}

		public void DeactivatePanel (GameObject panel) {
			if ( panel == LeftPanel ) HideLeftPanel ();
			if ( panel == RightPanel ) HideRightPanel ();
			if ( panel == TopPanel ) HideTopPanel ();
			if ( panel == BottomPanel ) HideBottomPanel ();
			if ( panel == CenterPanel ) HideCenterPanel ();
		}

		#endregion

		#region Left Controllers
		public void ShowLeftPanel () {
			if ( !LeftPanel )
				return;
			LeftPanel.SetActive (true);
			gameObject.SetActive (true);
		}

		public void HideLeftPanel () {
			if ( !LeftPanel )
				return;
			LeftPanel.SetActive (false);
			if ( !hasActiveHuD )
				gameObject.SetActive (false);
		}

		public void ToggleLeftPanel () {
			if ( !LeftPanel )
				return;
			if ( LeftPanel.activeSelf )
				HideLeftPanel ();
			else
				ShowLeftPanel ();
		}

		public void SetLeftPanelText (string text) {
			if ( !LeftPanel )
				return;
			Text oText = LeftPanel.GetComponentInChildren<Text> ();
			oText.text = text;
		}

		public void SetLeftPanelColor (Color32 color) {
			if ( !LeftPanel )
				return;
			Text oText = LeftPanel.GetComponentInChildren<Text> ();
			oText.color = color;
		}

		public void SetLeftPanelBckColor (Color32 color) {
			if ( !LeftPanel )
				return;
			Image img = LeftPanel.GetComponentInChildren<Image> ();
			img.color = color;
		}

		#endregion

		#region Right Controllers
		public void ShowRightPanel () {
			if ( !RightPanel )
				return;
			RightPanel.SetActive (true);
			gameObject.SetActive (true);
		}

		public void HideRightPanel () {
			if ( !RightPanel )
				return;
			RightPanel.SetActive (false);
			if ( !hasActiveHuD )
				gameObject.SetActive (false);
		}

		public void ToggleRightPanel () {
			if ( !RightPanel )
				return;
			if ( RightPanel.activeSelf )
				HideRightPanel ();
			else
				ShowRightPanel ();
		}

		public void SetRightPanelText (string text) {
			if ( !RightPanel )
				return;
			Text oText = RightPanel.GetComponentInChildren<Text> ();
			oText.text = text;
		}

		public void SetRightPanelColor (Color32 color) {
			if ( !RightPanel )
				return;
			Text oText = RightPanel.GetComponentInChildren<Text> ();
			oText.color = color;
		}

		public void SetRightPanelBckColor (Color32 color) {
			if ( !RightPanel )
				return;
			Image img = RightPanel.GetComponentInChildren<Image> ();
			img.color = color;
		}

		#endregion

		#region Top Controllers
		public void ShowTopPanel () {
			if ( !TopPanel )
				return;
			TopPanel.SetActive (true);
			gameObject.SetActive (true);
		}

		public void HideTopPanel () {
			if ( !TopPanel )
				return;
			TopPanel.SetActive (false);
			if ( !hasActiveHuD )
				gameObject.SetActive (false);
		}

		public void ToggleTopPanel () {
			if ( !TopPanel )
				return;
			if ( TopPanel.activeSelf )
				HideTopPanel ();
			else
				ShowTopPanel ();
		}

		public void SetTopPanelBckColor (Color32 color) {
			if ( !TopPanel )
				return;
			Image img = TopPanel.GetComponentInChildren<Image> ();
			img.color = color;
		}

		#endregion

		#region Bottom Controllers
		public void ShowBottomPanel () {
			if ( !BottomPanel )
				return;
			BottomPanel.SetActive (true);
			gameObject.SetActive (true);
		}

		public void HideBottomPanel () {
			if ( !BottomPanel )
				return;
			BottomPanel.SetActive (false);
			if ( !hasActiveHuD )
				gameObject.SetActive (false);
		}

		public void ToggleBottomPanel () {
			if ( !BottomPanel )
				return;
			if ( BottomPanel.activeSelf )
				HideBottomPanel ();
			else
				ShowBottomPanel ();
		}

		public void SetBottomPanelBckColor (Color32 color) {
			if ( !BottomPanel )
				return;
			Image img = BottomPanel.GetComponentInChildren<Image> ();
			img.color = color;
		}

		#endregion

		#region Center Controllers
		public void ShowCenterPanel () {
			if ( !CenterPanel )
				return;
			CenterPanel.SetActive (true);
			gameObject.SetActive (true);
		}

		public void HideCenterPanel () {
			if ( !CenterPanel )
				return;
			CenterPanel.SetActive (false);
			if ( !hasActiveHuD )
				gameObject.SetActive (false);
		}

		public void ToggleCenterPanel () {
			if ( !CenterPanel )
				return;
			if ( CenterPanel.activeSelf )
				HideCenterPanel ();
			else
				ShowCenterPanel ();
		}

		public void SetCenterPanelBckColor (Color32 color) {
			if ( !CenterPanel )
				return;
			Image img = CenterPanel.GetComponentInChildren<Image> ();
			img.color = color;
		}

		#endregion

	}

}