
## Hololens Example

This sample is a simple HoloLens demo using [MRLabs](https://github.com/Microsoft/MRDesignLabs_Unity) and the Forge Toolkit.

(if you wonder what RCDB means, Autodesk Forge RCDB: Responsive Connected Database. A cutting edge set of demos featuring Autodesk Forge Viewer and WebServices APIs.)

### Setup

1. Create a new Unity project. You need [Unity 2017.1.3](https://unity3d.com/get-unity/download/archive).
2. Import the Forge Toolkit package (Note you need the version for [Unity 2017.1.3](https://unity3d.com/get-unity/download/archive) which is the version supported by MRLabs)
3. Import the [MRLabs package](http://forgetoolkit.com/unity/MRLabs.unitypackage)
4. Import the [HoloLens RCDB sample code](http://forgetoolkit.com/unity/rcdb-Example.unitypackage)
5. Load the 'Assets/Forge Samples/Hololens-RCDB' scene in the editor
6. In the Player options, add the FORGE_HUX preprocessor symbol

### Usage

Note, you can emulate the sample directly in the Unity editor if you want to. Just press the 'Play' button, and using gesture or voice commands to run the demo. There is 2 assets burned as prefabs in the demo. You can also use the QR code scanner to connect your HoloLens to a model prepared online with Forge. See instructions below.

Using it real!
After having compiled and deployed the sample on your HoloLens, got to the [Forge RCDB web site](https://forge-rcdb.autodesk.io/configurator?id=5a2b31ee58144b89730d6d5a), and select one of the model. My favorite one is the BB8 mode ;). Next, select the 'Scenes' tab, and a scene in the dropdown. Last, select the 'QR Code' tab, and click on the QR code to make it bigger. On the HoloLens say 'Scan' or click on the 'Scan' button to scan the QR code, and enjoy ;)

#### Voice commands are:

* Menu / Hide Menu: Shows and Hides the toolbar menu
* Scan: Scans a QR Code to load a model prepared on the forge-rcdb website
* Load The Palace: Loads the palace prefab (note this is the [Palais Brongniart](http://www.palaisbrongniart.com/nef.html) in Paris coming from Revit and decimated from 20 millions polygon to just 500k polygons to fits into the HoloLens)
* Load My Car: Loads the car static prefab
* Bigger / Scales Up: Makes the mode bigger
* Smaller / Scales Down: Makes the model smaller
* Normal Size: Restores the size of the model as it was first loaded
* Explode: Breaks the model apart
* Combine: Restores the model in its orginal state after 'Explode'
* Reset: Resets the scene

#### License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). 
Please see the [LICENSE](LICENSE) file for full details.


#### Written by

Cyrille Fauvel <br />
Forge Partner Development <br />
http://developer.autodesk.com/ <br />
http://around-the-corner.typepad.com <br />