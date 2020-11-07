# Cinemachine Region Confiner #

* New Confiner used by Cinemachine extension
* Unity minimum version: **2018.4**
* Current version: **1.0.0-preview.1**
* Dependencies:
    - [Cinemachine](https://docs.unity3d.com/Packages/com.unity.cinemachine@2.2/changelog/CHANGELOG.html) : **2.2.0**


## How to Use

### Cinemachine Regions Confiner

Add a Cinemachine 2D Camera on your scene. Setup the ```Follow``` and ```Body``` part from your ```CinemachineVirtualCamera``` component.

Next, in the **Extension** section, add a ```CinemachineRegionsConfiner``` extension and create a ```Region Data``` asset. Select a folder and save it.

![alt text][cinemachine-regions-confiner]

You can select and edit any region in the Scene window, just like editing a ```BoxCollider2D```.

![alt text][edit-region]

For a greater accuracy, you can edit more precisely the selected region using the Inspector window.

![alt text][edit-world-position-region]

Finally, you can create new regions and remove any of them using the Scene buttons.

![alt text][add-remove-region]

### Optional: Bind events to OnRegionChanged action

If your game needs to run some logic between transitions you can easily do it using a custom ```UnityEvent``` provided by ```CinemachineRegionsConfiner```.

You can fire any Unity event when a transition between regions is completely done by adding your custom function to ```CinemachineRegionsConfiner.OnRegionChanged``` action.

The first argument is the previous Region and the second is the current one.

```csharp
using ActionCode.Cinemachine;

public sealed class Test_RegionTransitionEvent : MonoBehaviour
{
    public CinemachineRegionsConfiner confiner;

    private void OnEnable()
    {
        confiner.OnRegionChanged += OnTransition;
    }

    private void OnDisable()
    {
        confiner.OnRegionChanged -= OnTransition;
    }


    private void OnTransition(Region previous, Region current)
    {
        print($"Previous region was: {previous.name} and Current is: {current.name}");
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
        "url": "http://34.83.179.179:4873/", 
        "scopes": [ "com.actioncode" ] 
    } 
],
```

The package **ActionCode-Cinemachine Regions** will be available for you to install using the **Package Manager** windows.

### Using the Git URL

You will need a **Git client** installed on your computer with the Path variable already set. 

Use the **Package Manager** "Add package from git URL..." feature or add manually this line inside `dependencies` attribute: 

```json
"com.actioncode.cinemachine-regions":"https://bitbucket.org/nostgameteam/cinemachine-regions.git"
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