# Unity scene preparation sample code

## test-2legged
This script shows a simplified series of HTTP requests that prepares a design data file for use
in Unity, using two-legged authentication with Forge.

It shows how to:

- upload a file to the Forge data management API,
- call the model derivative API to convert the data to SVF,
- create a new scene ID for the data in the developer-api.autodesk.io service,
- run a translation job that extracts the viewable data from the SVF into a scene tree,
- and finally download the highest-level tree of scene objects.

Download it at [test-2legged](http://forgetoolkit.com/sceneprep/test-2legged)

## test-gen-oauth
This script carries out a simple two-legged authentication with Forge, and prints your access token to the console. (It's essentially the same as the beginning of the unity-scene-preparation script described above.)

Download it at [test-gen-oauth](http://forgetoolkit.com/sceneprep/test-gen-oauth)

## test-list-resources
This script finds all data objects in the Forge data management API that are in a given bucket. For each URN, it checks with the developer-api.autodesk.io server to query the manifest of viewable items available for that URN, and writes the results to a data file on disk.

Download it at [test-list-resources](http://forgetoolkit.com/sceneprep/test-list-resources)

## test-prep-scene
This script shows a simplified series of HTTP requests that prepares a scene id from a resource already available on Forge. The script take a URN as argument and use a two-legged authentication with Forge for now.

It shows how to:

- create a new scene ID for the data in the developer-api.autodesk.io service,
- run a translation job that extracts the viewable data from the SVF into a scene tree,
- and finally download the highest-level tree of scene objects.

Download it at [test-prep-scene](http://forgetoolkit.com/sceneprep/test-prep-scene)
