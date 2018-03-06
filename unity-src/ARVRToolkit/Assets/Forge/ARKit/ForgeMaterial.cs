//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System.IO;
using UnityEngine;
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	public class Texture {

		#region Enums
		public enum TextureType {
			Diffuse,
			Specular,
			Bump
		}

		#endregion

		#region Fields
		public float u, v;
		public bool ur, vr;
		public bool invert;
		public float RGBAmount;
		public string tex;
		public int channel;
		public TextureType texType;

		#endregion

		#region Constructors
		public Texture (TextureType _textType = TextureType.Diffuse) {
			texType = _textType;
		}

		public Texture (JSONNode node, TextureType _textType = TextureType.Diffuse) {
			this.texType = _textType;
			if ( node ["properties"] ["scalars"] ["texture_UScale"] ["values"] [0] == null )
				this.u = 1.0f;
			else
				this.u = node ["properties"] ["scalars"] ["texture_VScale"] ["values"] [0].AsFloat;
			if ( node ["properties"] ["scalars"] ["texture_VScale"] ["values"] [0] == null )
				this.v = 1.0f;
			else
				this.v = node ["properties"] ["scalars"] ["texture_VScale"] ["values"] [0].AsFloat;
			if ( node ["properties"] ["scalars"] ["unifiedbitmap_RGBAmount"] ["values"] ==null || node ["properties"] ["scalars"] ["unifiedbitmap_RGBAmount"] ["values"] [0] == null )
				this.RGBAmount = 1.0f;
			else
				this.RGBAmount = node ["properties"] ["scalars"] ["unifiedbitmap_RGBAmount"] ["values"] [0].AsFloat;
			this.tex = Path.GetFileName (node ["properties"] ["uris"] ["unifiedbitmap_Bitmap"] ["values"] [0]);
			this.ur = node ["properties"] ["booleans"] ["texture_URepeat"].AsBool;
			this.vr = node ["properties"] ["booleans"] ["texture_VRepeat"].AsBool;
			this.invert = node ["properties"] ["booleans"] ["unifiedbitmap_Invert"].AsBool;
			this.channel = node ["properties"] ["integers"] ["texture_MapChannel"].AsInt;
		}

		#endregion

	}

	public class ProteinMaterial {

		#region Fields
		private JSONNode _json;

		#endregion

		#region Constructors
		public ProteinMaterial (string jsonSt) {
			try {
				_json = JSON.Parse (jsonSt);
			} catch ( System.Exception /*e*/ ) {
			}
		}

		public ProteinMaterial (JSONNode json) {
			_json = json;
		}

		#endregion

		#region Bases
		public virtual int userassets {
			get { return (_json ["userassets"] [0].AsInt); }
		}

		public virtual JSONNode materials {
			get { return (_json ["materials"]); }
		}
		#endregion

		#region Prism
		public bool isPrismMaterial () {
			var innerMat = this.materials [0];
			if ( innerMat != null ) {
				var def = innerMat ["definition"];
				return (def == "PrismLayered"
					|| def == "PrismMetal"
					|| def == "PrismOpaque"
					|| def == "PrismTransparent"
					|| def == "PrismWood"
				);
			}
			return (false);
		}

		#endregion

		#region Material
		public virtual JSONNode this [int index] {
			get { return (this.materials [index]); }
		}

		public virtual JSONNode material {
			get { return (this.materials [this.userassets]); }
		}

		public virtual bool transparent {
			get { return (this.material ["transparent"].AsBool); }
		}

		public virtual string definition {
			get { return (this.material ["definition"]); }
		}

		#endregion

		#region colors
		private Color GetColor (string colorName) {
			JSONNode colors = this.material ["properties"] ["colors"].AsObject;
			if ( colors [colorName] == null )
				return (Color.clear);
			var jsonColor = colors [colorName] ["values"] [0].AsObject;
			return (GetColor (jsonColor));
		}

		private Color GetColor (JSONObject color) {
			return (new Color (
				color ["r"].AsFloat,
				color ["g"].AsFloat,
				color ["b"].AsFloat,
				color ["a"].AsFloat
			));
		}

		public virtual Color metal_f0 {
			get { return (GetColor ("metal_f0")); }
		}

		#endregion

		#region opaque albeto
		public virtual int hasAlbedo {
			get {
				return (this.material ["properties"] ["colors"] ["surface_albedo"] != null
			  		&& this.material ["properties"] ["colors"] ["surface_albedo"] ["connections"] != null ?
			  			  this.material ["properties"] ["colors"] ["surface_albedo"] ["connections"] [0].AsInt
				        : 0
		  		);
			}
		}

		public virtual Texture albedo {
			get { return (this.texture (this.hasAlbedo)); }
		}

		public virtual Color albedoColor {
			get { return (GetColor ("surface_albedo")); }
		}

		#endregion

		#region opaque albeto
		public virtual int hasOpaqueAlbeto {
			get {
				return (this.material ["properties"] ["colors"] ["opaque_albedo"] != null
			  		&& this.material ["properties"] ["colors"] ["opaque_albedo"] ["connections"] != null ?
			  			  this.material ["properties"] ["colors"] ["opaque_albedo"] ["connections"] [0].AsInt
				        : 0
		  		);
			}
		}

		public virtual Texture opaqueAlbeto {
			get { return (this.texture (this.hasOpaqueAlbeto)); }
		}

		public virtual Color opaqueAlbetoColor {
			get { return (GetColor (this.material ["properties"] ["colors"] ["opaque_albeto"] ["values"] [0].AsObject)); }
		}

		#endregion

		#region roughness
		public virtual int hasRoughness {
			get {
				return (this.material ["properties"] ["scalars"] ["surface_roughness"] != null
			  		&& this.material ["properties"] ["scalars"] ["surface_roughness"] ["connections"] != null ?
			  			  this.material ["properties"] ["scalars"] ["surface_roughness"] ["connections"] [0].AsInt
				        : 0
		  		);
			}
		}

		public virtual Texture roughness {
			get { return (this.texture (this.hasRoughness)); }
		}

		public virtual float roughnessFactor {
			get { return (this.material ["properties"] ["scalars"] ["surface_roughness"] ["values"] [0].AsFloat); }
		}

		#endregion

		#region rotation
		public virtual int hasRotation {
			get {
				return (this.material ["properties"] ["scalars"] ["surface_rotation"] != null
			  		&& this.material ["properties"] ["scalars"] ["surface_rotation"] ["connections"] != null ?
			  			  this.material ["properties"] ["scalars"] ["surface_rotation"] ["connections"] [0].AsInt
				        : 0
		  		);
			}
		}

		public virtual Texture rotation {
			get { return (this.texture (this.hasRotation)); }
		}

		public virtual float rotationFactor {
			get { return (this.material ["properties"] ["scalars"] ["surface_roughness"] ["values"] [0].AsFloat); }
		}

		#endregion

		#region normal/bump
		public virtual int hasNormal {
			get {
				return (this.textures ["surface_normal"] != null ?
			  		  this.textures ["surface_normal"] ["connections"] [0].AsInt
				    : 0
		  		);
			}
		}

		public virtual Texture normal {
			get { return (this.texture (this.hasNormal)); }
		}

		#endregion

		#region Textures
		public virtual JSONNode textures {
			get { return (this.material ["textures"]); }
		}

		public virtual Texture texture (int index) {
			JSONNode json = this [index];
			Texture tex = new Texture (Texture.TextureType.Diffuse);
			if ( json ["definition"] == "UnifiedBitmap" )
				tex.tex = Path.GetFileName (json ["properties"] ["uris"] ["unifiedbitmap_Bitmap"]);
			else if ( json ["definition"] == "BumpMap" )
				tex.tex = Path.GetFileName (json ["properties"] ["uris"] ["bumpmap_Bitmap"]);
			tex.ur = json ["properties"] ["booleans"] ["texture_URepeat"] ["values"] [0].AsBool;
			tex.vr = json ["properties"] ["booleans"] ["texture_VRepeat"] ["values"] [0].AsBool;
			tex.invert = false;
			if ( json ["properties"] ["booleans"] ["unifiedbitmap_Invert"] != null )
				tex.invert = json ["properties"] ["booleans"] ["unifiedbitmap_Invert"] ["values"] [0].AsBool;
			tex.channel = json ["properties"] ["integers"] ["texture_MapChannel"] ["values"] [0].AsInt;
			tex.u = json ["properties"] ["scalars"] ["texture_UScale"] ["values"] [0].AsFloat;
			tex.v = json ["properties"] ["scalars"] ["texture_VScale"] ["values"] [0].AsFloat;
			tex.RGBAmount = 1.0f;
			if ( json ["properties"] ["scalars"] ["unifiedbitmap_RGBAmount"] != null )
				tex.RGBAmount = json ["properties"] ["scalars"] ["unifiedbitmap_RGBAmount"] ["values"] [0].AsFloat;
			return (tex);
		}

		#endregion

	}

	public class StandardMaterial {

		#region Fields
		private JSONNode _json;
		private ProteinMaterial _proteinJson;

		#endregion

		#region Constructors
		public StandardMaterial (string jsonSt, string proteinJsonSt) {
			try {
				_json = JSON.Parse (jsonSt);
				if ( proteinJsonSt != null && proteinJsonSt != "" )
					_proteinJson = new ProteinMaterial (proteinJsonSt);
				else
					_proteinJson = null;
			} catch ( System.Exception /*e*/ ) {
			}
		}

		public StandardMaterial (JSONNode json, JSONNode proteinJson) {
			_json = json;
			_proteinJson = new ProteinMaterial (proteinJson);
		}

		#endregion

		#region Protein
		public virtual ProteinMaterial proteinMat {
			get { return (_proteinJson); }
		}

		public virtual JSONNode categories {
			get { return (this.material ["categories"]); }
		}

		#endregion

		#region Prism
		public bool isPrismMaterial () {
			var innerMat = this.material;
			if ( innerMat != null ) {
				var definition = innerMat ["definition"];
				return (definition == "PrismLayered"
					|| definition == "PrismMetal"
					|| definition == "PrismOpaque"
					|| definition == "PrismTransparent"
					|| definition == "PrismWood"
				);
			}
			return (false);
		}

		#endregion

		#region bases
		public virtual int userassets {
			get { return (_json ["userassets"] [0].AsInt); }
		}

		public virtual JSONNode materials {
			get { return (_json ["materials"]); }
		}
		#endregion

		#region Material
		public virtual JSONNode material {
			get { return (this.materials [this.userassets]); }
		}

		public virtual string proteinType {
			get { return (this.material ["proteinType"]); }
		}

		public virtual bool transparent {
			get { return (this.material ["transparent"].AsBool); }
		}

		public virtual string definition {
			get { return (this.material ["definition"]); }
		}

		#endregion

		#region integers
		public virtual int mode {
			get { return (this.material ["properties"] ["integers"] ["mode"].AsInt); }
		}
		#endregion

		#region booleans
		public virtual bool isMetal {
			get { return (this.material ["properties"] ["booleans"] ["generic_is_metal"].AsBool); }
			set { this.material ["properties"] ["booleans"] ["generic_is_metal"].AsBool = value; }
		}

		public virtual bool colorByObject {
			get { return (this.material ["properties"] ["booleans"] ["color_by_object"].AsBool); }
		}

		public virtual bool backfaceCull {
			get { return (this.material ["properties"] ["booleans"] ["generic_backface_cull"].AsBool); }
		}

		public virtual bool clearcoat {
			get { return (this.material ["properties"] ["booleans"] ["generic_clearcoat"].AsBool); }
			set { this.material ["properties"] ["booleans"] ["generic_clearcoat"].AsBool = value; }
		}
		#endregion

		#region scalars
		public virtual float refraction {
			get { return (this.material ["properties"] ["scalars"] ["refraction_index"] ["values"] [0].AsFloat); }
		}

		public virtual float glossiness { // == shininess; default to 30
			get { return (this.material ["properties"] ["scalars"] ["generic_glossiness"] ["values"] [0].AsFloat); }
		}

		public virtual float transparency {
			get { return (this.material ["properties"] ["scalars"] ["generic_transparency"] ["values"] [0].AsFloat); }
			set { this.material ["properties"] ["scalars"] ["generic_transparency"] ["values"] [0].AsFloat = value; }
		}

		public virtual float opacity {
			get { return (1.0f - this.material ["properties"] ["scalars"] ["generic_transparency"] ["values"] [0].AsFloat); }
			set { this.material ["properties"] ["scalars"] ["generic_transparency"] ["values"] [0].AsFloat = 1.0f - value; }
		}

		public virtual float reflectivity {
			get { return (this.material ["properties"] ["scalars"] ["generic_reflectivity"] ["values"] [0].AsFloat); }
			set { this.material ["properties"] ["scalars"] ["generic_reflectivity"] ["values"] [0].AsFloat = value; }
		}

		public virtual bool _reflectivity {
			get { return (this.material ["properties"] ["scalars"] ["generic_reflectivity"] != null); }
		}

		public virtual float alphaTest {
			get { return (this.material ["properties"] ["scalars"] ["generic_alphaTest"] ["values"] [0].AsFloat); }
			set { this.material ["properties"] ["scalars"] ["generic_alphaTest"].AsFloat = value; }
		}

		public virtual float extraDepthOffset {
			get { return (this.material ["properties"] ["scalars"] ["generic_depth_offset"] ["values"] [0].AsFloat); }
		}

		public virtual bool _extraDepthOffset {
			get { return (this.material ["properties"] ["scalars"] ["generic_depth_offset"] != null); }
		}

		#endregion

		#region colors
		private Color GetColor (string colorName) {
			JSONNode colors = this.material ["properties"] ["colors"].AsObject;
			if ( colors [colorName] == null )
				return (Color.clear);
			var jsonColor = colors [colorName] ["values"] [0].AsObject;
			return (GetColor (jsonColor));
		}

		private Color GetColor (JSONObject color) {
			return (new Color (
				color ["r"].AsFloat,
				color ["g"].AsFloat,
				color ["b"].AsFloat,
				color ["a"].AsFloat
			));
		}

		public virtual Color diffuse {
			get { return (GetColor ("generic_diffuse")); }
			set {
				this.material ["properties"] ["colors"] ["generic_diffuse"] ["values"] [0].AsObject ["r"].AsFloat = value.r;
				this.material ["properties"] ["colors"] ["generic_diffuse"] ["values"] [0].AsObject ["g"].AsFloat = value.g;
				this.material ["properties"] ["colors"] ["generic_diffuse"] ["values"] [0].AsObject ["b"].AsFloat = value.b;
				this.material ["properties"] ["colors"] ["generic_diffuse"] ["values"] [0].AsObject ["a"].AsFloat = value.a;
			}
		}

		public virtual Color specular {
			get { return (GetColor ("generic_specular")); }
			set {
				this.material ["properties"] ["colors"] ["generic_specular"] ["values"] [0].AsObject ["r"].AsFloat = value.r;
				this.material ["properties"] ["colors"] ["generic_specular"] ["values"] [0].AsObject ["g"].AsFloat = value.g;
				this.material ["properties"] ["colors"] ["generic_specular"] ["values"] [0].AsObject ["b"].AsFloat = value.b;
				this.material ["properties"] ["colors"] ["generic_specular"] ["values"] [0].AsObject ["a"].AsFloat = value.a;
			}
		}

		public virtual Color ambient {
			get { return (GetColor ("generic_ambient")); }
		}

		public virtual Color emissive {
			get { return (GetColor ("generic_emissive")); }
		}

		public virtual Color color {
			get { return (this.diffuse); }
			set { this.diffuse = value; }
		}
		#endregion

		#region Textures
		public virtual JSONNode textures {
			get { return (this.material ["textures"]); }
		}

		public virtual Texture diffuse_tex {
			get {
				if ( this.textures ["generic_diffuse"] == null )
					return (null);
				JSONNode node = this.materials [this.textures ["generic_diffuse"] ["connections"] [0].Value] ;
				return (new Texture (node, Texture.TextureType.Diffuse));
			}
		}

		public virtual Texture specular_tex {
			get {
				if ( this.textures ["generic_specular"] == null )
					return (null);
				JSONNode node = this.materials [this.textures ["generic_specular"] ["connections"] [0].Value];
				return (new Texture (node, Texture.TextureType.Specular));
			}
		}

		public virtual Texture bump_tex {
			get {
				if ( this.textures ["generic_bump"] == null )
					return (null);
				JSONNode node = this.materials [this.textures ["generic_bump"] ["connections"] [0].Value];
				return (new Texture (node, Texture.TextureType.Bump));
			}
		}

		#endregion

	}

}

