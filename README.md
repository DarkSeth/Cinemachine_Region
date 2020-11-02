# Cinemachine Region Confiner #

* New Confiner used by Cinemachine extension
* Unity minimum version: **2018.4**
* Current version: **1.0.0-preview.1**
* Dependencies:
    - [Cinemachine](https://docs.unity3d.com/Packages/com.unity.cinemachine@2.2/changelog/CHANGELOG.html) : **2.2.0**


## How to Use

### Attach a ```CinemachineRegionsConfiner``` component

If you didn't already, add a ```CinemachineVirtualCamera``` script to a GameObject. In the **Extension** section, add ```CinemachineRegionsConfiner``` extension.

### Bind events to ```OnRegionChanged``` action

You can fire an Unity event when a transition between regions is completely done.<br/>
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