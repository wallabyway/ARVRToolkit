<p align="center">
  <img src="logo_forge.png" alt="Forge ARVR-Toolkit" />
</p>

<p align="center">
<a href="https://www.npmjs.com/package/boardgame.io"><img src="https://badge.fury.io/js/boardgame.io.svg" alt="npm version" /></a>
<a href="https://travis-ci.org/google/boardgame.io"><img src="https://img.shields.io/travis/google/boardgame.io/master.svg" alt="Travis" /></a>
<a href="https://coveralls.io/github/google/boardgame.io?branch=master"><img src="https://img.shields.io/coveralls/google/boardgame.io.svg" alt="Coveralls" /></a>
<a href="https://gitter.im/boardgame-io"><img src="https://badges.gitter.im/boardgame-io.svg" alt="Gitter" /></a>
</p>

---

The primary goal of this toolkit is to load Autodesk content into the Unity platform.  Once you have Autodesk data, such as BIM/AEC or manufacturing content, inside Unity, you can start to build out more complex AR/VR experiences with Hololens, Apple's ARKit, Vive, Oculus, DayDream and GearVR.

<iframe width="640" height="480" src="https://www.youtube.com/embed/I5RBVA1Kipk" frameborder="0" allowfullscreen></iframe>


There are data-prep and query API's features too...
### Features
* mesh de-duplication and optimized instancing to reduce draw-calls and load times
* bounding box spatial queries, to only load meshes within a spatial volume
* string based queries,  to only load meshes based on meta-data

The APIs and data-prep services are the same Autodesk-Forge Services used by the web-client for BIM360 team collaboration.  So we know that fast-loading performance of large buildings as well as clash detection between versions (think git diff'ing in 3D) is now possible in Unity too.

By leveraging the Unity platform, we connect a highly specialised Unity community, into the workflows of BIM projects.  
Today, for example, a media firm would normally ask for an 'FBX' snapshot of a building from Revit, Autocad or Navisworks.  The FBX file gets out-dated as the BIM project progresses, and a new FBX file is needed.  Each time the FBX file is imported into Unity, some data-prep work is required.

This is the tradional manual data pipeline.  The Forge Toolkit is designed to remove this manual FBX approach and instead give Unity platform direct access to the Revit and Navis data flow at any stage of a BIM project.  No more FBX imports.


### Installation

1. Download the 'hello world' Unity Project
2. open the project in Unity
3. In a browser, go to 'http://vrock.it' and click on the 'house sample'
4. you will see a box with 3 text fields: UUID, Key and Secret.  Copy these down.
5. In Unity, select 'startup Script', and you will see the empty fields UUID, Key and Secret
6. Copy over UUID, Key and Secret into the fields
5. Now press the Unity 'play' button

You can find the original source code for the Unity project until the 'hello world' folder in this github repo.


### Changelog

See [changelog](CHANGELOG.md).


### Disclaimer

This is a beta product and not officially supported by Autodesk.

### License

MIT
