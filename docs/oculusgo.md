# Deploy to Oculus Go

### Introduction
We will build a simple Revit walkthrough and deploy it to an Oculus Go.

<p align="center">
    <img src="https://user-images.githubusercontent.com/440241/48243469-6c375d00-e395-11e8-9842-883e5258b8c6.jpg" alt="OculusGo" />
    <img src="https://user-images.githubusercontent.com/440241/47376973-e20aab80-d6a8-11e8-9c93-b95a6628e818.gif" alt="OculusGo" height="168px"/>
</p>


### Steps

1. Complete the '[helloworld](helloworld.md)' tutorial and choose this [Rac.rvt](https://wallabyway.github.io/toolkitServerv2/index.html) URN thumbnail...

> <p align="center">
    <img src="https://user-images.githubusercontent.com/440241/48242526-786ceb80-e390-11e8-95a0-11022d845250.jpg" alt="Forge ARVR-Toolkit Samples" />
    </p>

#### Build the teleporter
2. Add 'VR Simple Transport' from the Asset store
3. Add 'Oculus Unity Integration' from the Asset store
4. Configure the teleporter and click 'play' to test (WASD keys to move)

Youtube Video:
[![](https://user-images.githubusercontent.com/440241/47318127-1aa27a80-d600-11e8-8a59-9e7e97e5b97c.jpg)](https://youtu.be/i5QKh_fzJag)


#### Deploy to Oculus Go device
5. Download the Android SDK and install
6. Configure your Oculus Go, into Developer mode
7. Configure Unity - set Android v5.0 development and Oculus (see video)


Youtube Video:
[![](https://user-images.githubusercontent.com/440241/47318021-c8615980-d5ff-11e8-805e-9b3d00675031.jpg)](https://youtu.be/8OmLrbB9Szo)

8. Plug in your Oculus-Go via USB
9. run "./adb devices" and check the device appears
10. In Unity, click 'Build and Run' to deploy

Youtube Video:



You can find the final APK and source code, in the repo [github/wallabyway/Forge-OculusGo-Tutorial](https://www.github.com/wallabyway/Forge-OculusGo-Tutorial)

