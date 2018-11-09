# Download / Change Log

The files in this package make a Unity app capable of loading design data from Forge.

To understand how to use these files as part of the Forge to Unity data pipeline, click [**Hello World**](helloworld.md).

* Download [ForgeARKit-update-5-2018.1.unitypackage](http://forgetoolkit.com/unity/ForgeARKit-update-5-2018.1.unitypackage)
* or [ForgeARKit-update-3-2017.1.2f1.unitypackage](http://forgetoolkit.com/unity/ForgeARKit-update-3-2017.1.2f1.unitypackage) for
  [MRLabs](https://github.com/Microsoft/MRDesignLabs_Unity) support

!> Make sure to switch to .NET 4.0 runtime, like so... `File > Player Settings > Other Options > Configuration` Select: `Scripting Runtime Version*`

![net40runtime](https://user-images.githubusercontent.com/440241/48244190-490eac80-e399-11e8-809a-551efe2e01e6.png)

---
# Update 5 (Nov5)

#### Fix:
- switched to TLS1.2 (change settings to use .NET4.0 in Unity->'player settings')


---
# Update 4 (Oct23)

#### Enhancement:
- upgraded to faster server


---
# Update 3 (Mar18)

#### Bugfix
- (server) Reintroducing qrcode and camera code into the plugin - this was causing problems on UWP platforms
- (server) Rewrote the multipleScenes example
- (Unity) Fix the Loader dialog crash on Unity 2017.3.x (was randomly crashing on previous versions)

#### Enhancement:
- Enhancement: Update the SimpleJSON library to the lastest version
- Enhancement: Introduced various Oauth2 classes to handle Authorization
- Enhancement: Introduced a Metadata Forge Inespector class to display properties in panels
- Enhancement: Adding Colliders when instantiating meshes
- Enhancement: Make the code fully compatible with WSA/UWP standards

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


