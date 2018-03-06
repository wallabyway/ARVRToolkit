//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//

namespace Autodesk.Forge.ARKit {

	public abstract class ForgeLoaderConstants {

		public const string FORGE_CLIENT_ID = " ";
		public const string FORGE_CLIENT_SECRET = "";

		public const int NB_MAX_REQUESTS = 10;
		public const string _endpoint = "https://developer-api.autodesk.io/modelderivative/v2/arkit/";
		public const string _endpoint1 = "https://developer-api.autodesk.io/arkit/v1/";
		//public const string _endpoint = "http://localhost:3001/modelderivative/v2/arkit/";
		//public const string _endpoint1 = "http://localhost:3001/arkit/v1/";
		public const string _forgeoAuth2legged ="https://developer.api.autodesk.com/authentication/v1/authenticate";

		public const string _PROJECTID = "default";
		public const string _SCENEID = "scene";
		public const string _BEARER = "";
		public const string _URN = "";

		public const bool _forceHololens = false;
		public const bool _forceDAQRI = false;

	}

}