# ExpressionsEditor

*ExpressionsEditor* an animation editor that lets you create face expressions with the help of previews.

It does not require any SDK of any platform to be installed. In particular, VRChat is not required for the standalone version of *ExpressionsEditor* to run; it can be used along with other applications, social platforms, or your own games and applications.

<iframe src="https://streamable.com/e/8ysn22?loop=0" width="816" height="512" frameborder="0" allowfullscreen></iframe>

## Opening the editor

On the Unity Editor window menu at the top, click <span class="hai-btn">Window | Haï | EE Animation Editor</span>.

If you use *ComboGestureExpressions*, you can open an Activity click <span class="hai-btn">Create</span> on an empty slot, or click <span class="hai-btn">ExpressionsEditor</span> on the top right.

## Generate a Preview dummy

Make sure your scene has an avatar with an animator on it.

Click <span class="hai-btn">Automatically setup previews!</span> to create a preview dummy.

A preview dummy is a duplicate copy of your avatar that EE will use to generate previews of your animations. It ensures that EE will not modify the original avatar by accident. You are free to modify the preview dummy to make previews look nicer, including the camera positions.

For best results, you should:

- Make sure *Legacy Blendshape Normals* options is check in the import settings of your avatar mesh (on the VRChat platform, the upload process will force you to check this box).
- If you use glitter or any sort of animated skin, remove them by creating a new material that does not have those.
- Disable or remove particle systems.

## Add or remove blendshapes

Click the <span class="hai-btn">+</span> sign to open the *Property Explorer*.

Then click <span class="hai-btn">Generate previews</span>.

You can then click the <span class="hai-btn">+</span> under a preview to add the blendshape to the current animation.

## What are Reset blendshapes?

Depending on the platform you're working on, the animations may need to **reset** the unused blendshape values to the default values in order to prevent conflicts between animations. They are referred to as *Reset blendshapes* in EE.

Reset blendshapes are not shown in the EE even if they are present in the animation.

If you want to edit those anyways, you need to add them through the *Property Explorer* window.

Note: If you use *ComboGestureExpressions*, your animations do not need *Reset blendshapes*, as they are automatically generated.

## What are Based blendshapes?

Some blendshapes will have a preview that is either completely gray, or barely changes anything. These blendshapes are likely changing meshes that are not currently visible. For instance, it could be teeth variations such as fangs, tongue variations, tears positions, etc.

In order to generate more relevant previews, you need to make this blenshape generate a preview **based** on what another blendshape is animating. To do this:

- In *Property Explorer*, open Other tools.
- Click *Fix Tooth and oher hidden blendshapes*.
- Click *Select* on all the blendshapes of the same category that have incorrect previews.
- When done selecting, click *Assign* on the blendshape that you want be **based** on. For instance, to fix teeth blendshapes, click *Assign* on a blendshape that has an open mouth with teeth showing.
- The selected blendshapes now have the word **Based** on them, indicating that the preview only reflects what that blendshape is doing based on something else.

Based blendshapes are stored in the `Assets/Hai/EeMetadata.asset` file.

## Cameras

When the dummy is generated, three cameras are generated inside the dummy in order to preview the avatar with different angles.

You are free to change the position of these cameras within the hierarchy of the dummy.

If you need more than three cameras, you can edit the dummy configuration.

## Other animation options

If you need to edit the animation by hand, you can click <span class="hai-btn">Select animator to edit animation</span>. This will select the preview dummy which has an automatically generated animator controller, so that you can hit the 🔴 record button on the Unity's usual animation editor.

## Installing without VRChat SDK

In order to install without VRChat SDK, download *ComboGestureExpressions*. When installing the `.unitypackage` file, uncheck the `Hai/ComboGesture` folder, so that only `Hai/ExpressionsEditor` folder is installed.

When not using VRChat SDK, the avatar is detected by finding the last humanoid animator in the hierarchy that has blendshapes.

---

- [Back to main page](index.md)
