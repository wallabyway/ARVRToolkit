//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using UnityEngine;


namespace Autodesk.Forge.ARKit {

	public static class MaterialExtensions {

		#region Methods
		public static void SetMaterialKeywords (this Material material, Texture.TextureType texType, bool isMetal = false) {
			material.SetKeyword ("_NORMALMAP", material.GetTexture ("_BumpMap") || material.GetTexture ("_DetailNormalMap"));
			if ( texType == Texture.TextureType.Specular )
				material.SetKeyword ("_SPECGLOSSMAP", material.GetTexture ("_SpecGlossMap"));
			else if ( texType == Texture.TextureType.Specular && isMetal )
				material.SetKeyword ("_METALLICGLOSSMAP", material.GetTexture ("_MetallicGlossMap"));

			material.SetKeyword ("_PARALLAXMAP", material.GetTexture ("_ParallaxMap"));
			material.SetKeyword ("_DETAIL_MULX2", material.GetTexture ("_DetailAlbedoMap") || material.GetTexture ("_DetailNormalMap"));
			material.FixupEmissiveFlag ();
			bool state = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == MaterialGlobalIlluminationFlags.None;
			material.SetKeyword ("_EMISSION", state);
			if ( material.HasProperty ("_SmoothnessTextureChannel") )
				material.SetKeyword ("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", material.GetSmoothnessMapChannel () == 1 /*SmoothnessMapChannel.AlbedoAlpha*/);
		}

		public static void SetKeyword (this Material material, string keyword, bool state) {
			if ( state )
				material.EnableKeyword (keyword);
			else
				material.DisableKeyword (keyword);
		}

		public static void FixupEmissiveFlag (this Material material) {
			material.globalIlluminationFlags = FixupEmissiveFlag (material.GetColor ("_EmissionColor"), material.globalIlluminationFlags);
		}

		public static MaterialGlobalIlluminationFlags FixupEmissiveFlag (Color col, MaterialGlobalIlluminationFlags flags) {
			if ( (flags & MaterialGlobalIlluminationFlags.BakedEmissive) != MaterialGlobalIlluminationFlags.None && col.maxColorComponent == 0f )
				flags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
			else if ( flags != MaterialGlobalIlluminationFlags.EmissiveIsBlack )
				flags &= MaterialGlobalIlluminationFlags.AnyEmissive;
			return (flags);
		}

		public static int GetSmoothnessMapChannel (this Material material) {
			int num = (int)material.GetFloat ("_SmoothnessTextureChannel");
			return (num /*num == 1 ? SmoothnessMapChannel.AlbedoAlpha : SmoothnessMapChannel.SpecularMetallicAlpha*/);
		}

		public static void SetupMaterialWithBlendMode (this Material material, MaterialRequest.BlendMode blendMode) {
			switch ( blendMode ) {
				case MaterialRequest.BlendMode.Opaque:
					material.SetOverrideTag ("RenderType", "");
					material.SetInt ("_SrcBlend", 1);
					material.SetInt ("_DstBlend", 0);
					material.SetInt ("_ZWrite", 1);
					material.DisableKeyword ("_ALPHATEST_ON");
					material.DisableKeyword ("_ALPHABLEND_ON");
					material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
					material.renderQueue = -1;
					break;
				case MaterialRequest.BlendMode.Cutout:
					material.SetOverrideTag ("RenderType", "TransparentCutout");
					material.SetInt ("_SrcBlend", 1);
					material.SetInt ("_DstBlend", 0);
					material.SetInt ("_ZWrite", 1);
					material.EnableKeyword ("_ALPHATEST_ON");
					material.DisableKeyword ("_ALPHABLEND_ON");
					material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
					material.renderQueue = 2450;
					break;
				case MaterialRequest.BlendMode.Fade:
					material.SetOverrideTag ("RenderType", "Transparent");
					material.SetInt ("_SrcBlend", 5);
					material.SetInt ("_DstBlend", 10);
					material.SetInt ("_ZWrite", 0);
					material.DisableKeyword ("_ALPHATEST_ON");
					material.EnableKeyword ("_ALPHABLEND_ON");
					material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
					material.renderQueue = 3000;
					break;
				case MaterialRequest.BlendMode.Transparent:
					material.SetOverrideTag ("RenderType", "Transparent");
					material.SetInt ("_SrcBlend", 1);
					material.SetInt ("_DstBlend", 10);
					material.SetInt ("_ZWrite", 0);
					material.DisableKeyword ("_ALPHATEST_ON");
					material.DisableKeyword ("_ALPHABLEND_ON");
					material.EnableKeyword ("_ALPHAPREMULTIPLY_ON");
					material.renderQueue = 3000;
					break;
			}
		}

		#endregion

	}

}
