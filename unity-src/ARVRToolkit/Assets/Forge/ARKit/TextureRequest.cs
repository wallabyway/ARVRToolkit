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
using System.ComponentModel;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if !UNITY_WSA && !COROUTINE
using System.Net;
#endif
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public class TextureRequest : RequestObjectInterface {

		#region Enums
		public enum SmoothnessMapChannel {
			SpecularMetallicAlpha,
			AlbedoAlpha
		}

		#endregion

		#region Properties
		public Material material { get; set; }
		public Texture texture { get; set; }
		public byte [] image { get; set; }

		#endregion

		#region Constructors
		public TextureRequest (IForgeLoaderInterface _loader, Uri _uri, string _bearer, Material _material, Texture _texture, JSONNode node) : base (_loader, _uri, _bearer) {
			resolved = SceneLoadingStatus.eTexture;
			material = _material;
			texture = _texture;
			lmvtkDef = node;
		}

		#endregion

		#region Forge Request Object Interface
		//public override void FireRequest (Action<object, AsyncCompletedEventArgs> callback =null) ;

		//public override void CancelRequest () ;

		public override void ProcessResponse (AsyncCompletedEventArgs e) {
			DownloadDataCompletedEventArgs args = e as DownloadDataCompletedEventArgs;
			try {
				image = args.Result;
				state = SceneLoadingStatus.eReceived;
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			} finally {
			}
		}

		public override string GetName () {
			return ("texture");
		}

		public override GameObject BuildScene (string name, bool saveToDisk = false) {
#if UNITY_EDITOR
			if ( saveToDisk ) {
				//FileStream file =File.Create (ForgeConstants._resourcesPath + this.loader.PROJECTID + "/" + this.texture.tex) ;
				//file.Write (image, 0, image.Length) ;
				//file.Close () ;
				File.WriteAllBytes (ForgeConstants._resourcesPath + this.loader.PROJECTID + "/" + this.texture.tex, image);
				AssetDatabase.Refresh ();
			}
#endif

			StandardMaterial lmvMat = new StandardMaterial (lmvtkDef, null);
			Texture2D tex = new Texture2D (1, 1);
			tex.LoadImage (image);
			tex.Apply ();
#if UNITY_EDITOR
			if ( saveToDisk )
				tex = AssetDatabase.LoadAssetAtPath<Texture2D> (ForgeConstants._resourcesPath + this.loader.PROJECTID + "/" + this.texture.tex);
#endif
			image = null;

			switch ( texture.texType ) {
				case Texture.TextureType.Diffuse:
					material.SetTexture ("_MainTex", tex);
					material.mainTextureScale = new Vector2 (texture.u, texture.v);
					break;
				case Texture.TextureType.Specular:
					material.SetTexture ("_SpecGlossMap", tex);
					//material.SetFloat ("_Glossiness", texture.u) ;
					//if ( lmvMat.isMetal )
					//	material.SetFloat ("_MetallicGlossMap", texture.u) ;
					//else
					material.SetFloat ("_GlossMapScale", texture.u);
					//material.EnableKeyword ("_SPECULARHIGHLIGHTS_OFF") ;
					//material.SetFloat ("_SpecularHighlights", 0f) ;
					break;
				case Texture.TextureType.Bump:
					material.SetTexture ("_BumpMap", tex);
					material.SetFloat ("_BumpScale", texture.u);
					break;
			}

			material.SetMaterialKeywords (texture.texType, lmvMat.isMetal);

			base.BuildScene (name, saveToDisk);
			return (gameObject);
		}

		#endregion

	}

}