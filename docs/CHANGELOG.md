# Download / Change Log

The files in this package make a Unity app capable of loading design data from Forge.

To understand how to use these files as part of the Forge to Unity data pipeline, click `Hello World` link in the sidebar.

* Download [Forge-ARKit-20171221-update-2.unitypackage](/unity/Forge-ARKit-20171221-update-2.unitypackage)

---
# Update 2 (Dec17)

#### Bugfix
- (server) `503 error` fix after the outage which happened on Dec 6th
- (server) Reduced instanceTree was not processed properly for non graphical nodes
- (Unity) Loading material error on Daqri and iOS system; theses systems needs shaders to be included in their respective builds
(GraphicsSettings: Always Include "Standard” and “Standard (Specular setup)” shaders)
(Include shaders with material in the Resources folder, and/or include object with material references into the scene)
- (scripts) base64 bash command `implicit -w0` argument on MacOS
- (Unity) temporary remove qrcode and camera code from the plugin - this cause problems on UWP platforms
- (Unity) Default values were not set for textures unless specified in the material definition
- (Unity) Metadata (Properties) are requested for each transform nodes vs mesh
- (Unity) Transform nodes are renamed with their "name" Property in the Unity Hierarchy tree
- (Unity) Empty resources are now ignored (not requested)
- (Unity) `LoaderEngine::Load` method is now protected

#### Enhancement:
- Enhancement: (Unity) Properties requests are deferred in the loading order
- Enhancement: (Unity) Loading Properties & Meshes are now optional
- Enhancement: (Server/Unity) Client can now request compressed resources

#### New:
- New: (scripts) Added a `test-prep-scene` which takes a single URN as parameter to post and process an ARKit scene
- New: (scripts) Support for Windows platform (requires bash and jq)
- New: (scripts) Instructions to install/use scripts on all platforms (Linux, Windows, and MacOS)

---
# Update 1 (Nov17)

#### New:
- The project assets are now packaged as a .unityproject bundle, which you can install by choosing `Assets > Import Package > Custom Package` from the main menu of the Unity editor.
- The project assets now include sample scenes that illustrate different ways of setting up the ForgeLoader script component. You can find them under `Assets > Forge` Samples.
For details on these sample scenes, see the Get started with the Forge to Unity data pipeline page.
- The ForgeLoader script component has a new `UnityEvent` that reports the completion percentage of its current download task, and that fires an event when the model is fully downloaded. You can use these triggers to drive a UI element such as a progress bar. (For an example, see the new `sample scenes`.)
- You can now import Forge assets into the Unity editor and save them to prefabs, instead of having your app download them at runtime. Select `Forge > Import Scene` in the main menu of the Unity editor.

#### Bug fixes:
- Redefines the upVector based on metadata - the previous version always assumed a Z up vector, whereas Unity uses Y.
- Malformed mesh UVs were causing issues in the Unity engine.
- An HTTP error on getting the `instanceTree` could cause multiple scene objects to be created.
- WSA support was missing `#using` directives.
- Malformed Metadata (`ForgeProperties`) were not displayed properly in the Unity Editor.


