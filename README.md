# Cinemachine Region Confiner #

* New Confiner used by Cinemachine extension
* Unity minimum version: **2018.4**
* Current version: **1.1.0**
* Dependencies:
    - [Cinemachine](https://docs.unity3d.com/Packages/com.unity.cinemachine@2.2/changelog/CHANGELOG.html) : **2.2.0**

## Summary

For a 2D game using Cinemachine, you can create camera transitions between areas by creating multiple Virtual Cameras and active them using a custom script to detected when your player enters or leaves those areas. 

This setup can be a little time consuming sometimes as you need to create multiples Virtual Cameras. A better approach would be to have a Cinemachine component created just to deal with 2D Camera transitions between regions on you Scene.

This package contains a Cinemachine extension called **Cinemachine Regions Confiner** used to just do it.

## How to Use

### Create Regions

You need to add a Cinemachine Regions Confiner extension to a Virtual Camera and editing regions on the Scene window. Each region has a Name and an Area that you can edit as you like.

To do it, follow the steps bellow: 

If you don't have already, add a Cinemachine 2D Camera on your scene and proper set the ```Follow``` and ```Body``` part from your ```CinemachineVirtualCamera``` component.

Next, in the **Extension** section, add a ```CinemachineRegionsConfiner``` extension and create a ```Region Data``` asset. Select a folder and save it.

![alt text][cinemachine-regions-confiner]

This new asset is a ```ScriptableObject``` that will hold your regions information data. 

You can select and edit any region in the Scene window, just like editing a ```BoxCollider2D```.

![alt text][edit-region]

Also use the Inspector window to rename you selected region or to edit its area more precisely.

![alt text][edit-world-position-region]

Finally, you can create new regions and remove any of them using the Scene buttons.

![alt text][add-remove-region]

A linear camera transition will happen every time your Follow transform enters inside a new region.

![alt text][showcase]

### Optional: Bind custom Events for Region Transitions

If your game needs to run some logic between transitions you can easily do it using custom events provided by ```CinemachineRegionsConfiner```.

You can fire an event when a transition between regions begins and/or is completely done by adding your custom functions to ```OnRegionBeginChange``` and ```OnRegionChanged``` actions.

```csharp
using ActionCode.Cinemachine;

public sealed class Test_RegionTransitionEvent : MonoBehaviour
{
    public CinemachineRegionsConfiner confiner;

    private void OnEnable()
    {
        confiner.OnRegionBeginChange += OnRegionBeginChange;
        confiner.OnRegionChanged += OnRegionChanged;
    }

    private void OnDisable()
    {
        confiner.OnRegionBeginChange -= OnRegionBeginChange;
        confiner.OnRegionChanged -= OnRegionChanged;
    }

    private void OnRegionBeginChange(Region current, Region next)
    {
        print($"Begin region transition. Current: {current.name}. Next: {next.name}");
    }

    private void OnRegionChanged(Region previous, Region current)
    {
        print($"Finish region transition. Previous: {previous.name}. Current: {current.name}");
    }
}
```

## Installation

### Using the Package Registry Server

Open the **manifest.json** file inside your Unity project's **Packages** folder and add this code-block before `dependencies` attribute:

```json
"scopedRegistries": [ 
    { 
        "name": "Action Code", 
        "url": "http://35.185.220.19:4873/", 
        "scopes": [ "com.actioncode" ] 
    } 
],
```

The package **ActionCode-Cinemachine Regions** will be available for you to install using the **Package Manager** windows.

### Using the Git URL

You will need a **Git client** installed on your computer with the Path variable already set. 

Use the **Package Manager** "Add package from git URL..." option or add manually this line inside `dependencies` attribute: 

```json
"com.actioncode.cinemachine-regions": "https://bitbucket.org/nostgameteam/cinemachine-regions.git"
```

---

**Hyago Oliveira**

[BitBucket](https://bitbucket.org/HyagoGow/) -
[Unity Connect](https://connect.unity.com/u/hyago-oliveira) -
<hyagogow@gmail.com>

[cinemachine-regions-confiner]: /Documentation~/add-cinemachine-regions-confiner.gif "Adding a Cinemachine Regions Confiner extension"
[edit-region]: /Documentation~/edit-region.gif "Editing regions"
[add-remove-region]: /Documentation~/add-remove-region.gif "Adding and removing regions"
[edit-world-position-region]: /Documentation~/edit-world-position-region.gif "Editing region using World Position"
[showcase]: /Documentation~/showcase.gif "Showcase"