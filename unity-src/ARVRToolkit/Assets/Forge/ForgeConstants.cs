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

    public abstract class ForgeConstants {

        public const string ROOT ="Root" ;
        public const string ROOTPATH ="/" + ROOT ;
        public const string MGR ="Forge Managers" ;
        public const string MGRPATH ="/" + MGR ;

        public const string INTERACTIBLE ="Interactible" ;
        public const string ENVIRONEMENT ="Environment" ;
        public const string MENUITEMS ="Menu Items" ;
        public const string IGNORERAYCASTTEMS ="Ignore Raycast" ;

        public const string _resourcesPath ="Assets/Resources/" ;
        public const string _bundlePath ="Assets/Bundles/" ;
        public const string _toolkitPath ="Assets/HoloToolkit/" ;

        public const int IMPORT_SCENE_MENU =1 ;
        public const int MAKE_INTERACTIBLE_MENU =2 ;
		public const int BUILD_PREFAB_MENU =3 ;

	}

}

// https://docs.unity3d.com/Manual/ExecutionOrder.html
// https://docs.unity3d.com/Manual/Plugins.html
// https://github.com/VulcanTechnologies/HoloLensCameraStream/tree/unity20173upgrade
// https://mtaulty.com/2016/12/28/windows-10-uwp-qr-code-scanning-with-zxing-and-hololens/
// https://forum.unity.com/threads/how-to-set-project-wide-pragma-directives-with-javascript.71445/

// https://stackoverflow.com/questions/41391708/how-to-detect-click-touch-events-on-ui-and-gameobjects
// https://stackoverflow.com/questions/35529940/how-to-make-gameplay-ignore-clicks-on-ui-button-in-unity3d
