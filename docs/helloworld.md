# Hello World

#### Installation

1. [Download the 'hello world'](CHANGELOG.md) Unity Project
2. open the project in Unity
3. In a browser, go to [ToolkitService](https://forge-rcdb.autodesk.io/configurator?id=5a2a07e6d72f007fb27b7e0c), login and click on the 'house.rvt'
4. create a new Scene called '`helloworld-house`', this is the SceneId
5. Copy the `Urn` and `token` under manifest and token tabs
6. In Unity, select '`startup Script`', and paste in `URN`, `BEARER` (token) and `SCENEID`
    <p align="center">
    <img src="res/unity_component_settings.png" alt="Forge ARVR-Toolkit" />
    </p>
7. Now press the Unity '`play`' button

You should now see the following:

<p align="center">
  <img src="res/unity_game.gif" alt="Forge ARVR-Toolkit" />
</p>

You can find the original source code for the Unity project until the 'hello world' folder in this github repo.

