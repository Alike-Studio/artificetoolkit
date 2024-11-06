# Artifice Toolkit
Normally, extending Unity’s editor requires specialized knowledge of IMGUI or the UI Toolkit libraries, along with maintaining separate files for each editor extension. This makes the editor's capabilities less accessible and often a hassle to research, learn and develop.

The ArtificeToolkit simplifies this process by providing custom attributes that can be applied directly in the source code, automatically altering the appearance of properties in the editor. The toolkit can be used in a variety of creative ways and can even be extended by creating new attributes.

To demonstrate its usage and effectiveness, see the following MonoBehaviour script's inspector. On the left is the default inspector generated by Unity, and on the right is the same script with a few additional attributes.

<p align="center">
  <img src="./Documentation/artifice_comparison.png"/>
</p>

## Why use the ArtificeToolkit?

- Visually "lightweight" inspectors reduce cognitive load by removing unnecessary information.
- A simplified interface helps developers focus on relevant details, minimizing distractions.
- Streamlining the editor reduces mental effort, improving workflow efficiency and effectiveness.


## What is included in the ArtificeToolkit?
The Artifice Toolkit offers three powerful tools:

1. [Artifice Inspector](#artifice-inspector-and-custom-attributes): Alter the appearance of your editor's inspector with simple C# custom attributes. This is editor scripting without the editor scripting knowledge being required!

2. [Artifice Validator](#artifice-validator): Attach validator attributes to your serialized properties to always make sure certain rules are being followed. Do not waste time on absent minded mistakes ever again.

3. [Artifice Drawer](#artifice-drawer): The ArtificeDrawer is what renders everything in the ArtificeToolkit. Invoke the drawer in your Editor Scripts with a SerializedObject or SerializedProperty and you will receive the rendered result of it. This essentially makes the ArtificeDrawer a first-class citizen inspector.

## Unity Version Support
The ArtificeToolkit has been primarily tested and optimized for Unity 2022, ensuring stable and consistent performance. It is also compatible with Unity 2023, where it has been thoroughly tested to maintain functionality. While the toolkit works with Unity 2021, users may encounter occasional warnings; these will be addressed in upcoming updates to improve compatibility with older versions.

## How to install into your Unity project?
You can add the ArtificeToolkit as any other Unity Package. Since this is an alpha testing phase, it can only be added as a local package through Windows -> Package Manager. Then press the "plus" icon and choose "Add Package from disk...". The select the package.json file and the ArtificeToolkit will be linked to your project.

<p align="center">
  <img src="./Documentation/artifice_addpackage.png" />
</p>

<!-- CUSTOM ATTRIBUTE CATEGORIES AND LINKS -->
# Artifice Inspector and Custom Attributes
By using custom attributes in your MonoBehaviour scripts you can quickly alter the inspector's appearance. In this section, you will find all the attributes which are tested and ready for use.

By default, the Artifice Drawer is disabled. You can always turn it on/off through the dedicated MenuItem "ArtificeToolkit"

<p align="center">
  <img src="./Documentation/artifice_toggle.png" />
</p>

## Why Order Matters!
When applying custom attributes to a property, it’s important to understand the order in which they are applied during the rendering process. Internally, custom attributes are applied at the following key rendering points:

1. Pre Property GUI: Before the property is drawn in the Inspector.
2. On Property GUI: Replaces the property’s default GUI entirely.
3. Post Property GUI: Applied after the property is drawn.
4. Wrap Property GUI: Encapsulates all previous steps inside a new container. An example of this is how Group Attributes work.
5. Property Bound GUI: This is called after the property has been drawn, useful when dealing with PropertyField because its children are built lazily in the UI.

For most attributes, the order they are applied follows the order of declaration. However, attributes that use OnWrap GUI (like group attributes) are applied in <b>reverse order</b>, which can lead to unexpected behavior if not handled carefully.

Example
Consider this example with conflicting attributes. Both the BoxGroup and the EnableIf work by utilizing the Wrap Property GUI:
```c#
[SerializeField]
private bool shouldEnable;

[SerializeField, BoxGroup("Test")]
private int x;

[SerializeField, EnableIf(nameof(shouldEnable), true), BoxGroup("Test")]
private int y;
```

If we trace how property 'y' will get rendered, it would firstly resolve the BoxGroup("Test") which has already included property 'x'. Then, it would resolve EnableIf wrapping the BoxGroup inside of the EnableIf.

This is "probably" an undesired effect, unless executed by design. Its wrong, since now the enable if does not encapsulate only the 'y' property, but the entire BoxGroup which holds both 'x' and 'y'.

So the correct version of the above code would be 
```c#
[SerializeField]
private bool shouldEnable;

[SerializeField, BoxGroup("Test")]
private int x;

[SerializeField, BoxGroup("Test"), EnableIf(nameof(shouldEnable), true)]
private int y;
```
In this version, EnableIf is applied first, ensuring that property y behaves as expected—only, and the the BoxGroup is resolved, wrapping the wrapper of the EnableIf.


## Top 3 Recommended Attributes
Following are the simplest to use attributes which can have a big impact on visuals of the inspector.

- [FoldoutGroup](#foldout-group)
- [Required](#required)
- [PreviewScriptable](#previewscriptable)

## Group Attributes
Group Attributes can be used to bring together various properties in a form of a group. Such groups can also work in a nested manner as shown in the [BoxGroup](#boxgroup) example.

- [BoxGroup](#boxgroup)
- [FoldoutGroup](#foldout-group)
- [TabGroup](#tab-group)
- [HorizontalGroup](#horizontal-group)
- [VerticalGroup](#vertical-group)

Note: BoxGroup and FoldoutGroup can be further enhanced using the GroupColor enum.

## Validation Attributes
Validation Attributes are used to assert certain rules over your properties. This is achieved in the inspector visualy using an error prompt, and through the use of the [ArtificeValidator](#artifice-validator).

- [Required](#required)
- [AssetOnly](#assetonly)
- [SceneObjectOnly](#sceneobjectonly)
- [ChildGameObjectOnly](#childgameobjectonly)
- [MinValue](#minvalue)
- [MaxValue](#maxvalue)

## Essential and Easy Attributes
These attributes can and should be used frequently. They will at a bare minimum simplify and make the inspector more readable. 

- [Title](#title)
- [EnumToggle](#enumtoggle)
- [EnableIf](#enableif)
- [Button](#button)
- [PreviewSprite](#previewsprite)
- [PreviewScriptable](#previewscriptable)
- [ReadOnly](#readonly)

## Miscellaneous
- [Space](#space)
- [Range](#range)
- [HideLabel](#hidelabel)
- [HideInArtifice](#hideinartifice)
- [InfoBox](#infobox)
- [ConditionalInfoBox](#conditionalinfobox)
- [ListElementName](#listelementname)
- [MeasureUnit](#measureunit)
- [ForceArtifice](#forceartifice)


<!-- ALL ATTRIBUTES DETAILED -->
## All Attributes

<!-- GROUP ATTRIBUTES -->
### BoxGroup
The simplest form of a Group attribute is the BoxGroup. Every serialized property of the script will be rendered inside of a container with the given title.

```c#
[SerializeField, BoxGroup("GroupA")]
private int first;

[SerializeField, BoxGroup("GroupA")]
private int second;

[SerializeField, BoxGroup("GroupA/GroupB")]
private int third;
```  
![box-group-example](./Documentation/artifice_boxgroup.jpg)


### Foldout Group
The FoldoutGroup extends the BoxGroup by allowing the user to optionally collapse the container.

```c#
[SerializeField, BoxGroup("GroupExample/GroupA")]
private int first;

[SerializeField, BoxGroup("GroupExample/GroupA")]
private int second;

[SerializeField, FoldoutGroup("GroupExample/GroupB")]
private int third;

[SerializeField, FoldoutGroup("GroupExample/GroupB")]
private int fourth;
```
![foldout-group-example](./Documentation/artifice_foldoutgroup.jpg)

### Tab Group
The TabGroup allows you to create tabs inside of the Unity inspector. The syntax is more complex than Box and Foldout groups but it is well worth it. The first string dictates the name of the group and the second one dictates the name of the tab. All the properties that belong in the same group and same tab, will be contained together.

```c#
[SerializeField, TabGroup("Example", "Integers")]
private int first;

[SerializeField, TabGroup("Example", "Integers")]
private int second;

[SerializeField, TabGroup("Example", "Strings")]
private string third;

[SerializeField, TabGroup("Example", "Strings")]
private string fourth;

[SerializeField, TabGroup("Example", "Strings")]
private string fifth;
```
![tab-group-example](./Documentation/artifice_tabgroup.gif)

### Horizontal Group
The HorizontalGroup attribute allows you to align multiple properties into a single line, instead of having every property be rendered in a new line. Note, that the Horizontal and Vertical groups do not show their titles and are solely used for structuring.

```c#
[SerializeField, HorizontalGroup("horizontal1")]
private List<int> leftColumn;

[SerializeField, HorizontalGroup("horizontal1")]
private List<int> rightColumn;
```
![horizontal-group-example](./Documentation/artifice_horizontalgroup.png)

### Vertical Group
The Vertical Group is only useful inside of a horizontal group, to dictate a vertical column of properties inside of it.

```c#
[SerializeField, HorizontalGroup("horizontal1"), VerticalGroup("horizontal1/vertical")]
private List<int> leftColumn;

[SerializeField, HorizontalGroup("horizontal1"), VerticalGroup("horizontal1/vertical")]
private int leftColumnInteger;

[SerializeField, HorizontalGroup("horizontal1")]
private List<int> rightColumn;
```
![vertical-group-example](./Documentation/artifice_verticalgroup.jpg)

---

<!-- VALIDATION ATTRIBUTES -->
### Required
The Required field prompts the inspector with an error indicator if the property has not been set. This is GREATLY important in Unity, where it is common to initialize fields and dependencies through [SerializedField] properties. This is what makes the Required field the most important and most commonly used validation attribute.

```c#
[SerializeField, Required] 
private Transform requiredFieldExample;
```

![required-example](./Documentation/artifice_required.png)

### AssetOnly
AssetOnly validates that the value of the serialized property is an asset. This is useful when you have a field for a prefab that will be potentially instantiated. It is common to drag a GameObject from the scene, instead of the assets.

```c#
[SerializeField, Required, AssetOnly] 
private Transform requiredAssetOnlyExample;
```

![assetonly-example](./Documentation/artifice_assetonly.png)

### SceneObjectOnly
Works exactly like the [AssetOnly](#assetonly) attribute but in-reverse. This validates that the serialized property value is of an instantiated gameobject in any loaded scene.

```c#
[SerializeField, Required, SceneObjectOnly] 
private Transform requiredSceneOnlyExample;
```

![sceneobjectonly-example](./Documentation/artifice_sceneonly.png)

### ChildGameObjectOnly
In Unity, it is also common to have scripts which require references from the children of the GameObject. Use the ChildGameObjectOnly attribute to assert this behaviour. 

In addition, when this attribute is used, the inspector is further enhanced allowing for optimized search of the hierarchy, previewing only the valid GameObjects/Scripts based on the type of the serialized property..

```c#
[SerializeField, Required, ChildGameObjectOnly] 
private Transform requiredChildOnlyExample;
```

![sceneobjectonly-example](./Documentation/artifice_childgameobjectonly.gif)

---

<!-- EASY AND ESSENTIALS -->
### Title
The Title attribute allows you to highlight a specific part or property of your inspector. It can also be used in combination with the [HideLabel](#hidelabel) to create a new visual representation of fields as shown below.

```c#
[SerializeField, Title("Name")]
private string name;

[SerializeField, Title("Age"), HideLabel, Range(0, 100)]
private int age;

[SerializeField, Title("City"), HideLabel]
private string city;
```

![title-example](./Documentation/artifice_title.png)


### EnumToggle
EnumToggle converts the conventional for of enum rendering to a multi-button preview. The [Flags] attribute is also supported. Note, that this is useful mostly in enums with a small number of different values.

```c#
public enum Directions
{
    Up, Down, Left, Right
}

[SerializeField, EnumToggle] 
private Directions direction;
```

![enumtoggle-example](./Documentation/artifice_enumtoggle.png)


### EnableIf
This attributes allows you to set an equality condition using another field in the same scope to dictate where the target property will be shown or not. This is a really usefull attribute to optionally show properties that depend upon a bool check or enum check. 

```c#
[SerializeField]
private bool shouldSpawnParticlesOnDestroy;

[SerializeField, FoldoutGroup("On Death", GroupColor.Red)]
private ParticleSystem prefabOnDeathParticles;

[SerializeField, FoldoutGroup("On Death", GroupColor.Red)]
private float onDeathDurationSeconds;

[SerializeField, EnableIf(nameof(shouldSpawnParticlesOnDestroy), true), FoldoutGroup("On Death", GroupColor.Red)]
private float onDeathSoundFxVolume;
```

![enableif-example](./Documentation/artifice_enableif.gif)

---

**NOTE**: The custom attributes of the Artifice, are processed from last to first. This allows us to perform various tricks since both the EnableIf and the FoldoutGroup, wrap the property in another VisualElement container. By having the EnableIf before the FoldoutGroup, on the LAST instance of the FoldoutGroup, the EnableIf captures the entire FoldoutGroup, although we have only declared it at a single serialized property.

---

### Button
Button allows you to quickly turn any method into a button in the inspector to invoke at your hearts content. Buttons can be placed inline using an optional parameter. Otherwise they will be grouped in a sliding container to keep your inspector clean and simple.

It is worth noting that buttons will always appear last in the rendering order. This is something due to change in the far future.

```c#
[SerializeField] 
private string parameterTest = "test";

[Button]
private void TestMethod()
{
    Debug.Log("Invoked from editor button!");
}

[Button(false)]
private void TestMethodInline()
{
    Debug.Log("Invoked from editor button!");
}

[Button("parameterTest")]
private void TestMethodWithParameters(string parameter)
{
    Debug.Log($"Invoked from editor button! Dynamic Parameter: {parameter}");
}
```

![button-example](./Documentation/artifice_button.gif)


### PreviewSprite
PreviewSprite works only on the Sprite and Texture2D serialized properties. It renders an enlarged image of the selected value.

```c#
[SerializeField, PreviewSprite] 
private Sprite characterPortrait;
```

![previewsprite-example](./Documentation/artifice_previewsprite.png)

### PreviewScriptable
This is one of the most magical attributes in the Artifice Toolkit. It allows you to dynamically instantiate a scriptable object inspector inside of another inspector. This can even work in a nested manner, previewing scriptable objects inside of other scriptable objects etc.

```c#
[CreateAssetMenu(menuName = "ScriptableObject/Character")]
public class SCR_Character : ScriptableObject
{
    [PreviewSprite, HorizontalGroup("row"), HideLabel]
    public Texture2D icon;

    [Title("First Name"), HorizontalGroup("row"), VerticalGroup("col"), HideLabel]
    public string firstName;
        
    [Title("Last Name"), HorizontalGroup("row"), VerticalGroup("col"), HideLabel]
    public string lastName;
}

[SerializeField, PreviewScriptable]
private List<SCR_Character> mainCharacter;
```

![preview-scriptable-example](./Documentation/artifice_previewscriptable.gif)

---

### ReadOnly

```c#
[SerializeField, ReadOnly] 
private float gravity = 9.81f;

[SerializeField, ReadOnly, ForceArtifice]
private Vector3 up = new Vector3(0f, 1f, 0f);
```

![readonly-example](./Documentation/artifice_readonly.jpg)


**NOTE**: To make Vector3 and similar structs be readonly, we need to enforce the usage of artifice in their nested properties. This is why, [ForceArtifice](#forceartifice) is used.

---

<!-- Miscellaneous -->
### Space
The Space attribute can receive up to four parameters reflecting the margin in pixels you want your element to have from top, bottom, left, right directions.

### Range
The Range attribute works on integers and floats and allows you to control them through a sliding bar. For this, a min and max value are required.

### HideLabel
The HideLabel attribute as the name suggests, finds and dynamically hides the label of the label.

### HideInArtifice
The HideInArtifice label works exactly as the default HideInInspector and does as it suggests. It skips the property from being rendered.

### InfoBox
The InfoBox allows you to add informational or warning prompts to a property. This could be useful for example to document or explain some rules regarding a specific value.

### ConditionalInfoBox
The ConditionalInfoBox allows you to optionally show an InfoBox when some condition is met, in a similar fashion as [EnableIf](#enableif).

### ListElementName
Using this attribute you can append to a list's elements, an extra identifier based on some nested field of the element type. This element can be applied to a list or array only, and the string parameter should match a child property of the element type.

```c#
[Serializable]
public class RaceGeneralInfo
{
    public enum RaceType
    {
        Human,
        Elf,
        Orc
    }
    
    [SerializeField] 
    private RaceType race;
    
    [SerializeField] 
    private int maxAge;

    [SerializeField] 
    private float maxHeight;
}

[ListElementName("race")]
public List<RaceGeneralInfo> info = new();
```

![](./Documentation/artifice_listelementname.gif)

### MeasureUnit
MeasureUnit appends any string passed to it at the right of a property. This can be commonly used to indicate whether a time value is measured in seconds or milliseconds! It can even be used as a self documented part of the code it self.

### MinValue
MinValue asserts a minimum int or float value to the serialized property. 

Note: Currently this only works while the inspector is open. There is nothing stoping the value of going below the minimum value if the inspector is closed.

### MaxValue
As [MinValue](#minvalue) but for a maximum value.

### ForceArtifice
The ArtificeDrawer is responsible for rendering the inspector using VisualElements, applying custom properties when necessary. For optimization, if no custom attributes are detected, it skips the Artifice rendering and falls back to a default IMGUIContainer.

However, there are cases where we might want to enforce the use of Artifice, even for nested properties that don't have custom attributes. In these scenarios, this attribute ensures that Artifice is always used, overriding the default behavior.


<!-- ARTIFICE VALIDATOR -->
# Artifice Validator
The best way to solve bugs, is to avoid creating them. Assertions are one of the most powerful ways to do this and it is considered one of the best programming practices. Using the Artifice Validator, you can apply assertions in your serialized properties.

![artifice-validator](./Documentation/artifice_validator.png)

The Validator works with attributes which inherit from the ValidatorAttribute class. Such attributes have an additional implementation explaining what they are supposed to be asserting. The most common use case the [Required](#required) attribute, to make sure the property has been assigned with a reference.


<!-- ARTIFICE DRAWER -->
# Artifice Drawer
The ArtificeDrawer is what renders everything when the Artifice Inspector is enabled. The ArtificeDrawer can receive a SerializedObject or SerializedProperty and returns a VisualElement of the rendered result. It essentially parses the SerializedObject or SerializedProperty and renders either the default result or the enhanced result if CustomAttributes have been used on that property.

This section will only interest you if you want to learn the underlying secrets of how the ArtificeToolkit works at its core and learn how to extend it with your own CustomAttributes and tools. Knowledge regarding CustomEditors, CustomPropertyDrawers etc will be needed.

## ArtificeDrawer GUI Steps
When a property directly uses a CustomAttribute, the drawer will access the respective [CustomAttributeDrawer](#custom-attribute-drawer) and call its GUI steps in order

  1. Pre GUI: Appends a VisualElement before the property.
  2. On GUI: Replaces the property with the result of this method. Only applies with IsReplacingProperty is set on true.
  3. Post GUI: Appends a VisualElement after the property.
  4. Wrap GUI: Returns a new VisualElement which adds the VisualElements from the previous steps inside of it.
  5. On Bound Property GUI: Executes code when the OnGUI VisualElement is attached in the inspector.

## Creating new CustomAttributes
To create a new CustomAttribute, you need to create the following:
  1. YourCustomAttribute inheriting from CustomAttribute. This should be placed in a runtime folder.
  2. Artifice_CustomAttributeDrawer_YourAttribute inheriting from the Artifice_CustomAttributeDrawer placed inside an Editor folder. In a similar fashion as CustomPropertyDrawers, you need to mark this class with a [CustomAttributeDrawer(typeof(YourAttribute))] to link them together.


## Creating New CustomAttributes

To create a new `CustomAttribute`, follow these steps:

1. **YourCustomAttribute**: Create your custom attribute by inheriting from `System.Attribute`. This should be placed in a **runtime** folder so it can be applied to your components or ScriptableObjects.
   
2. **Artifice_CustomAttributeDrawer_YourAttribute**: Create a custom attribute drawer by inheriting from `Artifice_CustomAttributeDrawer`. This drawer class must be placed inside an **Editor** folder.

   To link the attribute with its drawer, mark the drawer class with `[CustomPropertyDrawer(typeof(YourAttribute))]`.

### Example: `TitleAttribute`

In this example, we create a custom `TitleAttribute` that adds a styled header to serialized fields in the Unity Inspector.

### Step 1: Create the `TitleAttribute`

Create the `TitleAttribute` in a **runtime** folder. This attribute takes a string title, which will be used as a label in the Inspector.

```csharp
using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class TitleAttribute : CustomAttribute
{
    public string Title { get; }

    public TitleAttribute(string title)
    {
        Title = title;
    }
}
```

### Step 2: Create the CustomDrawer
Now, create a custom drawer for the TitleAttribute in an Editor folder. This drawer will display the title as a label in the Unity Inspector.

```c#
using UnityEditor;
using UnityEngine.UIElements;
using ArtificeToolkit.Editor.Artifice_CustomAttributeDrawers;

[CustomPropertyDrawer(typeof(TitleAttribute))]
public class Artifice_CustomAttributeDrawer_Title : Artifice_CustomAttributeDrawer
{
    private TitleAttribute _titleAttribute;

    // Initialize the TitleAttribute
    public Artifice_CustomAttributeDrawer_Title()
    {
        _titleAttribute = (TitleAttribute)Attribute;
    }

    // Override to insert the custom label before the property field
    public override VisualElement OnPrePropertyGUI(SerializedProperty property)
    {
        // Create a label using the title from the attribute
        return new Label(_titleAttribute.Title)
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 14,
                color = Color.white
            }
        };
    }
}
```

How to Use:
You can now use the TitleAttribute in any of your MonoBehaviour or ScriptableObject classes to add custom headers to your serialized fields:

```c#
using UnityEngine;

public class ExampleComponent : MonoBehaviour
{
    [Title("Player Settings")]
    public float health;

    [Title("Weapon Settings")]
    public int ammoCount;
}

```

## Known Issues
- In Unity 2021.x.x the following warning may appear due to value tracking not working generic types of serialized properties.
```
Serialized property type Generic does not support value tracking; callback is not set for characters
UnityEditor.RetainedMode:UpdateSchedulers ()
```