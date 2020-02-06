# GoCube Unity Plugin

## Overview

---

The GoCube Unity Plugin is a group of C# classes that form a bridge between Unity and our native Android™ and iOS™. Unity can call down into each platform's native code for GoCube commands and receive asynchronous data callbacks from native code up into Unity.  The easiest way to get going with the plugin is by starting with one of the samples we have created.

	Notice: The GoCube Unity Plugin only works with Android and iOS.
	
## GoCube Unity Sample

---

* GoCubeExampleSDK - Connect to a GoCube, then it will display a virtual cube that is a mirror of the physical cube. The sample contains some easy API calls.

Just open a Unity project and select the folder that contains the 'Assets' and 'ProjectSettings' folders.
Open the GoCubeConnectionScene scene (switch to Android or iOS platform) and run the example.
NOTE: You must run the project in debug mode ("Development Build" checked)
	
## Adding GoCube to a New or Existing Project

---

Open up Unity and start by choosing File -> New Project.

Next, drag the `BLE`, `Particula` and `Plugins`  directories from `GoCubeUnityPlugin` into your Unity Project.

### GoCube Connection

We have created scripts for you that handle the connection lifecycle of a GoCube on iOS and Android. The first scene we need to create is the GoCubeConnectionScene.  Most of the time, this is the scene that should pop up when you first start the app.  

Start by choosing File -> New Scene, and save it as `GoCubeConnectionScene`.  All you need to do is locate the `GoCubeConnection` prefab that is in the `/Particula/Prefabs` directory in your project.  Drag the prefab into the scene.  

*Note* The `Next Scene` is a string of the name of the scene that you want Unity to load after the GoCubeConnectionScene is done.  It will proceed to the scene after it connects to GoCube.   

### Using GoCube Objects

Unfortunately, this SDK is only supported in C# at the moment.

The previous scene will handle GoCube connections.  After a GoCube is connected, your project has access to it through the `GoCubeProvider` singleton class.  Access them by this call:

C#
	
	IOnlineCube connectedGoCube = GoCubeProvider.GetProvider().GetConnectedGoCube();

		
This parameter will be null if no GoCube is connected.

#### Commands

These functions will operate fine on both mobile platforms.  

**Open GoCube led patterns (C#):**

	// Open GoCube leds (pattern 1)
	connectedGoCube.PlayLedPattern(LedPattern.Pattern1);
  
**Get whether the cube is solved or not (C#):**

	// Get if the cube is solved or not
	connectedGoCube.IsSolved()  
	
There are more commands that you can check out in the IOnlineCube class. 

#### Notifications, Asynchronous Messaging, and Responses

The GoCube SDK allows you to track the cube rotations in real time by getting notification once a rotation has performed.
The way to do it is to register to the cube rotations event:

	connectedGoCube.afterRotation += RotationEvent;
	
Remember to unregister for the notification when you no longer need it.

	connectedGoCube.afterRotation -= RotationEvent;

Receive the data and do whatever you like with it.  Every message has the unique face id that has rotated.

	private void RotationEvent(Rotation rot) 
	{
		// You can do your stuff here....
	}

## License

---
The GoCube Unity Plugin is distributed under the ParticulaCode Source Code License.  Developers are encouraged to help build the plugin and make pull requests to our main Github repository.

## Community and Help

---


*Android™ is a registerd trademark of Google Inc* |
*iOS™ is a licensed trademark of Apple*
